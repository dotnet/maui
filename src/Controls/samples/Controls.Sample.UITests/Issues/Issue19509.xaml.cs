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

		public string Text
		{
			get => _text;
			set
			{
				_text = value;
				OnPropertyChanged();
			}
		}

		void Button_Clicked(System.Object sender, System.EventArgs e)
		{
			Text = "Updated text on button click";
		}
	}
}