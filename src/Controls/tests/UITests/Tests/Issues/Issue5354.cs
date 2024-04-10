using Microsoft.Maui.AppiumTests;
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;


namespace Maui.Controls.Sample.Issues
{
	public partial class Issue5354 : _IssuesUITest
	{
		public Issue5354(TestDevice device) : base(device) { }

		public override string Issue => "[CollectionView] Updating the ItemsLayout type should refresh the layout";

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void CollectionViewItemsLayoutUpdate()
		{
			this.IgnoreIfPlatforms(new TestDevice[] { TestDevice.Windows },
				"Copied from Xamarin.Forms. Test was not running or working on Windows.");

			App.WaitForElement("CollectionView5354");
			App.WaitForElement("Button5354");
			var colView = App.WaitForElement("CollectionView5354");
			App.WaitForNoElement("NoElement", timeout: TimeSpan.FromSeconds(3));
		
			var linearRect0 = App.WaitForElement("Image0").GetRect();
			var linearRect1 = App.WaitForElement("Image1").GetRect();

			Assert.AreEqual(linearRect0.X, linearRect1.X);
			Assert.GreaterOrEqual(linearRect1.Y, linearRect0.Y + linearRect0.Height);

			App.Click("Button5354");

			App.WaitForNoElement("NoElement", timeout: TimeSpan.FromSeconds(3));
			var gridRect0 = App.WaitForElement("Image0").GetRect();
			var gridRect1 = App.WaitForElement("Image1").GetRect();

			Assert.AreEqual(gridRect0.Y, gridRect1.Y);
			Assert.AreEqual(gridRect0.Height, gridRect1.Height);
		}
	}
}