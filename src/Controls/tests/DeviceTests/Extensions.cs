using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace Microsoft.Maui.DeviceTests
{
	public static class Extensions
	{
		public static Task Wait(this Image image, int timeout = 1000) =>
			AssertionExtensions.Wait(() => !image.IsLoading, timeout);
	}
}

