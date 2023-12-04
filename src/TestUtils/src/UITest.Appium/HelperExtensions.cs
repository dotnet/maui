using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.Xml.Linq;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.MultiTouch;
using UITest.Core;

namespace UITest.Appium
{
	public static class HelperExtensions
	{
		static TimeSpan DefaultTimeout = TimeSpan.FromSeconds(15);

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

		public static void ClearText(this IApp app, string element)
		{
			app.FindElement(element).Clear();
		}

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
