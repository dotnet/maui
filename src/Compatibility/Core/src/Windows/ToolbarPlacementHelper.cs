using System;
using Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific;
using Microsoft.Maui.Devices;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using WBorder = Microsoft.UI.Xaml.Controls.Border;

namespace Microsoft.Maui.Controls.Compatibility.Platform.UWP
{
	internal class ToolbarPlacementHelper
	{
		WBorder _bottomCommandBarArea;
		CommandBar _commandBar;
		Func<ToolbarPlacement> _getToolbarPlacement;
		WBorder _titleArea;
		WBorder _topCommandBarArea;

		public void Initialize(CommandBar commandBar, Func<ToolbarPlacement> getToolbarPlacement,
			Func<string, DependencyObject> getTemplateChild)
		{
			_commandBar = commandBar;
			_getToolbarPlacement = getToolbarPlacement;
			_bottomCommandBarArea = getTemplateChild("BottomCommandBarArea") as WBorder;
			_topCommandBarArea = getTemplateChild("TopCommandBarArea") as WBorder;
			_titleArea = getTemplateChild("TitleArea") as WBorder;

			if (_commandBar != null && _bottomCommandBarArea != null && _topCommandBarArea != null)
			{
				// We have to wait for the command bar to load so that it'll be in the control hierarchy
				// otherwise we can't properly move it to wherever the toolbar is supposed to be
				_commandBar.Loaded += (sender, args) =>
				{
					UpdateToolbarPlacement();
					UpdateIsInValidLocation();
				};
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

		static void UpdateToolbarPlacement(CommandBar toolbar, ToolbarPlacement toolbarPlacement, WBorder bottomCommandBarArea,
			WBorder topCommandBarArea, WBorder titleArea)
		{
			// Figure out what's hosting the command bar right now
			var current = toolbar.Parent as WBorder;

			// And figure out where it should be
			WBorder target;

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
					target = DeviceInfo.Idiom == DeviceIdiom.Phone ? bottomCommandBarArea : topCommandBarArea;
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

		// For the time being, keeping this logic for dealing with consistency between the platforms
		// re: toolbar visibility here; at some point we should be separating toolbars from navigation bars,
		// and this won't be necessary
		bool _shouldShowToolBar;
		public bool ShouldShowToolBar
		{
			get { return _shouldShowToolBar; }
			set
			{
				_shouldShowToolBar = value;
				UpdateIsInValidLocation();
			}
		}

		void UpdateIsInValidLocation()
		{
			var formsCommandBar = _commandBar as FormsCommandBar;
			formsCommandBar?.IsInValidLocation = ShouldShowToolBar;
		}
	}
}