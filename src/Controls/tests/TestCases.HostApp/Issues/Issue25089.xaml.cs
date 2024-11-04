using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Issues
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	[Issue(IssueTracker.Github, 25089, "OnAppearing of Page called again, although this page was on already replaced NavigationStack", PlatformAffected.UWP | PlatformAffected.Android)]
	public partial class Issue25089 : Shell
	{
		public static SharedViewModel SharedViewModel { get; } = new SharedViewModel();
		public Issue25089()
		{
			InitializeComponent();
			Routing.RegisterRoute("FirstPage", typeof(FirstPage));
			Routing.RegisterRoute("SecondPage", typeof(SecondPage));
		}
	}

	public class SharedViewModel : INotifyPropertyChanged
	{
		string _statusText = "Page Appearing Sequence: Initial";

		public string StatusText
		{
			get => _statusText;
			set
			{
				if (_statusText != value)
				{
					_statusText = value;
					OnPropertyChanged();
				}
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}

	public partial class MainPage : ContentPage
	{
		public MainPage()
		{
			Appearing += MainPage_Appearing;	
			BindingContext = Issue25089.SharedViewModel;

			var statusLabel = new Label
			{
				AutomationId = "StatusLabel",
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center
			};
			statusLabel.SetBinding(Label.TextProperty, nameof(SharedViewModel.StatusText));

			var mainButton = new Button
			{
				Text = "Go to First Page",
				AutomationId = "MainButton",
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center,
			};

			mainButton.Clicked += async (sender, e) =>
			{
				await Shell.Current.GoToAsync("FirstPage", true);
			};

			var stackLayout = new VerticalStackLayout
			{
				Children = { statusLabel, mainButton }
			};

			Content = stackLayout;
		}

		void MainPage_Appearing(object sender, EventArgs e)
		{
			Issue25089.SharedViewModel.StatusText += " | MainPage appeared";
		}
	}

	public class FirstPage : ContentPage
	{
		public FirstPage()
		{
			Title = "First Page";
			Appearing += FirstPage_Appearing;

			var firstPageButton = new Button
			{
				Text = "Go to Second Page",
				AutomationId = "FirstPageButton",
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center
			};

			firstPageButton.Clicked += async (sender, e) =>
			{
				await Shell.Current.GoToAsync("//SecondPage", true);
			};

			var stackLayout = new VerticalStackLayout
			{
				Children = { firstPageButton }
			};

			Content = stackLayout;
		}

		void FirstPage_Appearing(object sender, EventArgs e)
		{
			Issue25089.SharedViewModel.StatusText += " | FirstPage appeared";
		}
	}

	public class SecondPage : ContentPage
	{
		public SecondPage()
		{
			Title = "Second Page";
			Appearing += SecondPage_Appearing;

			var secondPageButton = new Button
			{
				Text = "Go to Main Page",
				AutomationId = "SecondPageButton",
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center
			};

			secondPageButton.Clicked += async (sender, e) =>
			{
				await Shell.Current.GoToAsync("//MainPage", true);
			};

			var stackLayout = new VerticalStackLayout
			{
				Children = { secondPageButton }
			};

			Content = stackLayout;
		}

		void SecondPage_Appearing(object sender, EventArgs e)
		{
			Issue25089.SharedViewModel.StatusText += " | SecondPage appeared";
		}
	}
}
