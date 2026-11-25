#if IOS
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue8191 : _IssuesUITest
	{
		public override string Issue => "ContentButton item template roots apply CollectionView cell safe areas";

		public Issue8191(TestDevice device) : base(device) { }

		[Test]
		[ShardedTestCategory(UITestCategories.CollectionView, shard: 4)]
		public void ContentButtonItemRootsMatchCellSafeAreaInset()
		{
			App.SetOrientationLandscape();
			App.WaitForElement("MeasureButton");
			App.Tap("MeasureButton");
			var text = App.WaitForElement("MeasureResult").GetText() ?? string.Empty;
			var positions = text.Split(';', StringSplitOptions.RemoveEmptyEntries)
				.Select(entry => entry.Split('='))
				.ToDictionary(parts => parts[0], parts => double.Parse(parts[1], System.Globalization.CultureInfo.InvariantCulture));

			// The reference Border root is inset past the landscape notch; ContentButton roots,
			// including one wrapped for its Shadow, must receive the same per-cell inset.
			Assert.That(positions["reference"], Is.GreaterThan(20), text);
			Assert.That(positions["direct"], Is.EqualTo(positions["reference"]).Within(2), text);
			Assert.That(positions["wrapped"], Is.EqualTo(positions["reference"]).Within(2), text);
		}

		[TearDown]
		public void TearDown()
		{
			App.SetOrientationPortrait();
		}
	}
}
#endif
