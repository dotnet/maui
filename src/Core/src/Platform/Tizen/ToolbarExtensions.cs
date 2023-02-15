using Tizen.UIExtensions.Common.GraphicsView;
using TColor = Tizen.UIExtensions.Common.Color;
using TMaterialIconButton = Tizen.UIExtensions.NUI.GraphicsView.MaterialIconButton;

namespace Microsoft.Maui.Platform
{
	public static class ToolbarExtensions
	{
		public static void UpdateTitle(this MauiToolbar platformView, IToolbar toolbar)
		{
			platformView.Title = toolbar.Title ?? string.Empty;
		}

		public static void UpdateMenuButton(this MauiToolbar platformToolbar, IToolbar toolbar)
		{
			var menuButton = CreateMenuButton(platformToolbar, toolbar);
			menuButton.Clicked += (s, e) => platformToolbar.SendIconPressed();
			platformToolbar.Icon = menuButton;
		}

		static TMaterialIconButton CreateMenuButton(MauiToolbar platformToolbar, IToolbar toolbar)
		{
			var button = new TMaterialIconButton
			{
				Icon = MaterialIcons.Menu,
				Color = platformToolbar.GetAccentColor()
			};
			return button;
		}

		static TColor GetAccentColor(this MauiToolbar platformToolbar)
		{
			var grayscale = (platformToolbar.BackgroundColor.R + platformToolbar.BackgroundColor.G + platformToolbar.BackgroundColor.B) / 3.0f;
			return grayscale > 0.5 ? TColor.Black : TColor.White;
		}
	}
}
