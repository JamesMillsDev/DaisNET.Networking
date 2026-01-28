namespace DaisNET.Networking.Exceptions.Connection
{
	public class ClientDisconnectedException : Exception
	{
		public ClientDisconnectedException(string? message)
			: base(message ?? "Client disconnected.")
		{
		}

		public ClientDisconnectedException(string? message, Exception? innerException)
			: base(message ?? "Client disconnected.", innerException)
		{
		}
	}
}