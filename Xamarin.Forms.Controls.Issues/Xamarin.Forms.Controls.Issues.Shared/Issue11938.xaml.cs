using System.Collections.Generic;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using Xamarin.Forms.Core.UITests;
#endif

namespace Xamarin.Forms.Controls.Issues
{
#if UITEST
	[Category(UITestCategories.CollectionView)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 11938,
		"[Bug] Snap Points not working in CollectionView with GridItemsLayout",
		PlatformAffected.Android)]
	public partial class Issue11938 : TestContentPage
	{
		public Issue11938()
		{
#if APP
			InitializeComponent();
			BindingContext = this;
#endif
		}

		public List<Issue11938Model> GridItems { get; set; }

		protected override void Init()
		{
			GridItems = new List<Issue11938Model>();

			for (int i = 0; i < 12; i++)
			{
				GridItems.Add(new Issue11938Model
				{
					Text = "Item " + i.ToString()
				});
			}
		}
	}

	[Preserve(AllMembers = true)]
	public class Issue11938Model
	{
		public string Text { get; set; }
	}
}