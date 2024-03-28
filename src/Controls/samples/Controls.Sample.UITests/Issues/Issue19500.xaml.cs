using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Maui.Controls.Sample.Issues
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	[Issue(IssueTracker.Github, 19500, "[iOS] Editor is not be able to scroll if IsReadOnly is true", PlatformAffected.iOS)]
	public partial class Issue19500 : ContentPage
	{
		public Issue19500()
		{
			InitializeComponent();
		}
	}
}