using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Maui.Controls.Sample.Issues
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	[Issue(IssueTracker.ManualTest, "C2", "Can scroll CollectionView inside RefreshView", PlatformAffected.All)]
	public partial class Issue18751 : ContentPage
	{
		public Issue18751()
		{
			InitializeComponent();

			BindingContext = new MonkeysViewModel();
		}
	}
}