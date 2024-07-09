using System.Threading.Tasks;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 852, "Async loading of Content causes UI element to be unclickable", PlatformAffected.Android | PlatformAffected.iOS)]
	public class Issue852 : TestContentPage
	{
		protected override void Init() { }
		const string UsernameId = "username852";
		const string PasswordId = "password852";

		StackLayout _loggingInStackLayout;
		Button _loginButton;
		ScrollView _loginScrollView;

		public Issue852()
		{
			var welcomeLabel = new Label()
			{
				AutomationId = "WelcomeLabel",
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
#pragma warning disable CS0612 // Type or member is obsolete
#pragma warning disable CS0618 // Type or member is obsolete
				Device.BeginInvokeOnMainThread(() => BuildLogin());
#pragma warning restore CS0618 // Type or member is obsolete
#pragma warning restore CS0612 // Type or member is obsolete
			}
			else
			{
				await Navigation.PopModalAsync();
			}
			IsBusy = false;

		}

		void BuildLogin()
		{
			Title = "Login";
			var welcomeLabel = new Label()
			{
				AutomationId = "WelcomeLabel",
				Text = "Welcome to the System",
				HorizontalOptions = LayoutOptions.Center
			};

			var usernameEntry = new Entry
			{
				IsPassword = false,
				Placeholder = "Username",
				AutomationId = UsernameId
			};
			usernameEntry.SetBinding(Entry.TextProperty, new Binding("Username"));
			usernameEntry.Focused += (s, e) => welcomeLabel.Text = "Clicked User";

			var passwordEntry = new Entry
			{
				IsPassword = true,
				Placeholder = "Password",
				AutomationId = PasswordId
			};
			passwordEntry.SetBinding(Entry.TextProperty, new Binding("Password", BindingMode.TwoWay));
			passwordEntry.Focused += (s, e) => welcomeLabel.Text = "Clicked Password";

			_loginButton = new Button
			{ 
				AutomationId = "Login",
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

		// Can't tell if it wasn't working in iOS or Android, but in either case we can probably just use automationID
		// instead of querying on the placeholder text
	}
}
