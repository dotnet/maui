using System;
using Microsoft.Maui.Controls.Internals;

namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 26057, "[iOS & Mac] Gradient background size is incorrect when invalidating parent", PlatformAffected.iOS | PlatformAffected.macOS)]
	public partial class Issue26057 : ContentPage
	{
		public Issue26057()
		{
			InitializeComponent();
		}

		private void OnButtonClicked(object sender, EventArgs e)
		{
			(stack as IView).InvalidateMeasure();
		}

	}
}