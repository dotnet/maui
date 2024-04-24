using System.Collections.Immutable;
using System.Diagnostics;
using System.Drawing;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Interfaces;
using UITest.Core;

namespace UITest.Appium
{
	public static class HelperExtensions
	{
		static TimeSpan DefaultTimeout = TimeSpan.FromSeconds(15);

		/// <summary>
		/// For desktop, this will perform a mouse click on the target element.
		/// For mobile, this will tap the element.
		/// This API works for all platforms whereas TapCoordinates currently doesn't work on Catalyst
		/// https://github.com/dotnet/maui/issues/19754
		/// </summary>
		/// <param name="app">Represents the main gateway to interact with an app.</param>
		/// <param name="element">Target Element</param>
		public static void Click(this IApp app, string element)
		{
			app.FindElement(element).Click();
		}

		public static string? GetText(this IUIElement element)
		{
			var response = element.Command.Execute("getText", new Dictionary<string, object>()
			{
				{ "element", element },
			});
			return (string?)response.Value;
		}

		public static T? GetAttribute<T>(this IUIElement element, string attributeName)
		{
			var response = element.Command.Execute("getAttribute", new Dictionary<string, object>()
			{
				{ "element", element },
				{ "attributeName", attributeName },
			});
			return (T?)response.Value;
		}

		public static Rectangle GetRect(this IUIElement element)
		{
			var response = element.Command.Execute("getRect", new Dictionary<string, object>()
			{
				{ "element", element },
			});

			if (response?.Value != null)
			{
				return (Rectangle)response.Value;
			}

			throw new InvalidOperationException($"Could not get Rect of element");
		}

		public static void EnterText(this IApp app, string element, string text)
		{
			var appElement = app.FindElement(element);
			appElement.SendKeys(text);
			app.DismissKeyboard();
		}

		public static void DismissKeyboard(this IApp app)
		{
			app.CommandExecutor.Execute("dismissKeyboard", ImmutableDictionary<string, object>.Empty);
		}

		public static bool IsKeyboardShown(this IApp app)
		{
			var response = app.CommandExecutor.Execute("isKeyboardShown", ImmutableDictionary<string, object>.Empty);
			var responseValue = response?.Value ?? false;
			return (bool)responseValue;
		}

		/// <summary>
		/// (Android Only) Sends a device key event with meta state.
		/// </summary>
		/// <param name="app"></param>
		/// <param name="keyCode"> Code for the key pressed on the Android device</param>
		/// <param name="metastate">metastate for the key press</param>
		/// <exception cref="InvalidOperationException"></exception>
		public static void SendKeys(this IApp app, int keyCode, int metastate = 0)
		{
			if (app is not AppiumApp aaa)
			{
				throw new InvalidOperationException($"SendKeys is only supported on AppiumApp");
			}

			if (aaa.Driver is ISendsKeyEvents ske)
			{
				ske.PressKeyCode(keyCode, metastate);
				return;
			}

			throw new InvalidOperationException($"SendKeys is not supported on {aaa.Driver}");
		}

		public static void ClearText(this IApp app, string element)
		{
			app.FindElement(element).Clear();
		}

		/// <summary>
		/// For desktop, this will perform a mouse click on the target element.
		/// For mobile, this will tap the element.
		/// This API works for all platforms whereas TapCoordinates currently doesn't work on Catalyst
		/// https://github.com/dotnet/maui/issues/19754
		/// </summary>
		/// <param name="element">Target Element</param>
		public static void Click(this IUIElement element)
		{
			element.Command.Execute("click", new Dictionary<string, object>()
			{
				{ "element", element }
			});
		}

		public static void SendKeys(this IUIElement element, string text)
		{
			element.Command.Execute("sendKeys", new Dictionary<string, object>()
			{
				{ "element", element },
				{ "text", text }
			});
		}

		public static void Clear(this IUIElement element)
		{
			element.Command.Execute("clear", new Dictionary<string, object>()
			{
				{ "element", element },
			});
		}

		public static void DoubleClick(this IApp app, string element)
		{
			var elementToClick = app.FindElement(element);
			app.CommandExecutor.Execute("doubleClick", new Dictionary<string, object>
			{
				{ "element", elementToClick },
			});
		}

