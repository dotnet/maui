using System;

namespace Xamarin.Forms
{
	// These are copied from UWP so if you add additional values please use those
	// https://docs.microsoft.com/en-us/uwp/api/windows.applicationmodel.datatransfer.datapackageoperation?view=winrt-19041
	[Flags]
	public enum DataPackageOperation
	{
		None = 0,
		Copy = 1
	}
}