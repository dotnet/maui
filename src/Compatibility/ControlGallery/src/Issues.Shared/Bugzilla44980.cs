using System.Collections.Generic;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
#if UITEST
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.Bugzilla)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 44980, "ActivityIndicator disappears when scrolling", PlatformAffected.iOS)]
	public class Bugzilla44980 : TestContentPage
	{
		protected override void Init()
		{
			var list = new List<string>();
			for (var i = 0; i < 100; i++)
				list.Add(i.ToString());

			Content = new CListView
			{
				ItemsSource = list,
				ItemTemplate = new DataTemplate(() =>
				{
					var activityIndicator = new ActivityIndicator
					{
						IsRunning = true,
						IsVisible = true
					};
					return new ViewCell { View = activityIndicator };
				})
			};
		}
	}

	public class CListView : ListView
	{
		public CListView() : base(ListViewCachingStrategy.RecycleElement)
		{
		}
	}
}
