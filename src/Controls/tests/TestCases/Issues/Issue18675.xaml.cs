using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Maui.Controls.Sample.Issues
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	[Issue(IssueTracker.ManualTest, "D12", "Editor IsReadOnly property prevent from modifying the text", PlatformAffected.All)]
	public partial class Issue18675 : ContentPage
	{
		public Issue18675()
		{
			InitializeComponent();
		}
	}
}