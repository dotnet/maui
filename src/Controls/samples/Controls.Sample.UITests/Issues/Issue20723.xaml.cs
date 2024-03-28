using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Maui.Controls.Sample.Issues
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	[Issue(IssueTracker.Github, 20723, "[iOS] PullToRefresh activity indicator indicator hidden behind header in CollectionView", PlatformAffected.iOS)]
	public partial class Issue20723 : ContentPage
	{
		public Issue20723()
		{
			InitializeComponent();
		}
	}
}