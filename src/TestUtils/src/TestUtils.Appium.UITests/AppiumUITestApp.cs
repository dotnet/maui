using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.Reflection;
using System.Text.RegularExpressions;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Enums;
using OpenQA.Selenium.Appium.Interactions;
using OpenQA.Selenium.Appium.MultiTouch;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;
using Xamarin.UITest;
using Xamarin.UITest.Queries;
using Xamarin.UITest.Queries.Tokens;

using PointerInputDevice = OpenQA.Selenium.Appium.Interactions.PointerInputDevice;

namespace TestUtils.Appium.UITests
{
	public class AppiumUITestApp : IApp2
	{
		readonly string _appId;
		AppiumDriver? _driver;
		AppiumElement? _window;

		public bool IsAndroid => Platform.Equals("Android", StringComparison.OrdinalIgnoreCase);
		public bool IsWindows => Platform.Equals("Windows", StringComparison.OrdinalIgnoreCase);
		public bool IsiOS => Platform.Equals("iOS", StringComparison.OrdinalIgnoreCase);
		public bool IsMac => Platform.Equals("mac", StringComparison.OrdinalIgnoreCase);
		public string Platform => _driver?.Capabilities.GetCapability(MobileCapabilityType.PlatformName).ToString() ?? "";

		public PointerKind PointerType => IsMac ? PointerKind.Mouse : PointerKind.Touch;

		public static TimeSpan DefaultTimeout = TimeSpan.FromSeconds(15);

		// Using the default value from https://android.googlesource.com/platform/frameworks/base/+/master/core/java/android/view/ViewConfiguration.java#129
		// and shaving off 50ms so we come in under the threshold
		// iOS and Mac use a different way of simulating double taps, so no need for a variation of this constant for those platforms
		// The Windows default is 500 ms (https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-setdoubleclicktime?redirectedfrom=MSDN#parameters)
		// so this delay should be short enough to simulate a double-click on that platform.
		const int DOUBLE_TAP_DELAY_MS = 250;

		readonly Dictionary<string, string> _controlNameToTag = new Dictionary<string, string>
		{
			{ "button", "ControlType.Button" }
		};

		readonly Dictionary<string, string> _translatePropertyAccessor = new Dictionary<string, string>
		{
			{ "getAlpha", "Opacity" },
			{ "isEnabled", "IsEnabled" },
		};

		public AppiumUITestApp(string appId, AppiumDriver? driver)
		{
			if (driver == null)
				throw new ArgumentNullException(nameof(driver));

			_appId = appId;
			_driver = driver;
		}

		static AppResult ToAppResult(AppiumElement appiumElement)
		{
			return new AppResult
			{
				Rect = ToAppRect(appiumElement),
				Label = appiumElement.Id, // Not entirely sure about this one
				Text = appiumElement.Text,
				Description = appiumElement.Text, // or this one
				Enabled = appiumElement.Enabled,
				Id = appiumElement.Id
			};
		}

		static AppRect? ToAppRect(AppiumElement appiumElement)
		{
			try
			{
				var result = new AppRect
				{
					X = appiumElement.Location.X,
					Y = appiumElement.Location.Y,
					Height = appiumElement.Size.Height,
					Width = appiumElement.Size.Width
				};

				result.CenterX = result.X + result.Width / 2;
				result.CenterY = result.Y + result.Height / 2;

				return result;
			}
			catch (Exception ex)
			{
				Debug.WriteLine(
					$"Warning: error determining AppRect for {appiumElement}; "
					+ $"if this is a Label with a modified Text value, it might be getting confused. " +
					$"{ex}");
			}

			return null;
		}

		public AppPrintHelper Print => throw new NotImplementedException();

		public IDevice Device => throw new NotImplementedException();

		public ITestServer TestServer => throw new NotImplementedException();

		public string ElementTree => _driver?.PageSource ?? "";

		public ApplicationState AppState
		{
			get
			{
				return IsWindows
					? GetWindowsAppState()
					: IsAndroid
						? GetUIAutomator2TestAppState()
						: GetXCUITestAppState();
			}
		}

		public AppiumDriver? Driver => _driver;

		public void ResetApp()
		{
			_driver?.ResetApp();
		}

		public void ActivateApp()
		{
			_driver?.ActivateApp(_appId);
		}

		public void CloseApp()
		{
			_driver?.CloseApp();
		}

		public void Dispose()
		{
			if (_driver != null)
			{
				_driver.Quit();
				_driver.Dispose();
				_driver = null;
			}
		}

		public void Back()
		{
			if (IsAndroid)
			{
				var queryBy = MobileBy.AccessibilityId("Navigate up");
				_driver?.FindElements(queryBy).First().Click();
			}
			else if (IsMac || IsiOS)
			{
				// Get the first NavigationBar we can find and the first button in it (the back button), index starts at 1
				var queryBy = MobileBy.IosClassChain("**/XCUIElementTypeNavigationBar/XCUIElementTypeButton[1]");
				_driver?.FindElements(queryBy).First().Click();
			}
			else
			{
				QueryAppium("NavigationViewBackButton", true).First().Click();
			}
		}

		public void ClearText(Func<AppQuery, AppQuery> query)
		{
			var result = QueryAppium(query, true).First();
			result.Clear();
		}

		public void ClearText(Func<AppQuery, AppWebQuery> query)
		{
			throw new NotImplementedException();
		}

		public void ClearText(string marked)
		{
			var result = QueryAppium(marked, true).First();
			result.Clear();
		}

		public void ClearText()
		{
			throw new NotImplementedException();
		}

