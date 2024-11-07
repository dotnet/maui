using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Maui.Controls.Sample.Issues
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	[Issue(IssueTracker.Github, 11655, "Label's HorizontalTextAlignment property is not updated properly at runtime", PlatformAffected.Android)]
	public partial class Issue11655 : ContentPage
	{
		public Issue11655()
		{
			InitializeComponent();
		}

		private void ChangeAlignmentClicked(object sender, EventArgs e)
		{
			label.HorizontalTextAlignment = TextAlignment.Start;
			RTLlabel.HorizontalTextAlignment = TextAlignment.Start;
		}

	}
}