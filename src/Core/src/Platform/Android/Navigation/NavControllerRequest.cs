using System;
using System.Collections.Generic;
using System.Text;
using Android.OS;
using AndroidX.Navigation;

namespace Microsoft.Maui.Platform
{
	public record NavControllerNavigateToResIdRequest(
		StackNavigationManager StackNavigationManager, NavigationRequest NavigationRequest, int ResId, Bundle? Args, NavOptions? NavOptions, Navigator.IExtras? NavigatorExtras);

	public record NavControllerPopBackStackRequest(
		StackNavigationManager StackNavigationManager, NavigationRequest NavigationRequest, int? DestinationId, bool? Inclusive);
}
