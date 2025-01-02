using Microsoft.Maui.Platform;
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue5766 : _IssuesUITest
{
	const string StartText1 = "start1";
	const string BigText1 = "big string > big frame1";
	const string SmallText1 = "s1";
	const string EndText1 = "end1";
	const string List1 = "lst1";

	const string StartText2 = "start2";
	const string BigText2 = "big string > big frame2";
	const string SmallText2 = "s2";
	const string EndText2 = "end2";
	const string List2 = "lst2";

	public Issue5766(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Frame size gets corrupted when ListView is scrolled";

	System.Drawing.Rectangle[] GetLabels(IApp App, string label)
	{
		return App.FindElements(AppiumQuery.ByClass("FormsTextView")).Where(x => x.GetText() == label).Select(x => x.GetRect()).ToArray();
	}

	bool RectIsEquals(System.Drawing.Rectangle[] left, System.Drawing.Rectangle[] right)
	{
		if (left.Length != right.Length)
			return false;

		for (int i = 0; i < left.Length; i++)
		{
			if (left[i].X == right[i].X ||
				left[i].Y != right[i].Y ||
				left[i].Width != right[i].Width ||
				left[i].Height != right[i].Height)
				return false;
		}

		return true;
	}

	[Test]
	[Category(UITestCategories.Layout)]
	public void FrameSizeGetsCorruptedWhenListViewIsScrolled()
	{
		App.WaitForElement(StartText1);
		var start = GetLabels(App, StartText1);
		var smalls = GetLabels(App, SmallText1);
		var bigs = GetLabels(App, BigText1);

		App.ScrollDown(List1);
		App.ScrollUp(List1);

		var startAfter = GetLabels(App, StartText1);

		Assert.That(RectIsEquals(start, startAfter), Is.True);
		var smallAfter = GetLabels(App, SmallText1);
		Assert.That(RectIsEquals(smalls, smallAfter),Is.True);
		var bigAfter = GetLabels(App, BigText1);
		Assert.That(RectIsEquals(bigs, bigAfter), Is.True);

		// list2 with ListViewCachingStrategy.RecycleElement - issue 6297
		App.WaitForElement(StartText2);
		start = GetLabels(App, StartText2);
		smalls = GetLabels(App, SmallText2);
		bigs = GetLabels(App, BigText2);

		App.ScrollDown(List2);
		App.ScrollUp(List2);

		startAfter = GetLabels(RunningApp, StartText2);
		Assert.That(RectIsEquals(start, startAfter), Is.True);
		smallAfter = GetLabels(RunningApp, SmallText2);
		Assert.That(RectIsEquals(smalls, smallAfter), Is.True);
		bigAfter = GetLabels(RunningApp, BigText2);
		Assert.That(RectIsEquals(bigs, bigAfter), Is.True);
	}
}