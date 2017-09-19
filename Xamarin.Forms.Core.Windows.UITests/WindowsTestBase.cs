using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Resources;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium.MultiTouch;
using OpenQA.Selenium.Appium.Windows;
using OpenQA.Selenium.Interactions.Internal;
using OpenQA.Selenium.Remote;
using Xamarin.Forms.Xaml;
using Xamarin.UITest;
using Xamarin.UITest.Queries;

namespace Xamarin.Forms.Core.UITests
{
	public class WindowsTestBase
	{
		protected const string WindowsApplicationDriverUrl = "http://127.0.0.1:4723";
		protected static WindowsDriver<WindowsElement> Session;

		public static IApp ConfigureApp()
		{
			if (Session == null)
			{
				DesiredCapabilities appCapabilities = new DesiredCapabilities();
				appCapabilities.SetCapability("app", "0d4424f6-1e29-4476-ac00-ba22c3789cb6_wzjw7qdpbr1br!App");
				appCapabilities.SetCapability("deviceName", "WindowsPC");
				Session = new WindowsDriver<WindowsElement>(new Uri(WindowsApplicationDriverUrl), appCapabilities);
				Assert.IsNotNull(Session);
				Session.Manage().Timeouts().ImplicitlyWait(TimeSpan.FromSeconds(2));
				Reset();
			}
			
			return new WinDriverApp(Session);
		}

		public static void Reset()
		{
			try
			{
				Session?.Keyboard?.PressKey(Keys.Escape);
			}
			catch (Exception ex)
			{
				Debug.WriteLine($">>>>> WindowsTestBase ConfigureApp 49: {ex}");
				throw;
			}
		}
	}

	public class WinDriverApp : IApp
	{
	    readonly WindowsDriver<WindowsElement> _session;
		protected static RemoteTouchScreen _touchScreen;

		public WinDriverApp(WindowsDriver<WindowsElement> session)
		{
			_session = session;
			_touchScreen = new RemoteTouchScreen(session);
		}

		readonly Dictionary<string, string> _controlNameToTag = new Dictionary<string, string>
		{
            {"button", "ControlType.Button"} 
        };

		ReadOnlyCollection<WindowsElement> FilterControlType(IEnumerable<WindowsElement> elements, string controlType)
		{
			var tag = controlType;

			if (tag == "*")
			{
				return new ReadOnlyCollection<WindowsElement>(elements.ToList());
			}

			if (_controlNameToTag.ContainsKey(controlType))
			{
				tag = _controlNameToTag[controlType];
			}

			return new ReadOnlyCollection<WindowsElement>(elements.Where(element => element.TagName == tag).ToList()); 
		}

		class WinQuery
		{
			public static WinQuery FromQuery(Func<AppQuery, AppQuery> query)
			{
				var raw = GetRawQuery(query);
				return FromRaw(raw);
			}

			public static WinQuery FromMarked(string marked)
			{
				return new WinQuery("*", marked, $"* '{marked}'");
			}

			public static WinQuery FromRaw(string raw)
			{
				Debug.WriteLine($">>>>> Converting raw query '{raw}' to {nameof(WinQuery)}");

				var match = Regex.Match(raw, @"(.*)\s(marked|text):'(.*)'");

				var controlType = match.Groups[1].Captures[0].Value;
				var marked = match.Groups[3].Captures[0].Value;

				// Just ignoring everything else for now (parent, index statements, etc)

				return new WinQuery(controlType, marked, raw);
			}

			static string GetRawQuery(Func<AppQuery, AppQuery> query = null)
			{
				if (query == null)
				{
					return string.Empty;
				}

				return query(new AppQuery(QueryPlatform.iOS)).ToString();
			}

			WinQuery(string controlType, string marked, string raw)
			{
				ControlType = controlType;
				Marked = marked;
				Raw = raw;
			}

			public string ControlType { get; }

			public string Marked { get; }

			public string Raw { get; }

			public override string ToString()
			{
				return $"{nameof(ControlType)}: {ControlType}, {nameof(Marked)}: {Marked}";
			}
		}

		ReadOnlyCollection<WindowsElement> QueryWindows(WinQuery query)
		{
			var resultByName = _session.FindElementsByName(query.Marked);
			var resultByAccessibilityId = _session.FindElementsByAccessibilityId(query.Marked);

			var result = resultByName
				.Concat(resultByAccessibilityId);

			if (query.ControlType == "*")
			{
				var textBoxesByContent = _session.FindElementsByClassName("TextBox").Where(e => e.Text == query.Marked);
				result = result.Concat(textBoxesByContent);
			}

			return FilterControlType(result, query.ControlType);
		}

