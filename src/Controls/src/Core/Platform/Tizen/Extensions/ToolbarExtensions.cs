using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui.Graphics;
using Tizen.NUI.BaseComponents;
using Tizen.UIExtensions.NUI;
using GColor = Microsoft.Maui.Graphics.Color;
using MaterialIcons = Tizen.UIExtensions.Common.GraphicsView.MaterialIcons;
using NView = Tizen.NUI.BaseComponents.View;
using TButton = Tizen.UIExtensions.NUI.Button;
using TColor = Tizen.UIExtensions.Common.Color;
using TDeviceInfo = Tizen.UIExtensions.Common.DeviceInfo;
using TImage = Tizen.UIExtensions.NUI.Image;
using TMaterialIconButton = Tizen.UIExtensions.NUI.GraphicsView.MaterialIconButton;

namespace Microsoft.Maui.Controls.Platform
{
	internal static class ToolbarExtensions
	{
		const double s_toolbarItemTextSize = 16d;
		const double s_toolbarItemMaxWidth = 80d;
		static readonly TColor s_defaultBackgroundColor = TColor.FromHex("#2196f3");

		public static void UpdateIsVisible(this MauiToolbar platformToolbar, Toolbar toolbar)
		{
			if (toolbar.IsVisible)
			{
				platformToolbar.Expand();
			}
			else
			{
				platformToolbar.Collapse();
			}
		}

		public static void UpdateTitleIcon(this MauiToolbar platformToolbar, Toolbar toolbar)
		{
			_ = toolbar?.Handler?.MauiContext ?? throw new ArgumentNullException(nameof(toolbar.Handler.MauiContext));

			ImageSource source = toolbar.TitleIcon;

			if (source == null || source.IsEmpty)
			{
				if (!toolbar.BackButtonVisible && !toolbar.DrawerToggleVisible)
				{
					platformToolbar.Icon = null;
				}
				return;
			}

			source.LoadImage(toolbar.Handler.MauiContext, (result) =>
			{
				if (result?.Value != null)
				{
					var image = new TImage
					{
						ResourceUrl = result.Value.ResourceUrl,
					};
					platformToolbar.Icon = image;
				}
			});
		}

		public static void UpdateBackButton(this MauiToolbar platformToolbar, Toolbar toolbar)
		{
			if (toolbar.BackButtonVisible)
			{
				var backButton = CreateBackButton(platformToolbar, toolbar);
				backButton.Clicked += (s, e) => platformToolbar.SendIconPressed();
				platformToolbar.Icon = backButton;
			}
			else if (toolbar.DrawerToggleVisible)
			{
				var menuButton = CreateMenuButton(platformToolbar, toolbar);
				menuButton.Clicked += (s, e) => platformToolbar.SendIconPressed();
				platformToolbar.Icon = menuButton;
			}
			else if (toolbar.TitleIcon == null)
			{
				platformToolbar.Icon = null;
			}
		}

		public static void UpdateBarBackgroundColor(this MauiToolbar platformToolbar, Toolbar toolbar)
		{
			var barBackground = toolbar.BarBackground;

			// TODO. to support brush
			if (barBackground is SolidColorBrush solidColor)
			{
				platformToolbar.BackgroundColor = solidColor.Color.ToPlatform().ToNative();
			}
			else
			{
				platformToolbar.UpdateBackgroundColor(s_defaultBackgroundColor);
			}

			if (platformToolbar.Icon != null && platformToolbar.Icon is TMaterialIconButton button)
			{
				button.Color = toolbar.IconColor.IsNotDefault() ? toolbar.IconColor.ToPlatform() : platformToolbar.GetAccentColor();
			}
		}

		public static void UpdateBarTextColor(this MauiToolbar platformToolbar, Toolbar toolbar)
		{
			var textColor = toolbar.BarTextColor;
			platformToolbar.Label.TextColor = textColor.IsNotDefault() ? textColor.ToPlatform() : platformToolbar.GetAccentColor();
		}

		public static void UpdateBarTextColor(this MauiToolbar platformToolbar, GColor color)
		{
			platformToolbar.Label.TextColor = color.IsNotDefault() ? color.ToPlatform() : platformToolbar.GetAccentColor();
		}

		public static void UpdateBarIconColor(this MauiToolbar platformToolbar, GColor color)
		{
			if (platformToolbar.Icon != null && platformToolbar.Icon is TMaterialIconButton button)
			{
				button.Color = color.IsNotDefault() ? color.ToPlatform() : platformToolbar.GetAccentColor();
			}
		}

		public static void UpdateBarBackgroundColor(this MauiToolbar platformToolbar, GColor color)
		{
			platformToolbar.BackgroundColor = color.IsNotDefault() ? color.ToPlatform().ToNative() : s_defaultBackgroundColor.ToNative();
		}

