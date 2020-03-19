using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ClientServer.Interfaces;
using UniRx;
using UnityEngine;

namespace ClientServer {
	public class Client : IClient, IDisposable {
		public IObservable<string> OnMessageReceived => onMessageReceived.AsObservable();

		public bool IsConnecting{
			get{
				try{
					if ((client == null) || !client.Connected) return false;
					if (Socket == null) return false;

					return !(Socket.Poll(1, SelectMode.SelectRead) && (Socket.Available <= 0)); //сокет настроен на read и может читаться
				}
				catch{
					return false;
				}
			}
		}

		public IPEndPoint IpEndPoint => (IPEndPoint) client.Client.LocalEndPoint;

		private readonly Subject<string> onMessageReceived = new Subject<string>();

		private readonly TcpClient client;

		private readonly SynchronizationContext synchronizationContext; //from Unity thread

		private bool running = false;

		private Socket Socket => client?.Client;

		public Client(string host, int port){
			client = new TcpClient(host, port);

			synchronizationContext = SynchronizationContext.Current;
		}

		public void Send(byte[] data){
			if (data == null) throw new ArgumentNullException(nameof(data));
			if (!IsConnecting) throw new InvalidOperationException();

			try{
				var stream = client.GetStream();
				stream.Write(data, 0, data.Length);
			}
			catch (Exception ex){
				throw new ApplicationException("Attempt to send failed.", ex);
			}
		}

		public async Task Listen(){
			running = true;

			while (running){
				await Task.Run(Receive);
			}
		}

		private async Task Receive(){
			if (!IsConnecting) throw new InvalidOperationException();

			try{
				var stream = client.GetStream();

				while (stream.DataAvailable){
					var reader = new StreamReader(stream, Encoding.UTF8);

					var str = await reader.ReadLineAsync();

					synchronizationContext.Post(_ => onMessageReceived.OnNext(str), null);
				}
			}
			catch (Exception ex){
				throw new ApplicationException("Attempt to receive failed.", ex);
			}
		}

		public void Dispose(){
			if (client != null){
				running = false;

				client.Close();
				(client as IDisposable).Dispose();

				Debug.Log("Client disposed");
			}
		}
	}
}