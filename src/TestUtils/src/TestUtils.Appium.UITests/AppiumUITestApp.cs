using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Enums;
using Xamarin.UITest;
using Xamarin.UITest.Queries;

namespace TestUtils.Appium.UITests
{
	public class AppiumUITestApp : IApp
	{
		public bool IsAndroid => _driver.Capabilities.GetCapability(MobileCapabilityType.PlatformName).Equals("Android");
		public bool IsWindows => _driver.Capabilities.GetCapability(MobileCapabilityType.PlatformName).Equals("Windows");
		public bool IsiOS => _driver.Capabilities.GetCapability(MobileCapabilityType.PlatformName).Equals("iOS");
		public bool IsMac => _driver.Capabilities.GetCapability(MobileCapabilityType.PlatformName).Equals("mac");
		public string Platform => _driver.Capabilities.GetCapability(MobileCapabilityType.PlatformName).ToString() ?? "";

		public static TimeSpan DefaultTimeout = TimeSpan.FromSeconds(15);

		readonly Dictionary<string, string> _controlNameToTag = new Dictionary<string, string>
		{
			{ "button", "ControlType.Button" }
		};

		readonly Dictionary<string, string> _translatePropertyAccessor = new Dictionary<string, string>
		{
			{ "getAlpha", "Opacity" },
			{ "isEnabled", "IsEnabled" }
		};
		readonly string _appId;
		readonly AppiumDriver _driver;

		AppiumElement? _window;

		public AppiumUITestApp(string appId, AppiumDriver? driver)
		{
			if (driver == null)
				throw new ArgumentNullException(nameof(driver));

			_appId = appId;
			_driver = driver;
		}

		public AppPrintHelper Print => throw new NotImplementedException();

		public IDevice Device => throw new NotImplementedException();

		public ITestServer TestServer => throw new NotImplementedException();

		public void Back()
		{
			throw new NotImplementedException();
		}

		public void ClearText(Func<AppQuery, AppQuery> query)
		{
			throw new NotImplementedException();
		}

		public void ClearText(Func<AppQuery, AppWebQuery> query)
		{
			throw new NotImplementedException();
		}

		public void ClearText(string marked)
		{
			throw new NotImplementedException();
		}

		public void ClearText()
		{
			throw new NotImplementedException();
		}

		public void DismissKeyboard()
		{
			throw new NotImplementedException();
		}

		public void DoubleTap(Func<AppQuery, AppQuery> query)
		{
			throw new NotImplementedException();
		}

		public void DoubleTap(string marked)
		{
			throw new NotImplementedException();
		}

		public void DoubleTapCoordinates(float x, float y)
		{
			throw new NotImplementedException();
		}

		public void DragAndDrop(Func<AppQuery, AppQuery> from, Func<AppQuery, AppQuery> to)
		{
			throw new NotImplementedException();
		}

		public void DragAndDrop(string from, string to)
		{
			throw new NotImplementedException();
		}

		public void DragCoordinates(float fromX, float fromY, float toX, float toY)
		{
			throw new NotImplementedException();
		}

		public void EnterText(string text)
		{
			throw new NotImplementedException();
		}

		public void EnterText(Func<AppQuery, AppQuery> query, string text)
		{
			var result = QueryWindows(query, true).First();
			result.SendKeys(text);
		}

		public void EnterText(string marked, string text)
		{
			var result = QueryWindows(marked, true).First();
			result.SendKeys(text);
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
			throw new NotImplementedException();
		}

		public AppResult[] Query(string marked)
		{
			throw new NotImplementedException();
		}

		public AppWebResult[] Query(Func<AppQuery, AppWebQuery> query)
		{
			throw new NotImplementedException();
		}

		public Q[] Query<Q>(Func<AppQuery, AppTypedSelector<Q>> query)
		{
			throw new NotImplementedException();
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
			throw new NotImplementedException();
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

		public void Tap(Func<AppQuery, AppQuery> query)
		{
			AppiumQuery winQuery = AppiumQuery.FromQuery(query, _appId, Platform);
			Tap(winQuery);
		}

		public void Tap(string marked)
		{
			AppiumQuery winQuery = AppiumQuery.FromMarked(marked, _appId, Platform);
			Tap(winQuery);
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
			throw new NotImplementedException();
		}

		public void WaitForNoElement(string marked, string timeoutMessage = "Timed out waiting for no element...", TimeSpan? timeout = null, TimeSpan? retryFrequency = null, TimeSpan? postTimeout = null)
		{
			throw new NotImplementedException();
		}

		public void WaitForNoElement(Func<AppQuery, AppWebQuery> query, string timeoutMessage = "Timed out waiting for no element...", TimeSpan? timeout = null, TimeSpan? retryFrequency = null, TimeSpan? postTimeout = null)
		{
			throw new NotImplementedException();
		}

		ReadOnlyCollection<AppiumElement> QueryAppium(AppiumQuery query, bool findFirst = false)
		{
			try
			{
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
			AppiumQuery appiumQUery = AppiumQuery.FromMarked(marked, _appId, Platform);
			return QueryAppium(appiumQUery, findFirst);
		}

		ReadOnlyCollection<AppiumElement> QueryWindows(Func<AppQuery, AppQuery> query, bool findFirst = false)
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

		void Tap(AppiumQuery query)
		{
			var element = FindFirstElement(query);

			if (element == null)
			{
				return;
			}

			ClickOrTapElement(element);
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
