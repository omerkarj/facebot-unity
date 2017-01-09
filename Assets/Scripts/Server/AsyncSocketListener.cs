using System;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Collections;
using System.Collections.Generic;

public delegate void MessageReceivedHandler(int id, string msg);
public delegate void MessageSubmittedHandler(int id, bool close);

public sealed class AsyncSocketListener
{
	private const ushort Port = 8080;
	private const ushort Limit = 250;

	private static readonly AsyncSocketListener instance = new AsyncSocketListener();

	private readonly ManualResetEvent mre = new ManualResetEvent(false);
	private readonly IDictionary<int, StateObject> clients = new Dictionary<int, StateObject>();

	public event MessageReceivedHandler MessageReceived;
	public event MessageSubmittedHandler MessageSubmitted;

	private AsyncSocketListener()
	{
	}

	public static AsyncSocketListener Instance
	{
		get
		{
			return instance;
		}
	}

	/* Starts the AsyncSocketListener */
	public void StartListening()
	{
		IPHostEntry host = Dns.GetHostEntry(string.Empty);
		IPAddress ip = host.AddressList[3];
		IPEndPoint endpoint = new IPEndPoint(ip, Port);

		try
		{
			using (Socket listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
			{
				listener.Bind(endpoint);
				listener.Listen(Limit);

				while (true)
				{
					this.mre.Reset();
					listener.BeginAccept(this.OnClientConnect, listener);
					this.mre.WaitOne();
				}
			}
		}
		catch (SocketException)
		{
			// TODO:
		}
	}

	/* Gets a socket from the clients dictionary by his Id. */
	private StateObject GetClient(int id)
	{
		StateObject state;

		return this.clients.TryGetValue(id, out state) ? state : null;
	}

	/* Checks if the socket is connected. */
	public bool IsConnected(int id)
	{
		StateObject state = this.GetClient(id);

		return !(state.Listener.Poll(1000, SelectMode.SelectRead) && state.Listener.Available == 0);
	}

	/* Add a socket to the clients dictionary. Lock clients temporary to handle multiple access.
     * ReceiveCallback raise a event, after the message receive complete. */
	public void OnClientConnect(IAsyncResult result)
	{
		this.mre.Set();

		try
		{
			StateObject state;

			lock (this.clients)
			{
				int id = this.clients.Count + 1;

				state = new StateObject(((Socket)result.AsyncState).EndAccept(result), id);
				this.clients.Add(id, state);
				Console.WriteLine("Client connected. Get Id " + id);
			}

			state.Listener.BeginReceive(state.Buffer, 0, state.BufferSize, SocketFlags.None, this.ReceiveCallback, state);
		}
		catch (SocketException)
		{
			// TODO:
		}
	}

	public void ReceiveCallback(IAsyncResult result)
	{
		StateObject state = (StateObject)result.AsyncState;

		try
		{
			var receive = state.Listener.EndReceive(result);

			if (receive > 0)
			{
				state.Append(Encoding.UTF8.GetString(state.Buffer, 0, receive));
			}

			if (receive == state.BufferSize)
			{
				state.Listener.BeginReceive(state.Buffer, 0, state.BufferSize, SocketFlags.None, this.ReceiveCallback, state);
			}
			else
			{
				var messageReceived = this.MessageReceived;

				if (messageReceived != null)
				{
					messageReceived(state.Id, state.Text);
				}

				state.Reset();
			}
		}
		catch (SocketException)
		{
			// TODO:
		}
	}
		
	/* Send(int id, String msg, bool close) use bool to close the connection after the message sent. */
	public void Send(int id, string msg, bool close)
	{
		StateObject state = this.GetClient(id);

		if (state == null)
		{
			throw new Exception("Client does not exist.");
		}

		if (!this.IsConnected(state.Id))
		{
			throw new Exception("Destination socket is not connected.");
		}

		try
		{
			byte[] send = Encoding.UTF8.GetBytes(msg);

			state.Close = close;
			state.Listener.BeginSend(send, 0, send.Length, SocketFlags.None, this.SendCallback, state);
		}
		catch (SocketException)
		{
			// TODO:
		}
		catch (ArgumentException)
		{
			// TODO:
		}
	}

	private void SendCallback(IAsyncResult result)
	{
		StateObject state = (StateObject)result.AsyncState;

		try
		{
			state.Listener.EndSend(result);
		}
		catch (SocketException)
		{
			// TODO:
		}
		catch (ObjectDisposedException)
		{
			// TODO:
		}
		finally
		{
			MessageSubmittedHandler messageSubmitted = this.MessageSubmitted;

			if (messageSubmitted != null)
			{
				messageSubmitted(state.Id, state.Close);
			}
		}
	}


	public void Close(int id)
	{
		StateObject state = this.GetClient(id);

		if (state == null)
		{
			throw new Exception("Client does not exist.");
		}

		try
		{
			state.Listener.Shutdown(SocketShutdown.Both);
			state.Listener.Close();
		}
		catch (SocketException)
		{
			// TODO:
		}
		finally
		{
			lock (this.clients)
			{
				this.clients.Remove(state.Id);
				Console.WriteLine("Client disconnected with Id {0}", state.Id);
			}
		}
	}

	public void Dispose()
	{
		foreach (int id in this.clients.Keys)
		{
			this.Close(id);
		}

		this.mre.Close ();
	}
}
