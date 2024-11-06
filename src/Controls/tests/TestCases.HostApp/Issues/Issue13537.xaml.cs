using System;
using System.ComponentModel;
using System.Diagnostics;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;

namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 13537, "ApplyQueryAttributes should Trigger for all navigations",
		PlatformAffected.All)]
	public partial class Issue13537 : Shell
	{

		public Issue13537()
		{
			InitializeComponent();
	     	Routing.RegisterRoute("NewPage", typeof(Issue13537InnerPage));
			Application.Current.Windows[0].Page = new NavigationPage(new Issue13537Home());
		}
	}

	public class Issue13537Home : ContentPage
	{
		public Issue13537Home()
		{
			Title = "Home"; // Setting the title of the page
			var viewModel = new Issue13537ViewModel<Issue13537Home>();
			this.BindingContext = viewModel;

			var Label = new Label
			{
				AutomationId = "TestLabel",
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center
			};
			Label.SetBinding(Label.TextProperty, nameof(viewModel.DisplayText));

			var button = new Button
			{
				Text = "Navigate using PushAsync",
				AutomationId = "PushAsyncButton",
				HorizontalOptions =LayoutOptions.Center, 
				VerticalOptions=LayoutOptions.Center
			};
			button.Clicked += OnNavigationWithPushAsync;
			var button1 = new Button
			{
				Text = "Navigate using PushModalAsync",
				AutomationId = "PushModalButton",
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center
			};
			button1.Clicked += OnNavigationWithPushModalAsync;

			var stack = new StackLayout();
			stack.Children.Add(Label);
			stack.Children.Add(button);
			stack.Children.Add(button1);

			this.Content = stack;
		}
		private void OnNavigationWithPushAsync(object sender, EventArgs e)
		{
			Navigation.PushAsync(new Issue13537InnerPage());
		}

		private void OnNavigationWithPushModalAsync(object sender, EventArgs e)
		{
			Navigation.PushModalAsync(new Issue13537InnerPage());
		}
	}
	public class Issue13537Favorite : ContentPage
	{
		public Issue13537Favorite()
		{
			Title = "Favorite"; // Setting the title of the page
			var viewModel = new Issue13537ViewModel<Issue13537Favorite>();
			this.BindingContext = viewModel;

			var Label = new Label
			{
				AutomationId = "TestLabel1",
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center
			};
			Label.SetBinding(Label.TextProperty, nameof(viewModel.DisplayText));
			var button = new Button
			{
				Text = "Navigate using GoToAsync",
				AutomationId = "GoToAsyncButton",
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center
			};
			button.Clicked += OnGoToAsync;
			var stack = new StackLayout();
			stack.Children.Add(Label);
			stack.Children.Add(button);
			this.Content = stack;
		}
		private void OnGoToAsync(object sender, EventArgs e)
		{
			Shell.Current.GoToAsync("NewPage");
		}
	}
	public class Issue13537SettingPage : ContentPage
	{
		public Issue13537SettingPage()
		{
			Title = "Setting"; // Setting the title of the page
			var viewModel = new Issue13537ViewModel<Issue13537SettingPage>();
			this.BindingContext = viewModel;

			var Label = new Label
			{
				AutomationId = "TestLabel2",
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center,
				TextColor = Colors.Black
			};
			Label.SetBinding(Label.TextProperty, nameof(viewModel.DisplayText));
			var stack = new StackLayout();
			stack.Children.Add(Label);
			this.Content = stack;
		}
	}
	public class Issue13537InnerPage : ContentPage
	{
		public Issue13537InnerPage()
		{
			Title = "NewPage"; // Setting the title of the page
			var viewModel = new Issue13537ViewModel<Issue13537InnerPage>();
			this.BindingContext = viewModel;

			var Label = new Label
			{
				AutomationId = "TestLabel3",
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center
			};
			Label.SetBinding(Label.TextProperty, nameof(viewModel.DisplayText));
			var button = new Button
			{
				Text = "Navigate using PopAsync back",
				AutomationId = "PopAsyncButton",
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center
			};
			button.Clicked += OnNavigatingPopButton;
			var button1 = new Button
			{
				Text = "Navigate using Navigating back",
				AutomationId = "PopModalButton",
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center
			};
			button.Clicked += OnNavigatingPopModalButton;
			var stack = new StackLayout();
			stack.Children.Add(Label);
			stack.Children.Add(button);
			
			
			stack.AutomationId = "NewInnerPage";
		
			this.Content = stack;
		}
		private void OnNavigatingPopModalButton(object sender, EventArgs e)
		{
			Navigation.PopModalAsync();
		}
		private void OnNavigatingPopButton(object sender, EventArgs e)
		{
			Navigation.PopAsync();
		}
	}
	public class Issue13537ViewModel<T> : IQueryAttributable, INotifyPropertyChanged where T : Page
	{
		// Property to hold formatted query attributes as a string
		private string _displayText = "Default Query" ;
		public string DisplayText
		{
			get => _displayText;
			set
			{
				_displayText = value;
				OnPropertyChanged(nameof(DisplayText));
			}
		}

		public void ApplyQueryAttributes(IDictionary<string, object> query)
		{
			DisplayText = $"{typeof(T).Name} ViewModel.ApplyQueryAttributes";
		}

		// Implement INotifyPropertyChanged to support binding
		public event PropertyChangedEventHandler PropertyChanged;
		protected virtual void OnPropertyChanged(string propertyName)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}

}