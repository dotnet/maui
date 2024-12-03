#if ANDROID
using Microsoft.Maui.TestCases.Tests;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public partial class Issue5354 : _IssuesUITest
	{
		public Issue5354(TestDevice device) : base(device) { }

		public override string Issue => "[CollectionView] Updating the ItemsLayout type should refresh the layout";

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void CollectionViewItemsLayoutUpdate()
		{
			App.WaitForElement("CollectionView5354");
			App.WaitForElement("Button5354");

			for (int i = 0; i < 3; i++)
			{
				var linearRect0 = App.WaitForElement("Image0").GetRect();
				var linearRect1 = App.WaitForElement("Image1").GetRect();

				ClassicAssert.AreEqual(linearRect0.X, linearRect1.X);
				ClassicAssert.GreaterOrEqual(linearRect1.Y, linearRect0.Y + linearRect0.Height);

				App.Click("Button5354");

				var gridRect0 = App.WaitForElement("Image0").GetRect();
				var gridRect1 = App.WaitForElement("Image1").GetRect();

				ClassicAssert.AreEqual(gridRect0.Y, gridRect1.Y);
				ClassicAssert.AreEqual(gridRect0.Height, gridRect1.Height);

				App.Click("Button5354");
			}
		}
	}
}
#endif