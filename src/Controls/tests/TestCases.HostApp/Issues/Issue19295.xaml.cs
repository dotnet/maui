using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 19295, "Picker does Not Resize Automatically After Selection", PlatformAffected.iOS)]
public partial class Issue19295 : ContentPage
{
	public Issue19295()
	{
		InitializeComponent();
	}
}