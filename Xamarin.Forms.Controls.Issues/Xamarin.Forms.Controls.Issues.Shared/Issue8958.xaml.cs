using System.Collections.Generic;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.Forms.Core.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
#if UITEST
	[Category(UITestCategories.IndicatorView)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 8958, "[Bug] IndicatorView not updating to current page when CarouselView is bound to custom DataType",
		PlatformAffected.Android)]
	public partial class Issue8958 : TestContentPage
	{
		public Issue8958()
		{
#if APP
			InitializeComponent();
#endif
		}

		public List<Issue8958Model> Items { get; private set; }

		protected override void Init()
		{
			var item1 = new Issue8958Model()
			{
				Title = "Item 1",
			};

			var item2 = new Issue8958Model()
			{
				Title = "Item 2",
			};

			var item3 = new Issue8958Model()
			{
				Title = "Item 3",
			};

			Items = new List<Issue8958Model>() { item1, item2, item3 };
		}
	}

	[Preserve(AllMembers = true)]
	public struct Issue8958Model
	{
		public string Title { get; set; }
	}
}