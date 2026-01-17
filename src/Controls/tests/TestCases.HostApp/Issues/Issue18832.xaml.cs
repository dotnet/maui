using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Maui.Controls.Sample.Issues
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	[Issue(IssueTracker.Github, 18832, "[iOS] Button CharacterSpacing makes FontSize fixed and large", PlatformAffected.iOS)]
	public partial class Issue18832 : ContentPage
	{
		public Issue18832()
		{
			InitializeComponent();
		}
	}
}