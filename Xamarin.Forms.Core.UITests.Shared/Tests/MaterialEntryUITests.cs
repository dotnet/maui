using NUnit.Framework;
using Xamarin.Forms.Controls.Issues;
using Xamarin.Forms.CustomAttributes;

namespace Xamarin.Forms.Core.UITests
{

#if __ANDROID__ || __IOS__
	[TestFixture]
	[Category(UITestCategories.Entry)]
	[Category(UITestCategories.Visual)]
	internal class MaterialEntryUITests : EntryUITests
	{
		protected override void NavigateToGallery()
		{
			App.NavigateToGallery(GalleryQueries.EntryGallery, "Material");
		}

		[Test]
		[UiTest(typeof(Entry), "Focus")]
		public override void _Focus()
		{
			base._Focus();
		}
	}
#endif
}