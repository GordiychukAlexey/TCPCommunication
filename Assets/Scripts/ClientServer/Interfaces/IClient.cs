using System;
using System.Net;
using System.Threading.Tasks;
using UniRx;

namespace ClientServer.Interfaces {
	public interface IClient {
		IObservable<string> OnMessageReceived{ get; }

		IPEndPoint IpEndPoint{ get; }

		Task Listen();

		void Send(byte[] data);

		void Dispose();
	}
}