		public bool IsKeyboardShown() =>
			!IsWindows && !IsMac && _driver?.IsKeyboardShown() == true;

		public void DismissKeyboard()
		{
			if (!IsWindows && !IsMac)
			{
				if (_driver?.IsKeyboardShown() == true)
				{
					if (IsiOS)
					{
						try
						{
							_driver.HideKeyboard("return");
						}
						catch (InvalidElementStateException)
						{
							// Appium iOS driver does not have a consistent way to dismiss the keyboard
						}
					}
					else
					{
						_driver.HideKeyboard();
					}
				}
			}
		}

		public void DoubleTap(Func<AppQuery, AppQuery> query)
		{
			var result = QueryAppium(query, true).First();
			DoubleTap(result);
		}

		public void DoubleTap(string marked)
		{
			var result = QueryAppium(marked, true).First();
			DoubleTap(result);
		}

		private void DoubleTap(AppiumElement element)
		{
			if (IsiOS)
			{
				_driver?.ExecuteScript("mobile: doubleTap", new Dictionary<string, object>
				{
					{ "elementId", element.Id },
				});

			}
			else if (IsMac)
			{
				_driver?.ExecuteScript("macos: doubleClick", new Dictionary<string, object>
				{
					{ "elementId", element.Id },
				});
			}
			else
			{
				PointerInputDevice touchDevice = new PointerInputDevice(PointerType);
				ActionSequence sequence = new ActionSequence(touchDevice, 0);
				sequence.AddAction(touchDevice.CreatePointerMove(element, 0, 0, TimeSpan.FromMilliseconds(5)));
				AddDoubleTap(touchDevice, sequence);
				_driver?.PerformActions(new List<ActionSequence> { sequence });
			}
		}

		public void DoubleTapCoordinates(float x, float y)
		{
			if (IsiOS)
			{
				_driver?.ExecuteScript("mobile: doubleTap", new Dictionary<string, object>
				{
					{ "x", x },
					{ "y", y }
				});

			}
			else if (IsMac)
			{
				_driver?.ExecuteScript("macos: doubleClick", new Dictionary<string, object>
				{
					{ "x", x },
					{ "y", y }
				});
			}
			else
			{
				PointerInputDevice touchDevice = new PointerInputDevice(PointerType);
				ActionSequence sequence = new ActionSequence(touchDevice, 0);
				AddDoubleTap(touchDevice, sequence);
				_driver?.PerformActions(new List<ActionSequence> { sequence });
			}
		}

		static ActionSequence AddDoubleTap(PointerInputDevice touchDevice, ActionSequence sequence)
		{
			sequence.AddAction(touchDevice.CreatePointerDown(PointerButton.TouchContact));
			sequence.AddAction(touchDevice.CreatePointerUp(PointerButton.TouchContact));
			sequence.AddAction(touchDevice.CreatePause(TimeSpan.FromMilliseconds(DOUBLE_TAP_DELAY_MS)));
			sequence.AddAction(touchDevice.CreatePointerDown(PointerButton.TouchContact));
			sequence.AddAction(touchDevice.CreatePointerUp(PointerButton.TouchContact));

			return sequence;
		}

		public void DragAndDrop(Func<AppQuery, AppQuery> from, Func<AppQuery, AppQuery> to)
		{
			DragAndDrop(
				QueryAppium(from, true).First(),
				QueryAppium(to, true).First());
		}

		public void DragAndDrop(string from, string to)
		{
			DragAndDrop(
				QueryAppium(from, true).First(),
				QueryAppium(to, true).First());
		}

		public void DragAndDrop(AppiumElement source, AppiumElement destination)
		{
			if (IsiOS)
			{
				var sourceCenterX = source.Location.X + (source.Size.Width / 2);
				var sourceCenterY = source.Location.Y + (source.Size.Height / 2);
				var destCenterX = destination.Location.X + (destination.Size.Width / 2);
				var destCenterY = destination.Location.Y + (destination.Size.Height / 2);
				DragCoordinates(sourceCenterX, sourceCenterY, destCenterX, destCenterY);
			}
			else if (IsMac)
			{
				// Mac will work with 'Actions' but if you add a 'Pause' action it will deadlock
				_driver?.ExecuteScript("macos: clickAndDragAndHold", new Dictionary<string, object>
				{
					{ "holdDuration", .1 }, // Length of time to hold before releasing
					{ "duration", 1 }, // Length of time to hold after click before start dragging
					{ "velocity", 2500 }, // How fast to drag
					{ "sourceElementId", source.Id },
					{ "destinationElementId", destination.Id },
				});
			}
			else
			{
				PointerInputDevice touchDevice = new PointerInputDevice(PointerType);
				ActionSequence sequence = new ActionSequence(touchDevice, 0);
				sequence.AddAction(touchDevice.CreatePointerMove(source, 0, 0, TimeSpan.FromMilliseconds(5)));
				sequence.AddAction(touchDevice.CreatePointerDown(PointerButton.TouchContact));
				sequence.AddAction(touchDevice.CreatePause(TimeSpan.FromSeconds(1))); // Have to pause so the device doesn't think we are scrolling
				sequence.AddAction(touchDevice.CreatePointerMove(destination, 0, 0, TimeSpan.FromSeconds(1)));
				sequence.AddAction(touchDevice.CreatePointerUp(PointerButton.TouchContact));
				_driver?.PerformActions(new List<ActionSequence> { sequence });
			}

			Thread.Sleep(500);
		}

		public void DragCoordinates(float fromX, float fromY, float toX, float toY)
		{
			DragCoordinates(fromX, fromY, toX, toY, 1);
		}

