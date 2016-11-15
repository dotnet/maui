using System;
using System.Xml;
using Microsoft.Build.Utilities;
using Xamarin.Forms.Xaml;

namespace Xamarin.Forms.Build.Tasks
{
	public class Logger {
		public TaskLoggingHelper Helper { get; }
		public int Verbosity { get; }

		public Logger(TaskLoggingHelper helper, int verbosity)
		{
			Verbosity = verbosity;
			Helper = helper;
		}

		string buffer = "";

		public void LogException(string subcategory, string errorCode, string helpKeyword, string file, Exception e)
		{
			var xpe = e as XamlParseException;
			var xe = e as XmlException;
			if (xpe != null)
				LogError(subcategory, errorCode, helpKeyword, file, xpe.XmlInfo.LineNumber, xpe.XmlInfo.LinePosition, 0, 0, xpe.Message, xpe.HelpLink, xpe.Source);
			else if (xe != null)
				LogError(subcategory, errorCode, helpKeyword, file, xe.LineNumber, xe.LinePosition, 0, 0, xe.Message, xe.HelpLink, xe.Source);
			else
				LogError(subcategory, errorCode, helpKeyword, file, 0, 0, 0, 0, e.Message, e.HelpLink, e.Source);
		}

		public void LogError(string subcategory, string errorCode, string helpKeyword, string file, int lineNumber,
			int columnNumber, int endLineNumber, int endColumnNumber, string message, params object [] messageArgs)
		{
			if (!string.IsNullOrEmpty(buffer))
				LogLine(-1, null, null);
			if (Helper != null) {
				Helper.LogError(subcategory, errorCode, helpKeyword, file, lineNumber, columnNumber, endLineNumber,
					endColumnNumber, message, messageArgs);
			} else
				Console.Error.WriteLine($"{file} ({lineNumber}:{columnNumber}) : {message}");
		}

		public void LogLine(int level, string format, params object [] arg)
		{
			if (!string.IsNullOrEmpty(buffer)) {
				format = buffer + format;
				buffer = "";
			}

			if (level < 0) {
				if (Helper != null)
					Helper.LogError(format, arg);
				else
					Console.Error.WriteLine(format, arg);
			} else if (level <= Verbosity) {
				if (Helper != null)
					Helper.LogMessage(format, arg);
				else
					Console.WriteLine(format, arg);
			}
		}

		public void LogString(int level, string format, params object [] arg)
		{
			if (level <= 0)
				Console.Error.Write(format, arg);
			else if (level <= Verbosity) {
				if (Helper != null)
					buffer += String.Format(format, arg);
				else
					Console.Write(format, arg);
			}
		}
	}
}