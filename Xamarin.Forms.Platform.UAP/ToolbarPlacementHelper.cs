using Windows.UI.Xaml.Controls;
using Xamarin.Forms.PlatformConfiguration.WindowsSpecific;

namespace Xamarin.Forms.Platform.UWP
{
	internal class ToolbarPlacementHelper
	{
		public static void UpdateToolbarPlacement(CommandBar toolbar, ToolbarPlacement toolbarPlacement, Border bottomCommandBarArea, Border topCommandBarArea)
		{
			if (toolbar == null || bottomCommandBarArea == null || topCommandBarArea == null)
			{
				// Haven't applied the template yet, so we're not ready to do this
				return;
			}

			// Figure out what's hosting the command bar right now
			var current = toolbar.Parent as Border;

			// And figure out where it should be
			Border target;

			switch (toolbarPlacement)
			{
				case ToolbarPlacement.Top:
					target = topCommandBarArea;
					break;
				case ToolbarPlacement.Bottom:
					target = bottomCommandBarArea;
					break;
				case ToolbarPlacement.Default:
				default:
					target = Device.Idiom == TargetIdiom.Phone ? bottomCommandBarArea : topCommandBarArea;
					break;
			}

			if (current == null || target == null || current == target)
			{
				return;
			}

			// Remove the command bar from its current host and add it to the new one
			current.Child = null;
			target.Child = toolbar;
		}
	}
}