		public void DragCoordinates(float fromX, float fromY, float toX, float toY, int duration)
		{
			if (IsiOS)
			{
				// iOS doesn't seem to work with the action API, so we are using script calls
				_driver?.ExecuteScript("mobile: dragFromToWithVelocity", new Dictionary<string, object>
				{
					{ "pressDuration", 1 }, // Length of time to hold after click before start dragging
					{ "holdDuration", .1 }, // Length of time to hold before releasing
					{ "velocity", CalculateDurationForSwipe((int)fromX,(int)fromY,(int)toX,(int)toY, 500) }, // How fast to drag
					// from/to are absolute screen coordinates unless 'element' is specified then everything will be relative
					{ "fromX", fromX},
					{ "fromY", fromY },
					{ "toX", toX },
					{ "toY", toY }
				});
			}
			else if (IsMac)
			{
				_driver?.ExecuteScript("macos: clickAndDragAndHold", new Dictionary<string, object>
				{
					{ "holdDuration", .1 }, // Length of time to hold before releasing
					{ "duration", 1 }, // Length of time to hold after click before start dragging
					{ "velocity", CalculateDurationForSwipe((int)fromX,(int)fromY,(int)toX,(int)toY, 500) },
					{ "fromX", fromX},
					{ "fromY", fromY },
					{ "endX", toX },
					{ "endY", toY }
				});
			}
			else
			{
				PointerInputDevice touchDevice = new PointerInputDevice(PointerType);
				ActionSequence sequence = new ActionSequence(touchDevice, 0);
				sequence.AddAction(touchDevice.CreatePointerMove(CoordinateOrigin.Viewport, (int)fromX, (int)fromY, TimeSpan.FromMilliseconds(5)));
				sequence.AddAction(touchDevice.CreatePointerDown(PointerButton.TouchContact));
				sequence.AddAction(touchDevice.CreatePause(TimeSpan.FromSeconds(1))); // Have to pause so the device doesn't think we are scrolling
				sequence.AddAction(touchDevice.CreatePointerMove(CoordinateOrigin.Viewport, (int)toX, (int)toY, TimeSpan.FromSeconds(duration)));
				sequence.AddAction(touchDevice.CreatePointerUp(PointerButton.TouchContact));
				_driver?.PerformActions(new List<ActionSequence> { sequence });
			}
		}

		public void EnterText(string text)
		{
			new Actions(_driver)
				.SendKeys(text)
				.Perform();
		}

		public void EnterText(Func<AppQuery, AppQuery> query, string text)
		{
			var result = QueryAppium(query, true).First();
			EnterText(result, text);
		}

		public void EnterText(string marked, string text)
		{
			var result = QueryAppium(marked, true).First();
			EnterText(result, text);
		}

		private void EnterText(AppiumElement element, string text)
		{
			element.SendKeys(text);
			DismissKeyboard();
		}

		public bool WaitForTextToBePresentInElement(string automationId, string text)
		{
			WebDriverWait wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(15));

			return wait.Until(driver =>
			{
				var elementText = Query(automationId).FirstOrDefault()?.Text;
				if (elementText != null && elementText.Contains(text, StringComparison.OrdinalIgnoreCase))
				{
					return true;
				}

				return false;
			});
		}

		public void EnterText(Func<AppQuery, AppWebQuery> query, string text)
		{
			throw new NotImplementedException();
		}

		public AppResult[] Flash(Func<AppQuery, AppQuery>? query = null)
		{
			throw new NotImplementedException();
		}

		public AppResult[] Flash(string marked)
		{
			throw new NotImplementedException();
		}

		public object Invoke(string methodName, object? argument = null)
		{
			throw new NotImplementedException();
		}

		public object Invoke(string methodName, object[] arguments)
		{
			throw new NotImplementedException();
		}

		public void PinchToZoomIn(Func<AppQuery, AppQuery> query, TimeSpan? duration = null)
		{
			var element = QueryAppium(query).First();
			PinchToZoom(element, duration);
		}

		public void PinchToZoomIn(string marked, TimeSpan? duration = null)
		{
			var element = QueryAppium(marked, true).First();
			PinchToZoom(element, duration);
		}

		private void PinchToZoom(AppiumElement element, TimeSpan? duration)
		{
			if (IsiOS)
			{
				// mobile: pinch
				// scale	number	yes	Pinch scale of type float. Use a scale between 0 and 1 to "pinch close" or zoom out and a scale greater than 1 to "pinch open" or zoom in.
				_driver?.ExecuteScript("mobile: pinch", new Dictionary<string, object>
				{
					{ "elementId", element.Id },
					{ "scale", 2 },
					{ "velocity", 2.2 }
				});

			}
			else
			{
				throw new NotImplementedException();
			}
		}

		public void PinchToZoomInCoordinates(float x, float y, TimeSpan? duration)
		{
			throw new NotImplementedException();
		}

		public void PinchToZoomOut(Func<AppQuery, AppQuery> query, TimeSpan? duration = null)
		{
			throw new NotImplementedException();
		}

		public void PinchToZoomOut(string marked, TimeSpan? duration = null)
		{
			var element = QueryAppium(marked, true).First();

			if (IsiOS)
			{
				_driver?.ExecuteScript("mobile: pinch", new Dictionary<string, object>
				{
					{ "elementId", element.Id },
					{ "scale", 0 }
				});

			}
			else
			{
				throw new NotImplementedException();
			}
		}

		public void PinchToZoomOutCoordinates(float x, float y, TimeSpan? duration)
		{
			throw new NotImplementedException();
		}

		public void PressEnter()
		{
			throw new NotImplementedException();
		}

