using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

namespace Maui.Controls.Sample.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 12848, "[Bug] CarouselView position resets when visibility toggled",
		PlatformAffected.Android)]
	public partial class Issue12848 : TestContentPage
	{
		protected override void Init()
		{
			InitializeComponent();

			BindingContext = new List<int> { 1, 2, 3 };
		}

		void OnShowButtonClicked(object sender, EventArgs e)
		{
			carousel.IsVisible = true;
		}

		void OnHideButtonClicked(object sender, EventArgs e)
		{
			carousel.IsVisible = false;
		}
	}
}