using UnityEngine; 
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

public class Server : MonoBehaviour {

	// State object for reading client data asynchronously
	public class StateObject {
		// Client  socket.
		public Socket workSocket = null;
		// Size of receive buffer.
		public const int BufferSize = 1024;
		// Receive buffer.
		public byte[] buffer = new byte[BufferSize];
		// Received data string.
		public StringBuilder sb = new StringBuilder();  
	}
		
	// Thread signal.
	private ManualResetEvent allDone = new ManualResetEvent(false);
	private bool m_Running;
	private Thread m_Thread;
	private ClientDataService clientDataService;

	void Start() 
	{
		clientDataService = GetComponent<ClientDataService> ();
		m_Running = true;
		ThreadStart ts = new ThreadStart(StartListening);
		m_Thread = new Thread(ts);
		m_Thread.Start();
		Debug.Log("Thread done...");
	}

	void OnApplicationQuit()
	{
		// stop listening thread
		StopListening();
	}

	public void StopListening()
	{
		m_Running = false;
		Debug.Log("\nStopping server...");

		// wait for listening thread to terminate (max. 500ms)
		m_Thread.Join(500);
	}

	public void StartListening() {
		try {
			// Establish the local endpoint for the socket.
			IPAddress ipAddress = IPAddress.Parse("127.0.0.1");
			IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 1337);

			// Create a TCP/IP socket.
			Socket listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

			// Bind the socket to the local endpoint and listen for incoming connections.
			try {
				listener.Bind(localEndPoint);
				listener.Listen(100);

				Debug.Log("Server Listening on port 1337");

				while (m_Running) {
					// Set the event to nonsignaled state.
					allDone.Reset();

					// Start an asynchronous socket to listen for connections.
					Debug.Log("Waiting for a connection...");
					listener.BeginAccept(new AsyncCallback(AcceptCallback), listener);

					// Wait until a connection is made before continuing.
					allDone.WaitOne();
				}

			} catch (Exception e) {
				Debug.Log(e.ToString());
			}

			Debug.Log("\nServer stopped...");

			if (m_Running) {
				Debug.Log ("restarting...");
				StartListening ();
			} 
		}
		catch (ThreadAbortException)
		{
			Debug.Log("Thread Exception!");
		}
	}

	public void AcceptCallback(IAsyncResult result) {
		// Signal the main thread to continue.
		allDone.Set();

		// Get the socket that handles the client request.
		Socket listener = (Socket) result.AsyncState;
		Socket handler = listener.EndAccept(result);

		// Create the state object.
		StateObject state = new StateObject();
		state.workSocket = handler;

		handler.BeginReceive(
			state.buffer, 
			0, 
			StateObject.BufferSize, 
			0, 
			new AsyncCallback(ReadCallback), 
			state
		);
	}

	public void ReadCallback(IAsyncResult result) {
		String content = String.Empty;

		// Retrieve the state object and the handler socket
		// from the asynchronous state object.
		StateObject state = (StateObject) result.AsyncState;
		Socket handler = state.workSocket;

		// Read data from the client socket. 
		int bytesRead = handler.EndReceive(result);

		if (bytesRead > 0) {
			// There  might be more data, so store the data received so far.
			state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, bytesRead));
			content = state.sb.ToString();

			if (content.IndexOf ("<NEXT>") > -1) {
				Debug.Log ("NEXT");
				state.sb.Remove (0, state.sb.Length);

				byte[] currentFrame = clientDataService.GetCurrentFrameAsByteArray ();
				Send(handler, currentFrame);

			} else if (content.IndexOf ("<PLAY>") > -1) {
				Debug.Log ("PLAY");
				state.sb.Remove (0, state.sb.Length);
				content = content.Substring (0, content.IndexOf ("<PLAY>"));
				string[] toSend = content.Split (new string[] { "<EOL>" }, StringSplitOptions.None);

				Debug.Log (string.Format ("drawing {0} lines from socket. \n", toSend.Length));

				clientDataService.ClearLines ();

				foreach (string line in toSend) {
					if (!string.IsNullOrEmpty (line)) {
						Debug.Log (string.Format ("line: {0}", line));
						clientDataService.DrawCompleteLineFromString (line);
					}
				}

				clientDataService.Play ();

				handler.BeginReceive (
					state.buffer, 
					0, 
					StateObject.BufferSize, 
					0,
					new AsyncCallback (ReadCallback), 
					state
				);

			} else if (content.IndexOf ("<RESET>") > -1) {
				Debug.Log ("RESET");
				state.sb.Remove (0, state.sb.Length);

				clientDataService.Stop ();
				clientDataService.Reset ();	

				handler.BeginReceive (
					state.buffer, 
					0, 
					StateObject.BufferSize, 
					0,
					new AsyncCallback (ReadCallback), 
					state
				);

			} else if (content.IndexOf ("<QUIT>") > -1) {
				Debug.Log ("QUIT");
				state.sb.Remove (0, state.sb.Length);

				// Closing connection
				handler.Shutdown (SocketShutdown.Both);
				handler.Close ();
				StopListening ();

			} else {
				// Not all data received. Get more.
				handler.BeginReceive (
					state.buffer, 
					0, 
					StateObject.BufferSize, 
					0,
					new AsyncCallback (ReadCallback), 
					state
				);
			}
		}
	}

	private void Send(Socket handler, byte[] data) {
		// Begin sending the data to the remote device.
		handler.BeginSend(
			data, 
			0, 
			data.Length, 
			0,
			new AsyncCallback(SendCallback), 
			handler
		);
	}

	private void SendCallback(IAsyncResult ar) {
		try {
			// Retrieve the socket from the state object.
			Socket handler = (Socket) ar.AsyncState;

			// Complete sending the data to the remote device.
			int bytesSent = handler.EndSend(ar);

			// Create the state object.
			StateObject state = new StateObject();
			state.workSocket = handler;

			handler.BeginReceive (
				state.buffer, 
				0, 
				StateObject.BufferSize, 
				0,
				new AsyncCallback (ReadCallback), 
				state
			);

		} catch (Exception e) {
			Debug.Log(e.ToString());
		}
	}
}