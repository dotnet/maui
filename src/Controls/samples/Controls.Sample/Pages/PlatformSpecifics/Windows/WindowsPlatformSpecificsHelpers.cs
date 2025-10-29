#if WINDOWS
using System;
using System.Linq;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample.Pages
{
	internal static class WindowsPlatformSpecificsHelpers
	{
		public static string Title = "Reminders";
		public static string Message = "Tool bar item clicked";
		public static string Dismiss = "OK";

		public static void AddToolBarItems(Microsoft.Maui.Controls.Page page)
		{
			Action action = () => page.DisplayAlertAsync(Title, Message, Dismiss);

			page.ToolbarItems.Add(new ToolbarItem("Primary 1", "calculator.png", action, ToolbarItemOrder.Primary));
			page.ToolbarItems.Add(new ToolbarItem("Primary 2", "calculator.png", action, ToolbarItemOrder.Primary));
			page.ToolbarItems.Add(new ToolbarItem("Secondary 1", "calculator.png", action, ToolbarItemOrder.Secondary));
			page.ToolbarItems.Add(new ToolbarItem("Secondary 2", "calculator.png", action, ToolbarItemOrder.Secondary));
		}

		public static Layout CreateChanger(Type enumType, string defaultOption, Action<Picker> selectedIndexChanged, string text)
		{
			var picker = new Picker { WidthRequest = 100 };
			var placementOptions = Enum.GetNames(enumType);

			foreach (string option in placementOptions)
			{
				picker.Items.Add(option);
			}

			picker.SelectedIndex = Array.IndexOf(placementOptions, defaultOption);
			picker.SelectedIndexChanged += (sender, e) =>
			{
				selectedIndexChanged(picker);
			};

			return new StackLayout
			{
				Orientation = StackOrientation.Horizontal,
				HorizontalOptions = LayoutOptions.Center,
				Children =
				{
					new Microsoft.Maui.Controls.Label { Text = text, VerticalOptions = LayoutOptions.Center },
					picker
				}
			};
		}

		public static Layout CreateToolbarPlacementChanger(Microsoft.Maui.Controls.Page page)
		{
			var enumType = typeof(ToolbarPlacement);

			return CreateChanger(enumType, Enum.GetName(enumType, page.On<Microsoft.Maui.Controls.PlatformConfiguration.Windows>().GetToolbarPlacement())!, picker =>
			{
				page.On<Microsoft.Maui.Controls.PlatformConfiguration.Windows>().SetToolbarPlacement((ToolbarPlacement)Enum.Parse(enumType, picker.Items[picker.SelectedIndex]));
			}, "Select Toolbar Placement");
		}

		public static Layout CreateAddRemoveToolbarItemButtons(Microsoft.Maui.Controls.Page page)
		{
			Action action = () => page.DisplayAlertAsync(Title, Message, Dismiss);

			var primaryButton = new Button { Text = "Add Primary", BackgroundColor = Colors.Gray };
			primaryButton.Clicked += (sender, e) =>
			{
				int index = page.ToolbarItems.Count(item => item.Order == ToolbarItemOrder.Primary) + 1;
				page.ToolbarItems.Add(new ToolbarItem(string.Format("Primary {0}", index), "calculator.png", action, ToolbarItemOrder.Primary));
			};

			var secondaryButton = new Button { Text = "Add Secondary", BackgroundColor = Colors.Gray };
			secondaryButton.Clicked += (sender, e) =>
			{
				int index = page.ToolbarItems.Count(item => item.Order == ToolbarItemOrder.Secondary) + 1;
				page.ToolbarItems.Add(new ToolbarItem(string.Format("Secondary {0}", index), "calculator.png", action, ToolbarItemOrder.Secondary));
			};

			var removeButton = new Button { Text = "Remove", BackgroundColor = Colors.Gray };
			removeButton.Clicked += (sender, e) =>
			{
				if (page.ToolbarItems.Any())
				{
					page.ToolbarItems.RemoveAt(0);
				}
			};

			return new StackLayout
			{
				Orientation = StackOrientation.Horizontal,
				HorizontalOptions = LayoutOptions.Center,
				Children = { primaryButton, secondaryButton, removeButton }
			};
		}
	}
}
#endif