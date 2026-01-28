namespace DaisNET.Networking.Exceptions.Connection
{
	public class ConnectionFailedException : Exception
	{
		public ConnectionFailedException(string? message)
			: base(message ?? "Connection failed.")
		{
		}

		public ConnectionFailedException(string? message, Exception? innerException)
			: base(message ?? "Connection failed.", innerException)
		{
		}
	}
}