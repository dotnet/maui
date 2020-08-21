using System.Collections.Generic;
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
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 11573, "[Bug][Brushes] RadialGradient size on iOS",
		PlatformAffected.iOS)]
#if UITEST
	[NUnit.Framework.Category(UITestCategories.Brush)]
#endif
	public partial class Issue11573 : TestContentPage
	{
		public Issue11573()
		{
#if APP
			Device.SetFlags(new List<string> { ExperimentalFlags.BrushExperimental });

			InitializeComponent();
#endif
		}

		protected override void Init()
		{

		}
	}
}