		public void PressVolumeDown()
		{
			throw new NotImplementedException();
		}

		public void PressVolumeUp()
		{
			throw new NotImplementedException();
		}

		public AppResult[] Query(Func<AppQuery, AppQuery>? query = null)
		{
			ReadOnlyCollection<AppiumElement> elements = QueryAppium(query);
			return elements.Select(ToAppResult).ToArray();
		}

		public AppResult[] Query(string marked)
		{
			ReadOnlyCollection<AppiumElement> elements = QueryAppium(marked);
			return elements.Select(ToAppResult).ToArray();
		}

		public AppWebResult[] Query(Func<AppQuery, AppWebQuery> query)
		{
			throw new NotImplementedException();
		}

		public T[] Query<T>(Func<AppQuery, AppTypedSelector<T>> query)
		{
			AppTypedSelector<T> appTypedSelector = query(new AppQuery(QueryPlatform.iOS));

			// Swiss-Army Chainsaw time
			// We'll use reflection to dig into the query and get the element selector 
			// and the property value invocation in text form
			BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic;
			Type selectorType = appTypedSelector.GetType();
			PropertyInfo tokensProperty = selectorType.GetProperties(bindingFlags).First(t => t.PropertyType == typeof(IQueryToken[]));

			var tokens = tokensProperty.GetValue(appTypedSelector) as IQueryToken[] ?? throw new NullReferenceException("Couldn't get tokens from query");

			string selector = tokens[0].ToQueryString(QueryPlatform.iOS);
			string invoke = tokens[1].ToCodeString();

			// Now that we have them in text form, we can reinterpret them for Windows
			AppiumQuery winQuery = AppiumQuery.FromRaw(selector, _appId, Platform);
			// TODO hartez 2017/07/19 17:08:44 Make this a bit more resilient if the translation isn't there	
			var translationKey = invoke.Substring(8).Replace("\")", "", StringComparison.OrdinalIgnoreCase);

			//if (!_translatePropertyAccessor.ContainsKey(translationKey))
			//	throw new Exception($"{translationKey} not found please add to _translatePropertyAccessor");

			string attribute = _translatePropertyAccessor.ContainsKey(translationKey) ? _translatePropertyAccessor[translationKey] : translationKey;

			ReadOnlyCollection<AppiumElement> elements = QueryAppium(winQuery);

			foreach (AppiumElement e in elements)
			{
				string x = e.GetAttribute(attribute);
				Debug.WriteLine($">>>>> WinDriverApp Query 261: {x}");
			}

			// TODO hartez 2017/07/19 17:09:14 Alas, for now this simply doesn't work. Waiting for WinAppDriver to implement it	
			return elements.Select(e => (T)Convert.ChangeType(e.GetAttribute(attribute), typeof(T))).ToArray();
		}

		public string[] Query(Func<AppQuery, InvokeJSAppQuery> query)
		{
			throw new NotImplementedException();
		}

		public void Repl()
		{
			throw new NotImplementedException();
		}

		public FileInfo Screenshot(string title)
		{
			if (_driver == null)
			{
				throw new NullReferenceException("Screenshot: _driver is null");
			}

			string filename = $"{title}.png";
			Screenshot screenshot = _driver.GetScreenshot();
			screenshot.SaveAsFile(filename, ScreenshotImageFormat.Png);
			var file = new FileInfo(filename);
			return file;
		}

		public byte[] Screenshot()
		{
			if (_driver == null)
			{
				throw new NullReferenceException("Screenshot: _driver is null");
			}

			Screenshot screenshot = _driver.GetScreenshot();
			return screenshot.AsByteArray;
		}

		public void ScrollDown(Func<AppQuery, AppQuery>? withinQuery = null, ScrollStrategy strategy = ScrollStrategy.Auto, double swipePercentage = 0.67, int swipeSpeed = 500, bool withInertia = true)
		{
			throw new NotImplementedException();
		}

		public void ScrollDown(string withinMarked, ScrollStrategy strategy = ScrollStrategy.Auto, double swipePercentage = 0.67, int swipeSpeed = 500, bool withInertia = true)
		{
			throw new NotImplementedException();
		}

		public void ScrollDownTo(string toMarked, string? withinMarked = null, ScrollStrategy strategy = ScrollStrategy.Auto, double swipePercentage = 0.67, int swipeSpeed = 500, bool withInertia = true, TimeSpan? timeout = null)
		{
			ScrollTo(FromMarked(toMarked), withinMarked == null ? null : FromMarked(withinMarked), timeout);
		}

		public void ScrollDownTo(Func<AppQuery, AppWebQuery> toQuery, string withinMarked, ScrollStrategy strategy = ScrollStrategy.Auto, double swipePercentage = 0.67, int swipeSpeed = 500, bool withInertia = true, TimeSpan? timeout = null)
		{
			throw new NotImplementedException();
		}

		public void ScrollDownTo(Func<AppQuery, AppQuery> toQuery, Func<AppQuery, AppQuery>? withinQuery = null, ScrollStrategy strategy = ScrollStrategy.Auto, double swipePercentage = 0.67, int swipeSpeed = 500, bool withInertia = true, TimeSpan? timeout = null)
		{
			throw new NotImplementedException();
		}

		public void ScrollDownTo(Func<AppQuery, AppWebQuery> toQuery, Func<AppQuery, AppQuery>? withinQuery = null, ScrollStrategy strategy = ScrollStrategy.Auto, double swipePercentage = 0.67, int swipeSpeed = 500, bool withInertia = true, TimeSpan? timeout = null)
		{
			throw new NotImplementedException();
		}

