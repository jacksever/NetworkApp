using System;

namespace NetworkApp
{
	public static class ConsoleHelper
	{
		public static readonly object LockObject = new object();

		public static void WriteToConsole(string info, string write)
		{
			lock (LockObject)
				Console.WriteLine($"{info} : {write}");
		}
	}
}
