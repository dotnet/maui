using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.Reflection;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Enums;
using OpenQA.Selenium.Appium.Interactions;
using OpenQA.Selenium.Appium.iOS;
using OpenQA.Selenium.Interactions;
using Xamarin.UITest;
using Xamarin.UITest.Queries;
using Xamarin.UITest.Queries.Tokens;

namespace TestUtils.Appium.UITests
{
	public class AppiumUITestApp : IApp2
	{
		public bool IsAndroid => _driver != null && Platform.Equals("Android", StringComparison.OrdinalIgnoreCase);
		public bool IsWindows => _driver != null && Platform.Equals("Windows", StringComparison.OrdinalIgnoreCase);
		public bool IsiOS => _driver != null && Platform.Equals("iOS", StringComparison.OrdinalIgnoreCase);
		public bool IsMac => _driver != null && Platform.Equals("mac", StringComparison.OrdinalIgnoreCase);
		public string Platform => _driver?.Capabilities.GetCapability(MobileCapabilityType.PlatformName).ToString() ?? "";

		public PointerKind PointerType => IsMac ? PointerKind.Mouse : PointerKind.Touch;

		public static TimeSpan DefaultTimeout = TimeSpan.FromSeconds(15);

		readonly Dictionary<string, string> _controlNameToTag = new Dictionary<string, string>
		{
			{ "button", "ControlType.Button" }
		};

		readonly Dictionary<string, string> _translatePropertyAccessor = new Dictionary<string, string>
		{
			{ "getAlpha", "Opacity" },
			{ "isEnabled", "IsEnabled" },
		};

		readonly string _appId;
		AppiumDriver? _driver;

		AppiumElement? _window;

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

