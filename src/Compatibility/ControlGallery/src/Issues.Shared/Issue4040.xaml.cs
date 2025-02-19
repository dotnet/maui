using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 4040, "[iOS] Label TextColor has no effect with FormattedString", PlatformAffected.iOS)]
	public partial class Issue4040 : ContentPage
	{
		public Issue4040()
		{
#if APP
			InitializeComponent();
#endif
		}
	}
}