using System.Linq;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Maui.Controls.Sample.Issues
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	[Issue(IssueTracker.Github, 20631, "Editor does not scroll when cursor goes behind keyboard", PlatformAffected.iOS)]
	public partial class Issue20631 : ContentPage
	{
		public Issue20631()
		{
			InitializeComponent();
			Editor.Text = string.Join('\n', Enumerable.Range(0, 100).Select(i => $"Line {i}"));
		}
	}
}