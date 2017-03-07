using System;
using System.Linq;
using Xamarin.Forms.Internals;
using Xamarin.Forms.PlatformConfiguration;
using Xamarin.Forms.PlatformConfiguration.WindowsSpecific;

namespace Xamarin.Forms.Controls.GalleryPages.PlatformSpecificsGalleries
{
    internal static class WindowsPlatformSpecificsGalleryHelpers
    {
        const string CommandBarActionTitle = "Hey!";
        const string CommandBarActionMessage = "Command Bar Item Clicked";
        const string CommandBarActionDismiss = "OK";

        public static void AddToolBarItems(Page page)
        {
            Action action = () => page.DisplayAlert(CommandBarActionTitle, CommandBarActionMessage, CommandBarActionDismiss);

            var tb1 = new ToolbarItem("Primary 1", "coffee.png", action, ToolbarItemOrder.Primary)
            {
                IsEnabled = true,
                AutomationId = "toolbaritem_primary1"
            };

            var tb2 = new ToolbarItem("Primary 2", "coffee.png", action, ToolbarItemOrder.Primary)
            {
                IsEnabled = true,
                AutomationId = "toolbaritem_primary2"
            };

            var tb3 = new ToolbarItem("Seconday 1", "coffee.png", action, ToolbarItemOrder.Secondary)
            {
                IsEnabled = true,
                AutomationId = "toolbaritem_secondary3"
            };

            var tb4 = new ToolbarItem("Secondary 2", "coffee.png", action, ToolbarItemOrder.Secondary)
            {
                IsEnabled = true,
                AutomationId = "toolbaritem_secondary4"
            };

            page.ToolbarItems.Add(tb1);
            page.ToolbarItems.Add(tb2);
            page.ToolbarItems.Add(tb3);
            page.ToolbarItems.Add(tb4);
        }

        public static Layout CreateChanger(Type enumType, string defaultOption, Action<Picker> selectedIndexChanged, string label)
        {
            var picker = new Picker();
            string[] options = Enum.GetNames(enumType);
            foreach (string option in options)
            {
                picker.Items.Add(option);
            }

            picker.SelectedIndex = options.IndexOf(defaultOption);

            picker.SelectedIndexChanged += (sender, args) =>
            {
                selectedIndexChanged(picker);
            };

            var changerLabel = new Label { Text = label, VerticalOptions = LayoutOptions.Center };

            var layout = new Grid
            {
                HorizontalOptions = LayoutOptions.Center,
                ColumnDefinitions = new ColumnDefinitionCollection
                {
                    new ColumnDefinition { Width = 150 },
                    new ColumnDefinition { Width = 100 }
                },
                Children = { changerLabel, picker }
            };

            Grid.SetColumn(changerLabel, 0);
            Grid.SetColumn(picker, 1);

            return layout;
        }

        public static Layout CreateToolbarPlacementChanger(Page page)
        {
            Type enumType = typeof(ToolbarPlacement);

            return CreateChanger(enumType,
                Enum.GetName(enumType, page.On<Windows>().GetToolbarPlacement()),
                picker =>
                {
                    page.On<Windows>().SetToolbarPlacement((ToolbarPlacement)Enum.Parse(enumType, picker.Items[picker.SelectedIndex]));
                }, "Select Toolbar Placement");
        }

        public static Layout CreateAddRemoveToolBarItemButtons(Page page)
        {
            var layout = new StackLayout { Orientation = StackOrientation.Vertical, HorizontalOptions = LayoutOptions.Center };
            layout.Children.Add(new Label { Text = "Toolbar Items:" });

            var buttonLayout = new StackLayout
            {
                Orientation = StackOrientation.Horizontal,
                HorizontalOptions = LayoutOptions.Center
            };

            layout.Children.Add(buttonLayout);

            var addPrimary = new Button { Text = "Add Primary", BackgroundColor = Color.Gray };
            var addSecondary = new Button { Text = "Add Secondary", BackgroundColor = Color.Gray };
            var remove = new Button { Text = "Remove", BackgroundColor = Color.Gray };

            buttonLayout.Children.Add(addPrimary);
            buttonLayout.Children.Add(addSecondary);
            buttonLayout.Children.Add(remove);

            Action action = () => page.DisplayAlert(CommandBarActionTitle, CommandBarActionMessage, CommandBarActionDismiss);

            addPrimary.Clicked += (sender, args) =>
            {
                int index = page.ToolbarItems.Count(item => item.Order == ToolbarItemOrder.Primary) + 1;
                page.ToolbarItems.Add(new ToolbarItem($"Primary {index}", "coffee.png", action, ToolbarItemOrder.Primary));
            };

            addSecondary.Clicked += (sender, args) =>
            {
                int index = page.ToolbarItems.Count(item => item.Order == ToolbarItemOrder.Secondary) + 1;
                page.ToolbarItems.Add(new ToolbarItem($"Secondary {index}", "coffee.png", action, ToolbarItemOrder.Secondary));
            };

            remove.Clicked += (sender, args) =>
            {
                if (page.ToolbarItems.Any())
                {
                    page.ToolbarItems.RemoveAt(0);
                }
            };

            return layout;
        }
    }
}