		public static void DragAndDrop(this IApp app, string dragSource, string dragTarget)
		{
			var dragSourceElement = app.FindElement(dragSource);
			var targetSourceElement = app.FindElement(dragTarget);

			app.CommandExecutor.Execute("dragAndDrop", new Dictionary<string, object>
			{
				{ "sourceElement", dragSourceElement },
				{ "destinationElement", targetSourceElement }
			});
		}

		public static void ScrollTo(this IApp app, string toElementId, bool down = true)
		{
			app.CommandExecutor.Execute("scrollTo", new Dictionary<string, object>
			{
				{ "elementId", toElementId},
				{ "down", down }
			});
		}

		public static IUIElement WaitForElement(this IApp app, string marked, string timeoutMessage = "Timed out waiting for element...", TimeSpan? timeout = null, TimeSpan? retryFrequency = null, TimeSpan? postTimeout = null)
		{
			IUIElement result() => app.FindElement(marked);
			var results = WaitForAtLeastOne(result, timeoutMessage, timeout, retryFrequency);

			return results;
		}

		public static void WaitForNoElement(this IApp app, string marked, string timeoutMessage = "Timed out waiting for no element...", TimeSpan? timeout = null, TimeSpan? retryFrequency = null, TimeSpan? postTimeout = null)
		{
			IUIElement result() => app.FindElement(marked);
			WaitForNone(result, timeoutMessage, timeout, retryFrequency);
		}

		public static bool WaitForTextToBePresentInElement(this IApp app, string automationId, string text)
		{
			TimeSpan timeout = DefaultTimeout;
			TimeSpan retryFrequency = TimeSpan.FromMilliseconds(500);
			string timeoutMessage = $"Timed out on {nameof(WaitForTextToBePresentInElement)}.";

			DateTime start = DateTime.Now;

			while (true)
			{
				var element = app.FindElements(automationId).FirstOrDefault();
				if (element != null && (element.GetText()?.Contains(text, StringComparison.OrdinalIgnoreCase) ?? false))
				{
					return true;
				}

				long elapsed = DateTime.Now.Subtract(start).Ticks;
				if (elapsed >= timeout.Ticks)
				{
					Debug.WriteLine($">>>>> {elapsed} ticks elapsed, timeout value is {timeout.Ticks}");

					return false;
				}

				Task.Delay(retryFrequency.Milliseconds).Wait();
			}
		}

		/// <summary>
		/// Presses the volume up button on the device.
		/// </summary>
		/// <param name="app">Represents the main gateway to interact with an app.</param>
		public static void PressVolumeUp(this IApp app)
		{
			app.CommandExecutor.Execute("pressVolumeUp", ImmutableDictionary<string, object>.Empty);
		}

		/// <summary>
		/// Presses the volume down button on the device.
		/// </summary>
		/// <param name="app">Represents the main gateway to interact with an app.</param>
		public static void PressVolumeDown(this IApp app)
		{
			app.CommandExecutor.Execute("pressVolumeDown", ImmutableDictionary<string, object>.Empty);
		}

		/// <summary>
		/// Performs a left to right swipe gesture on the screen. 
		/// </summary>
		/// <param name="app">Represents the main gateway to interact with an app.</param>
		/// <param name="swipePercentage">How far across the element to swipe (from 0.0 to 1.0).</param>
		/// <param name="swipeSpeed">The speed of the gesture.</param>
		/// <param name="withInertia">Whether swipes should cause inertia.</param>
		public static void SwipeLeftToRight(this IApp app, double swipePercentage = 0.67, int swipeSpeed = 500, bool withInertia = true)
		{
			app.CommandExecutor.Execute("swipeLeftToRight", new Dictionary<string, object>
			{
				{ "swipePercentage", swipePercentage },
				{ "swipeSpeed", swipeSpeed },
				{ "withInertia", withInertia }
			});
		}

