using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Collections.Specialized;

namespace Maui.Controls.Sample.Issues
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	[Issue(IssueTracker.Github, 19752, "Button does not behave properly when pointer hovers over the button because it's in focused state.")]
	public partial class Issue19752
	{
		public Issue19752()
		{
			InitializeComponent();
		}

		private void OnButtonClicked(object sender, EventArgs e)
		{
			// this code just enables all the buttons and disables the current one
			// except for the first button which is always enabled

			button2.IsEnabled = sender != button2;
			button3.IsEnabled = sender != button3;
		}
    }
}
