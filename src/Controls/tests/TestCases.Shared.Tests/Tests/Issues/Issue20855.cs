using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue20855 : _IssuesUITest
	{

		public Issue20855(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Grouped CollectionView items not rendered properly on Android, works on Windows";

		[Test]
		[ShardedTestCategory(UITestCategories.CollectionView, shard: 7)]
		public void GroupedCollectionViewItems()
		{
			App.WaitForElement("GroupedItems");
			App.RetryAssert(() =>
			{
				var collection = App.WaitForElementAndGetRect("GroupedItems");
				var previousBottom = collection.Top;
				for (var group = 1; group <= 2; group++)
				{
					var header = App.WaitForElementAndGetRect($"Group {group}");
					Assert.That(header.Height, Is.GreaterThan(0));
					Assert.That(header.Top, Is.GreaterThanOrEqualTo(previousBottom - 1));
					previousBottom = header.Bottom;

					for (var item = group * 2 - 1; item <= group * 2; item++)
					{
						var name = App.WaitForElementAndGetRect($"Item {item}");
						var dateElement = App.WaitForElement($"Date_Item {item}");
						var date = dateElement.GetRect();
						Assert.That(dateElement.GetText(), Is.EqualTo($"Feb {21 - item}"));
						Assert.That(name.Height, Is.GreaterThan(0));
						Assert.That(date.Height, Is.GreaterThan(name.Height));
						Assert.That(date.Top, Is.GreaterThanOrEqualTo(previousBottom - 1));
						Assert.That(name.Right, Is.LessThanOrEqualTo(date.Left));
						Assert.That(name.CenterY(), Is.EqualTo(date.CenterY()).Within(2));
						Assert.That(name.Left, Is.GreaterThanOrEqualTo(collection.Left));
						Assert.That(date.Right, Is.LessThanOrEqualTo(collection.Right + 1));
						Assert.That(date.Bottom, Is.LessThanOrEqualTo(collection.Bottom));
						previousBottom = date.Bottom;
					}
				}
			});
		}
	}
}