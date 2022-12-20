using System;
using System.Threading;
using System.Threading.Tasks;
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

#if !TIZEN

		public static Task WaitForKeyboardToShow(this IView view, int timeout = 1000)
		{
#if !PLATFORM
			return Task.CompletedTask;
#else
			return view.ToPlatform().WaitForKeyboardToShow(timeout);
#endif
		}

		public static Task WaitForKeyboardToHide(this IView view, int timeout = 1000)
		{
#if !PLATFORM
			return Task.CompletedTask;
#else
			return view.ToPlatform().WaitForKeyboardToHide(timeout);
#endif
		}

		/// <summary>
		/// Shane: I haven't fully tested this API. I was trying to use this to send "ReturnType"
		/// and then found the correct API. But, I figured this would be useful to have so I left it here
		/// so a future tester can hopefully use it and be successful!
		/// </summary>
		/// <param name="view"></param>
		/// <param name="value"></param>
		/// <param name="timeout"></param>
		/// <returns></returns>
		public static Task SendValueToKeyboard(this IView view, char value, int timeout = 1000)
		{
#if !PLATFORM
			return Task.CompletedTask;
#else
			return view.ToPlatform().SendValueToKeyboard(value, timeout);
#endif
		}


		public static Task SendKeyboardReturnType(this IView view, ReturnType returnType, int timeout = 1000)
		{
#if !PLATFORM
			return Task.CompletedTask;
#else
			return view.ToPlatform().SendKeyboardReturnType(returnType, timeout);
#endif
		}

		public static Task ShowKeyboardForView(this IView view, int timeout = 1000)
		{
#if !PLATFORM
			return Task.CompletedTask;
#else
			return view.ToPlatform().ShowKeyboardForView(timeout);
#endif
		}

		public static Task WaitForFocused(this IView view, int timeout = 1000)
		{
#if !PLATFORM
			return Task.CompletedTask;
#else
			return view.ToPlatform().WaitForFocused(timeout);
#endif
		}

		public static Task FocusView(this IView view, int timeout = 1000)
		{

#if !PLATFORM
			return Task.CompletedTask;
#else
			return view.ToPlatform().FocusView(timeout);
#endif
		}

#if PLATFORM
		public static bool IsAccessibilityElement(this IView view) =>
			view.ToPlatform().IsAccessibilityElement();


		public static bool IsExcludedWithChildren(this IView view) =>
			view.ToPlatform().IsExcludedWithChildren();
#endif

#endif

	}
}