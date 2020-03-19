using System;
using System.Net;
using System.Threading.Tasks;
using UniRx;

namespace ClientServer.Interfaces {
	public interface IServer {
		IObservable<ClientMessage> OnClientMessage{ get; }
		IObservable<IPEndPoint> OnClientEstablished{ get; }
		IObservable<IPEndPoint> OnClientDisconnected{ get; }

		IPEndPoint IpEndPoint{ get; }

		Task Listen();

		void BroadcastToClients(byte[] dataBytes);

		void SendMessageToClient(IPEndPoint clientIpEndPoint, byte[] dataBytes);

		void Stop();
	}
}