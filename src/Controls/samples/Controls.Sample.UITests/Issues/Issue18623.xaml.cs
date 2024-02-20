using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;
using Maui.Controls.UITests;

namespace Maui.Controls.Sample.Issues
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	[Issue(IssueTracker.ManualTest, "E1", "Entry IsPassword obscure the text", PlatformAffected.All)]
	public partial class Issue18623 : ContentPage
	{
		public Issue18623()
		{
			InitializeComponent();
		}
	}
}