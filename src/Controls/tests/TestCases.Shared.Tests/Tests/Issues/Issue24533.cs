#if TEST_FAILS_ON_WINDOWS && TEST_FAILS_ON_CATALYST
// TEST_FAILS_ON_WINDOWS    : For more info : https://github.com/dotnet/maui/issues/31375
// TEST_FAILS_ON_CATALYST   : ScrollTo is not working properly on MacCatalyst.
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
			App.WaitForElement("ItemsLoadedLabel");
			App.ScrollTo("Footer");
			App.Tap("Footer");
			AssertLoaded(50);
			App.ScrollTo("Footer");
			App.Tap("Footer");
			AssertLoaded(75);
			App.ScrollTo("Footer");
			var verticalOffsetBeforeRefresh = GetVerticalOffset();
			Assert.That(verticalOffsetBeforeRefresh, Is.GreaterThan(0));

			App.Tap("Footer");
			AssertLoaded(100);
			// Scrolling again here would conceal the position reset this test must detect.
			App.RetryAssert(() => Assert.That(GetVerticalOffset(),
				Is.EqualTo(verticalOffsetBeforeRefresh).Within(1)));
		}

		void AssertLoaded(int count) =>
			App.RetryAssert(() => Assert.That(App.WaitForElement("ItemsLoadedLabel").GetText(),
				Is.EqualTo($"Loaded: {count}")));

		double GetVerticalOffset()
		{
			var verticalOffsetText = App.WaitForElement("VerticalOffsetLabel").GetText() ?? string.Empty;
			var verticalOffsetValue = verticalOffsetText.Replace("VerticalOffset:", string.Empty, StringComparison.Ordinal).Trim();

			return double.Parse(verticalOffsetValue, CultureInfo.InvariantCulture);
		}
	}
}
#endif