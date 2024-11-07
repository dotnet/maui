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

		public class MainPage : ContentPage
		{
			MenuFlyoutItem menuFlyoutItem;

			public MainPage()
			{
				MainPageViewModel mainPageViewModel = new MainPageViewModel();
				mainPageViewModel.MainPage = this;
				BindingContext = mainPageViewModel;

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
					Text = "Go to Second Page",
					AutomationId = "MainButton",
					HorizontalOptions = LayoutOptions.Center,
					VerticalOptions = LayoutOptions.Center
				};

				var commandButton = new Button
				{
					Text = "CommandButton",
					AutomationId = "CommandButton",
					HorizontalOptions = LayoutOptions.Center,
					VerticalOptions = LayoutOptions.Center,
				};

				var commandLabel = new Label
				{
					AutomationId = "CommandLabel",
					HorizontalOptions = LayoutOptions.Center,
					VerticalOptions = LayoutOptions.Center
				};

				commandLabel.SetBinding(Label.TextProperty, new Binding("LabelText"));

				mainButton.Command = mainPageViewModel.GoToSecondPageCommand;
				commandButton.Clicked += OnButtonClicked;
				var stackLayout = new VerticalStackLayout();
				stackLayout.Children.Add(mainButton);
				stackLayout.Children.Add(commandButton);
				stackLayout.Children.Add(commandLabel);

				Content = stackLayout;
			}

			private void OnButtonClicked(object sender, EventArgs e)
			{
				menuFlyoutItem?.Command.Execute(menuFlyoutItem.CommandParameter);
			}
		}

		public class SecondPage : ContentPage
		{
			public SecondPage()
			{
				BindingContext = new SecondPageViewModel(this);

				var goBackButton = new Button
				{
					Text = "Go Back",
					AutomationId = "GoBackButton",
					HorizontalOptions = LayoutOptions.Center,
					VerticalOptions = LayoutOptions.Center
				};

				goBackButton.SetBinding(Button.CommandProperty, nameof(SecondPageViewModel.GoBackCommand));

				var grid = new Grid();
				grid.Children.Add(goBackButton);

				Content = grid;
			}
		}

		public class SecondPageViewModel
		{
			SecondPage secondPage;
			public SecondPageViewModel(SecondPage page)
			{
				secondPage = page;
				GoBackCommand = new Command(GoBackAsync);
			}

			public ICommand GoBackCommand { get; }

			private void GoBackAsync()
			{
				secondPage.Navigation.PopAsync();
			}
		}

		public class MainPageViewModel : INotifyPropertyChanged
		{
			public MainPage MainPage;
			private string _labelText;
			public string LabelText
			{
				get => _labelText;
				set
				{
					if (_labelText != value)
					{
						_labelText = value;
						OnPropertyChanged(nameof(LabelText));
					}
				}
			}

			public ICommand MenuItemCommand { get; set; }
			public ICommand GoToSecondPageCommand { get; set; }

			public MainPageViewModel()
			{
				MenuItemCommand = new Command(() => LabelText = "Menu Item Clicked");
				GoToSecondPageCommand = new Command(GoToSecondPageAsync);
			}

			private void GoToSecondPageAsync()
			{
				MainPage.Navigation.PushAsync(new SecondPage());
			}

			public event PropertyChangedEventHandler PropertyChanged;
			protected void OnPropertyChanged(string propertyName) =>
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
