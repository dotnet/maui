using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using Xamarin.Forms.Core.UITests;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 11081, "[Bug] CarouselView should not animate an initial Position on Android",
		PlatformAffected.Android)]
#if UITEST
	[Category(UITestCategories.CarouselView)]
#endif
	public sealed partial class Issue11081 : TestContentPage
	{
		public Issue11081()
		{
#if APP
			this.InitializeComponent();
#endif
		}

		protected override void Init()
		{

		}
	}
}