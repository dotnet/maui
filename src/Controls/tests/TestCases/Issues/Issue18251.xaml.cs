using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 18251, "IllegalArgumentException when changing number of tabbar pages", PlatformAffected.Android)]
	public partial class Issue18251 : Shell
	{
		internal static Issue18251ViewModel ViewModel;

		public Issue18251()
		{
			InitializeComponent();
			BindingContext = ViewModel = new();
		}
	}

	public class Issue18251Page : ContentPage
	{
		public Issue18251Page()
		{
			Content =
				new VerticalStackLayout() {
					new Button()
					{
						AutomationId = "button",
						Command = Issue18251.ViewModel.LoginCommand,
						Text = "Click to change tabs"
					}
				};
		}
	}

	public class Issue18251ViewModel : BindableObject
	{
		private bool _isLoggedIn;
		public bool IsLoggedIn
		{
			get => _isLoggedIn;
			set
			{
				_isLoggedIn = value;
				OnPropertyChanged();
			}
		}

		private bool _isLoggedOut = true;
		public bool IsLoggedOut
		{
			get => _isLoggedOut;
			set
			{
				_isLoggedOut = value;
				OnPropertyChanged();
			}
		}

		public Command LoginCommand { get; set; }

		public Issue18251ViewModel()
		{
			LoginCommand = new Command(() =>
			{
				IsLoggedIn = !IsLoggedIn;
				IsLoggedOut = !IsLoggedIn;
			});
		}
	}
}