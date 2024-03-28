using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Maui.Controls.Sample.Issues
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	[Issue(IssueTracker.Github, 19509, "Entry TextColor property not working when the Text value is bound after some time", PlatformAffected.iOS)]
	public partial class Issue19509 : ContentPage
	{
		string _text;

		public Issue19509()
		{
			InitializeComponent();

			BindingContext = this;
		}

		protected override async void OnAppearing()
		{
			base.OnAppearing();

			await Task.Delay(1500);

			Text = "Updated text in OnAppearing";
		}

		public string Text
		{
			get => _text;
			set
			{
				_text = value;
				OnPropertyChanged();
			}
		}
	}
}