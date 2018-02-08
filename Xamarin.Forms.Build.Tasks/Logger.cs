using System;
using System.Xml;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Xamarin.Forms.Xaml;

namespace Xamarin.Forms.Build.Tasks
{
	public class Logger {
		public TaskLoggingHelper Helper { get; }

		public Logger(TaskLoggingHelper helper)
		{
			Helper = helper;
		}


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
			FlushBuffer();

			if (Helper != null) {
				Helper.LogError(subcategory, errorCode, helpKeyword, file, lineNumber, columnNumber, endLineNumber,
					endColumnNumber, message, messageArgs);
			} else
				Console.Error.WriteLine($"{file} ({lineNumber}:{columnNumber}) : {message}");
		}

		public void LogLine(MessageImportance messageImportance, string format, params object [] arg)
		{
			if (!string.IsNullOrEmpty(buffer)) {
				format = buffer + format;
				buffer = "";
				bufferImportance = MessageImportance.Low;
			}

			if (Helper != null)
				Helper.LogMessage(messageImportance, format, arg);
			else
				Console.WriteLine(format, arg);
		}

		public void LogString(MessageImportance messageImportance, string format, params object [] arg)
		{
			if (Helper != null) {
				buffer += String.Format(format, arg);
				bufferImportance = messageImportance;
			} else
				Console.Write(format, arg);
		}

		string buffer = "";
		MessageImportance bufferImportance = MessageImportance.Low;
		void FlushBuffer()
		{
			if (!string.IsNullOrEmpty(buffer)) {
				if (Helper != null)
					Helper.LogMessage(bufferImportance, buffer);
				else
					Console.WriteLine(buffer);
			}
			buffer = "";
			bufferImportance = MessageImportance.Low;
		}
	}
}