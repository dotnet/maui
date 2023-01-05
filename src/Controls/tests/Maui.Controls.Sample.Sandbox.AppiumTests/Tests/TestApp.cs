using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Android;
using OpenQA.Selenium.Appium.Windows;
using OpenQA.Selenium.Remote;
using Xamarin.UITest;
using Xamarin.UITest.Queries;
using Xamarin.UITest.Shared.Execution;

namespace Maui.Controls.Sample.Sandbox.AppiumTests.Tests
{
	public class TestApp<T, W> : IApp
			where T : AppiumDriver<W>
			where W : IWebElement
	{
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

		string _appId;
		T _engine;

		public TestApp(string appId, T driver)
		{
			_appId = appId;
			_engine = driver;
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
			throw new NotImplementedException();
		}

		public void EnterText(string marked, string text)
		{
			throw new NotImplementedException();
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
			throw new NotImplementedException();
		}

		public void Tap(string marked)
		{
			throw new NotImplementedException();
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
			throw new NotImplementedException();
		}

		public AppResult[] WaitForElement(string marked, string timeoutMessage = "Timed out waiting for element...", TimeSpan? timeout = null, TimeSpan? retryFrequency = null, TimeSpan? postTimeout = null)
		{

			Func<ReadOnlyCollection<W>> result = () => QueryWindows(marked);
			var results = WaitForAtLeastOne(result, timeoutMessage, timeout, retryFrequency).Select(ToAppResult).ToArray();

			var e = _engine.FindElement(By.Id($"{_appId}:id/{marked}"));

			return new AppResult[] { new AppResult
			{
				Id = marked
			}
			};
		}

		static AppRect? ToAppRect(W windowsElement)
		{
			try
			{
				if (windowsElement == null)
				{
					return null;
				}

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
			catch (Exception ex)
			{
				Debug.WriteLine(
					$"Warning: error determining AppRect for {windowsElement}; "
					+ $"if this is a Label with a modified Text value, it might be confusing Windows automation. " +
					$"{ex}");
			}

			return null;
		}


		static AppResult ToAppResult(W windowsElement)
		{
			if (windowsElement is WindowsElement win)
				return new AppResult
				{
					Rect = ToAppRect(windowsElement),
					Label = win.Id, // Not entirely sure about this one
					Description = SwapInUsefulElement(win)?.Text, // or this one
					Enabled = win.Enabled,
					Id = win.Id
				};
			else
				return new AppResult
				{
					Rect = ToAppRect(windowsElement),
					Id = windowsElement.GetAttribute("id")
				};
		}

		static RemoteWebElement? SwapInUsefulElement(WindowsElement element)
		{
			// AutoSuggestBox on UWP has some interaction issues with WebDriver
			// The AutomationID is set on the control group not the actual TextBox
			// This retrieves the actual TextBox which makes the behavior more consistent
			var isAutoSuggest = element?.FindElementsByXPath("//*[contains(@AutomationId,'_AutoSuggestBox')]")?.FirstOrDefault();
			return isAutoSuggest ?? element;
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

		ReadOnlyCollection<W> QueryWindows(AndroidQuery query, bool findFirst = false)
		{
			try
			{
			

				if (_engine is AppiumDriver<AndroidElement> androidDriver)
				{
					var resultByAccessibilityId = _engine.FindElementsByAccessibilityId(query.Marked);
					ReadOnlyCollection<AndroidElement> resultByName;

					if (!findFirst || (resultByAccessibilityId != null))
						resultByName = androidDriver.FindElementsById(query.Marked);

					IEnumerable<W> result = resultByAccessibilityId!;

					if(result != null)
					{
					//	result = result.Concat(resultByName?.Cast<W>())
					}
				

					//// TODO hartez 2017/10/30 09:47:44 Should this be == "*" || == "TextBox"?	
					//// what about other controls where we might be looking by content? TextBlock?
					//if (query.ControlType == "*")
					//{
					//	IEnumerable<WindowsElement> textBoxesByContent =
					//		_engine.FindElementByClassName("TextBox").Where(e => e.Text == query.Marked);
					//	result = result.Concat(textBoxesByContent);
					//}

					return FilterControlType(result!, query.ControlType);
				}
			}
			catch (Exception)
			{

				throw;
			}


			return null!;
		}

		ReadOnlyCollection<W> QueryWindows(string marked, bool findFirst = false)
		{
			AndroidQuery winQuery = AndroidQuery.FromMarked(_appId, marked);
			return QueryWindows(winQuery, findFirst);
		}

		ReadOnlyCollection<W> QueryWindows(Func<AppQuery, AppQuery> query, bool findFirst = false)
		{
			AndroidQuery winQuery = AndroidQuery.FromQuery(query);
			return QueryWindows(winQuery, findFirst);
		}

		ReadOnlyCollection<W> FilterControlType(IEnumerable<W> elements, string controlType)
		{
			string tag = controlType;

			if (tag == "*")
			{
				return new ReadOnlyCollection<W>(elements.ToList());
			}

			if (_controlNameToTag.ContainsKey(controlType))
			{
				tag = _controlNameToTag[controlType];
			}

			return new ReadOnlyCollection<W>(elements.Where(element => element.TagName == tag).ToList());
		}


		static ReadOnlyCollection<W> Wait(Func<ReadOnlyCollection<W>> query,
			Func<int, bool> satisfactory,
			string? timeoutMessage = null,
			TimeSpan? timeout = null, TimeSpan? retryFrequency = null)
		{
			timeout = timeout ?? DefaultTimeout;
			retryFrequency = retryFrequency ?? TimeSpan.FromMilliseconds(500);
			timeoutMessage = timeoutMessage ?? "Timed out on query.";

			DateTime start = DateTime.Now;

			ReadOnlyCollection<W> result = query();

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


		static ReadOnlyCollection<W> WaitForAtLeastOne(Func<ReadOnlyCollection<W>> query,
			string? timeoutMessage = null,
			TimeSpan? timeout = null,
			TimeSpan? retryFrequency = null)
		{
			var results = Wait(query, i => i > 0, timeoutMessage, timeout, retryFrequency);


			return results;
		}

		void WaitForNone(Func<ReadOnlyCollection<W>> query,
			string? timeoutMessage = null,
			TimeSpan? timeout = null, TimeSpan? retryFrequency = null)
		{
			Wait(query, i => i == 0, timeoutMessage, timeout, retryFrequency);
		}

		internal enum ClickType
		{
			SingleClick,
			DoubleClick,
			ContextClick
		}
	}
}
