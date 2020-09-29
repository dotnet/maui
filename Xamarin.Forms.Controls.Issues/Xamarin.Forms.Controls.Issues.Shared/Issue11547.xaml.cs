using System.Collections.Generic;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Xaml;

#if UITEST
using Xamarin.UITest;
using Xamarin.UITest.Queries;
using NUnit.Framework;
using Xamarin.Forms.Core.UITests;
#endif

namespace Xamarin.Forms.Controls.Issues
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