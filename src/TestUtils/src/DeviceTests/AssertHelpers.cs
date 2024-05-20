using System;
using System.Threading.Tasks;
using Xunit.Sdk;

namespace Microsoft.Maui.DeviceTests
{
	public static class AssertHelpers
	{
		public static async Task AssertEventually(Func<bool> assertion, int timeout = 1000, int interval = 100, string message = "Assertion timed out")
		{
			do
			{
				if (assertion())
				{
					return;
				}

				await Task.Delay(interval);
				timeout -= interval;

			}
			while (timeout >= 0);

			if (!assertion())
			{
				throw new XunitException(message);
			}
		}

		public static async Task AssertEventually(Func<Task<bool>> assertion, int timeout = 1000, int interval = 100, string message = "Assertion timed out")
		{
			do
			{
				if (await assertion())
				{
					return;
				}

				await Task.Delay(interval);
				timeout -= interval;

			}
			while (timeout >= 0);

			if (!(await assertion()))
			{
				throw new XunitException(message);
			}
		}

		public static async Task<bool> Wait(Func<bool> predicate, int timeout = 1000, int interval = 100)
		{
			do
			{
				if (predicate())
				{
					return true;
				}

				await Task.Delay(interval);
				timeout -= interval;

			}
			while (timeout >= 0);

			return predicate();
		}
	}
}