		/// <summary>
		/// Performs a left to right swipe gesture on the matching element. 
		/// If multiple elements are matched, the first one will be used.
		/// </summary>
		/// <param name="app">Represents the main gateway to interact with an app.</param>
		/// <param name="marked">Marked selector to match.</param>
		/// <param name="swipePercentage">How far across the element to swipe (from 0.0 to 1.0).</param>
		/// <param name="swipeSpeed">The speed of the gesture.</param>
		/// <param name="withInertia">Whether swipes should cause inertia.</param>
		public static void SwipeLeftToRight(this IApp app, string marked, double swipePercentage = 0.67, int swipeSpeed = 500, bool withInertia = true)
		{
			var elementToSwipe = app.FindElement(marked);

			app.CommandExecutor.Execute("swipeLeftToRight", new Dictionary<string, object>
			{
				{ "element", elementToSwipe},
				{ "swipePercentage", swipePercentage },
				{ "swipeSpeed", swipeSpeed },
				{ "withInertia", withInertia }
			});
		}

		/// <summary>
		///  Performs a right to left swipe gesture on the screen. 
		/// </summary>
		/// <param name="app">Represents the main gateway to interact with an app.</param>
		/// <param name="swipePercentage">How far across the element to swipe (from 0.0 to 1.0).</param>
		/// <param name="swipeSpeed">The speed of the gesture.</param>
		/// <param name="withInertia">Whether swipes should cause inertia.</param>
		public static void SwipeRightToLeft(this IApp app, double swipePercentage = 0.67, int swipeSpeed = 500, bool withInertia = true)
		{
			app.CommandExecutor.Execute("swipeRightToLeft", new Dictionary<string, object>
			{
				{ "swipePercentage", swipePercentage },
				{ "swipeSpeed", swipeSpeed },
				{ "withInertia", withInertia }
			});
		}

		/// <summary>
		/// Performs a right to left swipe gesture on the matching element. 
		/// If multiple elements are matched, the first one will be used.
		/// </summary>
		/// <param name="app">Represents the main gateway to interact with an app.</param>
		/// <param name="marked">Marked selector to match.</param>
		/// <param name="swipePercentage">How far across the element to swipe (from 0.0 to 1.0).</param>
		/// <param name="swipeSpeed">The speed of the gesture.</param>
		/// <param name="withInertia">Whether swipes should cause inertia.</param>
		public static void SwipeRightToLeft(this IApp app, string marked, double swipePercentage = 0.67, int swipeSpeed = 500, bool withInertia = true)
		{
			var elementToSwipe = app.FindElement(marked);

			app.CommandExecutor.Execute("swipeRightToLeft", new Dictionary<string, object>
			{
				{ "element", elementToSwipe},
				{ "swipePercentage", swipePercentage },
				{ "swipeSpeed", swipeSpeed },
				{ "withInertia", withInertia }
			});
		}

		/// <summary>
		/// Scrolls left on the first element matching query.
		/// </summary>
		/// <param name="app">Represents the main gateway to interact with an app.</param>
		/// <param name="marked">Marked selector to match.</param>
		/// <param name="strategy">Strategy for scrolling element.</param>
		/// <param name="swipePercentage">How far across the element to swipe (from 0.0 to 1.0).</param>
		/// <param name="swipeSpeed">The speed of the gesture.</param>
		/// <param name="withInertia">Whether swipes should cause inertia.</param>
		public static void ScrollLeft(this IApp app, string marked, ScrollStrategy strategy = ScrollStrategy.Auto, double swipePercentage = 0.67, int swipeSpeed = 500, bool withInertia = true)
		{
			var elementToSwipe = app.FindElement(marked);

			app.CommandExecutor.Execute("scrollLeft", new Dictionary<string, object>
			{
				{ "element", elementToSwipe},
				{ "strategy", strategy },
				{ "swipePercentage", swipePercentage },
				{ "swipeSpeed", swipeSpeed },
				{ "withInertia", withInertia }
			});
		}

