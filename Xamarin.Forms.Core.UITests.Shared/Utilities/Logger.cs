using System.IO;

using Xamarin.UITest.Queries;

namespace Xamarin.Forms.Core.UITests
{
	internal static class Logger
	{
		static StreamWriter queryWriter;

		public static void Init()
		{
			queryWriter = new StreamWriter("../../Xamarin.Forms.Core-UITest-queries.log", false);
		}

		public static void Log(string text)
		{
			queryWriter.Write(text);
		}

		public static void LogLine(string text = "")
		{
			queryWriter.WriteLine(text);
		}

		public static void Close()
		{
			queryWriter.Flush();
			queryWriter.Close();
		}

		public static void LogQueryResult(AppResult[] resultsForQuery)
		{
			foreach (AppResult result in resultsForQuery)
				WriteAppResult(result);
		}

		static void WriteAppResult(AppResult appResult)
		{
			var classText = string.Format("  {0, -10} : {1}", "Class", appResult.Class);
			var descriptionText = string.Format("  {0, -10} : {1}", "Description", appResult.Description);
			var enabledText = string.Format("  {0, -10} : {1}", "Enabled", appResult.Enabled);
			var idText = string.Format("  {0, -10} : {1}", "Id", appResult.Id);
			var labelText = string.Format("  {0, -10} : {1}", "Label", appResult.Id);
			var textText = string.Format("  {0, -10} : {1}", "Text", appResult.Text);

			var rectText = string.Format("  {0, -10}", "Rect");
			var rectContentsText = string.Format("    [X:{0} Y:{1} W:{2} H:{3}] [CX:{4} CY:{5}]",
				appResult.Rect.X,
				appResult.Rect.Y,
				appResult.Rect.Width,
				appResult.Rect.Height,
				appResult.Rect.CenterX,
				appResult.Rect.CenterY
			);

			queryWriter.WriteLine(classText);
			queryWriter.WriteLine(descriptionText);
			queryWriter.WriteLine(enabledText);
			queryWriter.WriteLine(idText);
			queryWriter.WriteLine(labelText);
			queryWriter.WriteLine(textText);
			queryWriter.WriteLine(rectText);
			queryWriter.WriteLine(rectContentsText);
			queryWriter.WriteLine();
		}

	}
}