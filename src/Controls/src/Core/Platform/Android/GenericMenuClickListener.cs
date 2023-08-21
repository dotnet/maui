// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable
using System;
using Android.Views;
using Object = Java.Lang.Object;

namespace Microsoft.Maui.Controls.Platform
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