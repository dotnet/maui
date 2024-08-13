UI Testing
===

# Note for New Contributors

We advise starting out with UI Testing instead of DeviceTests, as they are a bit easier to start writing, and more for routine, simplistic unit testing. Please see here for more information about DeviceTests: [DeviceTests](https://devdiv.visualstudio.com/DevDiv/_wiki/wikis/DevDiv.wiki/32195/MAUI#:~:text=%2D%20Device%20tests%20%2D%20These%20are%20tests%20that%20will%20run%20on%20an%20actual%20device%20or%20simulator)

# Introduction

Currently we are using [Appium](https://appium.io/docs/en/2.0/) to facilitate UI automation for Windows, Catalyst, iOS, and Android.
Appium relies on different implementations called `drivers` for each platform that have different behaviors/functions.
* Windows   - [WinAppDriver](https://github.com/appium/appium-windows-driver)
* Catalyst  - [mac2](https://github.com/appium/appium-mac2-driver)
* iOS       - [XCUITest](https://github.com/appium/appium-xcuitest-driver)
* Android   - [UIAutomator2](https://github.com/appium/appium-uiautomator2-driver)


## Creating a new test

### Adding a new Issue

This will be the majority of new tests added which will be primarily for testing functionality and adding regression tests.

You will need to create some kind of UI to test against, which will go in the Controls.TestCases.HostApp project. Create a new class within `src/Controls/tests/TestCases.HostApp/Issues` and attribute it with `[Issue]` and derive from `TestContentPage` (or `TestNavigationPage`, or `TestShellPage`, and so on).

Then in the Controls.TestCases.Shared.Tests project add a new class within this folder: `src/Controls/tests/TestCases.Shared.Tests/Tests/Issues`. Have the class derive from `_IssuesUITest` and add your test.

You can use the example for the sample project [here](https://github.com/dotnet/maui/blob/main/src/Controls/tests/TestCases.HostApp/Issues/RefreshViewPage.cs) and the example for the corresponding test [here](https://github.com/dotnet/maui/tree/main/src/Controls/tests/TestCases.Shared.Tests/Tests/Issues/RefreshViewTests.cs).

### Interacting with Elements in Tests

 All controls you intend to interact with need to set the `AutomationId` property as this will be what you use to query for the element. You can either set it in the xaml as an attribute when you create the element or you can assign it when you create the element in the `*.cs` file.

Note: AutomationId will not work on layouts on Windows. This is because Appium uses the accessibility tree to locate elements, and layouts are not visible in the accessibility tree. You will have to focus on the individual elements such as label, entry, editor, and so on.

### Example

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

### Screenshots

Testing against a previously saved screenshot of the simulator can be an important asset when it comes to writing tests. Currently, this is how you can do so when using the CI:
1. Call VerifyScreenshot() at the end of your test method.
2) Start a pull request, and have it run on the CI.
3) Navigate to the bottom of the request where there is a list of the various checks on the CI. Scroll down until you see `Maui-UITestpublic` (will have a required bubble next to it) and click details. At the top of the summary page, you should see a box with Repositories, Time Started and Elapsed, Related, and Tests and Coverage. Click on the link below the related heading. Click on the drop to download it.
4) When you unzip the archive, navigate to the Controls.TestCases.Shared folder which will have the snap shot. NOTE: in future testing, if this test will have failed, the snapshot will have a -diff attached to its filename, with red outlining to indicate problem areas.
5) Add the snapshot .png to your test
6) Commit the change to your PR.

## Adding a GalleryPage

Gallery tests are to make it easier to run the same set of tests on controls, if you are creating a new control you would want to add a new gallery page.

We have some base classes you can derive from to make setting this up easier: [CoreGalleryPage](https://github.com/dotnet/maui/blob/main/src/Controls/tests/TestCases.HostApp/CoreViews/CoreGalleryPage.cs) and [ContentViewGalleryPage](https://github.com/dotnet/maui/blob/main/src/Controls/tests/TestCases.HostApp/Elements/ContentViewGalleryPage.cs)

### Restarting the App after a Test

- When multiple tests are run, all methods under one class will be tested in the same instance of the app. The app will then restart as it changes to the next test class. If you would like the app to be be restarted after method in the class, add this override property to your class:
```csharp
protected override bool ResetAfterEachTest => true;
```

### Handling different operating systems

There may be times when you want to have the test run on some platforms and not others. For example, VerifyScreenshot() does not currently work on MacCatalyst. In this case, you would want to use preprocessor directives:

```csharp
#if ! MACCATALYST
//your code here
#endif
```

When you compile `Controls.TestCases.Mac.Tests`, the test will not appear in the list of tests. 

Note: you may see something like `[FailsOnAndroid("This test is failing, likely due to product issue")]` in some tests. This was an attribute created to help with compatibility with Xamarin Forms and is not advised to used with tests written in the future.

## Building and Running tests
Please see the [wiki](https://github.com/dotnet/maui/wiki/UITests) for setting up/running tests.

## Logging

Follow the steps above for accessing Screenshots to access the logs from the drop folder.

IOS - `logarchive` files from the console output of the simulator (currently there might be logarchives from other simulators so be sure to validate that there are logs from your test run in the log archive).
 
Android - If a test fails or the device crashes, there will be a `logcat` file in this folder that you can look at for information.

## Known Issues
- iOS doesn't support nested accessibility elements which will make some elements unreachable
