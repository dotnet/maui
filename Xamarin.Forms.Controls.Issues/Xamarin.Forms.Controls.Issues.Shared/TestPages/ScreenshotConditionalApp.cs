#if UITEST
using System;
using System.IO;
using Xamarin.UITest;
using Xamarin.UITest.Queries;

namespace Xamarin.Forms.Controls
{
	/// <summary>
	/// Decorator for IApp which only takes screenshots if the SCREENSHOTS symbol  is specified
	/// </summary>
	internal class ScreenshotConditionalApp : IApp
	{
		readonly IApp _app;

		public ScreenshotConditionalApp(IApp app)
		{
			_app = app;
		}

		public AppResult[] Query(Func<AppQuery, AppQuery> query = null)
		{
			return _app.Query(query);
		}

		public AppResult[] Query(string marked)
		{
			return _app.Query(marked);
		}

		public AppWebResult[] Query(Func<AppQuery, AppWebQuery> query)
		{
			return _app.Query(query);
		}

		public T[] Query<T>(Func<AppQuery, AppTypedSelector<T>> query)
		{
			return _app.Query(query);
		}

		public string[] Query(Func<AppQuery, InvokeJSAppQuery> query)
		{
			return _app.Query(query);
		}

		public AppResult[] Flash(Func<AppQuery, AppQuery> query = null)
		{
			return _app.Flash(query);
		}

		public AppResult[] Flash(string marked)
		{
			return _app.Flash(marked);
		}

		public void EnterText(string text)
		{
			_app.EnterText(text);
		}

		public void EnterText(Func<AppQuery, AppQuery> query, string text)
		{
			_app.EnterText(query, text);
		}

		public void EnterText(string marked, string text)
		{
			_app.EnterText(marked, text);
		}

		public void EnterText(Func<AppQuery, AppWebQuery> query, string text)
		{
			_app.EnterText(query, text);
		}

		public void ClearText(Func<AppQuery, AppWebQuery> query)
		{
			_app.ClearText(query);
		}

		public void ClearText(Func<AppQuery, AppQuery> query)
		{
			_app.ClearText(query);
		}

		public void ClearText(string marked)
		{
			_app.ClearText(marked);
		}

		public void ClearText()
		{
			_app.ClearText();
		}

		public void PressEnter()
		{
			_app.PressEnter();
		}

		public void DismissKeyboard()
		{
			_app.DismissKeyboard();
		}

		public void Tap(Func<AppQuery, AppQuery> query)
		{
			_app.Tap(query);
		}

		public void Tap(string marked)
		{
			_app.Tap(marked);
		}

		public void Tap(Func<AppQuery, AppWebQuery> query)
		{
			_app.Tap(query);
		}

		public void TapCoordinates(float x, float y)
		{
			_app.TapCoordinates(x, y);
		}

		public void TouchAndHold(Func<AppQuery, AppQuery> query)
		{
			_app.TouchAndHold(query);
		}

		public void TouchAndHold(string marked)
		{
			_app.TouchAndHold(marked);
		}

		public void TouchAndHoldCoordinates(float x, float y)
		{
			_app.TouchAndHoldCoordinates(x, y);
		}

		public void DoubleTap(Func<AppQuery, AppQuery> query)
		{
			_app.DoubleTap(query);
		}

		public void DoubleTap(string marked)
		{
			_app.DoubleTap(marked);
		}

		public void DoubleTapCoordinates(float x, float y)
		{
			_app.DoubleTapCoordinates(x, y);
		}

		public void PinchToZoomIn(Func<AppQuery, AppQuery> query, TimeSpan? duration = null)
		{
			_app.PinchToZoomIn(query, duration);
		}

		public void PinchToZoomIn(string marked, TimeSpan? duration = null)
		{
			_app.PinchToZoomIn(marked, duration);
		}

		public void PinchToZoomInCoordinates(float x, float y, TimeSpan? duration)
		{
			_app.PinchToZoomInCoordinates(x, y, duration);
		}

		public void PinchToZoomOut(Func<AppQuery, AppQuery> query, TimeSpan? duration = null)
		{
			_app.PinchToZoomOut(query, duration);
		}

