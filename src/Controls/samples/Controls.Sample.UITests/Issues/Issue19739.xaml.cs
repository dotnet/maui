using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Maui.Controls.Sample.Issues
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	[Issue(IssueTracker.Github, 19739, "[Android] Pickers reopen when selecting items", PlatformAffected.Android)]
	public partial class Issue19739 : ContentPage
	{
		public List<string> AutomationIds
			=> new() { "picker1", "picker2", "picker3" };

		public Issue19739()
		{
			InitializeComponent();
			BindingContext = this;
		}
	}
}