using System;
using System.Threading.Tasks;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.Platform;
using Xunit;
using Xunit.Sdk;

namespace Microsoft.Maui.DeviceTests
{
	public static partial class AssertionExtensions
	{
		static readonly Random rnd = new Random();

		public static async Task<bool> Wait(Func<bool> exitCondition, int timeout = 1000)
		{
			while ((timeout -= 100) > 0)
			{
				if (!exitCondition.Invoke())
					await Task.Delay(rnd.Next(100, 200));
				else
					break;
			}

			return exitCondition.Invoke();
		}

		public static void AssertHasFlag(this Enum self, Enum flag)
		{
			var hasFlag = self.HasFlag(flag);

			if (!hasFlag)
				throw new ContainsException(flag, self);
		}

		public static void AssertWithMessage(Action assertion, string message)
		{
			try
			{
				assertion();
			}
			catch (Exception e)
			{
				Assert.True(false, $"Message: {message} Failure: {e}");
			}
		}

		public static void CloseEnough(double expected, double actual, double epsilon = 0.2, string? message = null)
		{
			if (!String.IsNullOrWhiteSpace(message))
				message = " " + message;

			var diff = Math.Abs(expected - actual);
			Assert.True(diff <= epsilon, $"Expected: {expected}. Actual: {actual}. Diff: {diff} Epsilon: {epsilon}.{message}");
		}

#if !TIZEN && PLATFORM
		public static Task AssertHasContainer(this IView view, bool expectation)
		{
			// On Windows the `Parent` of an element only initializes when the view is added
			// to the Visual Tree
			var platformViewHandler = (IPlatformViewHandler)view.Handler!;
			var platformView = platformViewHandler.PlatformView!;

#if WINDOWS
			var dispatcher = platformViewHandler.MauiContext!.GetDispatcher();
			return dispatcher.DispatchAsync(async () =>
			{
				if (platformView.XamlRoot is null)
				{
					if (!expectation)
						await AttachAndRun(platformView, RunAssertions);
					else
						await AttachAndRun(platformViewHandler.ContainerView!, RunAssertions);
				}
				else
					RunAssertions();
			});

#else
			RunAssertions();
			return Task.CompletedTask;
#endif
			void RunAssertions()
			{
				Assert.Equal(expectation, view.Handler?.HasContainer ?? false);
				Assert.Equal(expectation, view.Handler?.ContainerView is not null);
				var parentView = platformView?.GetParent();
				Assert.Equal(expectation, parentView is WrapperView);
			}
		}

		public static Task WaitForKeyboardToShow(this IView view, int timeout = 1000)
		{
			return view.ToPlatform().WaitForKeyboardToShow(timeout);
		}

		public static Task WaitForKeyboardToHide(this IView view, int timeout = 1000) =>
			view.ToPlatform().WaitForKeyboardToHide(timeout);

		/// <summary>
		/// Shane: I haven't fully tested this API. I was trying to use this to send "ReturnType"
		/// and then found the correct API. But, I figured this would be useful to have so I left it here
		/// so a future tester can hopefully use it and be successful!
		/// </summary>
		/// <param name="view"></param>
		/// <param name="value"></param>
		/// <param name="timeout"></param>
		/// <returns></returns>
		public static Task SendValueToKeyboard(this IView view, char value, int timeout = 1000) =>
			view.ToPlatform().SendValueToKeyboard(value, timeout);

		public static Task SendKeyboardReturnType(this IView view, ReturnType returnType, int timeout = 1000) =>
			view.ToPlatform().SendKeyboardReturnType(returnType, timeout);

		public static Task ShowKeyboardForView(this IView view, int timeout = 1000) =>
			view.ToPlatform().ShowKeyboardForView(timeout);
		public static Task HideKeyboardForView(this IView view, int timeout = 1000, string? message = null) =>
			view.ToPlatform().HideKeyboardForView(timeout, message);

		public static Task WaitForUnFocused(this IView view, int timeout = 1000) =>
			view.ToPlatform().WaitForUnFocused(timeout);

		public static Task WaitForFocused(this IView view, int timeout = 1000) =>
			view.ToPlatform().WaitForFocused(timeout);

		public static Task FocusView(this IView view, int timeout = 1000) =>
			view.ToPlatform().FocusView(timeout);

		public static bool IsAccessibilityElement(this IView view) =>
			view.ToPlatform().IsAccessibilityElement();

		public static bool IsExcludedWithChildren(this IView view) =>
			view.ToPlatform().IsExcludedWithChildren();
#endif


		public static IDisposable OnUnloaded(this IElement element, Action action)
		{
#if PLATFORM
			if (element.Handler is IPlatformViewHandler platformViewHandler &&
				platformViewHandler.PlatformView is not null)
			{
				return platformViewHandler.PlatformView.OnUnloaded(action);
			}

			throw new InvalidOperationException("Handler is not set on element");
#else
			throw new NotImplementedException();
#endif
		}

		public static IDisposable OnLoaded(this IElement element, Action action)
		{
#if PLATFORM
			if (element.Handler is IPlatformViewHandler platformViewHandler &&
				platformViewHandler.PlatformView is not null)
			{
				return platformViewHandler.PlatformView.OnLoaded(action);
			}

			throw new InvalidOperationException("Handler is not set on element");
#else
			throw new NotImplementedException();
#endif
		}

		public static bool IsLoadedOnPlatform(this IElement element)
		{

#if PLATFORM
			if (element.Handler is not IPlatformViewHandler pvh)
				return false;

			return pvh.PlatformView?.IsLoaded() == true;
#else
			return true;
#endif
		}
	}
}