		public void PinchToZoomOut(string marked, TimeSpan? duration = null)
		{
			_app.PinchToZoomOut(marked, duration);
		}

		public void PinchToZoomOutCoordinates(float x, float y, TimeSpan? duration)
		{
			_app.PinchToZoomOutCoordinates(x, y, duration);
		}

		public void WaitFor(Func<bool> predicate, string timeoutMessage = "Timed out waiting...", TimeSpan? timeout = null,
			TimeSpan? retryFrequency = null, TimeSpan? postTimeout = null)
		{
			_app.WaitFor(predicate, timeoutMessage, timeout, retryFrequency, postTimeout);
		}

		public AppResult[] WaitForElement(Func<AppQuery, AppQuery> query, string timeoutMessage = "Timed out waiting for element...",
			TimeSpan? timeout = null, TimeSpan? retryFrequency = null, TimeSpan? postTimeout = null)
		{
			return _app.WaitForElement(query, timeoutMessage, timeout, retryFrequency, postTimeout);
		}

		public AppResult[] WaitForElement(string marked, string timeoutMessage = "Timed out waiting for element...",
			TimeSpan? timeout = null, TimeSpan? retryFrequency = null, TimeSpan? postTimeout = null)
		{
			return _app.WaitForElement(marked, timeoutMessage, timeout, retryFrequency, postTimeout);
		}

		public AppWebResult[] WaitForElement(Func<AppQuery, AppWebQuery> query, string timeoutMessage = "Timed out waiting for element...",
			TimeSpan? timeout = null, TimeSpan? retryFrequency = null, TimeSpan? postTimeout = null)
		{
			return _app.WaitForElement(query, timeoutMessage, timeout, retryFrequency, postTimeout);
		}

		public void WaitForNoElement(Func<AppQuery, AppQuery> query, string timeoutMessage = "Timed out waiting for no element...",
			TimeSpan? timeout = null, TimeSpan? retryFrequency = null, TimeSpan? postTimeout = null)
		{
			_app.WaitForNoElement(query, timeoutMessage, timeout, retryFrequency, postTimeout);
		}

		public void WaitForNoElement(string marked, string timeoutMessage = "Timed out waiting for no element...",
			TimeSpan? timeout = null, TimeSpan? retryFrequency = null, TimeSpan? postTimeout = null)
		{
			_app.WaitForNoElement(marked, timeoutMessage, timeout, retryFrequency, postTimeout);
		}

		public void WaitForNoElement(Func<AppQuery, AppWebQuery> query, string timeoutMessage = "Timed out waiting for no element...",
			TimeSpan? timeout = null, TimeSpan? retryFrequency = null, TimeSpan? postTimeout = null)
		{
			_app.WaitForNoElement(query, timeoutMessage, timeout, retryFrequency, postTimeout);
		}

		public FileInfo Screenshot(string title)
		{
#if SCREENSHOTS
			return _app.Screenshot(title);
#else
			return null;
#endif
		}

		public void SwipeRight()
		{
#pragma warning disable 618
			_app.SwipeRight();
#pragma warning restore 618
		}

		public void SwipeLeftToRight(double swipePercentage = 0.67, int swipeSpeed = 500, bool withInertia = true)
		{
			_app.SwipeLeftToRight(swipePercentage, swipeSpeed, withInertia);
		}

		public void SwipeLeftToRight(string marked, double swipePercentage = 0.67, int swipeSpeed = 500, bool withInertia = true)
		{
			_app.SwipeLeftToRight(marked, swipePercentage, swipeSpeed, withInertia);
		}

		public void SwipeLeft()
		{
#pragma warning disable 618
			_app.SwipeLeft();
#pragma warning restore 618
		}

		public void SwipeRightToLeft(double swipePercentage = 0.67, int swipeSpeed = 500, bool withInertia = true)
		{
			_app.SwipeRightToLeft(swipePercentage, swipeSpeed, withInertia);
		}

		public void SwipeRightToLeft(string marked, double swipePercentage = 0.67, int swipeSpeed = 500, bool withInertia = true)
		{
			_app.SwipeRightToLeft(marked, swipePercentage, swipeSpeed, withInertia);
		}

