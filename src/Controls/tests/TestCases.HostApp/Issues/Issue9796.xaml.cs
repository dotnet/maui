using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maui.Controls.Sample.Issues
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	[Issue(IssueTracker.Github, 9796, "[Android]Editor/Entry controls don't raise Completed event consistently",
		PlatformAffected.Android)]
	public partial class Issue9796 : ContentPage
	{
		public Issue9796()
		{
			InitializeComponent();
		}

		private void Editor_Completed(object sender, EventArgs e)
		{
			EditorStatusLabel.Text = "Editor Completed by UnFocused";	
		}

		private void Entry_Completed(object sender, EventArgs e)
		{
			EntryStatusLabel.Text = "Entry Completed by UnFocused";
		}
	}
}