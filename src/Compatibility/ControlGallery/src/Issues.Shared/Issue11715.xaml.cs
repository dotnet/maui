using System.Collections.Generic;
using Microsoft.Maui.Controls.CustomAttributes;

#if UITEST
using Microsoft.Maui.Controls.Compatibility.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
	[Issue(IssueTracker.Github, 11715, "[Bug] Brushes API - Android's direction is inverted to iOS's",
		PlatformAffected.Android)]
	public partial class Issue11715 : TestContentPage
	{
		public Issue11715()
		{
#if APP
			InitializeComponent();
#endif
		}

		protected override void Init()
		{

		}
	}
}