using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Input;
using Xamarin.Forms.PlatformConfiguration;
using Xamarin.Forms.PlatformConfiguration.WindowsSpecific;
using static Xamarin.Forms.Controls.GalleryPages.PlatformSpecificsGalleries.WindowsPlatformSpecificsGalleryHelpers;

using WindowsOS = Xamarin.Forms.PlatformConfiguration.Windows;

namespace Xamarin.Forms.Controls.GalleryPages.PlatformSpecificsGalleries
{
	public class FlyoutPageWindows : FlyoutPage
	{
		public FlyoutPageWindows(ICommand restore)
		{
			On<WindowsOS>()
				.SetCollapseStyle(CollapseStyle.Partial);
			FlyoutLayoutBehavior = FlyoutLayoutBehavior.Popover;

			var master = new ContentPage { Title = "Flyout Detail Page" };
			var masterContent = new StackLayout { Spacing = 10, Margin = new Thickness(0, 10, 5, 0) };
			var detail = new ContentPage { Title = "This is the detail page's Title" };

			// Build the navigation pane items
			var navItems = new List<NavItem>
			{
				new NavItem("Display Alert", "\uE171", new Command(() => DisplayAlert("Alert", "This is an alert", "OK"))),
				new NavItem("Return To Gallery", "\uE106", restore),
				new NavItem("Save", "\uE105", new Command(() => DisplayAlert("Save", "Fake save dialog", "OK"))),
				new NavItem("Audio", "\uE189", new Command(() => DisplayAlert("Audio", "Never gonna give you up...", "OK"))),
				new NavItem("Set Detail to Navigation Page", "\uE16F", new Command(() => Detail = CreateNavigationPage())),
				new NavItem("Set Detail to Content Page", "\uE160", new Command(() => Detail = detail)),
			};

			var navList = new NavList(navItems);

			// And add them to the navigation pane's content
			masterContent.Children.Add(navList);
			master.Content = masterContent;

			var detailContent = new StackLayout { VerticalOptions = LayoutOptions.Fill, HorizontalOptions = LayoutOptions.Fill };
			detailContent.Children.Add(new Label
			{
				Text = "Platform Features",
				FontAttributes = FontAttributes.Bold,
				HorizontalTextAlignment = TextAlignment.Center,
				VerticalTextAlignment = TextAlignment.Center
			});

			detailContent.Children.Add(CreateCollapseStyleChanger(this));
			detailContent.Children.Add(CreateToolbarPlacementChanger(this));
			detailContent.Children.Add(CreateCollapseWidthAdjuster(this));
			detailContent.Children.Add(CreateAddRemoveToolBarItemButtons(this));

			detail.Content = detailContent;

			Flyout = master;

			AddToolBarItems(this);

			Detail = detail;
		}

		static Layout CreateCollapseStyleChanger(FlyoutPage page)
		{
			Type enumType = typeof(CollapseStyle);

			return CreateChanger(enumType,
				Enum.GetName(enumType, page.On<WindowsOS>().GetCollapseStyle()),
				picker =>
				{
					page.On<WindowsOS>().SetCollapseStyle((CollapseStyle)Enum.Parse(enumType, picker.Items[picker.SelectedIndex]));
				},
				"Select Collapse Style");
		}

		static Layout CreateCollapseWidthAdjuster(FlyoutPage page)
		{
			var adjustCollapseWidthLabel = new Label
			{
				Text = "Adjust Collapsed Width",
				VerticalTextAlignment = TextAlignment.Center,
				VerticalOptions = LayoutOptions.Center
			};
			var adjustCollapseWidthEntry = new Entry { Text = page.On<WindowsOS>().CollapsedPaneWidth().ToString() };
			var adjustCollapseWidthButton = new Button { Text = "Change", BackgroundColor = Color.Gray };
			adjustCollapseWidthButton.Clicked += (sender, args) =>
			{
				double newWidth;
				if (double.TryParse(adjustCollapseWidthEntry.Text, out newWidth))
				{
					page.On<WindowsOS>().CollapsedPaneWidth(newWidth);
				}
			};

			var adjustCollapsedWidthSection = new StackLayout
			{
				HorizontalOptions = LayoutOptions.Center,
				Orientation = StackOrientation.Horizontal,
				Children = { adjustCollapseWidthLabel, adjustCollapseWidthEntry, adjustCollapseWidthButton }
			};

			return adjustCollapsedWidthSection;
		}

		public class NavItem
		{
			public NavItem(string text, string icon, ICommand command)
			{
				Text = text;
				Icon = icon;
				Command = command;
			}

			public ICommand Command { get; set; }

			public string Icon { get; set; }

			public string Text { get; set; }
		}

		public class NavList : ListView
		{
			public NavList(IEnumerable<NavItem> items)
			{
				ItemsSource = items;
				ItemTapped += (sender, args) => (args.Item as NavItem)?.Command.Execute(null);

				ItemTemplate = new DataTemplate(() =>
				{
					var grid = new Grid();
					grid.ColumnDefinitions.Add(new ColumnDefinition { Width = 48 });
					grid.ColumnDefinitions.Add(new ColumnDefinition { Width = 200 });

					grid.Margin = new Thickness(0, 10, 0, 10);

					var text = new Label
					{
						VerticalOptions = LayoutOptions.Fill
					};
					text.SetBinding(Label.TextProperty, "Text");

					var glyph = new Label
					{
						FontFamily = "Segoe MDL2 Assets",
						FontSize = 24,
						HorizontalTextAlignment = TextAlignment.Center
					};

					glyph.SetBinding(Label.TextProperty, "Icon");

					grid.Children.Add(glyph);
					grid.Children.Add(text);

					Grid.SetColumn(glyph, 0);
					Grid.SetColumn(text, 1);

					grid.WidthRequest = 48;

					var cell = new ViewCell
					{
						View = grid
					};

					return cell;
				});
			}
		}

		static NavigationPage CreateNavigationPage()
		{
			var page = new NavigationPage { Title = "This is the Navigation Page Title" };

			page.PushAsync(CreateNavSubPage());

			return page;
		}

		static ContentPage CreateNavSubPage()
		{
			var page = new ContentPage();

			var label = new Label { Text = "This is content in a nav page" };
			var button = new Button() { Text = "Push Another Page" };

			button.Clicked += (sender, args) => page.Navigation.PushAsync(CreateNavSubPage());

			page.Content = new StackLayout { Children = { label, button } };

			return page;
		}
	}
}