using NUnit.Framework;
using UITest.Appium.NUnit;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests
{
	public static class IUITestContextExtensions
	{
		public static void IgnoreIfPlatforms(this UITestBase? context, IEnumerable<TestDevice> devices, string? message = null)
		{
			if (context is null)
			{
				Assert.Ignore();
				return;
			}

			foreach (var device in devices)
				context.IgnoreIfPlatform(device, message);
		}

		public static void IgnoreIfPlatform(this UITestBase? context, TestDevice device, string? message = null)
		{
			if (context is null)
			{
				Assert.Ignore();
				return;
			}

			if (context.Device == device)
			{
				if (string.IsNullOrEmpty(message))
					Assert.Ignore();
				else
					Assert.Ignore(message);
			}
		}
	}
}