		/// <summary>
		/// Scrolls down on the first element matching query.
		/// </summary>
		/// <param name="app">Represents the main gateway to interact with an app.</param>
		/// <param name="marked">Marked selector to match.</param>
		/// <param name="strategy">Strategy for scrolling element.</param>
		/// <param name="swipePercentage">How far across the element to swipe (from 0.0 to 1.0).</param>
		/// <param name="swipeSpeed">The speed of the gesture.</param>
		/// <param name="withInertia">Whether swipes should cause inertia.</param>
		public static void ScrollDown(this IApp app, string marked, ScrollStrategy strategy = ScrollStrategy.Auto, double swipePercentage = 0.67, int swipeSpeed = 500, bool withInertia = true)
		{
			var elementToSwipe = app.FindElement(marked);

			app.CommandExecutor.Execute("scrollDown", new Dictionary<string, object>
			{
				{ "element", elementToSwipe},
				{ "strategy", strategy },
				{ "swipePercentage", swipePercentage },
				{ "swipeSpeed", swipeSpeed },
				{ "withInertia", withInertia }
			});
		}

		/// <summary>
		/// Scrolls right on the first element matching query.
		/// </summary>
		/// <param name="app">Represents the main gateway to interact with an app.</param>
		/// <param name="marked">Marked selector to match.</param>
		/// <param name="strategy">Strategy for scrolling element.</param>
		/// <param name="swipePercentage">How far across the element to swipe (from 0.0 to 1.0).</param>
		/// <param name="swipeSpeed">The speed of the gesture.</param>
		/// <param name="withInertia">Whether swipes should cause inertia.</param>
		public static void ScrollRight(this IApp app, string marked, ScrollStrategy strategy = ScrollStrategy.Auto, double swipePercentage = 0.67, int swipeSpeed = 500, bool withInertia = true)
		{
			var elementToSwipe = app.FindElement(marked);

			app.CommandExecutor.Execute("scrollRight", new Dictionary<string, object>
			{
				{ "element", elementToSwipe},
				{ "strategy", strategy },
				{ "swipePercentage", swipePercentage },
				{ "swipeSpeed", swipeSpeed },
				{ "withInertia", withInertia }
			});
		}

		/// <summary>
		/// Scrolls up on the first element matching query.
		/// </summary>
		/// <param name="app">Represents the main gateway to interact with an app.</param>
		/// <param name="marked">Marked selector to match.</param>
		/// <param name="strategy">Strategy for scrolling element.</param>
		/// <param name="swipePercentage">How far across the element to swipe (from 0.0 to 1.0).</param>
		/// <param name="swipeSpeed">The speed of the gesture.</param>
		/// <param name="withInertia">Whether swipes should cause inertia.</param>
		public static void ScrollUp(this IApp app, string marked, ScrollStrategy strategy = ScrollStrategy.Auto, double swipePercentage = 0.67, int swipeSpeed = 500, bool withInertia = true)
		{
			var elementToSwipe = app.FindElement(marked);

			app.CommandExecutor.Execute("scrollUp", new Dictionary<string, object>
			{
				{ "element", elementToSwipe},
				{ "strategy", strategy },
				{ "swipePercentage", swipePercentage },
				{ "swipeSpeed", swipeSpeed },
				{ "withInertia", withInertia }
			});
		}

		/// <summary>
		/// Changes the device orientation to landscape mode.
		/// </summary>
		/// <param name="app">Represents the main gateway to interact with an app.</param>
		public static void SetOrientationLandscape(this IApp app)
		{
			app.CommandExecutor.Execute("setOrientationLandscape", ImmutableDictionary<string, object>.Empty);
		}

		/// <summary>
		/// Changes the device orientation to portrait mode.
		/// </summary>
		/// <param name="app">Represents the main gateway to interact with an app.</param>
		public static void SetOrientationPortrait(this IApp app)
		{
			app.CommandExecutor.Execute("setOrientationPortrait", ImmutableDictionary<string, object>.Empty);
		}

		/// <summary>
		/// Performs a tap / touch gesture on the given coordinates.
		/// This API currently doesn't work on Catalyst https://github.com/dotnet/maui/issues/19754
		/// For Catalyst you'll currently need to use Click instead. 
		/// Tap is more mobile-specific and provides more flexibility than click. Click is more general and is 
		/// used for simpler interactions. Depending on the context of your test, you might prefer one over the other.
		/// </summary>
		/// <param name="app">Represents the main gateway to interact with an app.</param>
		/// <param name="x">The x coordinate to tap.</param>
		/// <param name="y">The y coordinate to tap.</param>
		public static void TapCoordinates(this IApp app, float x, float y)
		{
			app.CommandExecutor.Execute("tapCoordinates", new Dictionary<string, object>
			{
				{ "x", x },
				{ "y", y }
			});
		}

