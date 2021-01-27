using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Xaml;

#if UITEST
using Xamarin.Forms.Core.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
#if UITEST
	[Category(UITestCategories.ManualReview)]
#endif
#if APP
	[XamlCompilation(XamlCompilationOptions.Compile)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 12473, "iOS Label text alignment in RTL with LineHeight", PlatformAffected.iOS)]
	public partial class Issue12473 : TestContentPage
	{
		public Issue12473()
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