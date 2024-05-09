using Microsoft.Maui.Controls;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests
{
	internal static class PlatformMethods
	{
		public static Tuple<string[], bool> GetPlatformPropertyQuery(this BindableProperty bindableProperty, TestDevice device)
		{
			return device switch
			{
				TestDevice.Android => PlatformMethodQueries.AndroidPropertyPlatformMethodDictionary[bindableProperty],
				TestDevice.Windows => PlatformMethodQueries.WindowsPropertyPlatformMethodDictionary[bindableProperty],
				_ => PlatformMethodQueries.ApplePropertyPlatformMethodDictionary[bindableProperty],
			};
		}
	}
}