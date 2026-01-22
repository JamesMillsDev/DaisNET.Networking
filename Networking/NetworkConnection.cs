using System.Net;

namespace DaisNET.Networking
{
	/// <summary>
	/// Represents a network connection with a unique identifier and IP endpoint.
	/// Stores the essential information needed to identify and communicate with a connected client.
	/// </summary>
	public class NetworkConnection(IPEndPoint ipEndPoint)
	{
		// ReSharper disable InconsistentNaming
		/// <summary>
		/// Gets the unique identifier assigned to this connection by the server.
		/// Can only be set internally during initialization.
		/// </summary>
		public uint ID { get; internal init; }
       
		/// <summary>
		/// Gets the IP endpoint (address and port) for this connection.
		/// Set during construction and cannot be modified after initialization.
		/// </summary>
		public EndPoint? IP { get; private init; } = ipEndPoint;
		// ReSharper restore InconsistentNaming
	}
}