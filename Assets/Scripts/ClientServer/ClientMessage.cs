using System.Net;

namespace ClientServer {
	public struct ClientMessage {
		public IPEndPoint ipEndPoint;
		public string message;

		public ClientMessage(IPEndPoint ipEndPoint, string message){
			this.ipEndPoint = ipEndPoint;
			this.message    = message;
		}
	}
}