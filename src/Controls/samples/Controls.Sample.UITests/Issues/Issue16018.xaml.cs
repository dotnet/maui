using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Maui.Controls.Sample.Issues
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	[Issue(IssueTracker.Github, 16018, "WordWrap on labels with padding does not work properly on iOS", PlatformAffected.iOS)]
	public partial class Issue16018 : ContentPage
	{
		public Issue16018()
		{
			InitializeComponent();
		}

		void OnTextChanged(object sender, TextChangedEventArgs e)
		{
			LabelTest.Text = EntryTest.Text;
		}
	}
}