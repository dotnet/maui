// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Android.OS;
using Android.Runtime;
using Android.Views;
using AndroidX.Navigation;
using AndroidX.Navigation.Fragment;

namespace Microsoft.Maui.Platform
{
	[Register("microsoft.maui.platform.MauiNavHostFragment")]
	class MauiNavHostFragment : NavHostFragment
	{
		public StackNavigationManager? StackNavigationManager { get; set; }

		public MauiNavHostFragment()
		{
		}

		protected MauiNavHostFragment(nint javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
		{
		}
	}
}