		public static void UpdateMenuItems(this MauiToolbar platformToolbar, Toolbar toolbar)
		{
			platformToolbar.Actions.Clear();

			if (toolbar.ToolbarItems == null)
				return;

			foreach (var action in GetPrimaryActionButtons(platformToolbar, toolbar.ToolbarItems))
			{
				platformToolbar.Actions.Add(action);
			}

			var secondaryActions = toolbar.ToolbarItems.Where(i => i.Order == ToolbarItemOrder.Secondary).OrderBy(i => i.Priority);
			if (secondaryActions.Any())
			{
				var more = CreateMoreButton(platformToolbar, toolbar);
				more.Clicked += async (s, e) =>
				{
					var actions = secondaryActions.ToList();
					var actionTexts = actions.Select(i => i.Text).ToList();
					var modalStack = toolbar?.Handler.MauiContext?.GetModalStack();
					if (modalStack != null)
					{
						await modalStack.PushDummyPopupPage(async () =>
						{
							try
							{
								using var popup = new ActionSheetPopup("", "Cancel", null, buttons: actionTexts);
								var select = actionTexts.IndexOf(await popup.Open());
								actions[select].Command.Execute(actions[select].CommandParameter);
							}
							catch
							{
								// Cancel
							}
						});
					}
				};
				platformToolbar.Actions.Add(more);
			}
		}

		static TColor GetAccentColor(this MauiToolbar platformToolbar)
		{
			var grayscale = (platformToolbar.BackgroundColor.R + platformToolbar.BackgroundColor.G + platformToolbar.BackgroundColor.B) / 3.0f;
			return grayscale > 0.6 ? TColor.Black : TColor.White;
		}

		static IEnumerable<NView> GetPrimaryActionButtons(MauiToolbar platformToolbar, IEnumerable<ToolbarItem> toolbarItems)
		{
			return toolbarItems.Where(i => i.Order <= ToolbarItemOrder.Primary).OrderBy(i => i.Priority).Select(i => CreateToolbarButton(platformToolbar, i));
		}

		static TMaterialIconButton CreateBackButton(MauiToolbar platformToolbar, Toolbar toolbar)
		{
			var button = new TMaterialIconButton
			{
				Icon = MaterialIcons.ArrowBack,
				Color = toolbar.IconColor.IsNotDefault() ? toolbar.IconColor.ToPlatform() : platformToolbar.GetAccentColor()
			};
			return button;
		}

		static TMaterialIconButton CreateMenuButton(MauiToolbar platformToolbar, Toolbar toolbar)
		{
			var button = new TMaterialIconButton
			{
				Icon = MaterialIcons.Menu,
				Color = toolbar.IconColor.IsNotDefault() ? toolbar.IconColor.ToPlatform() : platformToolbar.GetAccentColor()
			};
			return button;
		}

		static TMaterialIconButton CreateMoreButton(MauiToolbar platformToolbar, Toolbar toolbar)
		{
			var button = new TMaterialIconButton
			{
				Icon = MaterialIcons.MoreVert,
				Color = toolbar.IconColor.IsNotDefault() ? toolbar.IconColor.ToPlatform() : platformToolbar.GetAccentColor()
			};
			return button;
		}


		static NView CreateToolbarButton(MauiToolbar platformToolbar, ToolbarItem item)
		{
			var grayscale = (platformToolbar.BackgroundColor.R + platformToolbar.BackgroundColor.G + platformToolbar.BackgroundColor.B) / 3.0f;
			var accentColor = grayscale > 0.6 ? TColor.Black : TColor.White;

			var button = new TButton
			{
				FontSize = s_toolbarItemTextSize.ToScaledPoint(),
				Text = item.Text,
				TextColor = accentColor,
				HeightSpecification = LayoutParamPolicies.MatchParent,
				WidthSpecification = LayoutParamPolicies.WrapContent,
			};
			button.SizeWidth = (float)button.Measure(TDeviceInfo.ScalingFactor * s_toolbarItemMaxWidth, double.PositiveInfinity).Width;
			button.UpdateBackgroundColor(TColor.Transparent);

			if (item.IconImageSource != null)
			{
				button.Text = string.Empty;
				button.Icon.AdjustViewSize = true;
				button.Icon.HeightSpecification = LayoutParamPolicies.MatchParent;
				_ = button.Icon.LoadImage(item.IconImageSource);
				button.SizeWidth = 0;
				button.WidthSpecification = LayoutParamPolicies.WrapContent;
			}
			button.Clicked += (s, e) =>
			{
				item.Command?.Execute(item.CommandParameter);
			};
			return button;
		}
	}
}
