## Empowering UITests with AI

### Example: Click With AI

```
using UITest.Appium.AI;

// Example of how to use ClickWithAI method
[Test]
public async Task PerformClick()
{
    string prompt = "Find the button labeled 'Submit'.";
    await App.ClickWithAI(prompt);
}
```

### Example: Click With AI

```
using UITest.Appium.AI;

// Example of how to use DoubleClickWithAI method
[Test]
public async Task PerformDoubleClick()
{
    string prompt = "Locate the 'Refresh' icon.";
    await App.DoubleClickWithAI(prompt);
}
```

### Example: Tap With AI (for Mobile)

```
using UITest.Appium.AI;

// Example of how to use TapWithAI method
[Test]
public async Task PerformTap()
{
    string prompt = "Identify the 'Next' button on the screen.";
    await App.TapWithAI(prompt);
}
```

### Example: Double Tap With AI

```
using UITest.Appium.AI;

// Example of how to use DoubleTapWithAI method
[Test]
public async Task PerformDoubleTap()
{
    string prompt = "Locate the image thumbnail.";
    await App.DoubleTapWithAI(prompt);
}
```

### Example: Right Click With AI

```
using UITest.Appium.AI;

// Example of how to use RightClickWithAI method
[Test]
public async Task PerformRightClick()
{
    string prompt = "Find the context menu icon.";
    await App.RightClickWithAI(prompt);
}
```

### Example: Press Down With AI

```
using UITest.Appium.AI;

// Example of how to use PressDownWithAI method
[Test]
public async Task PerformPressDown()
{
    string prompt = "Identify the 'Start' button.";
    await App.PressDownWithAI(prompt);
}
```

### Example: Enter Text With AI

```
using UITest.Appium.AI;

// Example of how to use EnterTextWithAI method
[Test]
public async Task PerformEnterText()
{
    string prompt = "Locate the username input field.";
    string text = "test_user";
    await App.EnterTextWithAI(prompt, text);
}
```

### Example: Clear Text With AI

```
using UITest.Appium.AI;

// Example of how to use ClearTextWithAI method
[Test]
public async Task PerformClearText()
{
    string prompt = "Find the input field labeled 'Username'.";
    await App.ClearTextWithAI(prompt);
}
```

### Example: Long Press With AI

```
using UITest.Appium.AI;

// Example of how to use LongPressWithAI method
[Test]
public async Task PerformLongPress()
{
    string prompt = "Locate the 'Login' button.";
    await App.LongPressWithAI(prompt);
}
```

### Example: Touch and Hold With AI

```
using UITest.Appium.AI;

// Example of how to use TouchAndHoldWithAI method
[Test]
public async Task PerformTouchAndHold()
{
    string prompt = "Identify the image thumbnail.";
    await App.TouchAndHoldWithAI(prompt);
}
```

### Example: VerifyScreenshot With AI

```
using UITest.Appium.AI;

// Example of how to use VerifyScreenshotWithAI method
[Test]
public async Task VerifyScreenshotWithAITest()
{
	// Use AI to compare the App screenshot and determinate if the image is equal to a reference one.
	bool areEquals = await VerifyScreenshotWithAI();
	Assert.That(areEquals, Is.True);
}
```