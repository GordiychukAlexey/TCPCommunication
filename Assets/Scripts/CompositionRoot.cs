using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using ClientServer;
using ClientServer.Interfaces;
using Interfaces;
using UniRx;
using UnityEngine;
using Zenject;

public class CompositionRoot : MonoBehaviour {
	[Inject] private IClientSideControls clientSideControls;
	[Inject] private IServerSide serverSide;
	[Inject] private ControlPanel controlPanel;

	[SerializeField] private int port = 1234;
	[SerializeField] private string host = "127.0.0.1";

	private int Port => int.Parse(controlPanel.PortInputField.text);
	private string Host => controlPanel.HostInputField.text;

	private IServer server;
	private IClient client;

	private const string message_SwitchLight = "Switch light";
	private const string message_Explode = "Explosion";

	private void Start(){
		controlPanel.PortInputField.text = port.ToString();
		controlPanel.RunServerButton.onClick.AddListener(RunServer);
		controlPanel.StopServerButton.onClick.AddListener(StopServer);

		controlPanel.HostInputField.text = host;
		controlPanel.RunClientButton.onClick.AddListener(RunClient);
		controlPanel.StopClientButton.onClick.AddListener(StopClient);

		clientSideControls.SwitchLight.Subscribe(_ => Client_SendMessage(message_SwitchLight));
		clientSideControls.Explode.Subscribe(_ => Client_SendMessage(message_Explode));
	}

	private void Client_SendMessage(string message){
		if (client != null){
			Log($"[Client {client.IpEndPoint}] Send message: {message}");
			client.Send(System.Text.Encoding.UTF8.GetBytes($"{message}\r\n"));
		}
	}

	private void RunServer(){
		if (server != null){
			LogError($"[Server {server.IpEndPoint.Port}] Server already started");
			return;
		}

		server = new Server(new IPEndPoint(IPAddress.Any, Port));

		void SendMessageToClient(IPEndPoint clientIpEndPoint, string message){
			Log($"[Server {server.IpEndPoint.Port}] Send message to {clientIpEndPoint}: {message}");
			server.SendMessageToClient(clientIpEndPoint, System.Text.Encoding.UTF8.GetBytes($"{message}\r\n"));
		}

		server.OnClientMessage.Subscribe(clientMessage => {
			Log($"[Server {server.IpEndPoint.Port}] Message received from {clientMessage.ipEndPoint}: {clientMessage.message}");
			switch (clientMessage.message){
				case message_SwitchLight:
					serverSide.SwitchLight();
					SendMessageToClient(clientMessage.ipEndPoint, "Light switched");
					break;
				case message_Explode:
					serverSide.Explode();
					SendMessageToClient(clientMessage.ipEndPoint, "Exploded");
					break;
				default:
					LogError($"[Server {server.IpEndPoint.Port}] Unknown command");
					server.SendMessageToClient(clientMessage.ipEndPoint, System.Text.Encoding.UTF8.GetBytes("Unknown command\r\n"));
					break;
			}
		});

		server.OnClientEstablished.Subscribe(endPoint => Log($"[Server {server.IpEndPoint.Port}] Client connected: {endPoint}"));
		server.OnClientDisconnected.Subscribe(endPoint => Log($"[Server {server.IpEndPoint.Port}] Client disconnected: {endPoint}"));

		Task serverListenTask = server.Listen();

		Log($"[Server {server.IpEndPoint.Port}] Server runned");
	}

	private void StopServer(){
		if (server != null){
			IPEndPoint endPoint = server.IpEndPoint;
			server?.Stop();
			server = null;
			Log($"[Server {endPoint.Port}] Server stopped");
		}
	}

	private void RunClient(){
		if (client != null){
			LogError($"[Client] Client already started");
			return;
		}

		try{
			client = new Client(Host, Port);
		}
		catch (SocketException e){
			LogError($"[Client] Connection with {Host}:{Port} failed");
			return;
		}

		client.OnMessageReceived.Subscribe(message => Log($"[Client {client.IpEndPoint}] Message received: {message}"));

		Task clientListenTask = client.Listen();

		Log($"[Client {client.IpEndPoint}] Runned");
	}

	private void StopClient(){
		if (client != null){
			IPEndPoint endPoint = client.IpEndPoint;
			client?.Dispose();
			client = null;
			Log($"[Client {endPoint}] Stopped");
		}
	}

	#region Helpers

	private void Log(string msg){
		Debug.Log(msg);
		controlPanel.LogText.text += $"\n[{TimeLabel}] Log: {msg}";
	}

	private void LogError(string msg){
		Debug.LogError(msg);
		controlPanel.LogText.text += $"\n[{TimeLabel}] Error: {msg}";
	}

	private static string TimeLabel => DateTime.Now.ToString("HH:mm:ss.fff");

	#endregion
}