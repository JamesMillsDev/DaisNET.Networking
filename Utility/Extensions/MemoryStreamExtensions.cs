namespace DaisNET.Utility.Extensions
{
	/// <summary>
	/// A collection of extension methods to assist working with the <see cref="MemoryStream"/> class.
	/// </summary>
	public static class MemoryStreamExtensions
	{
		/// <summary>
		/// Simple helper to get a specific amount of bytes from the memory stream.
		/// </summary>
		/// <param name="stream">The stream to get the memory from.</param>
		/// <param name="count">The amount of bytes to get </param>
		/// <returns></returns>
		public static byte[] ReadBytes(this MemoryStream stream, int count)
		{
			byte[] buffer = new byte[count];
			stream.ReadExactly(buffer, 0, count);
			return buffer;
		}
	}
}