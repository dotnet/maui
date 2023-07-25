UI Testing
===

# Introduction

Currently we are using [Appium](https://appium.io/docs/en/2.0/) to facilitate UI automation for Windows, Catalyst, iOS, and Android.
Appium relies on different implementations called `drivers` for each platform that have different behaviors/functions.
* Windows   - [WinAppDriver](https://github.com/appium/appium-windows-driver)
* Catalyst  - [mac2](https://github.com/appium/appium-mac2-driver)
* iOS       - [XCUITest](https://github.com/appium/appium-xcuitest-driver)
* Android   - [UIAutomator2](https://github.com/appium/appium-uiautomator2-driver)

## Creating a new test

### The sample project

The project that is used for UI Tests is located here: `src\Controls\samples\Controls.Sample.UITests\Controls.Sample.UITests.csproj`

There are two types of tests you can add, Issue and Gallery.

### Adding a new Issue

This will be the majority of new tests added which will be primarily for testing functionality and adding regression tests.

You will need to create some kind of UI to test against, which will go in the Controls.Sample.UITests project. Create a new class and attribute it with `[Issue]` and derive from `TestContentPage`.

Then in the Controls.AppiumTests project add a new class that derives from `_IssuesUITest` and add your test.

You can use the example for the sample project [here](https://github.com/dotnet/maui/blob/main/src/Controls/samples/Controls.Sample.UITests/Issues/RefreshViewPage.cs) and the example for the corresponding test [here](https://github.com/dotnet/maui/tree/main/src/Controls/tests/UITests/Tests/Issues/RefreshViewTests.cs).




### Adding a GalleryPage

Gallery tests are to make it easier to run the same set of tests on controls, if you are creating a new control you would want to add a new gallery page.

We have some base classes you can derive from to make setting this up easier: [CoreGalleryPage](https://github.com/dotnet/maui/blob/main/src/Controls/samples/Controls.Sample.UITests/CoreViews/CoreGalleryPage.cs) and [ContentViewGalleryPage](https://github.com/dotnet/maui/blob/main/src/Controls/samples/Controls.Sample.UITests/Elements/ContentViewGalleryPage.cs)

All controls you intend to interact with need to set the 'AutomationId' property as this will be what you use to query for the element.

Once you have created your GalleryPage, add it to [CorePageView](https://github.com/dotnet/maui/blob/5419846b1f20bdab1b5ce1dff40287edc5c38f12/src/Controls/samples/Controls.Sample.UITests/CoreViews/CorePageView.cs#L45C41-L45C41) so that it will show up on the main page at launch.

### Adding the test

The project that hosts the tests is located here: `src\Controls\tests\UITests\Controls.AppiumTests.csproj`

This project is using [NUnit](https://nunit.org/)

All tests should derive from `UITestBase` and should override `FixtureSetup/FixtureTeardown` to navigate to the specific UI you want to test and navigate back when finished.

```csharp
protected override void FixtureSetup()
{
    base.FixtureSetup();
    App.NavigateToGallery(DragAndDropGallery);
}
```

```csharp
protected override void FixtureTeardown()
{
    base.FixtureTeardown();
    App.NavigateBack();
}
```

The test will have access to gestures/interactions through the `App` property.

```csharp
App.WaitForElement("btnLogin");
App.EnterText("entryUsername", "user@email.com");
App.EnterText("entryPassword", "Password");

App.Tap("btnLogin");
var lblStatus = App.WaitForElement("lblStatus").FirstOrDefault();
var text = lblStatus?.Text;

Assert.IsNotNull(text);
Assert.IsTrue(text.StartsWith("Logging in", StringComparison.CurrentCulture));
```

## Running tests

Please see the [wiki](https://github.com/dotnet/maui/wiki/UITests) for setting up/running tests.


## Adding new functionality

We are implementing the IApp interface from Xamarin UITests, the implementation of which is [here](https://github.com/dotnet/maui/blob/main/src/TestUtils/src/TestUtils.Appium.UITests/AppiumUITestApp.cs).

## Known Issues
- iOS doesn't support nested accessibility elements which will make some elements unreachable