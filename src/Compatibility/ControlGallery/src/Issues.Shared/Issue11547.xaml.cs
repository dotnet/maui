using System.Collections.Generic;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Xaml;

#if UITEST
using Xamarin.UITest;
using Xamarin.UITest.Queries;
using NUnit.Framework;
using Microsoft.Maui.Controls.Compatibility.UITests;
#endif

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
#if UITEST
	[Category(UITestCategories.Brush)]
#endif
#if APP
	[XamlCompilation(XamlCompilationOptions.Compile)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 11547, "[Bug] [Shapes 4.8-pre2] Justice for baby elephant!", PlatformAffected.Android | PlatformAffected.iOS)]
	public partial class Issue11547 : TestContentPage
	{
		public Issue11547()
		{
#if APP
			Title = "Issue 11547";
			InitializeComponent();
#endif
		}

		protected override void Init()
		{

		}
	}
}