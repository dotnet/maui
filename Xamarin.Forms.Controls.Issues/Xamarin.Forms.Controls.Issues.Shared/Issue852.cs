using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using NUnit.Framework;
using Xamarin.UITest;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve (AllMembers=true)]
	[Issue (IssueTracker.Github, 852, "Async loading of Content causes UI element to be unclickable", PlatformAffected.Android | PlatformAffected.iOS)]
	public class Issue852 : ContentPage
	{
#if APP
		StackLayout _loggingInStackLayout;
		Button _loginButton;
		ScrollView _loginScrollView;

		public Issue852 ()
		{
			var welcomeLabel = new Label()
			{
				Text = "Logging into the System",
				HorizontalOptions = LayoutOptions.Center
			};

			var activitySpinner = new ActivityIndicator
			{
				Color = new Color(0, 0, 1),
				IsRunning = true
			};

			_loggingInStackLayout = new StackLayout
			{
				Orientation = StackOrientation.Vertical,
				VerticalOptions = LayoutOptions.Center,
				Spacing = 15
			};

			_loggingInStackLayout.Children.Add(welcomeLabel);
			_loggingInStackLayout.Children.Add(activitySpinner);

			Content = _loggingInStackLayout;
		}

		protected override async void OnAppearing()
		{
			base.OnAppearing();

			if (!(await AttemptLogin())) //try to log in, if login fails show login screen
			{
				Device.BeginInvokeOnMainThread (() => BuildLogin ());
			}
			else
			{
				Navigation.PopModalAsync();
			}
			IsBusy = false;

		}

		void BuildLogin()
		{
			Title = "Login";
			var welcomeLabel = new Label()
			{
				Text = "Welcome to the System",
				HorizontalOptions = LayoutOptions.Center
			};

			var usernameEntry = new Entry
			{
				IsPassword = false,
				Placeholder = "Username",
			};
			usernameEntry.SetBinding(Entry.TextProperty, new Binding("Username"));
			usernameEntry.Focused += (s, e) => welcomeLabel.Text = "Clicked User";

			var passwordEntry = new Entry
			{
				IsPassword = true,
				Placeholder = "Password",
			};
			passwordEntry.SetBinding(Entry.TextProperty, new Binding("Password", BindingMode.TwoWay));
			passwordEntry.Focused += (s, e) => welcomeLabel.Text = "Clicked Password";

			_loginButton = new Button
			{
				ClassId = "loginButton",
				Text = "Login",
			};
			_loginButton.SetBinding(Button.CommandProperty, new Binding("LoginCommand"));

			var loginStackLayout = new StackLayout
			{
				Orientation = StackOrientation.Vertical,
				VerticalOptions = LayoutOptions.Center,
				Spacing = 15
			};

			loginStackLayout.Children.Add(welcomeLabel);
			loginStackLayout.Children.Add(usernameEntry);
			loginStackLayout.Children.Add(passwordEntry);
			loginStackLayout.Children.Add(_loginButton);


			_loginScrollView = new ScrollView
			{
				Orientation = ScrollOrientation.Vertical,
				Content = loginStackLayout
			};
			Content = _loginScrollView;
		}

		async Task<bool> AttemptLogin()
		{
			await Task.Delay(2000);
			return false; //for this test we are always going ot fail, want to show login screen and error
		}
#endif
#if UITEST
		[Test]
		[UiTest (typeof(ContentPage))]
		public void Issue852TestsEntriesClickable ()
		{
			// TODO: Fix ME

			//App.WaitForElement (q => q.Marked ("Welcome to the System"));
			//App.WaitForElement (PlatformQueries.EntryWithPlaceholder ("Username"));
			//App.WaitForElement (PlatformQueries.EntryWithPlaceholder ("Password"));
			//App.WaitForElement (q => q.Button ("Login"));
			//App.Screenshot ("All elements present");

			//App.Tap (PlatformQueries.EntryWithPlaceholder ("Username"));
			//App.WaitForElement (q => q.Marked ("Clicked User"));
			//App.EnterText (PlatformQueries.EntryWithPlaceholder ("Username"), "Usertest");
			//App.Screenshot ("User entered");

			//App.Tap (PlatformQueries.EntryWithPlaceholder ("Password"));
			//App.WaitForElement (q => q.Marked ("Clicked Password"));
			//App.EnterText (PlatformQueries.EntryWithPlaceholder ("Password"), "Userpass");
			//App.Screenshot ("Password entered");

			//App.Screenshot ("Enties clickable");
			Assert.Inconclusive ("Fix Test");
		}
#endif
	}
}
