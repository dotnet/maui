using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Support.Design.Widget;
using Android.Support.Design.Internal;

#if __ANDROID81__
#else
using ALabelVisibilityMode = Android.Support.Design.BottomNavigation.LabelVisibilityMode;
#endif

namespace Xamarin.Forms.Platform.Android
{
	public static class BottomNavigationViewUtils
	{
		public static void SetShiftMode(this BottomNavigationView bottomNavigationView, bool enableShiftMode, bool enableItemShiftMode)
		{
			try
			{
				using (var menuView = bottomNavigationView.GetChildAt(0) as BottomNavigationMenuView)
				{
					if (menuView == null)
					{
						System.Diagnostics.Debug.WriteLine("Unable to find BottomNavigationMenuView");
						return;
					}

#if __ANDROID81__
					var shiftMode = menuView.Class.GetDeclaredField("mShiftingMode");
					shiftMode.Accessible = true;
					shiftMode.SetBoolean(menuView, enableShiftMode);
					shiftMode.Accessible = false;
					shiftMode.Dispose();
#else
					if (enableShiftMode)
						bottomNavigationView.LabelVisibilityMode = ALabelVisibilityMode.LabelVisibilityAuto;
					else
						bottomNavigationView.LabelVisibilityMode = ALabelVisibilityMode.LabelVisibilityLabeled;
#endif
					for (int i = 0; i < menuView.ChildCount; i++)
					{
						var child = menuView.GetChildAt(i);
						var item = child as BottomNavigationItemView;
						if (item != null)
						{
#if __ANDROID81__
							item.SetShiftingMode(enableItemShiftMode);
#else
							item.SetShifting(enableItemShiftMode);
#endif
							item.SetChecked(item.ItemData.IsChecked);
						}

						child.Dispose();
					}


					menuView.UpdateMenuView();
				}
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"Unable to set shift mode: {ex}");
			}
		}
	}
}