		ReadOnlyCollection<WindowsElement> QueryWindows(string marked)
		{
			var winQuery = WinQuery.FromMarked(marked);
			return QueryWindows(winQuery);
		}

		ReadOnlyCollection<WindowsElement> QueryWindows(Func<AppQuery, AppQuery> query)
		{
			var winQuery = WinQuery.FromQuery(query);
			return QueryWindows(winQuery);
		}

		static AppRect ToAppRect(WindowsElement windowsElement)
		{
			var result = new AppRect
			{
				X = windowsElement.Location.X,
				Y = windowsElement.Location.Y,
				Height = windowsElement.Size.Height,
				Width = windowsElement.Size.Width
			};

			result.CenterX = result.X + result.Width / 2;
			result.CenterY = result.Y + result.Height / 2;
			
			return result;
		}

		static AppResult ToAppResult(WindowsElement windowsElement)
		{
			return new AppResult
			{
				Rect = ToAppRect(windowsElement),
				Label = windowsElement.Id, // Not entirely sure about this one
				Description = windowsElement.Text, // or this one
				Enabled = windowsElement.Enabled,
				Id = windowsElement.Id
			};
		}

		public AppResult[] Query(Func<AppQuery, AppQuery> query = null)
		{
			var elements = QueryWindows(WinQuery.FromQuery(query));
			return elements.Select(ToAppResult).ToArray();
		}

		public AppResult[] Query(string marked)
		{
			var elements = QueryWindows(marked);
			return elements.Select(ToAppResult).ToArray();
		}

		public AppWebResult[] Query(Func<AppQuery, AppWebQuery> query)
		{
			throw new NotImplementedException();
		}

		readonly Dictionary<string, string> _translatePropertyAccessor = new Dictionary<string, string>
		{
			{"getAlpha", "Opacity"} 
		};

		public T[] Query<T>(Func<AppQuery, AppTypedSelector<T>> query)
		{
			var appTypedSelector = query(new AppQuery(QueryPlatform.iOS));

			// Swiss-Army Chainsaw time
			// We'll use reflection to dig into the query and get the element selector 
			// and the property value invocation in text form
			var bindingFlags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
			var selectorType = appTypedSelector.GetType();
			var tokensProperty = selectorType.GetProperties(bindingFlags)
				.First(t => t.PropertyType == typeof(Xamarin.UITest.Queries.Tokens.IQueryToken[]));

			var tokens = (Xamarin.UITest.Queries.Tokens.IQueryToken[])tokensProperty.GetValue(appTypedSelector);

			// Output some debugging info
			//foreach (var t in tokens)
			//{
			//	Debug.WriteLine($">>>>> WinDriverApp Query 208: {t.ToQueryString(QueryPlatform.iOS)}");
			//	Debug.WriteLine($">>>>> WinDriverApp Query 208: {t.ToCodeString()}");
			//}

			var selector = tokens[0].ToQueryString(QueryPlatform.iOS);
			var invoke = tokens[1].ToCodeString();
			
			// Now that we have them in text form, we can reinterpret them for Windows
			var winQuery = WinQuery.FromRaw(selector);
			// TODO hartez 2017/07/19 17:08:44 Make this a bit more resilient if the translation isn't there	
			var attribute = _translatePropertyAccessor[invoke.Substring(8).Replace("\")", "")]; 

			var elements = QueryWindows(winQuery);

			// TODO hartez 2017/07/19 17:09:14 Alas, for now this simply doesn't work. Waiting for WinAppDrive to implement it	
			return elements.Select(e => (T)Convert.ChangeType(e.GetAttribute(attribute), typeof(T))).ToArray();
		}

		public string[] Query(Func<AppQuery, InvokeJSAppQuery> query)
		{
			throw new NotImplementedException();
		}

		public AppResult[] Flash(Func<AppQuery, AppQuery> query = null)
		{
			throw new NotImplementedException();
		}

		public AppResult[] Flash(string marked)
		{
			throw new NotImplementedException();
		}

		public void EnterText(string text)
		{
			_session.Keyboard.SendKeys(text);
		}

		public void EnterText(Func<AppQuery, AppQuery> query, string text)
		{
			QueryWindows(query).First().SendKeys(text);
		}

		public void EnterText(string marked, string text)
		{
			QueryWindows(marked).First().SendKeys(text);
		}

		public void EnterText(Func<AppQuery, AppWebQuery> query, string text)
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

		public void PressEnter()
		{
			_session.Keyboard.PressKey(Keys.Enter);
		}

		public void DismissKeyboard()
		{
			// No-op for Desktop, which is all we're doing right now
		}

