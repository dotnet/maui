using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;
namespace Maui.Controls.Sample.Issues;

[XamlCompilation(XamlCompilationOptions.Compile)]
[Issue(IssueTracker.Github, 20273, "Previously selected item cannot be reselected", PlatformAffected.All)]

public partial class Issue20273 : ContentPage
{
	int _numberOfNavigations;

	public Issue20273()
	{
		InitializeComponent();
	}

	void CollectionView_SelectionChanged(System.Object sender, Microsoft.Maui.Controls.SelectionChangedEventArgs e)
	{
		numberOfNavigationsLabel.Text = (++_numberOfNavigations).ToString();
	}
}
