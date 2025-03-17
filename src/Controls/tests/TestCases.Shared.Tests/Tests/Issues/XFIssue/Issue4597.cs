using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

[Category(UITestCategories.Cells)]
public class Issue4597 : _IssuesUITest
{
	public Issue4597(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "[Android] ImageCell not loading images and setting ImageSource to null has no effect";

	//[Test]
	//[FailsOnIOS]
	//public void ImageFromFileSourceAppearsAndDisappearsCorrectly()
	//{
	//	RunTest(nameof(Image), true);
	//}

	//[Test]
	//[FailsOnIOS]
	//[FailsOnAndroid]
	//public void ImageFromUriSourceAppearsAndDisappearsCorrectly()
	//{
	//	RunTest(nameof(Image), false);
	//}


	//[Test]
	//[FailsOnIOS]
	//public void ButtonFromFileSourceAppearsAndDisappearsCorrectly()
	//{
	//	RunTest(nameof(Button), true);
	//}

	//[Test]
	//[FailsOnIOS]
	//[FailsOnAndroid]
	//public void ButtonFromUriSourceAppearsAndDisappearsCorrectly()
	//{
	//	RunTest(nameof(Button), false);
	//}


	//[Test]
	//[FailsOnIOS]
	//public void ImageButtonFromFileSourceAppearsAndDisappearsCorrectly()
	//{
	//	RunTest(nameof(ImageButton), true);
	//}

	//[Test]
	//[FailsOnIOS]
	//[FailsOnAndroid]
	//public void ImageButtonFromUriSourceAppearsAndDisappearsCorrectly()
	//{
	//	RunTest(nameof(ImageButton), false);
	//}

	//[Test]
	//[FailsOnIOS]
	//public void ImageCellFromFileSourceAppearsAndDisappearsCorrectly()
	//{
	//	ImageCellTest(true);
	//}

	//[Test]
	//[FailsOnIOS]
	//[FailsOnAndroid]
	//public void ImageCellFromUriSourceAppearsAndDisappearsCorrectly()
	//{
	//	ImageCellTest(false);
	//}

	//void ImageCellTest(bool fileSource)
	//{
	//	string className = "ImageView";
	//	SetupTest(nameof(ListView), fileSource);

	//	var imageVisible =
	//		App.QueryUntilPresent(GetImage, 10, 2000);

	//	Assert.AreEqual(1, imageVisible.Length);
	//	SetImageSourceToNull();

	//	imageVisible = GetImage();
	//	Assert.AreEqual(0, imageVisible.Length);

	//	Xamarin.UITest.Queries.AppResult[] GetImage()
	//	{
	//		return RunningApp
	//			.Query(app => app.Marked(_theListView).Descendant())
	//			.Where(x => x.Class != null && x.Class.Contains(className)).ToArray();
	//	}
	//}


	//void RunTest(string testName, bool fileSource)
	//{
	//	SetupTest(testName, fileSource);
	//	var foundImage = TestForImageVisible();
	//	SetImageSourceToNull();
	//	TestForImageNotVisible(foundImage);
	//}


	//void SetImageSourceToNull()
	//{
	//	App.Tap("ClickMe");
	//	App.WaitForElement(_appearText);
	//}

	//Xamarin.UITest.Queries.AppResult TestForImageVisible()
	//{
	//	var images = App.QueryUntilPresent(() =>
	//	{
	//		var result = App.WaitForElement(_fileNameAutomationId);

	//		if (result[0].Rect.Height > 1)
	//			return result;

	//		return Array.Empty<Xamarin.UITest.Queries.AppResult>();
	//	}, 10, 4000);

	//	Assert.AreEqual(1, images.Length);
	//	var imageVisible = images[0];

	//	Assert.Greater(imageVisible.Rect.Height, 1);
	//	Assert.Greater(imageVisible.Rect.Width, 1);
	//	return imageVisible;
	//}

	//void TestForImageNotVisible(Xamarin.UITest.Queries.AppResult previousFinding)
	//{
	//	var imageVisible = App.Query(_fileNameAutomationId);

	//	if (imageVisible.Length > 0)
	//	{
	//		Assert.Less(imageVisible[0].Rect.Height, previousFinding.Rect.Height);
	//	}
	//}

	//void SetupTest(string controlType, bool fileSource)
	//{
	//	App.WaitForElement(_nextTestId);
	//	string activeTest = null;
	//	while (App.Query(controlType).Length == 0)
	//	{
	//		activeTest = App.WaitForElement(_activeTestId)[0].ReadText();
	//		App.Tap(_nextTestId);
	//		App.WaitForNoElement(activeTest);
	//	}

	//	string sourceLabel = App.WaitForFirstElement("SourceLabel").ReadText();
	//	if (fileSource && sourceLabel != _imageFromFile)
	//		App.Tap(_switchUriId);
	//	else if (!fileSource && sourceLabel != _imageFromUri)
	//		App.Tap(_switchUriId);
	//}
}