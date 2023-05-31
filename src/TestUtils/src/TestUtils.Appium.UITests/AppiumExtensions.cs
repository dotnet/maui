using System.Diagnostics;
using OpenQA.Selenium.Appium;
using Xamarin.UITest.Queries;

namespace TestUtils.Appium.UITests
{
	public static class AppiumExtensions
	{
		public static AppResult ToAppResult(this AppiumElement element) => new AppResult
		{
			Rect = ToAppRect(element),
			Label = element.Id,
			Id = element.Id,
			Text = element.Text
		};

		static AppRect? ToAppRect(this AppiumElement element)
		{
			try
			{
				var result = new AppRect
				{
					X = element.Location.X,
					Y = element.Location.Y,
					Height = element.Size.Height,
					Width = element.Size.Width
				};

				result.CenterX = result.X + result.Width / 2;
				result.CenterY = result.Y + result.Height / 2;

				return result;
			}
			catch (Exception ex)
			{
				Debug.WriteLine(
					$"Warning: error determining AppRect for {element}; "
					+ $"if this is a Label with a modified Text value, it might be confusing Windows automation. " +
					$"{ex}");
			}

			return null;
		}
	}
}
