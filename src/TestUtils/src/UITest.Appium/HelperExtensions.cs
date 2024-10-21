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
		/// <param name="element">Target Element.</param>
		public static void Tap(this IApp app, string element)
		{
			app.FindElement(element).Click();
		}

		/// <summary>
		/// Performs a mouse click on the matched element.
		/// </summary>
		/// <param name="app">Represents the main gateway to interact with an app.</param>
		/// <param name="element">Target Element.</param>
		public static void Click(this IApp app, string element)
		{
			app.FindElement(element).Click();
		}

		public static void RightClick(this IApp app, string element)
		{
			var uiElement = app.FindElement(element);
			uiElement.Command.Execute("click", new Dictionary<string, object>()
			{
				{ "element", uiElement },
				{ "button", "right" }
			});
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

		/// <summary>
		/// Enters text into the currently focused element.
		/// </summary>
		/// <param name="app">Represents the main gateway to interact with an app.</param>
		/// <param name="element">Target Element.</param>
		/// <param name="text">The text to enter.</param>
		public static void EnterText(this IApp app, string element, string text)
		{
			var appElement = app.FindElement(element);
			appElement.SendKeys(text);
			app.DismissKeyboard();
		}

		/// <summary>
		/// Hides soft keyboard if present.
		/// </summary>
		/// <param name="app">Represents the main gateway to interact with an app.</param>
		public static void DismissKeyboard(this IApp app)
		{
			app.CommandExecutor.Execute("dismissKeyboard", ImmutableDictionary<string, object>.Empty);
		}

		/// <summary>
		/// Whether or not the soft keyboard is shown.
		/// </summary>
		/// <param name="app">Represents the main gateway to interact with an app.</param>
		/// <returns>true if the soft keyboard is shown; otherwise, false.</returns>
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

		/// <summary>
		/// Clears text from the currently focused element.
		/// </summary>
		/// <param name="app">Represents the main gateway to interact with an app.</param>
		/// <param name="element">Target Element.</param>
		public static void ClearText(this IApp app, string element)
		{
			app.FindElement(element).Clear();
		}

		/// <summary>
		/// Performs a mouse click on the matched element.
		/// </summary>
		/// <param name="element">Target Element.</param>
		public static void Click(this IUIElement element)
		{
			element.Command.Execute("click", new Dictionary<string, object>()
			{
				{ "element", element }
			});
		}

		/// <summary>
		/// For desktop, this will perform a mouse click on the target element.
		/// For mobile, this will tap the element.
		/// This API works for all platforms whereas TapCoordinates currently doesn't work on Catalyst
		/// https://github.com/dotnet/maui/issues/19754
		/// </summary>
		/// <param name="element">Target Element.</param>
		public static void Tap(this IUIElement element)
		{
			element.Command.Execute("tap", new Dictionary<string, object>()
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

		/// <summary>
		/// Performs a mouse double click on the matched element.
		/// </summary>
		/// <param name="app">Represents the main gateway to interact with an app.</param>
		/// <param name="element">Target Element.</param>
		public static void DoubleClick(this IApp app, string element)
		{
			var elementToDoubleClick = app.FindElement(element);
			app.CommandExecutor.Execute("doubleClick", new Dictionary<string, object>
			{
				{ "element", elementToDoubleClick },
			});
		}

		/// <summary>
		/// Performs a mouse double click on the given coordinates.
		/// </summary>
		/// <param name="app">Represents the main gateway to interact with an app.</param>
		/// <param name="x">The x coordinate to double click.</param>
		/// <param name="y">The y coordinate to double click.</param>
		public static void DoubleClickCoordinates(this IApp app, float x, float y)
		{
			app.CommandExecutor.Execute("doubleClickCoordinates", new Dictionary<string, object>
			{
				{ "x", x },
				{ "y", y }
			});
		}

		/// <summary>
		/// Performs two quick tap / touch gestures on the matched element.
		/// </summary>
		/// <param name="app">Represents the main gateway to interact with an app.</param>
		/// <param name="element">Target Element.</param>
		public static void DoubleTap(this IApp app, string element)
		{
			var elementToDoubleTap = app.FindElement(element);
			app.CommandExecutor.Execute("doubleTap", new Dictionary<string, object>
			{
				{ "element", elementToDoubleTap },
			});
		}

		/// <summary>
		/// Performs two quick tap / touch gestures on the given coordinates.
		/// </summary>
		/// <param name="app">Represents the main gateway to interact with an app.</param>
		/// <param name="x">The x coordinate to double tap.</param>
		/// <param name="y">The y coordinate to double tap.</param>
		public static void DoubleTapCoordinates(this IApp app, float x, float y)
		{
			app.CommandExecutor.Execute("doubleTapCoordinates", new Dictionary<string, object>
			{
				{ "x", x },
				{ "y", y }
			});
		}

		/// <summary>
		/// Performs a long mouse click on the matched element.
		/// </summary>
		/// <param name="app">Represents the main gateway to interact with an app.</param>
		/// <param name="element">Target Element.</param>
		public static void LongPress(this IApp app, string element)
		{
			var elementToLongPress = app.FindElement(element);
			app.CommandExecutor.Execute("longPress", new Dictionary<string, object>
			{
				{ "element", elementToLongPress },
			});
		}

		/// <summary>
		/// Performs a continuous touch gesture on the matched element.
		/// </summary>
		/// <param name="app">Represents the main gateway to interact with an app.</param>
		/// <param name="element">Target Element.</param>
		public static void TouchAndHold(this IApp app, string element)
		{
			var elementToTouchAndHold = app.FindElement(element);
			app.CommandExecutor.Execute("touchAndHold", new Dictionary<string, object>
			{
				{ "element", elementToTouchAndHold },
			});
		}

		/// <summary>
		/// Performs a continuous touch gesture on the given coordinates.
		/// </summary>
		/// <param name="app">Represents the main gateway to interact with an app.</param>
		/// <param name="x">The x coordinate to touch.</param>
		/// <param name="y">The y coordinate to touch.</param>
		public static void TouchAndHoldCoordinates(this IApp app, float x, float y)
		{
			app.CommandExecutor.Execute("touchAndHoldCoordinates", new Dictionary<string, object>
			{		
				{ "x", x },
				{ "y", y }
			});
		}

		/// <summary>
		/// Performs a long touch on an item, followed by dragging the item to a second item and dropping it.
		/// </summary>
		/// <param name="app">Represents the main gateway to interact with an app.</param>
		/// <param name="dragSource">Element to be dragged.</param>
		/// <param name="dragTarget">Element to be dropped.</param>
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

		/// <summary>
		/// Scroll until an element that matches the toElementId is shown on the screen.
		/// </summary>
		/// <param name="app">Represents the main gateway to interact with an app.</param>
		/// <param name="toElementId">Specify what element to scroll within.</param>
		/// <param name="down">Whether scrolls should be down or up.</param>
		public static void ScrollTo(this IApp app, string toElementId, bool down = true)
		{
			app.CommandExecutor.Execute("scrollTo", new Dictionary<string, object>
			{
				{ "elementId", toElementId},
				{ "down", down }
			});
		}

		/// <summary>
		/// Return the currently presented alert or action sheet.
		/// </summary>
		/// <param name="app">Represents the main gateway to interact with an app.</param>
		public static IUIElement? GetAlert(this IApp app)
		{
			return app.GetAlerts().FirstOrDefault();
		}

		/// <summary>
		/// Return the currently presented alerts or action sheets.
		/// </summary>
		/// <param name="app">Represents the main gateway to interact with an app.</param>
		public static IReadOnlyCollection<IUIElement> GetAlerts(this IApp app)
		{
			var result = app.CommandExecutor.Execute("getAlerts", ImmutableDictionary<string, object>.Empty);
			return (IReadOnlyCollection<IUIElement>?)result.Value ?? Array.Empty<IUIElement>();
		}

		/// <summary>
		/// Dismisses the alert.
		/// </summary>
		/// <param name="alertElement">The element that represents the alert or action sheet.</param>
		public static void DismissAlert(this IUIElement alertElement)
		{
			alertElement.Command.Execute("dismissAlert", new Dictionary<string, object>
			{
				["element"] = alertElement
			});
		}

		/// <summary>
		/// Return the buttons in the alert or action sheet.
		/// </summary>
		/// <param name="alertElement">The element that represents the alert or action sheet.</param>
		public static IReadOnlyCollection<IUIElement> GetAlertButtons(this IUIElement alertElement)
		{
			var result = alertElement.Command.Execute("getAlertButtons", new Dictionary<string, object>
			{
				["element"] = alertElement
			});
			return (IReadOnlyCollection<IUIElement>?)result.Value ?? Array.Empty<IUIElement>();
		}

		/// <summary>
		/// Return the text messages in the alert or action sheet.
		/// </summary>
		/// <param name="alertElement">The element that represents the alert or action sheet.</param>
		public static IReadOnlyCollection<string> GetAlertText(this IUIElement alertElement)
		{
			var result = alertElement.Command.Execute("getAlertText", new Dictionary<string, object>
			{
				["element"] = alertElement
			});
			return (IReadOnlyCollection<string>?)result.Value ?? Array.Empty<string>();
		}

		public static IUIElement WaitForElement(this IApp app, string marked, string timeoutMessage = "Timed out waiting for element...", TimeSpan? timeout = null, TimeSpan? retryFrequency = null, TimeSpan? postTimeout = null)
		{
			IUIElement result() => app.FindElement(marked);
			var results = WaitForAtLeastOne(result, timeoutMessage, timeout, retryFrequency);

			return results;
		}

		public static IUIElement WaitForElement(
			this IApp app,
			Func<IUIElement?> query,
			string? timeoutMessage = null,
			TimeSpan? timeout = null,
			TimeSpan? retryFrequency = null)
		{
			var results = Wait(query, i => i != null, timeoutMessage, timeout, retryFrequency);

			return results;
		}

		public static void WaitForNoElement(this IApp app, string marked, string timeoutMessage = "Timed out waiting for no element...", TimeSpan? timeout = null, TimeSpan? retryFrequency = null, TimeSpan? postTimeout = null)
		{
			IUIElement result() => app.FindElement(marked);
			WaitForNone(result, timeoutMessage, timeout, retryFrequency);
		}

		public static void WaitForNoElement(
			this IApp app,
			Func<IUIElement?> query,
			string? timeoutMessage = null,
			TimeSpan? timeout = null,
			TimeSpan? retryFrequency = null)
		{
			Wait(query, i => i is null, timeoutMessage, timeout, retryFrequency);
		}

		public static bool WaitForTextToBePresentInElement(this IApp app, string automationId, string text, TimeSpan? timeout = null)
		{
			timeout ??= DefaultTimeout;
			TimeSpan retryFrequency = TimeSpan.FromMilliseconds(500);

			DateTime start = DateTime.Now;

			while (true)
			{
				var element = app.FindElements(automationId).FirstOrDefault();
				if (element != null && (element.GetText()?.Contains(text, StringComparison.OrdinalIgnoreCase) ?? false))
				{
					return true;
				}

				long elapsed = DateTime.Now.Subtract(start).Ticks;
				if (elapsed >= timeout.Value.Ticks)
				{
					Debug.WriteLine($">>>>> {elapsed} ticks elapsed, timeout value is {timeout.Value.Ticks}");

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
		/// Presses the enter key in the app.
		/// </summary>
		/// <param name="app">Represents the main gateway to interact with an app.</param>
		public static void PressEnter(this IApp app)
		{
			app.CommandExecutor.Execute("pressEnter", ImmutableDictionary<string, object>.Empty);
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
		/// Performs a mouse click on the given coordinates.
		/// </summary>
		/// <param name="app">Represents the main gateway to interact with an app.</param>
		/// <param name="x">The x coordinate to click.</param>
		/// <param name="y">The y coordinate to click.</param>
		public static void ClickCoordinates(this IApp app, float x, float y)
		{
			app.CommandExecutor.Execute("clickCoordinates", new Dictionary<string, object>
			{
				{ "x", x },
				{ "y", y }
			});
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
		/// Increases the value of a Stepper control.
		/// </summary>
		/// <param name="app">Represents the main gateway to interact with an app.</param>
		/// <param name="marked">Marked selector of the Stepper element to increase.</param>
		public static void IncreaseStepper(this IApp app, string marked)
		{
			app.CommandExecutor.Execute("increaseStepper", new Dictionary<string, object>
			{
				["elementId"] = marked
			});
		}

		/// <summary>
		/// Decreases the value of a Stepper control.
		/// </summary>
		/// <param name="app">Represents the main gateway to interact with an app.</param>
		/// <param name="marked">Marked selector of the Stepper element to decrease.</param>
		public static void DecreaseStepper(this IApp app, string marked)
		{
			app.CommandExecutor.Execute("decreaseStepper", new Dictionary<string, object>
			{
				["elementId"] = marked
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
		/// Sets light device's theme
		/// </summary>
		/// <param name="app">Represents the main gateway to interact with an app.</param>
		public static void SetLightTheme(this IApp app)
		{
			if (app is not AppiumAndroidApp && app is not AppiumIOSApp)
			{
				throw new InvalidOperationException($"SetLightTheme is not supported");
			}

			app.CommandExecutor.Execute("setLightTheme", ImmutableDictionary<string, object>.Empty);
		}

		/// <summary>
		/// Sets dark device's theme
		/// </summary>
		/// <param name="app">Represents the main gateway to interact with an app.</param>
		public static void SetDarkTheme(this IApp app)
		{
			if (app is not AppiumAndroidApp && app is not AppiumIOSApp)
			{
				throw new InvalidOperationException($"SetDarkTheme is not supported");
			}

			app.CommandExecutor.Execute("setDarkTheme", ImmutableDictionary<string, object>.Empty);
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

		/// <summary>
		/// Lock the screen.
		/// Functionality that's only available on Android and iOS.
		/// </summary>
		/// <param name="app">Represents the main gateway to interact with an app.</param>
		/// <exception cref="InvalidOperationException">Lock is only supported on <see cref="AppiumAndroidApp"/>.</exception>
		public static void Lock(this IApp app)
		{
			if (app is not AppiumAndroidApp)
			{
				throw new InvalidOperationException($"Lock is only supported on AppiumAndroidApp");
			}

			app.CommandExecutor.Execute("lock", ImmutableDictionary<string, object>.Empty);
		}

		/// <summary>
		/// Unlock the screen.
		/// Functionality that's only available on Android and iOS.
		/// </summary>
		/// <param name="app">Represents the main gateway to interact with an app.</param>
		/// <exception cref="InvalidOperationException">Unlock is only supported on <see cref="AppiumAndroidApp"/>.</exception>
		public static void Unlock(this IApp app)
		{
			if (app is not AppiumAndroidApp)
			{
				throw new InvalidOperationException($"Unlock is only supported on AppiumAndroidApp");
			}

			app.CommandExecutor.Execute("unlock", ImmutableDictionary<string, object>.Empty);
		}

		/// <summary>
		/// Start recording screen.
		/// Functionality that's only available on Android, iOS and Windows.
		/// </summary>
		/// <param name="app">Represents the main gateway to interact with an app.</param>
		/// <exception cref="InvalidOperationException">StartRecordingScreen is only supported on <see cref="AppiumAndroidApp"/>.</exception>
		public static void StartRecordingScreen(this IApp app)
		{
			if (app is not AppiumAndroidApp)
			{
				throw new InvalidOperationException($"StartRecordingScreen is only supported on AppiumAndroidApp");
			}

			app.CommandExecutor.Execute("startRecordingScreen", ImmutableDictionary<string, object>.Empty);
		}

		/// <summary>
		/// Stop recording screen.
		/// Functionality that's only available on Android, iOS and Windows.
		/// </summary>
		/// <param name="app">Represents the main gateway to interact with an app.</param>
		/// <exception cref="InvalidOperationException">StopRecordingScreen is only supported on <see cref="AppiumAndroidApp"/>.</exception>
		public static void StopRecordingScreen(this IApp app)
		{
			if (app is not AppiumAndroidApp)
			{
				throw new InvalidOperationException($"StopRecordingScreen is only supported on AppiumAndroidApp");
			}

			app.CommandExecutor.Execute("stopRecordingScreen", ImmutableDictionary<string, object>.Empty);
		}

		/// <summary>
		/// Toggle airplane mode on device.
		/// Functionality that's only available on Android.
		/// </summary>
		/// <param name="app">Represents the main gateway to interact with an app.</param>
		/// <exception cref="InvalidOperationException">ToggleAirplaneMode is only supported on <see cref="AppiumAndroidApp"/>.</exception>
		public static void ToggleAirplaneMode(this IApp app)
		{
			if (app is not AppiumAndroidApp)
			{
				throw new InvalidOperationException($"ToggleAirplaneMode is only supported on AppiumAndroidApp");
			}

			app.CommandExecutor.Execute("toggleAirplaneMode", ImmutableDictionary<string, object>.Empty);
		}

		/// <summary>
		/// Switch the state of the wifi service.
		/// Functionality that's only available on Android.
		/// </summary>
		/// <param name="app">Represents the main gateway to interact with an app.</param>
		/// <exception cref="InvalidOperationException">ToggleWifi is only supported on <see cref="AppiumAndroidApp"/>.</exception>
		public static void ToggleWifi(this IApp app)
		{
			if (app is not AppiumAndroidApp)
			{
				throw new InvalidOperationException($"ToggleWifi is only supported on AppiumAndroidApp");
			}

			app.CommandExecutor.Execute("toggleWifi", ImmutableDictionary<string, object>.Empty);
		}

		/// <summary>
		/// Simulate the device shaking.
		/// Functionality that's only available on iOS.
		/// </summary>
		/// <param name="app">Represents the main gateway to interact with an app.</param>
		/// <exception cref="InvalidOperationException">ToggleWifi is only supported on <see cref="AppiumAndroidApp"/>.</exception>
		public static void Shake(this IApp app)
		{
			if (app is not AppiumIOSApp)
			{
				throw new InvalidOperationException($"Shake is only supported on AppiumIOSApp");
			}

			app.CommandExecutor.Execute("shake", ImmutableDictionary<string, object>.Empty);
		}

		/// <summary>
		/// Gets the information of the system state which is supported to read as like cpu, memory, network traffic, and battery.
		/// Functionality that's only available on Android.
		/// </summary>
		/// <param name="app">Represents the main gateway to interact with an app.</param>
		/// <param name="performanceDataType">The available performance data types(cpuinfo | batteryinfo | networkinfo | memoryinfo).</param>
		/// <exception cref="InvalidOperationException">ToggleWifi is only supported on <see cref="AppiumAndroidApp"/>.</exception>
		/// <returns>The information of the system related to the performance.</returns>
		public static IList<object> GetPerformanceData(this IApp app, string performanceDataType)
		{
			if (app is not AppiumAndroidApp)
			{
				throw new InvalidOperationException($"ToggleWifi is only supported on AppiumAndroidApp");
			}

			var response = app.CommandExecutor.Execute("getPerformanceData", new Dictionary<string, object>()
			{
				{ "performanceDataType", performanceDataType },
			});

			if (response?.Value != null)
			{
				return (IList<object>)response.Value;
			}

			throw new InvalidOperationException($"Could not get the performance data");
		}

		static IUIElement Wait(Func<IUIElement?> query,
			Func<IUIElement?, bool> satisfactory,
			string? timeoutMessage = null,
			TimeSpan? timeout = null, TimeSpan? retryFrequency = null)
		{
			timeout ??= DefaultTimeout;
			retryFrequency ??= TimeSpan.FromMilliseconds(500);
			timeoutMessage ??= "Timed out on query.";

			DateTime start = DateTime.Now;

			IUIElement? result = query();

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

			return result!;
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
