using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Maui.Controls.Sample.Issues
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	[Issue(IssueTracker.Github, 16737, "Title colour on Android Picker, initially appears grey", PlatformAffected.All)]
	public partial class Issue16737 : ContentPage
	{
		public Issue16737()
		{
			InitializeComponent();
		}
	}
}