		public void ScrollTo(string toMarked, string? withinMarked = null, ScrollStrategy strategy = ScrollStrategy.Auto, double swipePercentage = 0.67, int swipeSpeed = 500, bool withInertia = true, TimeSpan? timeout = null)
		{
			throw new NotImplementedException();
		}

		public void ScrollUp(Func<AppQuery, AppQuery>? query = null, ScrollStrategy strategy = ScrollStrategy.Auto, double swipePercentage = 0.67, int swipeSpeed = 500, bool withInertia = true)
		{
			throw new NotImplementedException();
		}

		public void ScrollUp(string withinMarked, ScrollStrategy strategy = ScrollStrategy.Auto, double swipePercentage = 0.67, int swipeSpeed = 500, bool withInertia = true)
		{
			throw new NotImplementedException();
		}

		public void ScrollUpTo(string toMarked, string? withinMarked = null, ScrollStrategy strategy = ScrollStrategy.Auto, double swipePercentage = 0.67, int swipeSpeed = 500, bool withInertia = true, TimeSpan? timeout = null)
		{
			throw new NotImplementedException();
		}

		public void ScrollUpTo(Func<AppQuery, AppWebQuery> toQuery, string withinMarked, ScrollStrategy strategy = ScrollStrategy.Auto, double swipePercentage = 0.67, int swipeSpeed = 500, bool withInertia = true, TimeSpan? timeout = null)
		{
			throw new NotImplementedException();
		}

		public void ScrollUpTo(Func<AppQuery, AppQuery> toQuery, Func<AppQuery, AppQuery>? withinQuery = null, ScrollStrategy strategy = ScrollStrategy.Auto, double swipePercentage = 0.67, int swipeSpeed = 500, bool withInertia = true, TimeSpan? timeout = null)
		{
			throw new NotImplementedException();
		}

		public void ScrollUpTo(Func<AppQuery, AppWebQuery> toQuery, Func<AppQuery, AppQuery>? withinQuery = null, ScrollStrategy strategy = ScrollStrategy.Auto, double swipePercentage = 0.67, int swipeSpeed = 500, bool withInertia = true, TimeSpan? timeout = null)
		{
			throw new NotImplementedException();
		}

		public void SetOrientationLandscape()
		{
			throw new NotImplementedException();
		}

		public void SetOrientationPortrait()
		{
			throw new NotImplementedException();
		}

		public void SetSliderValue(string marked, double value)
		{
			throw new NotImplementedException();
		}

		public void SetSliderValue(Func<AppQuery, AppQuery> query, double value)
		{
			throw new NotImplementedException();
		}

		public void SwipeLeftToRight(double swipePercentage = 0.67, int swipeSpeed = 500, bool withInertia = true)
		{
			throw new NotImplementedException();
		}

		public void SwipeLeftToRight(string marked, double swipePercentage = 0.67, int swipeSpeed = 500, bool withInertia = true)
		{
			throw new NotImplementedException();
		}

		public void SwipeLeftToRight(Func<AppQuery, AppQuery> query, double swipePercentage = 0.67, int swipeSpeed = 500, bool withInertia = true)
		{
			throw new NotImplementedException();
		}

		public void SwipeLeftToRight(Func<AppQuery, AppWebQuery> query, double swipePercentage = 0.67, int swipeSpeed = 500, bool withInertia = true)
		{
			throw new NotImplementedException();
		}

		public void SwipeRightToLeft(double swipePercentage = 0.67, int swipeSpeed = 500, bool withInertia = true)
		{
			throw new NotImplementedException();
		}

		public void SwipeRightToLeft(string marked, double swipePercentage = 0.67, int swipeSpeed = 500, bool withInertia = true)
		{
			var element = QueryAppium(AppiumQuery.FromMarked(marked, _appId, Platform)).FirstOrDefault() ?? throw new Exception("Didn't find the element");
			PerformSwipe(element.Rect, ScrollDirection.Left, swipePercentage, swipeSpeed, withInertia);
		}

		public void SwipeRightToLeft(Func<AppQuery, AppQuery> query, double swipePercentage = 0.67, int swipeSpeed = 500, bool withInertia = true)
		{
			var element = QueryAppium(AppiumQuery.FromQuery(query, _appId, Platform)).FirstOrDefault() ?? throw new Exception("Didn't find the element");
			PerformSwipe(element.Rect, ScrollDirection.Left, swipePercentage, swipeSpeed, withInertia);
		}

		public void SwipeRightToLeft(Func<AppQuery, AppWebQuery> query, double swipePercentage = 0.67, int swipeSpeed = 500, bool withInertia = true)
		{
			throw new NotImplementedException();
		}

		internal enum ScrollDirection
		{
			Up,
			Down,
			Left,
			Right
		}

