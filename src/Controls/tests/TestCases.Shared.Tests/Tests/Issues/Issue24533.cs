#if TEST_FAILS_ON_WINDOWS		//For more info : https://github.com/dotnet/maui/issues/31375
using System.Globalization;
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue24533 : _IssuesUITest
	{
		public override string Issue => "[iOS] RefreshView causes CollectionView scroll position to reset";

		public Issue24533(TestDevice device) : base(device)
		{
		}

		[Test]
		[Category(UITestCategories.RefreshView)]
		public void CollectionViewWithRefreshViewShouldNotReset()
		{
			App.WaitForElement("Footer");
			App.Tap("Footer");
			App.ScrollTo("Footer");
			App.Tap("Footer");
			App.ScrollTo("Footer");
			var verticalOffsetBeforeRefresh = GetVerticalOffset();
			Assert.That(verticalOffsetBeforeRefresh, Is.GreaterThan(0));

			App.Tap("Footer");
			App.ScrollTo("Footer");
			var verticalOffsetAfterRefresh = GetVerticalOffset();
			Assert.That(verticalOffsetAfterRefresh, Is.GreaterThan(0));
		}

		double GetVerticalOffset()
		{
			var verticalOffsetText = App.WaitForElement("VerticalOffsetLabel").GetText() ?? string.Empty;
			var verticalOffsetValue = verticalOffsetText.Replace("VerticalOffset:", string.Empty, StringComparison.Ordinal).Trim();

			return double.Parse(verticalOffsetValue, CultureInfo.InvariantCulture);
		}
	}
}
#endif