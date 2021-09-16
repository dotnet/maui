using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific;
using Microsoft.UI.Xaml.Controls;
using WImage = Microsoft.UI.Xaml.Controls.Image;
using NativeAutomationProperties = Microsoft.UI.Xaml.Automation.AutomationProperties;

namespace Microsoft.Maui.Controls.Platform
{
	public class ControlsNavigationManager : NavigationManager
	{
		readonly ImageConverter _imageConverter = new ImageConverter();
		readonly ImageSourceIconElementConverter _imageSourceIconElementConverter = new ImageSourceIconElementConverter();
		public new Page CurrentPage => (Page)base.CurrentPage;
		public new NavigationPage NavigationView => (NavigationPage)base.NavigationView;

		public ControlsNavigationManager(IMauiContext mauiContext) : base(mauiContext)
		{
		}

		internal void ToolbarPropertyChanged() => UpdateToolbar();

		public override void NavigateTo(NavigationRequest arg3)
		{
			base.NavigateTo(arg3);
			UpdateToolbar();
		}

		// TODO MAUI: This will need to be updated to handle multiple nested navigation pages
		protected virtual void UpdateToolbar()
		{
			if (WindowManager.RootView is not MauiNavigationView navigationView || NavigationStack.Count == 0)
				return;

			var commandBar = WindowManager.GetCommandBar();
			var header = navigationView.Header as WindowHeader;

			// TODO MAUI: these only apply for the top level NavigationManager
			if (commandBar != null)
				commandBar.IsDynamicOverflowEnabled = NavigationView.OnThisPlatform().GetToolbarDynamicOverflowEnabled();

			bool hasNavigationBar = NavigationPage.GetHasNavigationBar(CurrentPage);
			bool hasBackButton = NavigationPage.GetHasBackButton(CurrentPage) && NavigationStack.Count > 1;
			var title = CurrentPage.Title;
			var titleIcon = NavigationPage.GetTitleIconImageSource(CurrentPage);
			var titleView = NavigationPage.GetTitleView(CurrentPage);

			var barBackground = NavigationView.BarBackground;
			var barBackgroundColor = NavigationView.BarBackgroundColor;
			var barTextColor = NavigationView.BarTextColor;

			// TODO MAUI: it seems like this isn't wired up on WinUI
			//var iconColor = NavigationPage.GetIconColor(CurrentPage);

			// TODO MAUI: Should be able to just modify the GRID inside NavigationLayout to move header to footer
			// Or we add a control in the footer
			var toolbarPlacement = NavigationView.OnThisPlatform().GetToolbarPlacement();

			if (header != null)
			{
				header.Visibility = (hasNavigationBar) ? UI.Xaml.Visibility.Visible : UI.Xaml.Visibility.Collapsed;
				header.Title = title;

				ImageSourceLoader.LoadImage(titleIcon, MauiContext, (result) =>
				{
					header.TitleIcon = result.Value;
				});
				if (barTextColor != null)
					header.TitleColor = barTextColor.ToNative();

				header.TitleView = titleView?.ToNative(MauiContext);
			}

			navigationView.IsBackButtonVisible = (hasBackButton) ? NavigationViewBackButtonVisible.Visible : NavigationViewBackButtonVisible.Collapsed;

			navigationView.UpdateBarBackgroundBrush(
				barBackground?.ToBrush() ?? barBackgroundColor?.ToNative());

			UpdateToolbarItems();
			navigationView.PaneDisplayMode = NavigationViewPaneDisplayMode.LeftMinimal;
			navigationView.IsPaneOpen = false;
		}

		//TODO MAUI: This will need to be updated to handle nested pages propagating up
		internal void UpdateToolbarItems()
		{
			if (WindowManager.RootView is not MauiNavigationView navigationView || NavigationStack.Count == 0)
				return;

			var commandBar = WindowManager.GetCommandBar();

			if (commandBar == null)
			{
				return;
			}

			commandBar.PrimaryCommands.Clear();
			commandBar.SecondaryCommands.Clear();

			List<ToolbarItem> toolbarItems = new List<ToolbarItem>(NavigationView.ToolbarItems);
			toolbarItems.AddRange(CurrentPage.ToolbarItems);

			foreach (ToolbarItem item in toolbarItems)
			{
				var button = new AppBarButton();
				button.SetBinding(AppBarButton.LabelProperty, "Text");

				if (commandBar.IsDynamicOverflowEnabled && item.Order == ToolbarItemOrder.Secondary)
				{
					button.SetBinding(AppBarButton.IconProperty, "IconImageSource", _imageSourceIconElementConverter);
				}
				else
				{
					var img = new WImage();
					img.SetBinding(WImage.SourceProperty, "Value");
					img.SetBinding(WImage.DataContextProperty, "IconImageSource", _imageConverter);
					button.Content = img;
				}

				// TODO WINUI FIX
				//button.Command = new MenuItemCommand(item);
				button.DataContext = item;
				button.SetValue(NativeAutomationProperties.AutomationIdProperty, item.AutomationId);
				button.SetAutomationPropertiesName(item);
				button.SetAutomationPropertiesAccessibilityView(item);
				button.SetAutomationPropertiesHelpText(item);

				// TODO MAUI
				button.SetAutomationPropertiesLabeledBy(item, null);

				ToolbarItemOrder order = item.Order == ToolbarItemOrder.Default ? ToolbarItemOrder.Primary : item.Order;
				if (order == ToolbarItemOrder.Primary)
				{
					commandBar.PrimaryCommands.Add(button);
				}
				else
				{
					commandBar.SecondaryCommands.Add(button);
				}
			}
		}
	}
}