		public void Tap(Func<AppQuery, AppQuery> query)
		{
			var winQuery = WinQuery.FromQuery(query);
			Tap(winQuery);
		}

		public void Tap(string marked)
		{
			var winQuery = WinQuery.FromMarked(marked);
			Tap(winQuery);
		}

		void Tap(WinQuery query, int taps = 1)
		{
			Func<ReadOnlyCollection<WindowsElement>> fquery = () => QueryWindows(query);

			var timeoutMessage = $"Timed out waiting for element: {query.Raw}";

			var results = WaitForAtLeastOne(fquery, timeoutMessage);

			if (results.Any())
			{
				var element = results.First();

				for (int n = 0; n < taps; n++)
				{
					element.Click();
				}
			}
		}

		public void Tap(Func<AppQuery, AppWebQuery> query)
		{
			throw new NotImplementedException();
		}

		public void TapCoordinates(float x, float y)
		{
			// Okay, this one's a bit complicated. For some reason, _session.Tap() with coordinates does not work
			// (Filed https://github.com/Microsoft/WinAppDriver/issues/229 for that)
			// But we can do the equivalent by manipulating the mouse. The mouse methods all take an ICoordinates
			// object, and you'd think that the "coordinates" part of ICoordinates would have something do with 
			// where the mouse clicks. You'd be wrong. The coordinates parts of that object are ignored and it just
			// clicks the center of whatever WindowsElement the ICoordinates refers to in 'AuxiliaryLocator'

			// If we could just use the element, we wouldn't be tapping at specific coordinates, so that's not 
			// very helpful.

			// So here's how we're working around it for the moment:
			// 1. Get the Window viewport (which is a known-to-exist element)
			// 2. Using the Window's ICoordinates and the MouseMove() overload with x/y offsets, move the pointer
			//		to the location we care about
			// 3. Use the (undocumented, except in https://github.com/Microsoft/WinAppDriver/issues/118#issuecomment-269404335)
			//		null parameter for Mouse.Click() to click at the current pointer location
			
			var candidates = QueryWindows("Xamarin.Forms.ControlGallery.WindowsUniversal");
			var viewPort = candidates[3]; // We really just want the viewport; skip the full window, title bar, min/max buttons...
			var xOffset = viewPort.Coordinates.LocationInViewport.X;
			var yOffset = viewPort.Coordinates.LocationInViewport.Y;
			_session.Mouse.MouseMove(viewPort.Coordinates, (int)x - xOffset, (int)y - yOffset);
			_session.Mouse.Click(null);
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

		public void DoubleTap(Func<AppQuery, AppQuery> query)
		{
			var winQuery = WinQuery.FromQuery(query);
			Tap(winQuery, 2);
		}

		public void DoubleTap(string marked)
		{
			var winQuery = WinQuery.FromMarked(marked);
			Tap(winQuery, 2);
		}

		public void DoubleTapCoordinates(float x, float y)
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

		public void WaitFor(Func<bool> predicate, string timeoutMessage = "Timed out waiting...", TimeSpan? timeout = null,
			TimeSpan? retryFrequency = null, TimeSpan? postTimeout = null)
		{
			throw new NotImplementedException();
		}

		ReadOnlyCollection<WindowsElement> WaitForAtLeastOne(Func<ReadOnlyCollection<WindowsElement>> query,
			string timeoutMessage = null,
			TimeSpan? timeout = null, TimeSpan? retryFrequency = null)
		{
			return Wait(query, i => i > 0, timeoutMessage, timeout, retryFrequency);
		}

		void WaitForNone(Func<ReadOnlyCollection<WindowsElement>> query,
			string timeoutMessage = null,
			TimeSpan? timeout = null, TimeSpan? retryFrequency = null)
		{
			Wait(query, i => i == 0, timeoutMessage, timeout, retryFrequency);
		}

		ReadOnlyCollection<WindowsElement> Wait(Func<ReadOnlyCollection<WindowsElement>> query,
			Func<int, bool> satisfactory,
			string timeoutMessage = null,
			TimeSpan? timeout = null, TimeSpan? retryFrequency = null)
		{
			timeout = timeout ?? TimeSpan.FromSeconds(5);
			retryFrequency = retryFrequency ?? TimeSpan.FromMilliseconds(500);
			timeoutMessage = timeoutMessage ?? "Timed out on query.";

			var start = DateTime.Now;

			var result = query();

			while (!satisfactory(result.Count))
			{
				if (DateTime.Now.Subtract(start).Ticks >= timeout.Value.Ticks)
				{
					throw new TimeoutException(timeoutMessage);
				}

				Task.Delay(retryFrequency.Value.Milliseconds).Wait();
			}

			return result;
		}

		public AppResult[] WaitForElement(Func<AppQuery, AppQuery> query, string timeoutMessage = "Timed out waiting for element...",
			TimeSpan? timeout = null, TimeSpan? retryFrequency = null, TimeSpan? postTimeout = null)
		{
			Func<ReadOnlyCollection<WindowsElement>> result = () => QueryWindows(query);
			return WaitForAtLeastOne(result, timeoutMessage, timeout, retryFrequency).Select(ToAppResult).ToArray();
		}

		public AppResult[] WaitForElement(string marked, string timeoutMessage = "Timed out waiting for element...",
			TimeSpan? timeout = null, TimeSpan? retryFrequency = null, TimeSpan? postTimeout = null)
		{
			Func<ReadOnlyCollection<WindowsElement>> result = () => QueryWindows(marked);
			return WaitForAtLeastOne(result, timeoutMessage, timeout, retryFrequency).Select(ToAppResult).ToArray();
		}

		public AppWebResult[] WaitForElement(Func<AppQuery, AppWebQuery> query, string timeoutMessage = "Timed out waiting for element...",
			TimeSpan? timeout = null, TimeSpan? retryFrequency = null, TimeSpan? postTimeout = null)
		{
			throw new NotImplementedException();
		}

		public void WaitForNoElement(Func<AppQuery, AppQuery> query, string timeoutMessage = "Timed out waiting for no element...",
			TimeSpan? timeout = null, TimeSpan? retryFrequency = null, TimeSpan? postTimeout = null)
		{
			Func<ReadOnlyCollection<WindowsElement>> result = () => QueryWindows(query);
			WaitForNone(result, timeoutMessage, timeout, retryFrequency);
		}

		public void WaitForNoElement(string marked, string timeoutMessage = "Timed out waiting for no element...",
			TimeSpan? timeout = null, TimeSpan? retryFrequency = null, TimeSpan? postTimeout = null)
		{
			Func<ReadOnlyCollection<WindowsElement>> result = () => QueryWindows(marked);
			WaitForNone(result, timeoutMessage, timeout, retryFrequency);
		}

		public void WaitForNoElement(Func<AppQuery, AppWebQuery> query, string timeoutMessage = "Timed out waiting for no element...",
			TimeSpan? timeout = null, TimeSpan? retryFrequency = null, TimeSpan? postTimeout = null)
		{
			throw new NotImplementedException();
		}

		public FileInfo Screenshot(string title)
		{
			// TODO hartez 2017/07/18 10:16:56 Verify that this is working; seems a bit too simple	
			var filename = $"{title}.png";

			var screenshot = _session.GetScreenshot();
			screenshot.SaveAsFile(filename, ImageFormat.Png);
			return new FileInfo(filename);
		}

		public void SwipeRight()
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

		public void SwipeLeft()
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

		public void SwipeLeftToRight(Func<AppQuery, AppQuery> query, double swipePercentage = 0.67, int swipeSpeed = 500, bool withInertia = true)
		{
			throw new NotImplementedException();
		}

		public void SwipeLeftToRight(Func<AppQuery, AppWebQuery> query, double swipePercentage = 0.67, int swipeSpeed = 500, bool withInertia = true)
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

		public void ScrollUp(Func<AppQuery, AppQuery> query = null, ScrollStrategy strategy = ScrollStrategy.Auto, double swipePercentage = 0.67, int swipeSpeed = 500,
			bool withInertia = true)
		{
			throw new NotImplementedException();
		}

		public void ScrollUp(string withinMarked, ScrollStrategy strategy = ScrollStrategy.Auto, double swipePercentage = 0.67, int swipeSpeed = 500,
			bool withInertia = true)
		{
			throw new NotImplementedException();
		}

		public void ScrollDown(Func<AppQuery, AppQuery> withinQuery = null, ScrollStrategy strategy = ScrollStrategy.Auto, double swipePercentage = 0.67,
			int swipeSpeed = 500, bool withInertia = true)
		{
			throw new NotImplementedException();
		}

		public void ScrollDown(string withinMarked, ScrollStrategy strategy = ScrollStrategy.Auto, double swipePercentage = 0.67,
			int swipeSpeed = 500, bool withInertia = true)
		{
			throw new NotImplementedException();
		}

		public void ScrollTo(string toMarked, string withinMarked = null, ScrollStrategy strategy = ScrollStrategy.Auto,
			double swipePercentage = 0.67, int swipeSpeed = 500, bool withInertia = true, TimeSpan? timeout = null)
		{
			throw new NotImplementedException();
		}

		public void ScrollUpTo(string toMarked, string withinMarked = null, ScrollStrategy strategy = ScrollStrategy.Auto,
			double swipePercentage = 0.67, int swipeSpeed = 500, bool withInertia = true, TimeSpan? timeout = null)
		{
			throw new NotImplementedException();
		}

		public void ScrollUpTo(Func<AppQuery, AppWebQuery> toQuery, string withinMarked, ScrollStrategy strategy = ScrollStrategy.Auto, double swipePercentage = 0.67,
			int swipeSpeed = 500, bool withInertia = true, TimeSpan? timeout = null)
		{
			throw new NotImplementedException();
		}

		public void ScrollDownTo(string toMarked, string withinMarked = null, ScrollStrategy strategy = ScrollStrategy.Auto,
			double swipePercentage = 0.67, int swipeSpeed = 500, bool withInertia = true, TimeSpan? timeout = null)
		{
			throw new NotImplementedException();
		}

		public void ScrollDownTo(Func<AppQuery, AppWebQuery> toQuery, string withinMarked, ScrollStrategy strategy = ScrollStrategy.Auto, double swipePercentage = 0.67,
			int swipeSpeed = 500, bool withInertia = true, TimeSpan? timeout = null)
		{
			throw new NotImplementedException();
		}

		public void ScrollUpTo(Func<AppQuery, AppQuery> toQuery, Func<AppQuery, AppQuery> withinQuery = null, ScrollStrategy strategy = ScrollStrategy.Auto, double swipePercentage = 0.67,
			int swipeSpeed = 500, bool withInertia = true, TimeSpan? timeout = null)
		{
			throw new NotImplementedException();
		}

		public void ScrollUpTo(Func<AppQuery, AppWebQuery> toQuery, Func<AppQuery, AppQuery> withinQuery = null, ScrollStrategy strategy = ScrollStrategy.Auto, double swipePercentage = 0.67,
			int swipeSpeed = 500, bool withInertia = true, TimeSpan? timeout = null)
		{
			throw new NotImplementedException();
		}

		public void ScrollDownTo(Func<AppQuery, AppQuery> toQuery, Func<AppQuery, AppQuery> withinQuery = null, ScrollStrategy strategy = ScrollStrategy.Auto, double swipePercentage = 0.67,
			int swipeSpeed = 500, bool withInertia = true, TimeSpan? timeout = null)
		{
			throw new NotImplementedException();
		}

		public void ScrollDownTo(Func<AppQuery, AppWebQuery> toQuery, Func<AppQuery, AppQuery> withinQuery = null, ScrollStrategy strategy = ScrollStrategy.Auto, double swipePercentage = 0.67,
			int swipeSpeed = 500, bool withInertia = true, TimeSpan? timeout = null)
		{
			throw new NotImplementedException();
		}

		public void SetOrientationPortrait()
		{
			// Deliberately leaving this as a no-op for now
			// Trying to set the orientation on the Desktop (the only version of UWP we're testing for the moment)
			// gives us a 405 Method Not Allowed, which makes sense. Haven't figured out how to determine
			// whether we're in a mode which allows orientation, but if we were, the next line is probably how to set it.
			//_session.Orientation = ScreenOrientation.Portrait;
		}

		public void SetOrientationLandscape()
		{
			// Deliberately leaving this as a no-op for now
			// Trying to set the orientation on the Desktop (the only version of UWP we're testing for the moment)
			// gives us a 405 Method Not Allowed, which makes sense. Haven't figured out how to determine
			// whether we're in a mode which allows orientation, but if we were, the next line is probably how to set it.
			//_session.Orientation = ScreenOrientation.Landscape;
		}

		public void Repl()
		{
			throw new NotImplementedException();
		}

		public void Back()
		{
			QueryWindows("Back").First().Click();
		}

		public void PressVolumeUp()
		{
			throw new NotImplementedException();
		}

		public void PressVolumeDown()
		{
			throw new NotImplementedException();
		}

		public object Invoke(string methodName, object argument = null)
		{
			throw new NotImplementedException();
		}

		public object Invoke(string methodName, object[] arguments)
		{
			throw new NotImplementedException();
		}

		public void DragCoordinates(float fromX, float fromY, float toX, float toY)
		{
			throw new NotImplementedException();
		}

		public void DragAndDrop(Func<AppQuery, AppQuery> @from, Func<AppQuery, AppQuery> to)
		{
			throw new NotImplementedException();
		}

		public void DragAndDrop(string @from, string to)
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

		public AppPrintHelper Print { get; }

		public IDevice Device { get; }

		public ITestServer TestServer { get; }
	}
}
