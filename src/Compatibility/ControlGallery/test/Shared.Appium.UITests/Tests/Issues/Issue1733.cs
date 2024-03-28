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
		const string EditorHeightShrinkWithPressureId = "editorHeightShrinkWithPressureId";
		const string EditorHeightGrowId = "editorHeightGrowId";
		const string EditorWidthGrow1Id = "editorWidthGrow1Id";
		const string EditorWidthGrow2Id = "editorWidthGrow2Id";
		const string BtnChangeFontToDefault = "Change the Font to Default";
		const string BtnChangeFontToLarger = "Change the Font to Larger";
		const string BtnChangeToHasText = "Change to Has Text";
		const string BtnChangeToNoText = "Change to Has No Text";
		const string BtnChangeSizeOption = "Change the Size Option";

		Dictionary<string, Size> _results;

		public Issue1733(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Autoresizable Editor";

		[Test]
		[Category(UITestCategories.Editor)]
		[FailsOnAndroid]
		[FailsOnIOS]
		public void EditorAutoResize()
		{
			this.IgnoreIfPlatforms([TestDevice.Mac, TestDevice.Windows]);

			string[] editors = new string[] { EditorHeightShrinkWithPressureId, EditorHeightGrowId, EditorWidthGrow1Id, EditorWidthGrow2Id };
			RunningApp.WaitForElement(EditorHeightShrinkWithPressureId);

			_results = new Dictionary<string, Size>();

			foreach (var editor in editors)
			{
				_results.Add(editor, GetDimensions(editor));
			}

			RunningApp.Tap(BtnChangeToHasText);
			RunningApp.WaitForElement(BtnChangeToNoText);
			TestGrowth(false);
			RunningApp.Tap(BtnChangeFontToLarger);
			RunningApp.WaitForElement(BtnChangeFontToDefault);
			TestGrowth(true);

			// Reset back to being empty and make sure everything sets back to original size
			RunningApp.Tap(BtnChangeFontToDefault);
			RunningApp.Tap(BtnChangeToNoText);
			RunningApp.WaitForElement(BtnChangeToHasText);
			RunningApp.WaitForElement(BtnChangeFontToLarger);

			foreach (var editor in editors)
			{
				var allTheSame = GetDimensions(editor);
				ClassicAssert.AreEqual(allTheSame.Width, _results[editor].Width, editor);
				ClassicAssert.AreEqual(allTheSame.Height, _results[editor].Height, editor);
			}

			// This sets it back to not auto size and we click everything again to see if it grows
			RunningApp.Tap(BtnChangeSizeOption);
			RunningApp.Tap(BtnChangeFontToLarger);
			RunningApp.Tap(BtnChangeToHasText);
			RunningApp.WaitForElement(BtnChangeFontToDefault);
			RunningApp.WaitForElement(BtnChangeToNoText);
			foreach (var editor in editors)
			{
				var allTheSame = GetDimensions(editor);
				ClassicAssert.AreEqual(allTheSame.Width, _results[editor].Width, editor);
				ClassicAssert.AreEqual(allTheSame.Height, _results[editor].Height, editor);
			}
		}

		void TestGrowth(bool heightPressureShrink)
		{
			var testSizes = GetDimensions(EditorHeightShrinkWithPressureId);
			ClassicAssert.AreEqual(testSizes.Width, _results[EditorHeightShrinkWithPressureId].Width, EditorHeightShrinkWithPressureId);

			if (heightPressureShrink)
				ClassicAssert.Less(testSizes.Height, _results[EditorHeightShrinkWithPressureId].Height, EditorHeightShrinkWithPressureId);
			else
				ClassicAssert.Greater(testSizes.Height, _results[EditorHeightShrinkWithPressureId].Height, EditorHeightShrinkWithPressureId);

			testSizes = GetDimensions(EditorHeightGrowId);
			ClassicAssert.AreEqual(testSizes.Width, _results[EditorHeightGrowId].Width, EditorHeightGrowId);
			ClassicAssert.Greater(testSizes.Height, _results[EditorHeightGrowId].Height, EditorHeightGrowId);

			var grow1 = GetDimensions(EditorWidthGrow1Id);
			ClassicAssert.Greater(grow1.Width, _results[EditorWidthGrow1Id].Width, EditorWidthGrow1Id);
			ClassicAssert.Greater(grow1.Height, _results[EditorWidthGrow1Id].Height, EditorWidthGrow1Id);

			var grow2 = GetDimensions(EditorWidthGrow2Id);
			ClassicAssert.Greater(grow2.Width, _results[EditorWidthGrow2Id].Width, EditorWidthGrow2Id);
			ClassicAssert.Greater(grow2.Height, _results[EditorWidthGrow2Id].Height, EditorWidthGrow2Id);

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