		public void SwipeLeftToRight(Func<AppQuery, AppQuery> query, double swipePercentage = 0.67, int swipeSpeed = 500, bool withInertia = true)
		{
			_app.SwipeLeftToRight(query, swipePercentage, swipeSpeed, withInertia);
		}

		public void SwipeLeftToRight(Func<AppQuery, AppWebQuery> query, double swipePercentage = 0.67, int swipeSpeed = 500, bool withInertia = true)
		{
			_app.SwipeLeftToRight(query, swipePercentage, swipeSpeed, withInertia);
		}

		public void SwipeRightToLeft(Func<AppQuery, AppQuery> query, double swipePercentage = 0.67, int swipeSpeed = 500, bool withInertia = true)
		{
			_app.SwipeRightToLeft(query, swipePercentage, swipeSpeed, withInertia);
		}

		public void SwipeRightToLeft(Func<AppQuery, AppWebQuery> query, double swipePercentage = 0.67, int swipeSpeed = 500, bool withInertia = true)
		{
			_app.SwipeRightToLeft(query, swipePercentage, swipeSpeed, withInertia);
		}

		public void ScrollUp(Func<AppQuery, AppQuery> query = null, ScrollStrategy strategy = ScrollStrategy.Auto, double swipePercentage = 0.67, int swipeSpeed = 500,
			bool withInertia = true)
		{
			_app.ScrollUp(query, strategy, swipePercentage, swipeSpeed, withInertia);
		}

		public void ScrollUp(string withinMarked, ScrollStrategy strategy = ScrollStrategy.Auto, double swipePercentage = 0.67, int swipeSpeed = 500,
			bool withInertia = true)
		{
			_app.ScrollUp(withinMarked, strategy, swipePercentage, swipeSpeed, withInertia);
		}

		public void ScrollDown(Func<AppQuery, AppQuery> withinQuery = null, ScrollStrategy strategy = ScrollStrategy.Auto, double swipePercentage = 0.67,
			int swipeSpeed = 500, bool withInertia = true)
		{
			_app.ScrollDown(withinQuery, strategy, swipePercentage, swipeSpeed, withInertia);
		}

		public void ScrollDown(string withinMarked, ScrollStrategy strategy = ScrollStrategy.Auto, double swipePercentage = 0.67, int swipeSpeed = 500,
			bool withInertia = true)
		{
			_app.ScrollDown(withinMarked, strategy, swipePercentage, swipeSpeed, withInertia);
		}

		public void ScrollTo(string toMarked, string withinMarked = null, ScrollStrategy strategy = ScrollStrategy.Auto, double swipePercentage = 0.67,
			int swipeSpeed = 500, bool withInertia = true, TimeSpan? timeout = null)
		{
			_app.ScrollTo(toMarked, withinMarked, strategy, swipePercentage, swipeSpeed, withInertia, timeout);
		}

		public void ScrollUpTo(string toMarked, string withinMarked = null, ScrollStrategy strategy = ScrollStrategy.Auto,
			double swipePercentage = 0.67, int swipeSpeed = 500, bool withInertia = true, TimeSpan? timeout = null)
		{
			_app.ScrollUpTo(toMarked, withinMarked, strategy, swipePercentage, swipeSpeed, withInertia, timeout);
		}

		public void ScrollUpTo(Func<AppQuery, AppWebQuery> toQuery, string withinMarked, ScrollStrategy strategy = ScrollStrategy.Auto, double swipePercentage = 0.67,
			int swipeSpeed = 500, bool withInertia = true, TimeSpan? timeout = null)
		{
			_app.ScrollUpTo(toQuery, withinMarked, strategy, swipePercentage, swipeSpeed, withInertia, timeout);
		}

		public void ScrollDownTo(string toMarked, string withinMarked = null, ScrollStrategy strategy = ScrollStrategy.Auto,
			double swipePercentage = 0.67, int swipeSpeed = 500, bool withInertia = true, TimeSpan? timeout = null)
		{
			_app.ScrollDownTo(toMarked, withinMarked, strategy, swipePercentage, swipeSpeed, withInertia, timeout);
		}

