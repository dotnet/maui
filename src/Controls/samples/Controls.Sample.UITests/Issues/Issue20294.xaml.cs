using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Maui.Controls.Sample.Issues
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	[Issue(IssueTracker.Github, 20294, "CollectionView containing a Footer and a Border with StrokeThickness set to decimal value crashes on scroll", PlatformAffected.iOS)]
	public partial class Issue20294 : ContentPage
	{
		public Issue20294()
		{
			InitializeComponent();
		}
	}
}