		public string ElementTree => _driver?.PageSource ?? "";

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
				QueryWindows("NavigationViewBackButton", true).First().Click();
			}
		}

		public void ClearText(Func<AppQuery, AppQuery> query)
		{
			var result = QueryWindows(query, true).First();
			result.Clear();
		}

		public void ClearText(Func<AppQuery, AppWebQuery> query)
		{
			throw new NotImplementedException();
		}

		public void ClearText(string marked)
		{
			var result = QueryWindows(marked, true).First();
			result.Clear();
		}

		public void ClearText()
		{
			throw new NotImplementedException();
		}

		public void DismissKeyboard()
		{
			if (!IsWindows && !IsMac)
			{
				if (_driver != null && _driver.IsKeyboardShown())
				{
					if (IsiOS)
					{
						_driver.HideKeyboard("return");
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
			var result = QueryWindows(query, true).First();
			DoubleTap(result);
		}

		public void DoubleTap(string marked)
		{
			var result = QueryWindows(marked, true).First();
			DoubleTap(result);
		}

		private void DoubleTap(IWebElement element)
		{
			OpenQA.Selenium.Appium.Interactions.PointerInputDevice touchDevice = new OpenQA.Selenium.Appium.Interactions.PointerInputDevice(PointerType);
			ActionSequence sequence = new ActionSequence(touchDevice, 0);
			sequence.AddAction(touchDevice.CreatePointerMove(element, 0, 0, TimeSpan.FromMilliseconds(5)));
			sequence.AddAction(touchDevice.CreatePointerDown(PointerButton.TouchContact));
			sequence.AddAction(touchDevice.CreatePointerUp(PointerButton.TouchContact));
			sequence.AddAction(touchDevice.CreatePause(TimeSpan.FromMilliseconds(600)));
			sequence.AddAction(touchDevice.CreatePointerDown(PointerButton.TouchContact));
			sequence.AddAction(touchDevice.CreatePointerUp(PointerButton.TouchContact));
			_driver?.PerformActions(new List<ActionSequence> { sequence });
			Thread.Sleep(1000);
		}

		public void DoubleTapCoordinates(float x, float y)
		{
			OpenQA.Selenium.Appium.Interactions.PointerInputDevice touchDevice = new OpenQA.Selenium.Appium.Interactions.PointerInputDevice(PointerType);
			ActionSequence sequence = new ActionSequence(touchDevice, 0);
			sequence.AddAction(touchDevice.CreatePointerMove(CoordinateOrigin.Viewport, (int)x, (int)y, TimeSpan.FromMilliseconds(5)));
			sequence.AddAction(touchDevice.CreatePointerDown(PointerButton.TouchContact));
			sequence.AddAction(touchDevice.CreatePointerUp(PointerButton.TouchContact));
			sequence.AddAction(touchDevice.CreatePause(TimeSpan.FromMilliseconds(600)));
			sequence.AddAction(touchDevice.CreatePointerDown(PointerButton.TouchContact));
			sequence.AddAction(touchDevice.CreatePointerUp(PointerButton.TouchContact));
			_driver?.PerformActions(new List<ActionSequence> { sequence });
			Thread.Sleep(1000);
		}

		public void DragAndDrop(Func<AppQuery, AppQuery> from, Func<AppQuery, AppQuery> to)
		{
			DragAndDrop(
				QueryWindows(from, true).First(),
				QueryWindows(to, true).First());
		}

		public void DragAndDrop(string from, string to)
		{
			DragAndDrop(
				QueryWindows(from, true).First(),
				QueryWindows(to, true).First());
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
				OpenQA.Selenium.Appium.Interactions.PointerInputDevice touchDevice = new OpenQA.Selenium.Appium.Interactions.PointerInputDevice(PointerType);
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
			if (IsiOS)
			{
				// iOS doesn't seem to work with the action API, so we are using script calls
				_driver?.ExecuteScript("mobile: dragFromToForDuration", new Dictionary<string, object>
				{
					{ "duration", 1 }, // Length of time to hold after click before start dragging
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
					{ "velocity", 2500 }, // How fast to drag
					{ "fromX", fromX},
					{ "fromY", fromY },
					{ "endX", toX },
					{ "endY", toY }
				});
			}
			else
			{
				OpenQA.Selenium.Appium.Interactions.PointerInputDevice touchDevice = new OpenQA.Selenium.Appium.Interactions.PointerInputDevice(PointerType);
				ActionSequence sequence = new ActionSequence(touchDevice, 0);
				sequence.AddAction(touchDevice.CreatePointerMove(CoordinateOrigin.Viewport, (int)fromX, (int)fromY, TimeSpan.FromMilliseconds(5)));
				sequence.AddAction(touchDevice.CreatePointerDown(PointerButton.TouchContact));
				sequence.AddAction(touchDevice.CreatePause(TimeSpan.FromSeconds(1))); // Have to pause so the device doesn't think we are scrolling
				sequence.AddAction(touchDevice.CreatePointerMove(CoordinateOrigin.Viewport, (int)toX, (int)toY, TimeSpan.FromSeconds(1)));
				sequence.AddAction(touchDevice.CreatePointerUp(PointerButton.TouchContact));
				_driver?.PerformActions(new List<ActionSequence> { sequence });
			}

			Thread.Sleep(500);
		}

		public void EnterText(string text)
		{
			new Actions(_driver)
				.SendKeys(text)
				.Perform();
		}

		public void EnterText(Func<AppQuery, AppQuery> query, string text)
		{
			var result = QueryWindows(query, true).First();
			EnterText(result, text);
		}

		public void EnterText(string marked, string text)
		{
			var result = QueryWindows(marked, true).First();
			EnterText(result, text);
		}

		private void EnterText(AppiumElement element, string text)
		{
			element.SendKeys(text);
			DismissKeyboard();
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
			throw new NotImplementedException();
		}

		public void PinchToZoomIn(string marked, TimeSpan? duration = null)
		{
			throw new NotImplementedException();
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
			throw new NotImplementedException();
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
			ReadOnlyCollection<AppiumElement> elements = QueryWindows(query);
			return elements.Select(ToAppResult).ToArray();
		}

		public AppResult[] Query(string marked)
		{
			ReadOnlyCollection<AppiumElement> elements = QueryWindows(marked);
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
			throw new NotImplementedException();
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
			throw new NotImplementedException();
		}

		public void SwipeRightToLeft(Func<AppQuery, AppQuery> query, double swipePercentage = 0.67, int swipeSpeed = 500, bool withInertia = true)
		{
			throw new NotImplementedException();
		}

		public void SwipeRightToLeft(Func<AppQuery, AppWebQuery> query, double swipePercentage = 0.67, int swipeSpeed = 500, bool withInertia = true)
		{
			throw new NotImplementedException();
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
			throw new NotImplementedException();
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
			throw new NotImplementedException();
		}

		public AppResult[] WaitForElement(Func<AppQuery, AppQuery> query, string timeoutMessage = "Timed out waiting for element...", TimeSpan? timeout = null, TimeSpan? retryFrequency = null, TimeSpan? postTimeout = null)
		{
			Func<ReadOnlyCollection<AppiumElement>> result = () => QueryWindows(query);
			return WaitForAtLeastOne(result, timeoutMessage, timeout, retryFrequency).Select(AppiumExtensions.ToAppResult).ToArray();
		}

		public AppResult[] WaitForElement(string marked, string timeoutMessage = "Timed out waiting for element...", TimeSpan? timeout = null, TimeSpan? retryFrequency = null, TimeSpan? postTimeout = null)
		{
			Func<ReadOnlyCollection<AppiumElement>> result = () => QueryWindows(marked);
			var results = WaitForAtLeastOne(result, timeoutMessage, timeout, retryFrequency).Select(AppiumExtensions.ToAppResult).ToArray();

			return results;
		}

		public AppWebResult[] WaitForElement(Func<AppQuery, AppWebQuery> query, string timeoutMessage = "Timed out waiting for element...", TimeSpan? timeout = null, TimeSpan? retryFrequency = null, TimeSpan? postTimeout = null)
		{
			throw new NotImplementedException();
		}

		public void WaitForNoElement(Func<AppQuery, AppQuery> query, string timeoutMessage = "Timed out waiting for no element...", TimeSpan? timeout = null, TimeSpan? retryFrequency = null, TimeSpan? postTimeout = null)
		{
			Func<ReadOnlyCollection<AppiumElement>> result = () => QueryWindows(query);
			WaitForNone(result, timeoutMessage, timeout, retryFrequency);
		}

		public void WaitForNoElement(string marked, string timeoutMessage = "Timed out waiting for no element...", TimeSpan? timeout = null, TimeSpan? retryFrequency = null, TimeSpan? postTimeout = null)
		{
			Func<ReadOnlyCollection<AppiumElement>> result = () => QueryWindows(marked);
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
					throw new ArgumentNullException(nameof(_driver));
				}

				By queryBy = IsWindows ? MobileBy.AccessibilityId(query.Marked) : MobileBy.Id(query.Marked);
				return _driver.FindElements(queryBy);
			}
			catch (Exception)
			{
				throw;
			}
		}

		ReadOnlyCollection<AppiumElement> QueryWindows(string marked, bool findFirst = false)
		{
			AppiumQuery appiumQuery = AppiumQuery.FromMarked(marked, _appId, Platform);
			return QueryAppium(appiumQuery, findFirst);
		}

		ReadOnlyCollection<AppiumElement> QueryWindows(Func<AppQuery, AppQuery>? query, bool findFirst = false)
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

			_window = QueryWindows(_appId)[0];
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

	}
}