		public void ScrollDownTo(Func<AppQuery, AppWebQuery> toQuery, string withinMarked, ScrollStrategy strategy = ScrollStrategy.Auto, double swipePercentage = 0.67,
			int swipeSpeed = 500, bool withInertia = true, TimeSpan? timeout = null)
		{
			_app.ScrollDownTo(toQuery, withinMarked, strategy, swipePercentage, swipeSpeed, withInertia, timeout);
		}

		public void ScrollUpTo(Func<AppQuery, AppQuery> toQuery, Func<AppQuery, AppQuery> withinQuery = null, ScrollStrategy strategy = ScrollStrategy.Auto, double swipePercentage = 0.67,
			int swipeSpeed = 500, bool withInertia = true, TimeSpan? timeout = null)
		{
			_app.ScrollUpTo(toQuery, withinQuery, strategy, swipePercentage, swipeSpeed, withInertia, timeout);
		}

		public void ScrollUpTo(Func<AppQuery, AppWebQuery> toQuery, Func<AppQuery, AppQuery> withinQuery = null, ScrollStrategy strategy = ScrollStrategy.Auto, double swipePercentage = 0.67,
			int swipeSpeed = 500, bool withInertia = true, TimeSpan? timeout = null)
		{
			_app.ScrollUpTo(toQuery, withinQuery, strategy, swipePercentage, swipeSpeed, withInertia, timeout);
		}

		public void ScrollDownTo(Func<AppQuery, AppQuery> toQuery, Func<AppQuery, AppQuery> withinQuery = null, ScrollStrategy strategy = ScrollStrategy.Auto, double swipePercentage = 0.67,
			int swipeSpeed = 500, bool withInertia = true, TimeSpan? timeout = null)
		{
			_app.ScrollDownTo(toQuery, withinQuery, strategy, swipePercentage, swipeSpeed, withInertia, timeout);
		}

		public void ScrollDownTo(Func<AppQuery, AppWebQuery> toQuery, Func<AppQuery, AppQuery> withinQuery = null, ScrollStrategy strategy = ScrollStrategy.Auto, double swipePercentage = 0.67,
			int swipeSpeed = 500, bool withInertia = true, TimeSpan? timeout = null)
		{
			_app.ScrollDownTo(toQuery, withinQuery, strategy, swipePercentage, swipeSpeed, withInertia, timeout);
		}

		public void SetOrientationPortrait()
		{
			_app.SetOrientationPortrait();
		}

		public void SetOrientationLandscape()
		{
			_app.SetOrientationLandscape();
		}

		public void Repl()
		{
			_app.Repl();
		}

		public void Back()
		{
			_app.Back();
		}

		public void PressVolumeUp()
		{
			_app.PressVolumeUp();
		}

		public void PressVolumeDown()
		{
			_app.PressVolumeDown();
		}

		public object Invoke(string methodName, object argument = null)
		{
			return _app.Invoke(methodName, argument);
		}

		public object Invoke(string methodName, object[] arguments)
		{
			return _app.Invoke(methodName, arguments);
		}

		public void DragCoordinates(float fromX, float fromY, float toX, float toY)
		{
			_app.DragCoordinates(fromX, fromY, toX, toY);
		}

		public void DragAndDrop(Func<AppQuery, AppQuery> @from, Func<AppQuery, AppQuery> to)
		{
			_app.DragAndDrop(@from, to);
		}

		public void DragAndDrop(string @from, string to)
		{
			_app.DragAndDrop(@from, to);
		}

		public void SetSliderValue(string marked, double value)
		{
			_app.SetSliderValue(marked, value);
		}

		public void SetSliderValue(Func<AppQuery, AppQuery> query, double value)
		{
			_app.SetSliderValue(query, value);
		}

		public AppPrintHelper Print
		{
			get { return _app.Print; }
		}

		public IDevice Device
		{
			get { return _app.Device; }
		}

		public ITestServer TestServer
		{
			get { return _app.TestServer; }
		}
	}
}
#endif