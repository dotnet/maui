#nullable disable
using System.Drawing;
using System.Globalization;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using UITest.Appium;

namespace UITests
{
	public class Issue1733 : IssuesUITest
	{
		const string editorHeightShrinkWithPressureId = "editorHeightShrinkWithPressureId";
		const string editorHeightGrowId = "editorHeightGrowId";
		const string editorWidthGrow1Id = "editorWidthGrow1Id";
		const string editorWidthGrow2Id = "editorWidthGrow2Id";
		const string btnChangeFontToDefault = "Change the Font to Default";
		const string btnChangeFontToLarger = "Change the Font to Larger";
		const string btnChangeToHasText = "Change to Has Text";
		const string btnChangeToNoText = "Change to Has No Text";
		const string btnChangeSizeOption = "Change the Size Option";

		Dictionary<string, Size> _results;

		public Issue1733(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Autoresizable Editor";

		[Test]
		[Category(UITestCategories.Editor)]
		public void EditorAutoResize()
		{
			this.IgnoreIfPlatforms([TestDevice.Mac, TestDevice.Windows]);

			string[] editors = new string[] { editorHeightShrinkWithPressureId, editorHeightGrowId, editorWidthGrow1Id, editorWidthGrow2Id };
			RunningApp.WaitForElement(editorHeightShrinkWithPressureId);

			_results = new Dictionary<string, Size>();

			foreach (var editor in editors)
			{
				_results.Add(editor, GetDimensions(editor));
			}

			RunningApp.Tap(btnChangeToHasText);
			RunningApp.WaitForElement(btnChangeToNoText);
			TestGrowth(false);
			RunningApp.Tap(btnChangeFontToLarger);
			RunningApp.WaitForElement(btnChangeFontToDefault);
			TestGrowth(true);

			// Reset back to being empty and make sure everything sets back to original size
			RunningApp.Tap(btnChangeFontToDefault);
			RunningApp.Tap(btnChangeToNoText);
			RunningApp.WaitForElement(btnChangeToHasText);
			RunningApp.WaitForElement(btnChangeFontToLarger);

			foreach (var editor in editors)
			{
				var allTheSame = GetDimensions(editor);
				ClassicAssert.AreEqual(allTheSame.Width, _results[editor].Width, editor);
				ClassicAssert.AreEqual(allTheSame.Height, _results[editor].Height, editor);
			}

			// This sets it back to not auto size and we click everything again to see if it grows
			RunningApp.Tap(btnChangeSizeOption);
			RunningApp.Tap(btnChangeFontToLarger);
			RunningApp.Tap(btnChangeToHasText);
			RunningApp.WaitForElement(btnChangeFontToDefault);
			RunningApp.WaitForElement(btnChangeToNoText);
			foreach (var editor in editors)
			{
				var allTheSame = GetDimensions(editor);
				ClassicAssert.AreEqual(allTheSame.Width, _results[editor].Width, editor);
				ClassicAssert.AreEqual(allTheSame.Height, _results[editor].Height, editor);
			}
		}

		void TestGrowth(bool heightPressureShrink)
		{
			var testSizes = GetDimensions(editorHeightShrinkWithPressureId);
			ClassicAssert.AreEqual(testSizes.Width, _results[editorHeightShrinkWithPressureId].Width, editorHeightShrinkWithPressureId);

			if (heightPressureShrink)
				ClassicAssert.Less(testSizes.Height, _results[editorHeightShrinkWithPressureId].Height, editorHeightShrinkWithPressureId);
			else
				ClassicAssert.Greater(testSizes.Height, _results[editorHeightShrinkWithPressureId].Height, editorHeightShrinkWithPressureId);

			testSizes = GetDimensions(editorHeightGrowId);
			ClassicAssert.AreEqual(testSizes.Width, _results[editorHeightGrowId].Width, editorHeightGrowId);
			ClassicAssert.Greater(testSizes.Height, _results[editorHeightGrowId].Height, editorHeightGrowId);

			var grow1 = GetDimensions(editorWidthGrow1Id);
			ClassicAssert.Greater(grow1.Width, _results[editorWidthGrow1Id].Width, editorWidthGrow1Id);
			ClassicAssert.Greater(grow1.Height, _results[editorWidthGrow1Id].Height, editorWidthGrow1Id);

			var grow2 = GetDimensions(editorWidthGrow2Id);
			ClassicAssert.Greater(grow2.Width, _results[editorWidthGrow2Id].Width, editorWidthGrow2Id);
			ClassicAssert.Greater(grow2.Height, _results[editorWidthGrow2Id].Height, editorWidthGrow2Id);

			// Grow 1 has a lower minimum width request so it's width should be smaller than grow 2
			ClassicAssert.Greater(grow2.Width, grow1.Width, "grow2.Width > grow1.Width");
		}

		Size GetDimensions(string editorName)
		{
			RunningApp.WaitForElement($"{editorName}_height");
			RunningApp.WaitForElement($"{editorName}_width");

			var height = RunningApp.WaitForElement($"{editorName}_height").GetText();
			var width = RunningApp.WaitForElement($"{editorName}_width").GetText();

			if (height == null)
			{
				throw new ArgumentException($"{editorName}_height not found");
			}
			if (width == null)
			{
				throw new ArgumentException($"{editorName}_width not found");
			}
			return new Size(Convert.ToInt32(width, CultureInfo.InvariantCulture), Convert.ToInt32(height, CultureInfo.InvariantCulture));
		}
	}
}