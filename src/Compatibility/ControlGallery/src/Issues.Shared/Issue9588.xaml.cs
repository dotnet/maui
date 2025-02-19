using System.Collections.Generic;
using Microsoft.Maui.Controls.CustomAttributes;

#if UITEST
using Microsoft.Maui.Controls.Compatibility.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
	[Issue(IssueTracker.Github, 9588, "Frame inside SwipeView can't be swiped", PlatformAffected.iOS)]
	public partial class Issue9588 : TestContentPage
	{
		public Issue9588()
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