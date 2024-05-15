using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Maui.Controls.Sample.Issues
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	[Issue(IssueTracker.Github, 22209, "[Windows] FlexLayout crashes when using a custom display scale factor value", PlatformAffected.UWP)]
	public partial class Issue22209 : ContentPage
	{
		public Issue22209()
		{
			InitializeComponent();
		}
	}
}