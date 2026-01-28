namespace DaisNET.Networking.Exceptions.Connection
{
	public class ConnectionTimeoutException : Exception
	{
		public ConnectionTimeoutException(int ms, string? message)
			: base(message ?? $"Connection timed out in {ms}ms.")
		{
		}

		public ConnectionTimeoutException(int ms, string? message, Exception? innerException)
			: base(message ?? $"Connection timed out in {ms}ms.", innerException)
		{
		}
	}
}