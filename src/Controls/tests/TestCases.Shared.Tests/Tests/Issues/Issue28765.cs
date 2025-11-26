#if TEST_FAILS_ON_ANDROID
// This test started failing on the safeareaedges changes because currently the safeareaedges changes
// cause a second measure pass which exposes a bug that already existed in CollectionView on Android
// You can replicate this bug on NET10 but rotating the device and rotating back and then you will see that the
// footer will disappear because on the second measure pass the layout of the content is too big. 
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue28765 : _IssuesUITest
{
	public override string Issue => "[Android] Inconsistent footer scrolling in CollectionView when EmptyView as string";
	public Issue28765(TestDevice device) : base(device)
	{
	}

	[Test, Order(1)]
	[Category(UITestCategories.CollectionView)]
	public void EmptyViewStringWithHeaderAndFooterAsView()
	{
		App.WaitForElement("Footer View");
	}

	[Test, Order(2)]
	[Category(UITestCategories.CollectionView)]
	public void EmptyViewStringWithHeaderAndFooterString()
	{
		App.WaitForElement("Footer String");
	}
}
#endif