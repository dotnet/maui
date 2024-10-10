using System.Collections.Generic;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, "22520", "ImageButton border (BorderWidth) overlaps the image", PlatformAffected.All)]
	public partial class Issue22520 : ContentPage
	{
		public Issue22520()
		{
			InitializeComponent();
		}
	}
}