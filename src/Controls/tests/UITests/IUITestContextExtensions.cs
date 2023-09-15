using System;
using Microsoft.Maui.Appium;
using NUnit.Framework;
using TestUtils.Appium.UITests;

namespace Microsoft.Maui.AppiumTests
{
	public static class IUITestContextExtensions
	{
		public static void IgnoreIfPlatforms(this IUITestContext? context, IEnumerable<TestDevice> devices, string? message = null)
		{
			foreach (var device in devices)
			{
				context?.IgnoreIfPlatform(device, message);
			}
		}

		public static void IgnoreIfPlatform(this IUITestContext? context, TestDevice device, string? message = null)
		{
			if (context != null && context.TestConfig.TestDevice == device)
			{
				if (string.IsNullOrEmpty(message))
					Assert.Ignore();
				else
					Assert.Ignore(message);
			}
		}
	}
}