		/// <summary>
		/// Executes an existing application on the device. 
		/// If the application is already running then it will be brought to the foreground.
		/// </summary>
		/// <param name="app">Represents the main gateway to interact with an app.</param>
		public static void LaunchApp(this IApp app)
		{
			app.CommandExecutor.Execute("launchApp", ImmutableDictionary<string, object>.Empty);
		}

		/// <summary>
		/// Send the currently running app for this session to the background.
		/// </summary>
		/// <param name="app">Represents the main gateway to interact with an app.</param>
		public static void BackgroundApp(this IApp app)
		{
			app.CommandExecutor.Execute("backgroundApp", ImmutableDictionary<string, object>.Empty);
		}

		/// <summary>
		/// If the application is already running then it will be brought to the foreground.
		/// </summary>
		/// <param name="app">Represents the main gateway to interact with an app.</param>
		public static void ForegroundApp(this IApp app)
		{
			app.CommandExecutor.Execute("foregroundApp", ImmutableDictionary<string, object>.Empty);
		}

		/// <summary>
		/// Reset the currently running app for this session.
		/// </summary>
		/// <param name="app">Represents the main gateway to interact with an app.</param>
		public static void ResetApp(this IApp app)
		{
			app.CommandExecutor.Execute("resetApp", ImmutableDictionary<string, object>.Empty);
		}

		/// <summary>
		/// Close an app on device.
		/// </summary>
		/// <param name="app">Represents the main gateway to interact with an app.</param>
		public static void CloseApp(this IApp app)
		{
			app.CommandExecutor.Execute("closeApp", ImmutableDictionary<string, object>.Empty);
		}

		/// <summary>
		/// Sets the value of a Slider element that matches marked.
		/// </summary>
		/// <param name="app">Represents the main gateway to interact with an app.</param>
		/// <param name="marked">Marked selector of the Slider element to update.</param>
		/// <param name="value">The value to set the Slider to.</param>
		public static void SetSliderValue(this IApp app, string marked, double value)
		{
			var element = app.FindElement(marked);

			double defaultMinimum = 0d;
			double defaultMaximum = 1d;

			app.CommandExecutor.Execute("setSliderValue", new Dictionary<string, object>
			{
				{ "element", element },
				{ "value", value },
				{ "minimum", defaultMinimum },
				{ "maximum", defaultMaximum },
			});
		}

		/// <summary>
		/// Sets the value of a Slider element that matches marked.
		/// </summary>
		/// <param name="app">Represents the main gateway to interact with an app.</param>
		/// <param name="marked">Marked selector of the Slider element to update.</param>
		/// <param name="value">The value to set the Slider to.</param>
		/// <param name="minimum">Te minimum selectable value for the Slider.</param>
		/// <param name="maximum">Te maximum selectable value for the Slider.</param>
		public static void SetSliderValue(this IApp app, string marked, double value, double minimum = 0d, double maximum = 1d)
		{
			var element = app.FindElement(marked);

			app.CommandExecutor.Execute("setSliderValue", new Dictionary<string, object>
			{
				{ "element", element },
				{ "value", value },
				{ "minimum", minimum },
				{ "maximum", maximum },
			});
		}

		/// <summary>
		/// Performs a continuous drag gesture between 2 points.
		/// </summary>
		/// <param name="app">Represents the main gateway to interact with an app.</param>
		/// <param name="fromX">The x coordinate to start dragging from.</param>
		/// <param name="fromY">The y coordinate to start dragging from.</param>
		/// <param name="toX">The x coordinate to drag to.</param>
		/// <param name="toY">The y coordinate to drag to.</param>
		public static void DragCoordinates(this IApp app, float fromX, float fromY, float toX, float toY)
		{
			app.CommandExecutor.Execute("dragCoordinates", new Dictionary<string, object>
			{
				{ "fromX", fromX },
				{ "fromY", fromY },
				{ "toX", toX },
				{ "toY", toY },
			});
		}