		private void PerformSwipe(Rectangle target, ScrollDirection direction, double swipePercentage, int swipeSpeed, bool withInertia)
		{
			var centerX = target.X + (target.Width / 2);
			var centerY = target.Y + (target.Height / 2);
			var targetWidth = target.Width;
			var targetHeight = target.Height;

			int startX, endX, startY, endY;
			startX = endX = centerX;
			startY = endY = centerY;
			var xOffset = (int)((swipePercentage / 2.0f) * targetWidth);
			var yOffset = (int)((swipePercentage / 2.0f) * targetHeight);

			bool percentTooBig;
			switch (direction)
			{
				case ScrollDirection.Right: // left to right
					startX = (centerX - xOffset);
					endX = (centerX + xOffset);
					percentTooBig = (startX <= target.X || endX >= targetWidth + target.X);
					break;
				case ScrollDirection.Left: // right to left
					startX = (centerX + xOffset);
					endX = (centerX - xOffset);
					percentTooBig = (endX <= target.X || startX >= targetWidth + target.X);
					break;
				case ScrollDirection.Up: // down to up
					startY = (centerY + yOffset);
					endY = (centerY - yOffset);
					percentTooBig = (endY <= target.Y || startY >= targetHeight + target.Y);
					break;
				case ScrollDirection.Down: // up to down
					startY = (centerY - yOffset);
					endY = (centerY + yOffset);
					percentTooBig = (startY <= target.Y || endY >= targetHeight + target.Y);
					break;
				default:
					throw new Exception(string.Format("Unable to swipe in direction {0}", direction));
			}

			if (percentTooBig)
			{
				throw new Exception(string.Format(
					"Invalid swipe coordinates ({0}, {1}) to ({2}, {3}).{4}Try setting swipePercentage smaller than {5}.",
					startX, startY, endX, endY, Environment.NewLine, swipePercentage));
			}

			// var duration = CalculateDurationForSwipe(startX, startY, endX, endY, swipeSpeed);

			DragCoordinates(startX, startY, endX, endY);
			// _gestures.SwipeCoordinates(startX, endX, startY, endY, withInertia, TimeSpan.FromMilliseconds(duration));
		}

		static int CalculateDurationForSwipe(int startX, int startY, int endX, int endY, int pixelsPerSecond)
		{
			var distance = Math.Sqrt(Math.Pow(startX - endX, 2) + Math.Pow(startY - endY, 2));

			return (int)(distance / (pixelsPerSecond / 1000.0));
		}

		void Tap(AppiumQuery query)
		{
			var element = FindFirstElement(query) ?? throw new NullReferenceException("Didn't find the element");
			ClickOrTapElement(element);
		}

		public void Tap(Func<AppQuery, AppQuery> query)
		{
			AppiumQuery appiumQuery = AppiumQuery.FromQuery(query, _appId, Platform);
			Tap(appiumQuery);
		}

		public void Tap(string marked)
		{
			AppiumQuery appiumQuery = AppiumQuery.FromMarked(marked, _appId, Platform);
			Tap(appiumQuery);
		}

		public void Tap(Func<AppQuery, AppWebQuery> query)
		{
			throw new NotImplementedException();
		}

		public void TapCoordinates(float x, float y)
		{
			PointerInputDevice touchDevice = new PointerInputDevice(PointerType);
			ActionSequence sequence = new ActionSequence(touchDevice, 0);
			sequence.AddAction(touchDevice.CreatePointerMove(CoordinateOrigin.Viewport, (int)x, (int)y, TimeSpan.FromMilliseconds(5)));
			sequence.AddAction(touchDevice.CreatePointerDown(PointerButton.TouchContact));
			sequence.AddAction(touchDevice.CreatePointerUp(PointerButton.TouchContact));
			_driver?.PerformActions(new List<ActionSequence> { sequence });
			Thread.Sleep(1000);
		}

		public void TouchAndHold(Func<AppQuery, AppQuery> query)
		{
			throw new NotImplementedException();
		}

		public void TouchAndHold(string marked)
		{
			throw new NotImplementedException();
		}

		public void TouchAndHoldCoordinates(float x, float y)
		{
			throw new NotImplementedException();
		}

		public void WaitFor(Func<bool> predicate, string timeoutMessage = "Timed out waiting...", TimeSpan? timeout = null, TimeSpan? retryFrequency = null, TimeSpan? postTimeout = null)
		{
			timeout ??= DefaultTimeout;
			retryFrequency ??= TimeSpan.FromMilliseconds(500);
			timeoutMessage ??= "Timed out on query.";

			DateTime start = DateTime.Now;

			while (!predicate())
			{
				var elapsed = DateTime.Now.Subtract(start).Ticks;
				if (elapsed >= timeout.Value.Ticks)
				{
					Debug.WriteLine($">>>>> {elapsed} ticks elapsed, timeout value is {timeout.Value.Ticks}");

					throw new TimeoutException(timeoutMessage);
				}

				Task.Delay(retryFrequency.Value.Milliseconds).Wait();
			}
		}

		public AppResult[] WaitForElement(Func<AppQuery, AppQuery> query, string timeoutMessage = "Timed out waiting for element...", TimeSpan? timeout = null, TimeSpan? retryFrequency = null, TimeSpan? postTimeout = null)
		{
			Func<ReadOnlyCollection<AppiumElement>> result = () => QueryAppium(query);
			return WaitForAtLeastOne(result, timeoutMessage, timeout, retryFrequency).Select(AppiumExtensions.ToAppResult).ToArray();
		}

		public AppResult[] WaitForElement(string marked, string timeoutMessage = "Timed out waiting for element...", TimeSpan? timeout = null, TimeSpan? retryFrequency = null, TimeSpan? postTimeout = null)
		{
			Func<ReadOnlyCollection<AppiumElement>> result = () => QueryAppium(marked);
			var results = WaitForAtLeastOne(result, timeoutMessage, timeout, retryFrequency).Select(AppiumExtensions.ToAppResult).ToArray();

			return results;
		}

		public AppWebResult[] WaitForElement(Func<AppQuery, AppWebQuery> query, string timeoutMessage = "Timed out waiting for element...", TimeSpan? timeout = null, TimeSpan? retryFrequency = null, TimeSpan? postTimeout = null)
		{
			throw new NotImplementedException();
		}

