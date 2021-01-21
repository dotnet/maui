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
	[Category(UITestCategories.Shape)]
#endif
#if APP
	[XamlCompilation(XamlCompilationOptions.Compile)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 12521, "[Bug][iOS] Shapes disappear in a Frame in a ListView when scrolled",
		PlatformAffected.iOS)]
	public partial class Issue12521 : TestContentPage
	{
		public Issue12521()
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