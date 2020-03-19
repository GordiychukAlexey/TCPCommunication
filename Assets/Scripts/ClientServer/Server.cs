using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ClientServer.Interfaces;
using UniRx;

namespace ClientServer {
	public class Server : IServer, IDisposable {
		public IObservable<ClientMessage> OnClientMessage => onClientMessage.AsObservable();
		public IObservable<IPEndPoint> OnClientEstablished => onClientEstablished.AsObservable();
		public IObservable<IPEndPoint> OnClientDisconnected => onClientDisconnected.AsObservable();

		public IPEndPoint IpEndPoint => ipEndPoint;

		private readonly Subject<ClientMessage> onClientMessage = new Subject<ClientMessage>();
		private readonly Subject<IPEndPoint> onClientEstablished = new Subject<IPEndPoint>();
		private readonly Subject<IPEndPoint> onClientDisconnected = new Subject<IPEndPoint>();

		private readonly IPEndPoint ipEndPoint;

		private readonly List<TcpClient> clients = new List<TcpClient>();

		private readonly SynchronizationContext synchronizationContext; //from Unity thread

		private volatile bool acceptLoop = true; //состояние приёма нового соединения

		private TcpListener tcpListener;

		public Server(IPEndPoint ipEndPoint){
			this.ipEndPoint = ipEndPoint ?? throw new ArgumentNullException("endpoint should not be null");

			synchronizationContext = SynchronizationContext.Current;
		}

		public async Task Listen(){
			lock (this){
				if (tcpListener != null)
					throw new InvalidOperationException("Already started");

				acceptLoop  = true;
				tcpListener = new TcpListener(ipEndPoint);
			}

			tcpListener.Start();

			while (acceptLoop){
				try{
					TcpClient client = await tcpListener.AcceptTcpClientAsync()
														.ConfigureAwait(false); //?? не возвращаться в исходный поток (остаться в потоке await task)

					Task _ = Task.Run(() => OnConnectClient(client));
				}
				catch (ObjectDisposedException e){
					// thrown if the listener socket is closed
				}
				catch (SocketException e){
					// Some socket error
				}
			}
		}

		private async Task OnConnectClient(TcpClient client){
			IPEndPoint clientIpEndpoint = (IPEndPoint) client.Client.RemoteEndPoint;

			synchronizationContext.Post(_ => onClientEstablished.OnNext(clientIpEndpoint), null);
			clients.Add(client);

			await NetworkStreamHandler(client);

			synchronizationContext.Post(_ => onClientDisconnected.OnNext(clientIpEndpoint), null);
			clients.Remove(client);
		}

		private async Task NetworkStreamHandler(TcpClient client){
			while (client.Connected){
				using (NetworkStream stream = client.GetStream()){
					StreamReader reader = new StreamReader(stream, Encoding.UTF8);

					while (!reader.EndOfStream){
						string str = await reader.ReadLineAsync(); //todo try ReadToEndAsync
						synchronizationContext.Post(_ => onClientMessage.OnNext(new ClientMessage((IPEndPoint) client.Client.RemoteEndPoint, str)),
							null);
					}
				}
			}

			// Disconnected
		}

		public void Stop(){
			if (tcpListener == null) return;

			lock (this){
				acceptLoop = false;

				tcpListener.Stop();
				tcpListener = null;
			}


			lock (clients){
				foreach (var c in clients){
					c.Close();
				}
			}
		}

		public void BroadcastToClients(byte[] dataBytes){
			foreach (var tcpClient in clients){
				tcpClient.GetStream().Write(dataBytes, 0, dataBytes.Length);
				tcpClient.GetStream().Flush();
			}
		}

		public void SendMessageToClient(IPEndPoint clientIpEndPoint, byte[] dataBytes){
			var client = clients.FirstOrDefault(tcpClient => tcpClient.Client.RemoteEndPoint.Equals(clientIpEndPoint));
			if (client == null){
				throw new ArgumentException($"Client with end point {clientIpEndPoint} not found");
			}

			var stream = client.GetStream();
			stream.Write(dataBytes, 0, dataBytes.Length);
			stream.Flush();
		}

		public void Dispose(){
			Stop();
		}
	}
}