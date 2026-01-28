namespace DaisNET.Networking.Exceptions.Connection
{
	public class ServerShutdownException : Exception
	{
		public ServerShutdownException(string? message)
			: base(message ?? "Server shutdown unexpectedly.")
		{
		}

		public ServerShutdownException(string? message, Exception? innerException)
			: base(message ?? "Server shutdown unexpectedly.", innerException)
		{
		}
	}
}