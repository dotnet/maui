using System;
using System.Collections.Generic;
using System.Text;
using AndroidX.Navigation.Fragment;
using AndroidX.Navigation;
using Java.Lang;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Platform
{
	internal static partial class NavigationViewExtensions
	{

		public static void IterateBackStack(this NavController navController, Action<FragmentNavigator.Destination> action)
		{
			var iterator = navController.Graph.Iterator();

			while (iterator.HasNext)
			{
				if (iterator.Next() is FragmentNavigator.Destination nvd)
				{
					try
					{
						if (navController.GetBackStackEntry(nvd.Id).Destination is FragmentNavigator.Destination found)
							action.Invoke(found);
					}
					catch (IllegalArgumentException) { }
				}
			}
		}
	}
}