		public void WaitForNoElement(Func<AppQuery, AppQuery> query, string timeoutMessage = "Timed out waiting for no element...", TimeSpan? timeout = null, TimeSpan? retryFrequency = null, TimeSpan? postTimeout = null)
		{
			Func<ReadOnlyCollection<AppiumElement>> result = () => QueryAppium(query);
			WaitForNone(result, timeoutMessage, timeout, retryFrequency);
		}

		public void WaitForNoElement(string marked, string timeoutMessage = "Timed out waiting for no element...", TimeSpan? timeout = null, TimeSpan? retryFrequency = null, TimeSpan? postTimeout = null)
		{
			Func<ReadOnlyCollection<AppiumElement>> result = () => QueryAppium(marked);
			WaitForNone(result, timeoutMessage, timeout, retryFrequency);
		}

		public void WaitForNoElement(Func<AppQuery, AppWebQuery> query, string timeoutMessage = "Timed out waiting for no element...", TimeSpan? timeout = null, TimeSpan? retryFrequency = null, TimeSpan? postTimeout = null)
		{
			throw new NotImplementedException();
		}

		ReadOnlyCollection<AppiumElement> QueryAppium(AppiumQuery query, bool findFirst = false)
		{
			try
			{
				if (_driver == null)
				{
					throw new InvalidOperationException("Driver should not be null when trying to query the app");
				}

				By queryBy = IsWindows ? MobileBy.AccessibilityId(query.Marked) : MobileBy.Id(query.Marked);
				var primaryElement = _driver.FindElements(queryBy);

				// Try to handle the rest of the query string after marked:'{x}', e.g. "* marked:'Tab1Element' child android.webkit.WebView child android.widget.TextView"
				var match = Regex.Match(query.Raw, "(.*)'((?:\\s)(\\S*))*");
				if (match.Groups.Count > 3 && match.Groups[3].Captures.Count != 0)
				{
					int index = 0;
					while (index < match.Groups[3].Captures.Count)
					{
						switch (match.Groups[3].Captures[index++].Value)
						{
							case "child":
								var parentElement = primaryElement.First();
								var childElement = (AppiumElement)parentElement.FindElement(By.ClassName(match.Groups[3].Captures[index++].Value));
								primaryElement = new ReadOnlyCollection<AppiumElement>(new List<AppiumElement>(1) { childElement });
								break;
						}
					}
				}
				return primaryElement;
			}
			catch (Exception)
			{
				throw;
			}
		}

		ReadOnlyCollection<AppiumElement> QueryAppium(string marked, bool findFirst = false)
		{
			AppiumQuery appiumQuery = AppiumQuery.FromMarked(marked, _appId, Platform);
			return QueryAppium(appiumQuery, findFirst);
		}

		ReadOnlyCollection<AppiumElement> QueryAppium(Func<AppQuery, AppQuery>? query, bool findFirst = false)
		{
			AppiumQuery winQuery = AppiumQuery.FromQuery(query, _appId, Platform);
			return QueryAppium(winQuery, findFirst);
		}

		ReadOnlyCollection<AppiumElement> FilterControlType(IEnumerable<AppiumElement> elements, string controlType)
		{
			string tag = controlType;

			if (tag == "*")
			{
				return new ReadOnlyCollection<AppiumElement>(elements.ToList());
			}

			if (_controlNameToTag.ContainsKey(controlType))
			{
				tag = _controlNameToTag[controlType];
			}

			return new ReadOnlyCollection<AppiumElement>(elements.Where(element => element.TagName == tag).ToList());
		}


