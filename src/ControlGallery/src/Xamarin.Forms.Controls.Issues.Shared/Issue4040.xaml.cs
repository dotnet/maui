using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Xamarin.Forms;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.Issues
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