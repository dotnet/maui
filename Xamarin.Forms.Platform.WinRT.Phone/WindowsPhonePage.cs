using System;
using System.ComponentModel;
using Windows.ApplicationModel;

namespace Xamarin.Forms.Platform.WinRT
{
	public abstract class WindowsPhonePage
		: WindowsBasePage
	{
		protected override Platform CreatePlatform ()
		{
			return new WindowsPhonePlatform (this);
		}
	}
}