		static ReadOnlyCollection<AppiumElement> Wait(Func<ReadOnlyCollection<AppiumElement>> query,
			Func<int, bool> satisfactory,
			string? timeoutMessage = null,
			TimeSpan? timeout = null, TimeSpan? retryFrequency = null)
		{
			timeout ??= DefaultTimeout;
			retryFrequency ??= TimeSpan.FromMilliseconds(500);
			timeoutMessage ??= "Timed out on query.";

			DateTime start = DateTime.Now;

			ReadOnlyCollection<AppiumElement> result = query();

			while (!satisfactory(result.Count))
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

		static ReadOnlyCollection<AppiumElement> WaitForAtLeastOne(Func<ReadOnlyCollection<AppiumElement>> query,
			string? timeoutMessage = null,
			TimeSpan? timeout = null,
			TimeSpan? retryFrequency = null)
		{
			var results = Wait(query, i => i > 0, timeoutMessage, timeout, retryFrequency);

			return results;
		}

		void WaitForNone(Func<ReadOnlyCollection<AppiumElement>> query,
			string? timeoutMessage = null,
			TimeSpan? timeout = null, TimeSpan? retryFrequency = null)
		{
			Wait(query, i => i == 0, timeoutMessage, timeout, retryFrequency);
		}

		AppiumElement? FindFirstElement(AppiumQuery query)
		{
			Func<ReadOnlyCollection<AppiumElement>> fquery =
				() => QueryAppium(query, true);

			string timeoutMessage = $"Timed out waiting for element: {query.Raw}";

			ReadOnlyCollection<AppiumElement> results =
				WaitForAtLeastOne(fquery, timeoutMessage);

			AppiumElement? element = results?.FirstOrDefault();

			return element;
		}

		void ClickOrTapElement(AppiumElement element)
		{
			try
			{
				// For most stuff, a simple click will work
				element.Click();
			}
			catch (InvalidOperationException)
			{
				ProcessException();
			}
			catch (WebDriverException)
			{
				ProcessException();
			}

			void ProcessException()
			{
				// Some elements aren't "clickable" from an automation perspective (e.g., Frame renders as a Border
				// with content in it; if the content is just a TextBlock, we'll end up here)

				// All is not lost; we can figure out the location of the element in in the application window
				// and Tap in that spot
				PointF p = ElementToClickablePoint(element);
				TapCoordinates(p.X, p.Y);
			}
		}

		PointF ElementToClickablePoint(AppiumElement element)
		{
			PointF clickablePoint = GetClickablePoint(element);

			AppiumElement window = GetWindow();
			PointF origin = GetOriginOfBoundingRectangle(window);

			// Use the coordinates in the app window's viewport relative to the window's origin
			return new PointF(clickablePoint.X - origin.X, clickablePoint.Y - origin.Y);
		}

		static PointF GetOriginOfBoundingRectangle(AppiumElement element)
		{
			string vpcpString = element.GetAttribute("BoundingRectangle");

			// returned string format looks like:
			// Left:-1868 Top:382 Width:1013 Height:680

			string[] vpparts = vpcpString.Split(new[] { ':', ' ' }, StringSplitOptions.RemoveEmptyEntries);
			float vpx = float.Parse(vpparts[1]);
			float vpy = float.Parse(vpparts[3]);

			return new PointF(vpx, vpy);
		}

		AppiumElement GetWindow()
		{
			if (_window != null)
			{
				return _window;
			}

			_window = QueryAppium(_appId)[0];
			return _window;
		}

		static PointF GetClickablePoint(AppiumElement element)
		{
			string cpString = element.GetAttribute("ClickablePoint");
			string[] parts = cpString.Split(',');
			float x = float.Parse(parts[0]);
			float y = float.Parse(parts[1]);

			return new PointF(x, y);
		}

		internal enum ClickType
		{
			SingleClick,
			DoubleClick,
			ContextClick
		}

		AppiumQuery FromMarked(string marked)
		{
			return AppiumQuery.FromMarked(marked, _appId, Platform);
		}

		void ScrollTo(AppiumQuery toQuery, AppiumQuery? withinQuery = null, TimeSpan? timeout = null, bool down = true)
		{
			// This method will keep scrolling in the specified direction until it finds an element 
			// which matches the query, or until it times out.

			// First we need to determine the area within which we'll make our scroll gestures
			Size? scrollAreaSize = null;

			if (withinQuery != null)
			{
				var within = FindFirstElement(withinQuery);
				scrollAreaSize = within?.Size;
			}

			if (scrollAreaSize is null)
			{
				var window = _driver?.Manage().Window ?? throw new InvalidOperationException("Element to scroll within not specified, and no Window available. Cannot scroll.");
				scrollAreaSize = window.Size;
			}

			var x = scrollAreaSize.Value.Width / 2;
			var windowHeight = scrollAreaSize.Value.Height;
			var topEdgeOfScrollAction = windowHeight * 0.1;
			var bottomEdgeOfScrollAction = windowHeight * 0.5;
			var startY = down ? bottomEdgeOfScrollAction : topEdgeOfScrollAction;
			var endY = down ? topEdgeOfScrollAction : bottomEdgeOfScrollAction;

			timeout ??= DefaultTimeout;
			DateTime start = DateTime.Now;

			TimeSpan iterationTimeout = TimeSpan.FromMilliseconds(0);
			TimeSpan retryFrequency = TimeSpan.FromMilliseconds(0);
			Func<ReadOnlyCollection<AppiumElement>> result = () => QueryAppium(toQuery);

			while (true)
			{
				try
				{
					ReadOnlyCollection<AppiumElement> found = WaitForAtLeastOne(result, timeoutMessage: null,
						timeout: iterationTimeout, retryFrequency: retryFrequency);

					if (found.Count > 0)
					{
						// Success!
						return;
					}
				}
				catch (TimeoutException)
				{
					// Haven't found it yet, keep scrolling
				}

				long elapsed = DateTime.Now.Subtract(start).Ticks;
				if (elapsed >= timeout.Value.Ticks)
				{
					Debug.WriteLine($">>>>> {elapsed} ticks elapsed, timeout value is {timeout.Value.Ticks}");
					throw new TimeoutException($"Timed out scrolling to {toQuery}");
				}

				var scrollAction = new TouchAction(_driver).Press(x, startY).MoveTo(x, endY).Release();
				scrollAction.Perform();
			}
		}


		ApplicationState GetXCUITestAppState()
		{
			var state = _driver?.ExecuteScript(IsMac ? "macos: queryAppState" : "mobile: queryAppState", new Dictionary<string, object>
						{
							{ "bundleId", _appId },
						});

			// https://developer.apple.com/documentation/xctest/xcuiapplicationstate?language=objc
			return Convert.ToInt32(state) switch
			{
				1 => ApplicationState.Not_Running,
				2 or
				3 or
				4 => ApplicationState.Running,
				_ => ApplicationState.Unknown,
			};
		}

		ApplicationState GetWindowsAppState()
		{
			try
			{
				// WinAppDriver doesn't support QueryAppState
				_ = _driver?.CurrentWindowHandle;
				return ApplicationState.Running;
			}
			catch (NoSuchWindowException)
			{
				return ApplicationState.Not_Running;
			}
		}

		ApplicationState GetUIAutomator2TestAppState()
		{
			var state = _driver?.ExecuteScript("mobile: queryAppState", new Dictionary<string, object>
						{
							{ "appId", _appId },
						});

			// https://github.com/appium/appium-uiautomator2-driver#mobile-queryappstate
			if (state == null)
			{
				return ApplicationState.Unknown;
			}

			return Convert.ToInt32(state) switch
			{
				0 => ApplicationState.Not_Installed,
				1 => ApplicationState.Not_Running,
				3 or
				4 => ApplicationState.Running,
				_ => ApplicationState.Unknown,
			};
		}
	}
}
