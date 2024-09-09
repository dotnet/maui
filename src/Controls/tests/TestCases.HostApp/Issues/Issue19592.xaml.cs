using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Maui.Controls.Sample.Issues
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	[Issue(IssueTracker.Github, 19592, "Span LineHeight Wrong on Android", PlatformAffected.Android)]
	public partial class Issue19592 : ContentPage
	{
		public Issue19592()
		{
			InitializeComponent();
		}
	}
}