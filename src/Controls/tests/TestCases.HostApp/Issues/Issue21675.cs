using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Microsoft.Maui.Controls;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 21675, "MenuFlyoutItem stops working after navigating away from and back to page", PlatformAffected.UWP | PlatformAffected.macOS)]
	public class Issue21675 : NavigationPage
	{
		public Issue21675() : base(new MainPage())
		{

		}
	}

	public class MainPage : ContentPage
	{
		internal Label label;
		MainPageModel mainModel;
		MenuFlyoutItem menuFlyoutItem;
		public MainPage()
		{
			mainModel = new MainPageModel(this);
			BindingContext = mainModel;

			var menuBarItem = new MenuBarItem
			{
				Text = "Custom Menu",
				AutomationId = "MenuBarItem"
			};

			menuFlyoutItem = new MenuFlyoutItem
			{
				Text = "Custom Item",
				AutomationId = "MenuFlyoutItem"
			};

			menuFlyoutItem.SetBinding(MenuFlyoutItem.CommandProperty, new Binding("MenuItemCommand"));
			menuBarItem.Add(menuFlyoutItem);

			MenuBarItems.Add(menuBarItem);

			var mainButton = new Button
			{
				Text = "Go to Other Page",
				AutomationId = "MainButton",
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center
			};

			var button = new Button
			{
				Text = "Button1",
				AutomationId = "Button1",
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center,
			};

			label = new Label
			{
				AutomationId = "Label",
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center
			};
			mainButton.Command = mainModel.GoToOtherPageCommand;
			button.Clicked += OnButtonClicked;
			var stackLayout = new VerticalStackLayout();
			stackLayout.Children.Add(mainButton);
			stackLayout.Children.Add(button);
			stackLayout.Children.Add(label);

			Content = stackLayout;
		}

		private void OnButtonClicked(object sender, EventArgs e)
		{
			menuFlyoutItem?.Command.Execute(menuFlyoutItem.CommandParameter);
		}
	}

	public class OtherPage : ContentPage
	{
		public OtherPage()
		{
			BindingContext = new OtherPageModel(this);

			var goBackButton = new Button
			{
				Text = "Go Back",
				AutomationId = "GoBackButton",
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center
			};

			goBackButton.SetBinding(Button.CommandProperty, nameof(OtherPageModel.GoBackCommand));

			var grid = new Grid();
			grid.Children.Add(goBackButton);

			Content = grid;
		}
	}

	public class OtherPageModel
	{
		OtherPage otherPage;
		public OtherPageModel(OtherPage page)
		{
			otherPage = page;
			GoBackCommand = new Command(GoBackAsync);
		}

		public ICommand GoBackCommand { get; }

		private void GoBackAsync()
		{
			otherPage.Navigation.PopAsync();
		}
	}

	public class MainPageModel
	{
		MainPage mainPage;
		public MainPageModel(MainPage page)
		{
			mainPage = page;
			MenuItemCommand = new Command(() => mainPage.label.Text = "Menu Item Clicked");
			GoToOtherPageCommand = new Command(GoToOtherPageAsync);
		}

		public ICommand MenuItemCommand { get; set; }
		public ICommand GoToOtherPageCommand { get; set; }

		private void GoToOtherPageAsync()
		{
			mainPage.Navigation.PushAsync(new OtherPage());
		}
	}
}
