using System.IO;

namespace Xamarin.Forms.Loader
{
	internal static class Logger
	{
		static StreamWriter writer;

		public static void Init()
		{
			writer = new StreamWriter("../../Logs/Xamarin.Forms.UITest.Validator.log", false);
		}

		public static void Log(string text)
		{
			writer.WriteLine(text);
		}

		public static void LogLine(string line = "")
		{
			writer.WriteLine(line);
		}

		public static void Close()
		{
			writer.Flush();
			writer.Close();
		}
	}
}