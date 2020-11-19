using System.Collections.Generic;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Shapes;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using Xamarin.Forms.Core.UITests;
#endif

namespace Xamarin.Forms.Controls.Issues
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
