using System.Collections.Generic;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Shapes;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using Microsoft.Maui.Controls.Compatibility.UITests;
#endif

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
#if UITEST
	[Category(UITestCategories.ViewBaseTests)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(
		IssueTracker.Github, 11931,
		"[Bug] View translation is incorrectly calculated",
		PlatformAffected.All)]
	public partial class Issue11931 : TestContentPage
	{
		public Issue11931()
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
