namespace DaisNET.Utility
{
	public enum LogLevel
	{
		All,
		Trace,
		Debug,
		Info,
		Warning,
		Error,
		Fatal,
		None
	}

	public static class ConsoleLog
	{
		public static void Log(string msg, LogLevel level)
		{
			Console.ForegroundColor = LogLevelToColor(level);
			
			Console.WriteLine($"[{LogLevelToString(level)}] {msg}");
			
			Console.ForegroundColor = ConsoleColor.White;
		}
		
		private static ConsoleColor LogLevelToColor(LogLevel logLevel) => logLevel switch
		{
			LogLevel.All => ConsoleColor.White,
			LogLevel.Trace => ConsoleColor.Black,
			LogLevel.Info => ConsoleColor.Green,
			LogLevel.Debug => ConsoleColor.Blue,
			LogLevel.Warning => ConsoleColor.DarkYellow,
			LogLevel.Error => ConsoleColor.Red,
			LogLevel.Fatal => ConsoleColor.DarkMagenta,
			LogLevel.None => ConsoleColor.White,
			_ => throw new ArgumentOutOfRangeException(nameof(logLevel), logLevel, null)
		};

		private static string LogLevelToString(LogLevel logLevel) => logLevel switch
		{
			LogLevel.All => "UKN",
			LogLevel.Trace => "TRC",
			LogLevel.Debug => "DBG",
			LogLevel.Info => "INF",
			LogLevel.Warning => "WRN",
			LogLevel.Error => "ERR",
			LogLevel.Fatal => "FTL",
			LogLevel.None => "NON",
			_ => throw new ArgumentOutOfRangeException(nameof(logLevel), logLevel, null)
		};
	}
}