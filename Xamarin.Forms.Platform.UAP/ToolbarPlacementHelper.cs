using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Xamarin.Forms.PlatformConfiguration.WindowsSpecific;

namespace Xamarin.Forms.Platform.UWP
{
	internal class ToolbarPlacementHelper
	{
		Border _bottomCommandBarArea;
		CommandBar _commandBar;
		Func<ToolbarPlacement> _getToolbarPlacement;
		Border _titleArea;
		Border _topCommandBarArea;

		public void Initialize(CommandBar commandBar, Func<ToolbarPlacement> getToolbarPlacement,
			Func<string, DependencyObject> getTemplateChild)
		{
			_commandBar = commandBar;
			_getToolbarPlacement = getToolbarPlacement;
			_bottomCommandBarArea = getTemplateChild("BottomCommandBarArea") as Border;
			_topCommandBarArea = getTemplateChild("TopCommandBarArea") as Border;
			_titleArea = getTemplateChild("TitleArea") as Border;

			if (_commandBar != null && _bottomCommandBarArea != null && _topCommandBarArea != null)
			{
				// We have to wait for the command bar to load so that it'll be in the control hierarchy
				// otherwise we can't properly move it to wherever the toolbar is supposed to be
				_commandBar.Loaded += (sender, args) => UpdateToolbarPlacement();
			}
		}

		public void UpdateToolbarPlacement()
		{
			if (_commandBar == null || _getToolbarPlacement == null || _bottomCommandBarArea == null ||
			    _topCommandBarArea == null)
			{
				// Template hasn't been applied yet, so we're not ready to update the toolbar placement
				return;
			}

			UpdateToolbarPlacement(_commandBar, _getToolbarPlacement(), _bottomCommandBarArea, _topCommandBarArea, _titleArea);
		}

		static void UpdateToolbarPlacement(CommandBar toolbar, ToolbarPlacement toolbarPlacement, Border bottomCommandBarArea,
			Border topCommandBarArea, Border titleArea)
		{
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

			if (titleArea != null)
			{
				if (target == bottomCommandBarArea)
				{
					// If the title is hosted in the command bar and we're moving the command bar to the bottom,
					// put the title into the topCommandBarArea
					toolbar.Content = null;
					topCommandBarArea.Child = titleArea;
				}
				else
				{
					// Put the title back into the command bar
					toolbar.Content = titleArea;
				}
			}
		}
	}
}