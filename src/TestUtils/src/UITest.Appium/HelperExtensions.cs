using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Android.Enums;
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
		/// </summary>
		/// <param name="app">Represents the main gateway to interact with an app.</param>
		/// <param name="element">Target Element.</param>
		public static void Tap(this IApp app, string element)
		{
			FindElement(app, element).Click();
		}

		/// <summary>
		/// For desktop, this will perform a mouse click on the target element.
		/// For mobile, this will tap the element.
		/// </summary>
		/// <param name="app">Represents the main gateway to interact with an app.</param>
		/// <param name="query">Represents the query that identify an element by parameters such as type, text it contains or identifier.</param>
		public static void Tap(this IApp app, IQuery query)
		{
			app.FindElement(query).Tap();
		}

		/// <summary>
		/// Performs a mouse click on the matched element.
		/// </summary>
		/// <param name="app">Represents the main gateway to interact with an app.</param>
		/// <param name="element">Target Element.</param>
		public static void Click(this IApp app, string element)
		{
			FindElement(app, element).Click();
		}

		/// <summary>
		/// Performs a mouse click on the matched element.
		/// </summary>
		/// <param name="app">Represents the main gateway to interact with an app.</param>
		/// <param name="query">Represents the query that identify an element by parameters such as type, text it contains or identifier.</param>
		public static void Click(this IApp app, IQuery query)
		{
			app.FindElement(query).Click();
		}

		public static void RightClick(this IApp app, string element)
		{
			var uiElement = FindElement(app, element);
			uiElement.Command.Execute("click", new Dictionary<string, object>()
			{
				{ "element", uiElement },
				{ "button", "right" }
			});
		}

		/// <summary>
		/// Performs a down/press on the matched element, without a matching release
		/// </summary>
		/// <param name="app"></param>
		/// <param name="element"></param>
		public static void PressDown(this IApp app, string element)
		{
			var uiElement = FindElement(app, element);
			uiElement.Command.Execute("pressDown", new Dictionary<string, object>()
			{
				{ "element", uiElement }
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

		public static bool TryGetText(this IUIElement element, [NotNullWhen(true)] out string? text)
		{
			try
			{
				text = GetText(element);
				return text is not null;
			}
			catch
			{
				text = null;
				return false;
			}
		}

		public static string? ReadText(this IUIElement element)
			=> element.GetText();

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
		/// Determine if a form or form-like element (checkbox, select, etc...) is selected.
		/// </summary>
		/// <param name="element">Target Element.</param>
		/// <returns>Whether the element is selected (boolean).</returns>
		public static bool IsSelected(this IUIElement element)
		{
			var response = element.Command.Execute("getSelected", new Dictionary<string, object>()
			{
				{ "element", element },
			});

			if (response?.Value != null)
			{
				return (bool)response.Value;
			}

			throw new InvalidOperationException($"Could not get Selected of element");
		}

		/// <summary>
		/// Determine if an element is currently displayed.
		/// </summary>
		/// <param name="element">Target Element.</param>
		/// <returns>Whether the element is displayed (boolean).</returns>
		public static bool IsDisplayed(this IUIElement element)
		{
			var response = element.Command.Execute("getDisplayed", new Dictionary<string, object>()
			{
				{ "element", element },
			});

			if (response?.Value != null)
			{
				return (bool)response.Value;
			}

			throw new InvalidOperationException($"Could not get Displayed of element");
		}

		/// <summary>
		/// Determine if an element is currently enabled.
		/// </summary>
		/// <param name="element">Target Element.</param>
		/// <returns>Whether the element is enabled (boolean).</returns>
		public static bool IsEnabled(this IUIElement element)
		{
			var response = element.Command.Execute("getEnabled", new Dictionary<string, object>()
			{
				{ "element", element },
			});

			if (response?.Value != null)
			{
				return (bool)response.Value;
			}

			throw new InvalidOperationException($"Could not get Enabled of element");
		}

		/// <summary>
		/// Enters text into the element identified by the query.
		/// </summary>
		/// <param name="app">Represents the main gateway to interact with an app.</param>
		/// <param name="element">Target Element.</param>
		/// <param name="text">The text to enter.</param>
		public static void EnterText(this IApp app, string element, string text)
		{
			var appElement = app.FindElement(element);

			app.EnterText(appElement, text);
		}

		/// <summary>
		/// Enters text into the element identified by the query.
		/// </summary>
		/// <param name="app">Represents the main gateway to interact with an app.</param>
		/// <param name="element">Target Element.</param>
		/// <param name="query">Represents the query that identify an element by parameters such as type, text it contains or identifier.</param>
		public static void EnterText(this IApp app, IQuery query, string text)
		{
			var appElement = app.FindElement(query);

			app.EnterText(appElement, text);
		}

		internal static void EnterText(this IApp app, IUIElement? element, string text)
		{
			if (element is not null)
			{
				element.SendKeys(text);
				app.DismissKeyboard();
			}
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
		/// Waits for the soft keyboard to be shown on the screen.
		/// </summary>
		/// <param name="app">Represents the main gateway to interact with an app.</param>
		/// <param name="timeout">The TimeSpan to wait before failing. Default is 15 seconds.</param>
		/// <param name="retryFrequency">The TimeSpan to wait between each check. Default is 500ms.</param>
		/// <returns>true if the keyboard becomes visible within the timeout; otherwise, false.</returns>
		public static bool WaitForKeyboardToShow(this IApp app, TimeSpan? timeout = null, TimeSpan? retryFrequency = null)
		{
			timeout ??= DefaultTimeout;
			retryFrequency ??= TimeSpan.FromMilliseconds(500);

			DateTime start = DateTime.Now;

			while (true)
			{
				if (app.IsKeyboardShown())
				{
					return true;
				}

				long elapsed = DateTime.Now.Subtract(start).Ticks;
				if (elapsed >= timeout.Value.Ticks)
				{
					return false;
				}

				Thread.Sleep(retryFrequency.Value.Milliseconds);
			}
		}

		/// <summary>
		/// Waits for the soft keyboard to be hidden from the screen.
		/// </summary>
		/// <param name="app">Represents the main gateway to interact with an app.</param>
		/// <param name="timeout">The TimeSpan to wait before failing. Default is 15 seconds.</param>
		/// <param name="retryFrequency">The TimeSpan to wait between each check. Default is 500ms.</param>
		/// <returns>true if the keyboard becomes hidden within the timeout; otherwise, false.</returns>
		public static bool WaitForKeyboardToHide(this IApp app, TimeSpan? timeout = null, TimeSpan? retryFrequency = null)
		{
			timeout ??= DefaultTimeout;
			retryFrequency ??= TimeSpan.FromMilliseconds(500);

			DateTime start = DateTime.Now;

			while (true)
			{
				if (!app.IsKeyboardShown())
				{
					return true;
				}

				long elapsed = DateTime.Now.Subtract(start).Ticks;
				if (elapsed >= timeout.Value.Ticks)
				{
					return false;
				}

				Thread.Sleep(retryFrequency.Value);
			}
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
			FindElement(app, element).Clear();
		}

		/// <summary>
		/// Clears text from the currently focused element.
		/// </summary>
		/// <param name="app">Represents the main gateway to interact with an app.</param>
		/// <param name="query">Represents the query that identify an element by parameters such as type, text it contains or identifier.</param>
		public static void ClearText(this IApp app, IQuery query)
		{
			app.FindElement(query).Clear();
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
			var elementToDoubleClick = FindElement(app, element);
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

			app.DoubleTap(elementToDoubleTap);
		}

		/// <summary>
		/// Performs two quick tap / touch gestures on the matched element by 'query'.
		/// </summary>
		/// <param name="app">Represents the main gateway to interact with an app.</param>
		/// <param name="query"></param>
		public static void DoubleTap(this IApp app, IQuery query)
		{
			var elementToDoubleTap = app.FindElement(query);

			app.DoubleTap(elementToDoubleTap);
		}

		internal static void DoubleTap(this IApp app, IUIElement? element)
		{
			if (element is not null)
			{
				app.CommandExecutor.Execute("doubleTap", new Dictionary<string, object>
				{
					{ "element", element },
				});
			}
		}

		/// <summary>
		/// Performs two quick tap / touch gestures on the given coordinates.
		/// </summary>
		/// <param name="app">Represents the main gateway to interact with an app.</param>
		/// <param name="x">The x coordinate to double tap.</param>
		/// <param name="y">The y coordinate to double tap.</param>
		public static void DoubleTapCoordinates(this IApp app, float x, float y)
		{
			if (app is AppiumCatalystApp)
			{
				app.DoubleClickCoordinates(x, y); // Directly invoke coordinate-based double click for AppiumCatalystApp.
			}
			else
			{
				app.CommandExecutor.Execute("doubleTapCoordinates", new Dictionary<string, object>
				{
					{ "x", x },
					{ "y", y }
				});
			}
		}

		/// <summary>
		/// Performs a long mouse click on the matched element.
		/// </summary>
		/// <param name="app">Represents the main gateway to interact with an app.</param>
		/// <param name="element">Target Element.</param>
		public static void LongPress(this IApp app, string element)
		{
			var elementToLongPress = FindElement(app, element);
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

			app.TouchAndHold(elementToTouchAndHold);
		}

		/// <summary>
		/// Performs a continuous touch gesture on an element matched by 'query'.
		/// </summary>
		/// <param name="app">Represents the main gateway to interact with an app.</param>
		/// <param name="query">Represents the query that identify an element by parameters such as type, text it contains or identifier.</param>
		public static void TouchAndHold(this IApp app, IQuery query)
		{
			var elementToTouchAndHold = app.FindElement(query);

			app.TouchAndHold(elementToTouchAndHold);
		}

		internal static void TouchAndHold(this IApp app, IUIElement? element)
		{
			if (element is not null)
			{
				app.CommandExecutor.Execute("touchAndHold", new Dictionary<string, object>
				{
					{ "element", element },
				});
			}
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
			var dragSourceElement = FindElement(app, dragSource);
			var targetSourceElement = FindElement(app, dragTarget);

			app.DragAndDrop(dragSourceElement, targetSourceElement);
		}

		/// <summary>
		/// Performs a long touch on an item, followed by dragging the item to a second item and dropping it.
		/// </summary>
		/// <param name="app">Represents the main gateway to interact with an app.</param>
		/// <param name="dragSource">Represents the query that identify a source element by parameters such as type, text it contains or identifier.</param>
		/// <param name="dragTarget">Represents the query that identify a target element by parameters such as type, text it contains or identifier.</param>
		public static void DragAndDrop(this IApp app, IQuery dragSource, IQuery dragTarget)
		{
			var dragSourceElement = app.FindElement(dragSource);
			var targetSourceElement = app.FindElement(dragTarget);

			app.DragAndDrop(dragSourceElement, targetSourceElement);
		}

		internal static void DragAndDrop(this IApp app, IUIElement? dragSourceElement, IUIElement? targetSourceElement)
		{
			if (dragSourceElement is not null && targetSourceElement is not null)
			{
				app.CommandExecutor.Execute("dragAndDrop", new Dictionary<string, object>
				{
					{ "sourceElement", dragSourceElement },
					{ "destinationElement", targetSourceElement }
				});
			}
		}

		/// <summary>
		/// Performs a pinch gestures on the matched element to zoom the view in. 
		/// If multiple elements are matched, the first one will be used.
		/// </summary>
		/// <param name="app">Represents the main gateway to interact with an app.</param>
		/// <param name="element">Element to zoom in.</param>
		/// <param name="duration">The TimeSpan duration of the pinch gesture.</param>
		public static void PinchToZoomIn(this IApp app, string element, TimeSpan? duration = null)
		{
			var elementToPinchToZoomIn = app.FindElement(element);

			app.CommandExecutor.Execute("pinchToZoomIn", new Dictionary<string, object>
			{
				{ "element", elementToPinchToZoomIn },
				{ "duration", duration ?? TimeSpan.FromSeconds(1) }
			});
		}

		/// <summary>
		/// Performs a pinch gestures to zoom the view in on the given coordinates.
		/// </summary>
		/// <param name="app">Represents the main gateway to interact with an app.</param>
		/// <param name="x">The x coordinate of the center of the pinch.</param>
		/// <param name="y">The y coordinate of the center of the pinch.</param>
		/// <param name="duration">The TimeSpan duration of the pinch gesture.</param>
		public static void PinchToZoomInCoordinates(this IApp app, float x, float y, TimeSpan? duration = null)
		{
			app.CommandExecutor.Execute("pinchToZoomInCoordinates", new Dictionary<string, object>
			{
				{ "x", x },
				{ "y", y },
				{ "duration", duration ?? TimeSpan.FromSeconds(1) }
			});
		}

		/// <summary>
		/// Performs a pinch gestures on the matched element to zoom the view out. 
		/// If multiple elements are matched, the first one will be used.
		/// </summary>
		/// <param name="app">Represents the main gateway to interact with an app.</param>
		/// <param name="element">Element to zoom in.</param>
		/// <param name="duration">The TimeSpan duration of the pinch gesture.</param>
		public static void PinchToZoomOut(this IApp app, string element, TimeSpan? duration = null)
		{
			var elementToPinchToZoomOut = app.FindElement(element);

			app.CommandExecutor.Execute("pinchToZoomOut", new Dictionary<string, object>
			{
				{ "element", elementToPinchToZoomOut },
				{ "duration", duration ?? TimeSpan.FromSeconds(1) }
			});
		}

		/// <summary>
		/// Performs a pinch gestures to zoom the view out on the given coordinates.
		/// </summary>
		/// <param name="app">Represents the main gateway to interact with an app.</param>
		/// <param name="x">The x coordinate of the center of the pinch.</param>
		/// <param name="y">The y coordinate of the center of the pinch.</param>
		/// <param name="duration">The TimeSpan duration of the pinch gesture.</param>
		public static void PinchToZoomOutCoordinates(this IApp app, float x, float y, TimeSpan? duration = null)
		{
			app.CommandExecutor.Execute("pinchToZoomOutCoordinates", new Dictionary<string, object>
			{
				{ "x", x },
				{ "y", y },
				{ "duration", duration ?? TimeSpan.FromSeconds(1) }
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

		/// <summary>
		/// Wait function that will repeatedly query the app until a matching element is found. 
		/// Throws a TimeoutException if no element is found within the time limit.
		/// </summary>
		/// <param name="app">Represents the main gateway to interact with an app.</param>
		/// <param name="marked">Target Element.</param>
		/// <param name="timeoutMessage">The message used in the TimeoutException.</param>
		/// <param name="timeout">The TimeSpan to wait before failing.</param>
		/// <param name="retryFrequency">The TimeSpan to wait between each query call to the app.</param>
		/// <param name="postTimeout">The final TimeSpan to wait after the element has been found.</param>
		public static IUIElement WaitForElement(this IApp app, string marked, string timeoutMessage = "Timed out waiting for element...", TimeSpan? timeout = null, TimeSpan? retryFrequency = null, TimeSpan? postTimeout = null)
		{
			IUIElement result() => FindElement(app, marked);
			var results = WaitForAtLeastOne(result, timeoutMessage, timeout, retryFrequency);

			return results;
		}

		/// <summary>
		/// Wait function that will repeatedly query the app until any matching element is found. 
		/// Throws a TimeoutException if no element is found within the time limit.
		/// </summary>
		/// <param name="app">Represents the main gateway to interact with an app.</param>
		/// <param name="marked">Collection of target Elements.</param>
		/// <param name="timeoutMessage">The message used in the TimeoutException.</param>
		/// <param name="timeout">The TimeSpan to wait before failing.</param>
		/// <param name="retryFrequency">The TimeSpan to wait between each query call to the app.</param>
		/// <param name="postTimeout">The final TimeSpan to wait after the element has been found.</param>
		public static IUIElement WaitForAnyElement(this IApp app, string[] marked, string timeoutMessage = "Timed out waiting for element...", TimeSpan? timeout = null, TimeSpan? retryFrequency = null, TimeSpan? postTimeout = null)
		{
			IUIElement result() => FindAnyElement(app, marked);
			var results = WaitForAtLeastOne(result, timeoutMessage, timeout, retryFrequency);

			return results;
		}

		/// <summary>
		/// Wait function that will repeatedly query the app until a matching element is found. 
		/// Throws a TimeoutException if no element is found within the time limit.
		/// </summary>
		/// <param name="app">Represents the main gateway to interact with an app.</param>
		/// <param name="query">Represents the query that identify an element by parameters such as type, text it contains or identifier.</param>
		/// <param name="timeoutMessage">The message used in the TimeoutException.</param>
		/// <param name="timeout">The TimeSpan to wait before failing.</param>
		/// <param name="retryFrequency">The TimeSpan to wait between each query call to the app.</param>
		/// <param name="postTimeout">The final TimeSpan to wait after the element has been found.</param>
		public static IUIElement WaitForElement(this IApp app, IQuery query, string timeoutMessage = "Timed out waiting for element...", TimeSpan? timeout = null, TimeSpan? retryFrequency = null, TimeSpan? postTimeout = null)
		{
			IUIElement result() => app.FindElement(query);
			var results = WaitForAtLeastOne(result, timeoutMessage, timeout, retryFrequency);

			return results;
		}

		/// <summary>
		/// Wait function that will repeatedly query the app until a matching element is found. 
		/// Throws a TimeoutException if no element is found within the time limit.
		/// </summary>
		/// <param name="app">Represents the main gateway to interact with an app.</param>
		/// <param name="query">Entry point for the fluent API to specify the element.</param>
		/// <param name="timeoutMessage">The message used in the TimeoutException.</param>
		/// <param name="timeout">The TimeSpan to wait before failing.</param>
		/// <param name="retryFrequency">The TimeSpan to wait between each query call to the app.</param>
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

		/// <summary>
		/// Wait function that will repeatedly query the app until a matching element is no longer found. 
		/// Throws a TimeoutException if the element is visible at the end of the time limit.
		/// </summary>
		/// <param name="app">Represents the main gateway to interact with an app.</param>
		/// <param name="marked">Target Element.</param>
		/// <param name="timeoutMessage">The message used in the TimeoutException.</param>
		/// <param name="timeout">The TimeSpan to wait before failing.</param>
		/// <param name="retryFrequency">The TimeSpan to wait between each query call to the app.</param>
		/// <param name="postTimeout">The final TimeSpan to wait after the element has been found.</param>
		public static void WaitForNoElement(this IApp app, string marked, string timeoutMessage = "Timed out waiting for no element...", TimeSpan? timeout = null, TimeSpan? retryFrequency = null, TimeSpan? postTimeout = null)
		{
			IUIElement result() => app.FindElement(marked);
			WaitForNone(result, timeoutMessage, timeout, retryFrequency);
		}

		/// <summary>
		/// Wait function that will repeatedly query the app until a matching element is no longer found. 
		/// Throws a TimeoutException if the element is visible at the end of the time limit.
		/// </summary>
		/// <param name="app">Represents the main gateway to interact with an app.</param>
		/// <param name="query">Represents the query that identify an element by parameters such as type, text it contains or identifier.</param>
		/// <param name="timeoutMessage">The message used in the TimeoutException.</param>
		/// <param name="timeout">The TimeSpan to wait before failing.</param>
		/// <param name="retryFrequency">The TimeSpan to wait between each query call to the app.</param>
		/// <param name="postTimeout">The final TimeSpan to wait after the element has been found.</param>
		public static void WaitForNoElement(this IApp app, IQuery query, string timeoutMessage = "Timed out waiting for no element...", TimeSpan? timeout = null, TimeSpan? retryFrequency = null, TimeSpan? postTimeout = null)
		{
			IUIElement result() => app.FindElement(query);
			WaitForNone(result, timeoutMessage, timeout, retryFrequency);
		}

		/// <summary>
		/// Wait function that will repeatedly query the app until a matching element is no longer found. 
		/// Throws a TimeoutException if the element is visible at the end of the time limit.
		/// </summary>
		/// <param name="app">Represents the main gateway to interact with an app.</param>
		/// <param name="query">Entry point for the fluent API to specify the element.</param>
		/// <param name="timeoutMessage">The message used in the TimeoutException.</param>
		/// <param name="timeout">The TimeSpan to wait before failing.</param>
		/// <param name="retryFrequency">The TimeSpan to wait between each query call to the app.</param>
		public static void WaitForNoElement(
			this IApp app,
			Func<IUIElement?> query,
			string? timeoutMessage = null,
			TimeSpan? timeout = null,
			TimeSpan? retryFrequency = null)
		{
			Wait(query, i => i is null, timeoutMessage, timeout, retryFrequency);
		}

		public static IUIElement WaitForFirstElement(this IApp app, string marked, string timeoutMessage = "Timed out waiting for element...", TimeSpan? timeout = null, TimeSpan? retryFrequency = null, TimeSpan? postTimeout = null)
		{
			IReadOnlyCollection<IUIElement> elements = FindElements(app, marked);

			if (elements is not null && elements.Count > 0)
			{
				IUIElement firstElement() => elements.First();

				var result = Wait(firstElement, i => i != null, timeoutMessage, timeout, retryFrequency);

				return result;
			}

			return WaitForElement(app, marked, timeoutMessage, timeout, retryFrequency, postTimeout);
		}

		public static bool WaitForTextToBePresentInElement(this IApp app, string automationId, string text, TimeSpan? timeout = null)
		{
			timeout ??= DefaultTimeout;
			TimeSpan retryFrequency = TimeSpan.FromMilliseconds(500);

			DateTime start = DateTime.Now;

			while (true)
			{
				var element = app.FindElements(automationId).FirstOrDefault();

				if (element is not null && element.TryGetText(out var s) && s.Contains(text, StringComparison.OrdinalIgnoreCase))
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
		/// Repeatedly executes a query until it returns a non-empty value or the specified retry count is reached.
		/// </summary>
		/// <typeparam name="T">The type of the element.</typeparam>
		/// <param name="app">Represents the main gateway to interact with an app.</param>
		/// <param name="func">The query to execute.</param>
		/// <param name="retryCount">The number of times to retry execution. Default is 10.</param>
		/// <param name="delayInMs">The delay in milliseconds between retries. Default is 2000ms.</param>
		/// <returns>An value of type T.</returns>
		public static T QueryUntilPresent<T>(
			this IApp app,
			Func<T> func,
			int retryCount = 10,
			int delayInMs = 2000)
		{
			var result = func();

			int counter = 0;
			while ((result is null) && counter < retryCount)
			{
				Thread.Sleep(delayInMs);
				result = func();
				counter++;
			}

			return result;
		}

		/// <summary>
		/// Repeatedly executes a query until it returns a null value or the specified retry count is reached.
		/// </summary>
		/// <typeparam name="T">The type of the element.</typeparam>
		/// <param name="app">Represents the main gateway to interact with an app.</param>
		/// <param name="func">The query to execute.</param>
		/// <param name="retryCount">The number of times to retry execution. Default is 10.</param>
		/// <param name="delayInMs">The delay in milliseconds between retries. Default is 2000ms.</param>
		/// <returns>An value of type T.</returns>
		public static T QueryUntilNotPresent<T>(
			this IApp app,
			Func<T> func,
			int retryCount = 10,
			int delayInMs = 2000)
		{
			var result = func();

			int counter = 0;
			while ((result is not null) && counter < retryCount)
			{
				Thread.Sleep(delayInMs);
				result = func();
				counter++;
			}

			return result;
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
			var elementToSwipe = FindElement(app, marked);

			app.SwipeLeftToRight(elementToSwipe, swipePercentage, swipeSpeed, withInertia);
		}

		/// <summary>
		/// Performs a left to right swipe gesture on an element matched by 'query'.
		/// </summary>
		/// <param name="app">Represents the main gateway to interact with an app.</param>
		/// <param name="query">Represents the query that identify an element by parameters such as type, text it contains or identifier.</param>
		/// <param name="swipePercentage">How far across the element to swipe (from 0.0 to 1.0).</param>
		/// <param name="swipeSpeed">The speed of the gesture.</param>
		/// <param name="withInertia">Whether swipes should cause inertia.</param>
		public static void SwipeLeftToRight(this IApp app, IQuery query, double swipePercentage = 0.67, int swipeSpeed = 500, bool withInertia = true)
		{
			var elementToSwipe = app.FindElement(query);

			app.SwipeLeftToRight(elementToSwipe, swipePercentage, swipeSpeed, withInertia);
		}

		internal static void SwipeLeftToRight(this IApp app, IUIElement? element, double swipePercentage = 0.67, int swipeSpeed = 500, bool withInertia = true)
		{
			if (element is not null)
			{
				app.CommandExecutor.Execute("swipeLeftToRight", new Dictionary<string, object>
				{
					{ "element", element },
					{ "swipePercentage", swipePercentage },
					{ "swipeSpeed", swipeSpeed },
					{ "withInertia", withInertia }
				});
			}
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
			var elementToSwipe = FindElement(app, marked);

			app.SwipeRightToLeft(elementToSwipe, swipePercentage, swipeSpeed, withInertia);
		}

		/// <summary>
		/// Performs a right to left swipe gesture on an element matched by 'query'.
		/// </summary>
		/// <param name="app">Represents the main gateway to interact with an app.</param>
		/// <param name="query">Represents the query that identify an element by parameters such as type, text it contains or identifier.</param>
		/// <param name="swipePercentage">How far across the element to swipe (from 0.0 to 1.0).</param>
		/// <param name="swipeSpeed">The speed of the gesture.</param>
		/// <param name="withInertia">Whether swipes should cause inertia.</param>
		public static void SwipeRightToLeft(this IApp app, IQuery query, double swipePercentage = 0.67, int swipeSpeed = 500, bool withInertia = true)
		{
			var elementToSwipe = app.FindElement(query);

			app.SwipeRightToLeft(elementToSwipe, swipePercentage, swipeSpeed, withInertia);
		}

		internal static void SwipeRightToLeft(this IApp app, IUIElement? element, double swipePercentage = 0.67, int swipeSpeed = 500, bool withInertia = true)
		{
			if (element is not null)
			{
				app.CommandExecutor.Execute("swipeRightToLeft", new Dictionary<string, object>
				{
					{ "element", element },
					{ "swipePercentage", swipePercentage },
					{ "swipeSpeed", swipeSpeed },
					{ "withInertia", withInertia }
				});
			}
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
			var elementToScroll = FindElement(app, marked);

			app.ScrollLeft(elementToScroll, strategy, swipePercentage, swipeSpeed, withInertia);
		}

		/// <summary>
		/// Scrolls left on the first element matching query.
		/// </summary>
		/// <param name="app">Represents the main gateway to interact with an app.</param>
		/// <param name="query">Represents the query that identify an element by parameters such as type, text it contains or identifier.</param>
		/// <param name="strategy">Strategy for scrolling element.</param>
		/// <param name="swipePercentage">How far across the element to swipe (from 0.0 to 1.0).</param>
		/// <param name="swipeSpeed">The speed of the gesture.</param>
		/// <param name="withInertia">Whether swipes should cause inertia.</param>
		public static void ScrollLeft(this IApp app, IQuery query, ScrollStrategy strategy = ScrollStrategy.Auto, double swipePercentage = 0.67, int swipeSpeed = 500, bool withInertia = true)
		{
			var elementToScroll = app.FindElement(query);

			app.ScrollLeft(elementToScroll, strategy, swipePercentage, swipeSpeed, withInertia);
		}

		internal static void ScrollLeft(this IApp app, IUIElement? element, ScrollStrategy strategy = ScrollStrategy.Auto, double swipePercentage = 0.67, int swipeSpeed = 500, bool withInertia = true)
		{
			if (element is not null)
			{
				app.CommandExecutor.Execute("scrollLeft", new Dictionary<string, object>
				{
					{ "element", element },
					{ "strategy", strategy },
					{ "swipePercentage", swipePercentage },
					{ "swipeSpeed", swipeSpeed },
					{ "withInertia", withInertia }
				});
			}
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
			var elementToScroll = FindElement(app, marked);

			app.ScrollDown(elementToScroll, strategy, swipePercentage, swipeSpeed, withInertia);
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
		public static void ScrollDown(this IApp app, IQuery query, ScrollStrategy strategy = ScrollStrategy.Auto, double swipePercentage = 0.67, int swipeSpeed = 500, bool withInertia = true)
		{
			var elementToScroll = app.FindElement(query);

			app.ScrollDown(elementToScroll, strategy, swipePercentage, swipeSpeed, withInertia);
		}

		internal static void ScrollDown(this IApp app, IUIElement? element, ScrollStrategy strategy = ScrollStrategy.Auto, double swipePercentage = 0.67, int swipeSpeed = 500, bool withInertia = true)
		{
			if (element is not null)
			{
				app.CommandExecutor.Execute("scrollDown", new Dictionary<string, object>
				{
					{ "element", element },
					{ "strategy", strategy },
					{ "swipePercentage", swipePercentage },
					{ "swipeSpeed", swipeSpeed },
					{ "withInertia", withInertia }
				});
			}
		}

		/// <summary>
		/// Scroll down until an element that matches the toMarked is shown on the screen.
		/// </summary>
		/// <param name="app">Represents the main gateway to interact with an app.</param>
		/// <param name="toMarked">Marked selector to select what element to bring on screen.</param>
		/// <param name="withinMarked">Marked selector to select what element to scroll within.</param>
		/// <param name="strategy">Strategy for scrolling element.</param>
		/// <param name="swipePercentage">How far across the element to swipe (from 0.0 to 1.0).</param>
		/// <param name="swipeSpeed">The speed of the gesture.</param>
		/// <param name="withInertia">Whether swipes should cause inertia.</param>
		public static void ScrollDownTo(this IApp app, string toMarked, string withinMarked, ScrollStrategy strategy = ScrollStrategy.Auto, double swipePercentage = 0.67, int swipeSpeed = 500, bool withInertia = true)
		{
			var elementToScroll = FindElement(app, withinMarked);

			app.ScrollDownTo(toMarked, elementToScroll, strategy, swipePercentage, swipeSpeed, withInertia);
		}

		/// <summary>
		/// Scroll down until an element that matches the query is shown on the screen.
		/// </summary>
		/// <param name="app">Represents the main gateway to interact with an app.</param>
		/// <param name="toMarked">Marked selector to select what element to bring on screen.</param>
		/// <param name="query">Represents the query that identify an element by parameters such as type, text it contains or identifier.</param>
		/// <param name="strategy">Strategy for scrolling element.</param>
		/// <param name="swipePercentage">How far across the element to swipe (from 0.0 to 1.0).</param>
		/// <param name="swipeSpeed">The speed of the gesture.</param>
		/// <param name="withInertia">Whether swipes should cause inertia.</param>
		public static void ScrollDownTo(this IApp app, string toMarked, IQuery query, ScrollStrategy strategy = ScrollStrategy.Auto, double swipePercentage = 0.67, int swipeSpeed = 500, bool withInertia = true)
		{
			var elementToScroll = app.FindElement(query);

			app.ScrollDownTo(toMarked, elementToScroll, strategy, swipePercentage, swipeSpeed, withInertia);
		}

		internal static void ScrollDownTo(this IApp app, string toMarked, IUIElement? element, ScrollStrategy strategy = ScrollStrategy.Auto, double swipePercentage = 0.67, int swipeSpeed = 500, bool withInertia = true)
		{
			if (element is not null)
			{
				app.CommandExecutor.Execute("scrollDown", new Dictionary<string, object>
				{
					{ "marked", toMarked },
					{ "element", element },
					{ "strategy", strategy },
					{ "swipePercentage", swipePercentage },
					{ "swipeSpeed", swipeSpeed },
					{ "withInertia", withInertia }
				});
			}
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
			var elementToSwipe = FindElement(app, marked);

			app.ScrollRight(elementToSwipe, strategy, swipePercentage, swipeSpeed, withInertia);
		}

		/// <summary>
		/// Scrolls right on the first element matching query.
		/// </summary>
		/// <param name="app">Represents the main gateway to interact with an app.</param>
		/// <param name="query">Represents the query that identify an element by parameters such as type, text it contains or identifier.</param>
		/// <param name="strategy">Strategy for scrolling element.</param>
		/// <param name="swipePercentage">How far across the element to swipe (from 0.0 to 1.0).</param>
		/// <param name="swipeSpeed">The speed of the gesture.</param>
		/// <param name="withInertia">Whether swipes should cause inertia.</param>
		public static void ScrollRight(this IApp app, IQuery query, ScrollStrategy strategy = ScrollStrategy.Auto, double swipePercentage = 0.67, int swipeSpeed = 500, bool withInertia = true)
		{
			var elementToScroll = app.FindElement(query);

			app.ScrollRight(elementToScroll, strategy, swipePercentage, swipeSpeed, withInertia);
		}

		internal static void ScrollRight(this IApp app, IUIElement? element, ScrollStrategy strategy = ScrollStrategy.Auto, double swipePercentage = 0.67, int swipeSpeed = 500, bool withInertia = true)
		{
			if (element is not null)
			{
				app.CommandExecutor.Execute("scrollRight", new Dictionary<string, object>
				{
					{ "element", element },
					{ "strategy", strategy },
					{ "swipePercentage", swipePercentage },
					{ "swipeSpeed", swipeSpeed },
					{ "withInertia", withInertia }
				});
			}
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
			var elementToScroll = FindElement(app, marked);

			app.ScrollUp(elementToScroll, strategy, swipePercentage, swipeSpeed, withInertia);
		}

		public static void ScrollUp(this IApp app, IQuery query, ScrollStrategy strategy = ScrollStrategy.Auto, double swipePercentage = 0.67, int swipeSpeed = 500, bool withInertia = true)
		{
			var elementToScroll = app.FindElement(query);

			app.ScrollUp(elementToScroll, strategy, swipePercentage, swipeSpeed, withInertia);
		}

		public static void ScrollUp(this IApp app, IUIElement? element, ScrollStrategy strategy = ScrollStrategy.Auto, double swipePercentage = 0.67, int swipeSpeed = 500, bool withInertia = true)
		{
			if (element is not null)
			{
				app.CommandExecutor.Execute("scrollUp", new Dictionary<string, object>
				{
					{ "element", element },
					{ "strategy", strategy },
					{ "swipePercentage", swipePercentage },
					{ "swipeSpeed", swipeSpeed },
					{ "withInertia", withInertia }
				});
			}
		}

		/// <summary>
		/// Scroll up until an element that matches the <paramref name="toMarked"/> is shown on the screen in <paramref name="withinMarked"/>.
		/// </summary>
		/// <param name="app">Represents the main gateway to interact with an app.</param>
		/// <param name="toMarked">Marked selector to select what element to bring on screen.</param>
		/// <param name="withinMarked">Marked selector to select what element to scroll within.</param>
		/// <param name="strategy">Strategy for scrolling element.</param>
		/// <param name="swipePercentage">How far across the element to swipe (from 0.0 to 1.0).</param>
		/// <param name="swipeSpeed">The speed of the gesture.</param>
		/// <param name="withInertia">Whether swipes should cause inertia.</param>
		public static void ScrollUpTo(this IApp app, string toMarked, string withinMarked, ScrollStrategy strategy = ScrollStrategy.Auto, double swipePercentage = 0.67, int swipeSpeed = 500, bool withInertia = true)
		{
			var elementToScroll = FindElement(app, withinMarked);

			app.ScrollUpTo(toMarked, elementToScroll, strategy, swipePercentage, swipeSpeed, withInertia);
		}

		/// <summary>
		/// Scroll up until an element that matches <paramref name="toMarked"/> is shown on the screen.
		/// </summary>
		/// <param name="app">Represents the main gateway to interact with an app.</param>
		/// <param name="toMarked">Marked selector to select what element to bring on screen.</param>
		/// <param name="query">Represents the query that identify an element by parameters such as type, text it contains or identifier.</param>
		/// <param name="strategy">Strategy for scrolling element.</param>
		/// <param name="swipePercentage">How far across the element to swipe (from 0.0 to 1.0).</param>
		/// <param name="swipeSpeed">The speed of the gesture.</param>
		/// <param name="withInertia">Whether swipes should cause inertia.</param>
		public static void ScrollUpTo(this IApp app, string toMarked, IQuery query, ScrollStrategy strategy = ScrollStrategy.Auto, double swipePercentage = 0.67, int swipeSpeed = 500, bool withInertia = true)
		{
			var elementToScroll = app.FindElement(query);

			app.ScrollUpTo(toMarked, elementToScroll, strategy, swipePercentage, swipeSpeed, withInertia);
		}

		internal static void ScrollUpTo(this IApp app, string toMarked, IUIElement? element, ScrollStrategy strategy = ScrollStrategy.Auto, double swipePercentage = 0.67, int swipeSpeed = 500, bool withInertia = true)
		{
			if (element is not null)
			{
				app.CommandExecutor.Execute("scrollUpTo", new Dictionary<string, object>
				{
					{ "marked", toMarked },
					{ "element", element },
					{ "strategy", strategy },
					{ "swipePercentage", swipePercentage },
					{ "swipeSpeed", swipeSpeed },
					{ "withInertia", withInertia }
				});
			}
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
		/// Get the current device orientation.
		/// </summary>
		/// <param name="app">Represents the main gateway to interact with an app.</param>
		/// <returns>The current device orientation</returns>
		public static OpenQA.Selenium.ScreenOrientation GetOrientation(this IApp app)
		{
			var response = app.CommandExecutor.Execute("getOrientation", new Dictionary<string, object>());

			if (response?.Value != null)
			{
				return (OpenQA.Selenium.ScreenOrientation)response.Value;
			}

			throw new InvalidOperationException($"Could not get the current orientation");
		}

		/// <summary>
		/// Get the text of the system clipboard.
		/// </summary>
		/// <param name="app">Represents the main gateway to interact with an app.</param>
		/// <returns>Clipboard content as string or an empty string if the clipboard is empty.</returns>
		public static string GetClipboardText(this IApp app)
		{
			if (app is not AppiumAndroidApp && app is not AppiumIOSApp)
			{
				throw new InvalidOperationException($"GetClipboard is not supported");
			}

			var response = app.CommandExecutor.Execute("getClipboardText", new Dictionary<string, object>());

			if (response?.Value != null)
			{
				return (string)response.Value;
			}

			throw new InvalidOperationException($"Could not get clipboard text");
		}

		/// <summary>
		/// Set the content of the system clipboard.
		/// </summary>
		/// <param name="app">Represents the main gateway to interact with an app.</param>
		/// <param name="content">The actual clipboard content.</param>
		/// <param name="label">Clipboard data label for Android.</param>
		public static void SetClipboardText(this IApp app, string content, string? label = null)
		{
			if (app is not AppiumAndroidApp && app is not AppiumIOSApp)
			{
				throw new InvalidOperationException($"SetClipboard is not supported");
			}

			app.CommandExecutor.Execute("setClipboardText", new Dictionary<string, object>
			{
				{ "content", content },
				{ "label", label! }
			});
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
			if (app is AppiumCatalystApp)
			{
				app.ClickCoordinates(x, y); // // Directly invoke coordinate-based click for AppiumCatalystApp.
			}
			else
			{
				app.CommandExecutor.Execute("tapCoordinates", new Dictionary<string, object>
				{
					{ "x", x },
					{ "y", y }
				});
			}
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
		/// Executes an existing application on the device with additional parameters.
		/// If the application is already running then it will be brought to the foreground.
		/// </summary>
		/// <param name="app">Represents the main gateway to interact with an app.</param>
		/// <param name="parameters">Additional parameters to send with the launch command.</param>
		public static void LaunchApp(this IApp app, string parameters, bool isResetAfterEachTest = false)
		{
			app.CommandExecutor.Execute("launchApp", new Dictionary<string, object>
			{
				{ "testName", parameters },
				{ "isResetAfterEachTest", isResetAfterEachTest }
			});
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
			var element = FindElement(app, marked);

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
			var element = FindElement(app, marked);

			app.SetSliderValue(element, value, minimum, maximum);
		}

		/// <summary>
		/// Sets the value of a slider element that matches query.
		/// </summary>
		/// <param name="app">Represents the main gateway to interact with an app.</param>
		/// <param name="query">Represents the query that identify an element by parameters such as type, text it contains or identifier.</param>
		/// <param name="value">The value to set the Slider to.</param>
		/// <param name="minimum">Te minimum selectable value for the Slider.</param>
		/// <param name="maximum">Te maximum selectable value for the Slider.</param>
		public static void SetSliderValue(this IApp app, IQuery query, double value, double minimum = 0d, double maximum = 1d)
		{
			var element = app.FindElement(query);

			app.SetSliderValue(element, value, minimum, maximum);
		}

		internal static void SetSliderValue(this IApp app, IUIElement? element, double value, double minimum = 0d, double maximum = 1d)
		{
			if (element is not null)
			{
				app.CommandExecutor.Execute("setSliderValue", new Dictionary<string, object>
				{
					{ "element", element },
					{ "value", value },
					{ "minimum", minimum },
					{ "maximum", maximum },
				});
			}
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
		/// Performs a pan gesture between 2 points.
		/// </summary>
		/// <param name="app">Represents the main gateway to interact with an app.</param>
		/// <param name="fromX">The x coordinate to start panning from.</param>
		/// <param name="fromY">The y coordinate to start panning from.</param>
		/// <param name="toX">The x coordinate to pan to.</param>
		/// <param name="toY">The y coordinate to pan to.</param>
		public static void Pan(this IApp app, float fromX, float fromY, float toX, float toY)
		{
			app.DragCoordinates(fromX, fromY, toX, toY);
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
		/// Refresh the current page.
		/// </summary>
		/// <param name="app">Represents the main gateway to interact with an app.</param>
		public static void Refresh(this IApp app)
		{
			app.CommandExecutor.Execute("refresh", ImmutableDictionary<string, object>.Empty);
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
			app.CommandExecutor.Execute("setLightTheme", ImmutableDictionary<string, object>.Empty);
		}

		/// <summary>
		/// Sets dark device's theme
		/// </summary>
		/// <param name="app">Represents the main gateway to interact with an app.</param>
		public static void SetDarkTheme(this IApp app)
		{
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
			if (app is not AppiumAndroidApp && app is not AppiumIOSApp)
			{
				throw new InvalidOperationException($"Lock is only supported on AppiumAndroidApp and AppiumIOSApp");
			}

			app.CommandExecutor.Execute("lock", ImmutableDictionary<string, object>.Empty);
		}

		/// <summary>
		/// Unlock the screen.
		/// Functionality that's only available on Android and iOS.
		/// </summary>
		/// <param name="app">Represents the main gateway to interact with an app.</param>
		/// <param name="unlockType">This capability supports the following possible values: pin, pinWithKeyEvent, password, pattern.</param>
		/// <param name="unlockKey">a valid pin (digits in range 0-9), password (latin characters) or pattern (treat the pattern pins similarly to numbers on a digital phone dial).</param>
		/// <exception cref="InvalidOperationException">Unlock is only supported on <see cref="AppiumAndroidApp"/>.</exception>
		public static void Unlock(this IApp app, string unlockType = "", string unlockKey = "")
		{
			if (app is not AppiumAndroidApp && app is not AppiumIOSApp)
			{
				throw new InvalidOperationException($"Unlock is only supported on AppiumAndroidApp and AppiumIOSApp");
			}

			app.CommandExecutor.Execute("unlock", new Dictionary<string, object>()
			{
				{ "unlockType", unlockType },
				{ "unlockKey", unlockKey },
			});
		}

		/// <summary>
		/// Check whether the device is locked or not.
		/// </summary>
		/// <param name="app">Represents the main gateway to interact with an app.</param>
		public static bool IsLocked(this IApp app)
		{
			if (app is not AppiumAndroidApp && app is not AppiumIOSApp)
			{
				throw new InvalidOperationException($"IsLocked is only supported on AppiumAndroidApp and AppiumIOSApp");
			}
			var response = app.CommandExecutor.Execute("isLocked", new Dictionary<string, object>());

			var responseValue = response?.Value ?? false;

			return (bool)responseValue;
		}

		/// <summary>
		/// Perform a shake action on the device.
		/// </summary>
		/// <param name="app">Represents the main gateway to interact with an app.</param>
		public static void Shake(this IApp app)
		{
			if (app is not AppiumAndroidApp)
			{
				throw new InvalidOperationException($"Shake is only supported on AppiumAndroidApp");
			}

			app.CommandExecutor.Execute("shake", ImmutableDictionary<string, object>.Empty);
		}

		/// <summary>
		/// Triggers the SwipeBackNavigation, simulating the default swipe-back navigation.
		/// </summary>
		/// <param name="app">Represents the main gateway to interact with an app.</param>
		/// /// <exception cref="InvalidOperationException">SwipeBackNavigation is only supported on <see cref="AppiumIOSApp"/> and <see cref="AppiumAndroidApp"/>.</exception>
		public static void SwipeBackNavigation(this IApp app)
		{
			if (app is not AppiumIOSApp && app is not AppiumAndroidApp)
			{
				throw new InvalidOperationException($"Interactive Pop Gesture is only supported on AppiumIOSAppp and AppiumAndroidApp");
			}

			if (app is AppiumIOSApp)
			{
				app.CommandExecutor.Execute("interactivePopGesture", ImmutableDictionary<string, object>.Empty);
			}
			else if (app is AppiumAndroidApp)
			{
				var response = app.CommandExecutor.Execute("checkIfGestureNavigationIsEnabled", new Dictionary<string, object>());
				if (response?.Value is bool gestureNavigationIsEnabled && gestureNavigationIsEnabled)
					SwipeLeftToRight(app);
				else
					Back(app);
			}
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
		/// Switch the System animations state.
		/// Optimize and accelerate tests, eliminating animations entirely when Appium is executing tests, as they serve no practical purpose in this context.
		/// Functionality that's only available on Android and Catalyst.
		/// </summary>
		/// <param name="app">Represents the main gateway to interact with an app.</param>
		/// <param name="enableSystemAnimations">Enable/disable the system animations.</param>
		/// <exception cref="InvalidOperationException">ToggleSystemAnimations is only supported on <see cref="AppiumAndroidApp"/> and <see cref="AppiumCatalystApp"/>.</exception>
		public static void ToggleSystemAnimations(this IApp app, bool enableSystemAnimations)
		{
			if (app is not AppiumAndroidApp && app is not AppiumCatalystApp)
			{
				throw new InvalidOperationException($"ToggleSystemAnimations is not supported");
			}

			app.CommandExecutor.Execute("toggleSystemAnimations", new Dictionary<string, object>()
			{
				{ "enableSystemAnimations", enableSystemAnimations },
			});
		}

		/// <summary>
		/// Switch the state of data service.
		/// Functionality that's only available on Android.
		/// This API does not work for Android API level 21+ because it requires system or carrier privileged permission, 
		/// and Android <= 21 does not support granting permissions.
		/// </summary>
		/// <param name="app">Represents the main gateway to interact with an app.</param>
		/// <exception cref="InvalidOperationException">ToggleData is only supported on <see cref="AppiumAndroidApp"/>.</exception>
		public static void ToggleData(this IApp app)
		{
			if (app is not AppiumAndroidApp)
			{
				throw new InvalidOperationException($"ToggleData is only supported on AppiumAndroidApp");
			}

			app.CommandExecutor.Execute("toggleData", ImmutableDictionary<string, object>.Empty);
		}

		/// <summary>
		/// Gets the information of the system state which is supported to read as like cpu, memory, network traffic, and battery.
		/// Functionality that's only available on Android.
		/// </summary>
		/// <param name="app">Represents the main gateway to interact with an app.</param>
		/// <param name="performanceDataType">The available performance data types(cpuinfo | batteryinfo | networkinfo | memoryinfo).</param>
		/// <exception cref="InvalidOperationException">GetPerformanceData is only supported on <see cref="AppiumAndroidApp"/>.</exception>
		/// <returns>The information of the system related to the performance.</returns>
		public static IList<object> GetPerformanceData(this IApp app, string performanceDataType)
		{
			if (app is not AppiumAndroidApp)
			{
				throw new InvalidOperationException($"GetPerformanceData is only supported on AppiumAndroidApp");
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

		/// <summary>
		/// Gets the information of the system state regarding memory.
		/// Functionality that's only available on Android.
		/// </summary>
		/// <param name="app">Represents the main gateway to interact with an app.</param>
		/// <returns>The information of the system related to the memory performance.</returns>
		/// <exception cref="InvalidOperationException">GetPerformanceData is only supported on <see cref="AppiumAndroidApp"/>.</exception>
		public static IReadOnlyDictionary<string, int> GetPerformanceMemoryInfo(this IApp app)
		{
			var performanceData = GetPerformanceData(app, "memoryinfo");
			var countersTitles = (object?[])performanceData[0];
			var countersStrings = (object?[])performanceData[1];
			var data = countersTitles.Zip(countersStrings)
				.Where(x => x is { First: string, Second: string })
				.ToDictionary(x => (string)x.First!, x => int.TryParse((string)x.Second!, out var value) ? value : 0);
			return data;
		}

		/// <summary>
		/// Maximize the active App window.
		/// </summary>
		/// <param name="app">Represents the main gateway to interact with an app.</param>
		public static void EnterFullScreen(this IApp app)
		{
			if (app is not AppiumCatalystApp)
			{
				throw new InvalidOperationException($"EnterFullScreen is only supported on AppiumCatalystApp");
			}

			app.CommandExecutor.Execute("enterFullScreen", new Dictionary<string, object>());
		}

		/// <summary>
		/// Leave the App full screen mode.
		/// </summary>
		/// <param name="app">Represents the main gateway to interact with an app.</param>
		public static void ExitFullScreen(this IApp app)
		{
			if (app is not AppiumCatalystApp)
			{
				throw new InvalidOperationException($"ExitFullScreen is only supported on AppiumCatalystApp");
			}

			app.CommandExecutor.Execute("exitFullScreen", new Dictionary<string, object>());
		}

		/// <summary>
		/// Print in the Output the current application hierarchy XML (app).
		/// </summary>
		/// <param name="app">Represents the main gateway to interact with an app.</param>
		public static void PrintTree(this IApp app)
		{
			if (app is not AppiumApp aaa)
			{
				throw new InvalidOperationException($"PrintTree is only supported on AppiumApp");
			}

			var pageSource = aaa.Driver.PageSource;
			Console.WriteLine(pageSource);
		}

		/// <summary>
		/// Retrieve visibility and bounds information of the status and navigation bars
		/// </summary>
		/// <param name="app">Represents the main gateway to interact with an app.</param>
		/// <returns>Information about visibility and bounds of status and navigation bar.</returns>
		public static IDictionary<string, object> GetSystemBars(this IApp app)
		{
			if (app is not AppiumAndroidApp)
			{
				throw new InvalidOperationException($"GetSystemBars is only supported on AppiumAndroidApp");
			}

			var response = app.CommandExecutor.Execute("getSystemBars", new Dictionary<string, object>());

			if (response?.Value != null)
			{
				return (IDictionary<string, object>)response.Value;
			}

			throw new InvalidOperationException($"Could not get the Android System Bars");
		}

		/// <summary>
		/// Navigates back in the application by simulating a tap on the platform-specific back navigation button or using a custom identifier.
		/// </summary>
		/// <param name="app">The IApp instance representing the main gateway to interact with the application.</param>
		/// <param name="customBackButtonIdentifier">Optional custom identifier string for the back button. If not provided, the default back arrow query will be used.</param>
		public static void TapBackArrow(this IApp app, string customBackButtonIdentifier = "")
		{
			var query = string.IsNullOrEmpty(customBackButtonIdentifier)
				? GetDefaultBackArrowQuery(app)
				: GetCustomBackArrowQuery(app, customBackButtonIdentifier);

			TapBackArrow(app, query);
		}

		/// <summary>
		/// Navigates back in the application using a custom IQuery.
		/// </summary>
		/// <param name="app">The IApp instance representing the main gateway to interact with the application.</param>
		/// <param name="query">The custom IQuery for the back button.</param>
		public static void TapBackArrow(this IApp app, IQuery query)
		{
			app.WaitForElement(query).Tap();
		}

		/// <summary>
		/// Taps a button in a display alert dialog.
		/// For AppiumCatalystApp, it uses specific element identifiers to locate and tap the alert button.
		/// For other app types, it locates and taps the button using the provided text.
		/// </summary>
		/// <param name="app">The IApp instance representing the application.</param>
		/// <param name="text">The text of the button to tap in the display alert (used for non-AppiumCatalystApp instances).</param>
		/// <param name="buttonIndex">
		/// The index of the button in the alert dialog, used to generate the correct element identifier.
		/// For example, in a alert with two buttons:
		/// - 0 (default) corresponds to the leftmost button (e.g., "OK" with identifier ending in 999)
		/// - 1 corresponds to the button to its right (e.g., "Cancel" with identifier ending in 998)
		/// </param>
		public static void TapDisplayAlertButton(this IApp app, string text, int buttonIndex = 0)
		{
			if (app is AppiumCatalystApp)
			{
				app.WaitForElement(AppiumQuery.ById($"action-button--{999 - buttonIndex}"));
				app.Tap(AppiumQuery.ById($"action-button--{999 - buttonIndex}"));
			}
			else
			{
				app.WaitForElement(text);
				app.Tap(text);
			}
		}

		/// <summary>
		/// Gets the default query for the back arrow button based on the app type.
		/// </summary>
		/// <param name="app">The IApp instance representing the application.</param>
		/// <returns>An IQuery for the default back arrow button.</returns>
		/// <exception cref="ArgumentException">Thrown when an unsupported app type is provided.</exception>
		static IQuery GetDefaultBackArrowQuery(IApp app)
		{
			return app switch
			{
				AppiumAndroidApp _ => AppiumQuery.ByXPath("//android.widget.ImageButton[@content-desc='Navigate up']"),
				AppiumIOSApp _ => AppiumQuery.ByAccessibilityId("Back"),
				AppiumCatalystApp _ => AppiumQuery.ByAccessibilityId("Back"),
				AppiumWindowsApp _ => AppiumQuery.ByAccessibilityId("NavigationViewBackButton"),
				_ => throw new ArgumentException("Unsupported app type", nameof(app))
			};
		}

		/// <summary>
		/// Gets a custom query for the back arrow button based on the app type and a custom identifier.
		/// Note that for Windows apps, the back button is not customizable, so the default identifier is used.
		/// </summary>
		/// <param name="app">The IApp instance representing the application.</param>
		/// <param name="customBackButtonIdentifier">The custom identifier for the back button.</param>
		/// <returns>An IQuery for the custom back arrow button.</returns>
		/// <exception cref="ArgumentException">Thrown when an unsupported app type is provided.</exception>
		static IQuery GetCustomBackArrowQuery(IApp app, string customBackButtonIdentifier)
		{
			return app switch
			{
				AppiumAndroidApp _ => AppiumQuery.ByXPath($"//android.widget.ImageButton[@content-desc='{customBackButtonIdentifier}']"),
				AppiumIOSApp _ => AppiumQuery.ByXPath($"//XCUIElementTypeButton[@name='{customBackButtonIdentifier}']"),
				AppiumCatalystApp _ => AppiumQuery.ByName(customBackButtonIdentifier),
				AppiumWindowsApp _ => AppiumQuery.ByAccessibilityId("NavigationViewBackButton"),
				_ => throw new ArgumentException("Unsupported app type", nameof(app))
			};
		}

		/// <summary>
		/// Waits for an element to be ready until page navigation has settled, with additional waiting for MacCatalyst.
		/// This method helps prevent null reference exceptions during page transitions, especially in MacCatalyst.
		/// </summary>
		/// <param name="app">The IApp instance.</param>
		/// <param name="elementId">The id of the element to wait for.</param>
		/// <param name="timeout">Optional timeout for the wait operation. Default is null, which uses the default timeout.</param>
		public static IUIElement WaitForElementTillPageNavigationSettled(this IApp app, string elementId, TimeSpan? timeout = null)
		{
			if (app is AppiumCatalystApp)
				app.WaitForElement(AppiumQuery.ById(elementId), timeout: timeout);

			return app.WaitForElement(elementId, timeout: timeout);
		}

		/// <summary>
		/// Waits for an element to be ready until page navigation has settled, with additional waiting for MacCatalyst.
		/// This method helps prevent null reference exceptions during page transitions, especially in MacCatalyst.
		/// </summary>
		/// <param name="app">The IApp instance.</param>
		/// <param name="query">The query to use for finding the element.</param>
		/// <param name="timeout">Optional timeout for the wait operation. Default is null, which uses the default timeout.</param>
		public static void WaitForElementTillPageNavigationSettled(this IApp app, IQuery query, TimeSpan? timeout = null)
		{
			if (app is AppiumCatalystApp)
				app.WaitForElement(query, timeout: timeout);

			app.WaitForElement(query, timeout: timeout);
		}

		/// <summary>
		/// Waits for the flyout icon to appear in the app.
		/// </summary>
		/// <param name="app">The IApp instance representing the application.</param>
		/// <param name="automationId">The automation ID of the flyout icon (default is an empty string).</param>
		/// <param name="isShell">Indicates whether the app is using Shell navigation (default is true).</param>
		public static void WaitForFlyoutIcon(this IApp app, string automationId = "", bool isShell = true)
		{
			if (app is AppiumAndroidApp)
			{
				app.WaitForElement(AppiumQuery.ByXPath("//android.widget.ImageButton[@content-desc=\"Open navigation drawer\"]"));
			}
			else if (app is AppiumIOSApp || app is AppiumCatalystApp || app is AppiumWindowsApp)
			{
				if (isShell)
				{
					app.WaitForElement("OK");
				}
				if (!isShell)
				{
					if (app is AppiumWindowsApp)
					{
						app.WaitForElement(AppiumQuery.ByAccessibilityId("TogglePaneButton"));
					}
					else
					{
						app.WaitForElement(automationId);
					}
				}
			}
		}

		/// <summary>
		/// Waits for the flyout icon to disappear in the app.
		/// </summary>
		/// <param name="app">The IApp instance representing the application.</param>
		/// <param name="automationId">The automation ID of the flyout icon (default is an empty string).</param>
		/// <param name="isShell">Indicates whether the app is using Shell navigation (default is true).</param>
		public static void WaitForNoFlyoutIcon(this IApp app, string automationId = "", bool isShell = true)
		{
			if (app is AppiumAndroidApp)
			{
				app.WaitForNoElement(AppiumQuery.ByXPath("//android.widget.ImageButton[@content-desc=\"Open navigation drawer\"]"));
			}
			else if (app is AppiumIOSApp || app is AppiumCatalystApp || app is AppiumWindowsApp)
			{
				if (isShell)
				{
					app.WaitForNoElement("OK");
				}
				if (!isShell)
				{
					if (app is AppiumWindowsApp)
					{
						app.WaitForNoElement(AppiumQuery.ByAccessibilityId("TogglePaneButton"));
					}
					else
					{
						app.WaitForNoElement(automationId);
					}
				}
			}
		}

		/// <summary>
		/// Shows the flyout menu in the app.
		/// </summary>
		/// <param name="app">The IApp instance representing the application.</param>
		/// <param name="automationId">The automation ID of the flyout icon (default is an empty string).</param>
		/// <param name="usingSwipe">Indicates whether to use swipe gesture to open the flyout (default is false).</param>
		/// <param name="waitForFlyoutIcon">Indicates whether to wait for the flyout icon before showing the flyout (default is true).</param>
		/// <param name="isShell">Indicates whether the app is using Shell navigation (default is true).</param>
		public static void ShowFlyout(this IApp app, string automationId = "", bool usingSwipe = false, bool waitForFlyoutIcon = true, bool isShell = true)
		{
			if (waitForFlyoutIcon)
			{
				app.WaitForFlyoutIcon(automationId, isShell);
			}

			if (usingSwipe)
			{
				app.DragCoordinates(5, 500, 800, 500);
			}
			else
			{
				app.TapFlyoutIcon(automationId, isShell, false);
			}
		}

		/// <summary>
		/// Taps the Flyout icon for Shell or FlyoutPage.
		/// </summary>
		/// <param name="app">Represents the main gateway to interact with an app.</param>
		/// <param name="title">Optional title for FlyoutPage (default is empty string).</param>
		/// <param name="isShell">Indicates whether the Flyout is for Shell (true) or FlyoutPage (false).</param>
		private static void TapFlyoutIcon(this IApp app, string title = "", bool isShell = true, bool waitForFlyoutIcon = true)
		{
			if (waitForFlyoutIcon)
			{
				app.WaitForFlyoutIcon(title, isShell);
			}
			if (app is AppiumAndroidApp)
			{
				app.Tap(AppiumQuery.ByXPath("//android.widget.ImageButton[@content-desc=\"Open navigation drawer\"]"));
			}
			else if (app is AppiumIOSApp || app is AppiumCatalystApp || app is AppiumWindowsApp)
			{
				if (isShell)
				{
					app.Tap(AppiumQuery.ByAccessibilityId("OK"));
				}
				else
				{
					if (app is AppiumWindowsApp)
					{
						app.Tap(AppiumQuery.ByAccessibilityId("TogglePaneButton"));
					}
					else
					{
						app.Tap(title);
					}
				}
			}
		}

		/// <summary>
		/// Taps the Flyout icon for Shell pages.
		/// </summary>
		/// <param name="app">Represents the main gateway to interact with an app.</param>
		public static void TapShellFlyoutIcon(this IApp app)
		{
			app.TapFlyoutIcon();
		}

		/// <summary>
		/// Taps the Flyout icon for FlyoutPage.
		/// </summary>
		/// <param name="app">Represents the main gateway to interact with an app.</param>
		/// <param name="title">Optional title for FlyoutPage (default is empty string).</param>
		public static void TapFlyoutPageIcon(this IApp app, string title = "")
		{
			app.TapFlyoutIcon(title, false);
		}

		/// <summary>
		/// Taps an item in the specified flyout menu.
		/// </summary>
		/// <param name="app">The IApp instance representing the application.</param>
		/// <param name="flyoutItem">The text or accessibility identifier of the flyout item to tap.</param>
		/// <param name="isShellFlyout">True if it's a Shell flyout, false for FlyoutPage flyout.</param>
		private static void TapInFlyout(this IApp app, string flyoutItem, bool isShellFlyout)
		{
			if (isShellFlyout)
			{
				app.TapShellFlyoutIcon();
			}
			else
			{
				app.TapFlyoutPageIcon();
			}

			app.WaitForElement(flyoutItem);
			app.Tap(flyoutItem);
		}

		/// <summary>
		/// Taps an item in the Shell flyout menu.
		/// </summary>
		/// <param name="app">The IApp instance representing the application.</param>
		/// <param name="flyoutItem">The text or accessibility identifier of the flyout item to tap.</param>
		public static void TapInShellFlyout(this IApp app, string flyoutItem)
		{
			app.TapInFlyout(flyoutItem, true);
		}

		/// <summary>
		/// Taps an item in the FlyoutPage flyout menu.
		/// </summary>
		/// <param name="app">The IApp instance representing the application.</param>
		/// <param name="flyoutItem">The text or accessibility identifier of the flyout item to tap.</param>
		public static void TapInFlyoutPageFlyout(this IApp app, string flyoutItem)
		{
			app.TapInFlyout(flyoutItem, false);
		}


		/// <summary>
		/// Toggles the visibility of secondary toolbar items.
		/// </summary>
		/// <param name="app">Represents the main gateway to interact with an app.</param>
		public static void ToggleSecondaryToolbarItems(this IApp app)
		{
			if (app is not AppiumAndroidApp && app is not AppiumWindowsApp)
			{
				throw new InvalidOperationException($"ToggleSecondaryToolbarItems is not supported");
			}

			app.CommandExecutor.Execute("toggleSecondaryToolbarItems", ImmutableDictionary<string, object>.Empty);
		}

		/// <summary>
		/// Waits for the "More" button in the app, with platform-specific logic for Android and Windows.
		/// This method does not currently support iOS and macOS platforms, where the "More" button is not shown.
		/// </summary>
		/// <param name="app">Represents the main gateway to interact with an app.</param>
		public static void WaitForMoreButton(this IApp app)
		{
			if (app is AppiumAndroidApp)
			{
				app.WaitForElement(AppiumQuery.ByXPath("//android.widget.ImageView[@content-desc=\"More options\"]"));
			}
			else if (app is AppiumWindowsApp)
			{
				app.WaitForElement(AppiumQuery.ByAccessibilityId("MoreButton"));
			}
			else if (app is AppiumIOSApp || app is AppiumCatalystApp)
			{
				app.WaitForElement("SecondaryToolbarMenuButton");
			}
			else
			{
				throw new InvalidOperationException($"WaitForMoreButton is not supported on this platform.");
			}
		}

		/// <summary>
		/// Taps the "More" button in the app, with platform-specific logic for Android and Windows.
		/// This method does not currently support iOS and macOS platforms, where the "More" button is not shown.
		/// </summary>
		/// <param name="app">Represents the main gateway to interact with an app.</param>
		public static void TapMoreButton(this IApp app)
		{
			if (app is AppiumAndroidApp)
			{
				app.Tap(AppiumQuery.ByXPath("//android.widget.ImageView[@content-desc=\"More options\"]"));
			}
			else if (app is AppiumWindowsApp)
			{
				app.Tap(AppiumQuery.ByAccessibilityId("MoreButton"));
			}
			else if (app is AppiumIOSApp || app is AppiumCatalystApp)
			{
				app.Tap("SecondaryToolbarMenuButton");
			}
		}

		/// <summary>
		/// Taps a tab in the application.
		/// </summary>
		/// <param name="app">The IApp instance representing the application.</param>
		/// <param name="tabName">The name of the tab to tap.</param>
		/// <param name="isTopTab">Indicates whether the tab is a top tab (default is false).</param>
		/// <remarks>
		/// This method handles platform-specific behaviors:
		/// - For Android, it converts the tab name to uppercase.
		/// - For Windows, if it's a top tab, it taps the navigation view item first.
		/// The method waits for the tab element to be available before tapping it.
		/// </remarks>
		public static void TapTab(this IApp app, string tabName, bool isTopTab = false)
		{
			tabName = app is AppiumAndroidApp ? tabName.ToUpperInvariant() : tabName;

			if (isTopTab && app is AppiumWindowsApp)
			{
				app.WaitForElement("navViewItem");
				app.Tap("navViewItem");
			}

			app.WaitForElementTillPageNavigationSettled(tabName);
			app.Tap(tabName);
		}

		/// <summary>
		/// Waits for a tab element with the specified name to appear and for page navigation to settle.
		/// </summary>
		/// <param name="app">The IApp instance.</param>
		/// <param name="tabName">The name of the tab to wait for.</param>
		/// <remarks>
		/// For Android apps, the tab name is converted to uppercase before searching.
		/// </remarks>
		public static IUIElement WaitForTabElement(this IApp app, string tabName)
		{
			tabName = app is AppiumAndroidApp ? tabName.ToUpperInvariant() : tabName;
			return app.WaitForElementTillPageNavigationSettled(tabName);
		}

		/// <summary>
		/// Activates the context menu for the specified element.
		/// </summary>
		/// <param name="app">Represents the main gateway to interact with an app.</param>
		/// <param name="marked">Marked selector of the Slider element to update.</param>
		public static void ActivateContextMenu(this IApp app, string marked)
		{
			var element = FindElement(app, marked);
			app.CommandExecutor.Execute("activateContextMenu", new Dictionary<string, object>
 			{
 				{ "element", element },
 			});
		}

		/// <summary>
		/// Dismisses the context menu.
		/// </summary>
		/// <param name="app">Represents the main gateway to interact with an app.</param>
		public static void DismissContextMenu(this IApp app)
		{
			app.CommandExecutor.Execute("dismissContextMenu", new Dictionary<string, object>());
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

		static IUIElement FindElement(IApp app, string element)
		{
			var result = app.FindElement(element);

			if (result is null)
				result = app.FindElementByText(element);

			return result;
		}

		static IUIElement FindAnyElement(IApp app, string[] elements)
		{
			foreach (var element in elements)
			{
				if (FindElement(app, element) is IUIElement result)
					return result;
			}

			throw new InvalidOperationException($"Did not find any elements in the list: {string.Join(", ", elements)}");
		}

		static IReadOnlyCollection<IUIElement> FindElements(IApp app, string element)
		{
			var result = app.FindElements(element);

			if (result is null)
				result = app.FindElementsByText(element);

			return result;
		}


		public static void SetTestConfigurationArg(this IConfig config, string key, string value)
		{
			var startupArg = config.GetProperty<Dictionary<string, string>>("TestConfigurationArgs") ?? new Dictionary<string, string>();
			startupArg.Add(key, value);
			config.SetProperty("TestConfigurationArgs", startupArg);
		}

		/// <summary>
		/// Gets the search handler element for the shell.
		/// This method is used to find the search handler element in the app.
		/// It uses different queries based on the app type (Android, iOS, Catalyst, or Windows).
		/// </summary>
		/// <param name="app">The IApp instance representing the application.</param>
		/// <returns>The search handler element for the shell.</returns>
		public static IUIElement GetShellSearchHandler(this IApp app)
		{
			IUIElement? element = null;

			if (app is AppiumAndroidApp)
			{
				element = app.WaitForElement(AppiumQuery.ByXPath("//android.widget.EditText"));
			}
			else if (app is AppiumIOSApp || app is AppiumCatalystApp)
			{
				element = app.WaitForElement(AppiumQuery.ByXPath("//XCUIElementTypeSearchField"));
			}
			else if (app is AppiumWindowsApp)
			{
				element = app.WaitForElement("TextBox");
			}

			// Ensure the element is not null before returning
			if (element is null)
			{
				throw new InvalidOperationException("SearchHandler element not found.");
			}

			return element;
		}

		/// <summary>
		/// Taps an element and retries until another element appears and is ready for interaction.
		/// Sometimes elements may appear but are not yet ready for interaction; this helper method retries the tap until the target element is interactable or the retry limit is reached.
		/// </summary>
		/// <param name="app">The app instance</param>
		/// <param name="elementToTap">The element to tap</param>
		/// <param name="elementToWaitFor">The element to wait for after tapping</param>
		/// <param name="maxRetries">Maximum number of retry attempts</param>
		/// <param name="retryDelayMs">Delay between retries in milliseconds</param>
		/// <returns>True if the target element appeared and is ready, false otherwise</returns>
		public static bool TapWithRetriesUntilElementReady(this IApp app, string elementToTap, string elementToWaitFor,
			int maxRetries = 5, int retryDelayMs = 500)
		{
			// Initial tap
			app.Tap(elementToTap);

			for (int retry = 0; retry < maxRetries - 1; retry++)
			{
				// Check if target element is visible
				if (IsElementVisible(app, elementToWaitFor))
					return true;

				// Element not found, wait and tap again
				System.Threading.Thread.Sleep(retryDelayMs);
				app.Tap(elementToTap);
			}

			// Final check
			return IsElementVisible(app, elementToWaitFor);
		}

		/// <summary>
		/// Determines whether a UI element with the specified name is currently visible in the app.
		/// </summary>
		/// <param name="app">The IApp instance representing the application.</param>
		/// <param name="elementName">The name or identifier of the element to check for visibility.</param>
		/// <returns>True if the element is visible; otherwise, false.</returns>
		public static bool IsElementVisible(IApp app, string elementName)
		{
			try
			{
				app.WaitForElement(elementName);
				return true;
			}
			catch (TimeoutException)
			{
				return false;
			}
		}
	}
}