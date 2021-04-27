using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;
using WImage = Microsoft.UI.Xaml.Controls.Image;
using NativeAutomationProperties = Microsoft.UI.Xaml.Automation.AutomationProperties;

namespace Microsoft.Maui.Controls.Platform
{
	// TODO MAUI
	// this needs to be fixed to handle multi page
	static internal class ToolbarManager
	{
		static ToolbarTracker _toolbarTracker = new ToolbarTracker();
		static ImageConverter _imageConverter = new ImageConverter();
		static ImageSourceIconElementConverter _imageSourceIconElementConverter = new ImageSourceIconElementConverter();

		static internal async Task UpdateToolbarItems(NavigationPage page)
		{
			_toolbarTracker.Target = page.CurrentPage;
			var toolbarProvider = GetToolbarProvider(page);

			if (toolbarProvider == null)
			{
				return;
			}

			var commandBar = await toolbarProvider.GetCommandBarAsync();

			if (commandBar == null)
			{
				return;
			}

			commandBar.PrimaryCommands.Clear();
			commandBar.SecondaryCommands.Clear();

			var toolBarForegroundBinder = GetToolbarProvider(page) as IToolBarForegroundBinder;

			foreach (ToolbarItem item in _toolbarTracker.ToolbarItems)
			{
				toolBarForegroundBinder?.BindForegroundColor(commandBar);

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

				button.Command = new MenuItemCommand(item);
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
					toolBarForegroundBinder?.BindForegroundColor(button);
					commandBar.PrimaryCommands.Add(button);
				}
				else
				{
					commandBar.SecondaryCommands.Add(button);
				}
			}
		}

		static internal IToolbarProvider GetToolbarProvider(Page page)
		{
			IToolbarProvider provider = null;
			
			Page element = page;
			while (element != null)
			{
				provider = element.Handler as IToolbarProvider;
				if (provider != null)
					break;

				var pageContainer = element as IPageContainer<Page>;
				element = pageContainer?.CurrentPage;
			}

			return provider;
		}

	}
}
