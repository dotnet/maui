using System;
using Android.Views;
using Object = Java.Lang.Object;

namespace Xamarin.Forms.Platform.Android
{
	internal class GenericMenuClickListener : Object, IMenuItemOnMenuItemClickListener
	{
		readonly Action _callback;

		public GenericMenuClickListener(Action callback)
		{
			_callback = callback;
		}

		public bool OnMenuItemClick(IMenuItem item)
		{
			_callback();
			return true;
		}
	}
}