		/// <summary>
		/// Navigate back on the device.
		/// </summary>
		/// <param name="app">Represents the main gateway to interact with an app.</param>
		public static void Back(this IApp app)
		{
			app.CommandExecutor.Execute("back", ImmutableDictionary<string, object>.Empty);
		}

		/// <summary>
		/// Return the AppId of the running app. This is used inside any appium command that want the app id
		/// </summary>
		/// <param name="app">Represents the main gateway to interact with an app.</param>
		public static string GetAppId(this IApp app)
		{
			if (app is not AppiumApp aaa)
			{
				throw new InvalidOperationException($"GetAppId is only supported on AppiumApp");
			}

			var appId = aaa.Config.GetProperty<string>("AppId");
			if (appId is not null)
			{
				return appId;
			}

			throw new InvalidOperationException("AppId not found");
		}

		/// <summary>
		/// Retrieve the target device this test is running against
		/// </summary>
		/// <param name="app">Represents the main gateway to interact with an app.</param>
		/// <returns></returns>
		/// <exception cref="InvalidOperationException"></exception>
		public static TestDevice GetTestDevice(this IApp app)
		{
			if (app is not AppiumApp aaa)
			{
				throw new InvalidOperationException($"GetTestDevice is only supported on AppiumApp");
			}

			return aaa.Config.GetProperty<TestDevice>("TestDevice");
		}

		/// <summary>
		/// Check if element has focused
		/// </summary>
		/// <param name="app">Represents the main gateway to interact with an app.</param>
		/// <param name="id">Target element</param>
		/// <returns>Returns <see langword="true"/> if focused</returns>
		/// <exception cref="InvalidOperationException"></exception>
		public static bool IsFocused(this IApp app, string id)
		{
			if (app is not AppiumApp aaa)
			{
				throw new InvalidOperationException($"IsFocused is only supported on AppiumApp");
			}

			var activeElement = aaa.Driver.SwitchTo().ActiveElement();
			var element = (AppiumDriverElement)app.WaitForElement(id);

			if (app.GetTestDevice() == TestDevice.Mac && activeElement is AppiumElement activeAppiumElement)
			{
				// For some reason on catalyst the ActiveElement returns an AppiumElement with a different id
				// The TagName (AutomationId) and the location all match, so, other than the Id it walks and talks
				// like the same element
				return element.AppiumElement.TagName.Equals(activeAppiumElement.TagName, StringComparison.OrdinalIgnoreCase) &&
					element.AppiumElement.Location.Equals(activeAppiumElement.Location);
			}

			return element.AppiumElement.Equals(activeElement);
		}

		static IUIElement Wait(Func<IUIElement> query,
			Func<IUIElement, bool> satisfactory,
			string? timeoutMessage = null,
			TimeSpan? timeout = null, TimeSpan? retryFrequency = null)
		{
			timeout ??= DefaultTimeout;
			retryFrequency ??= TimeSpan.FromMilliseconds(500);
			timeoutMessage ??= "Timed out on query.";

			DateTime start = DateTime.Now;

			IUIElement result = query();

			while (!satisfactory(result))
			{
				long elapsed = DateTime.Now.Subtract(start).Ticks;
				if (elapsed >= timeout.Value.Ticks)
				{
					Debug.WriteLine($">>>>> {elapsed} ticks elapsed, timeout value is {timeout.Value.Ticks}");

					throw new TimeoutException(timeoutMessage);
				}

				Task.Delay(retryFrequency.Value.Milliseconds).Wait();
				result = query();
			}

			return result;
		}

		static IUIElement WaitForAtLeastOne(Func<IUIElement> query,
			string? timeoutMessage = null,
			TimeSpan? timeout = null,
			TimeSpan? retryFrequency = null)
		{
			var results = Wait(query, i => i != null, timeoutMessage, timeout, retryFrequency);

			return results;
		}

		static void WaitForNone(Func<IUIElement> query,
			string? timeoutMessage = null,
			TimeSpan? timeout = null, TimeSpan? retryFrequency = null)
		{
			Wait(query, i => i == null, timeoutMessage, timeout, retryFrequency);
		}
	}
}
