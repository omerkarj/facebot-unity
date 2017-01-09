using UnityEngine;
using System.Threading;
using System.Collections;
using System.Collections.Generic;

public sealed class Server : MonoBehaviour {

	private AsyncSocketListener listener;

	public void StartServer()
	{
		listener = AsyncSocketListener.Instance;
		new Thread(new ThreadStart(listener.StartListening)).Start();
		listener.MessageReceived += new MessageReceivedHandler(MessageReceived);
		listener.MessageSubmitted += new MessageSubmittedHandler(ServerMessageSubmitted);
	}

	/* Code to handle the events from AsyncSocketListener and AsyncClient. */
	private void MessageReceived(int id, string msg)
	{
		listener.Send(id, msg.Replace("client", "server"), true);
		Debug.Log("Received " + msg);
	}

	private void ServerMessageSubmitted(int id, bool close)
	{
		if (close)
			listener.Close(id);
	}
}