using NUnit.Framework;
using Xamarin.UITest;
using System.Linq;

namespace Xamarin.Forms.Core.UITests
{
	[TestFixture]
	[Category ("RelativeLayout")]
	internal class RelativeLayoutGalleryTests : BaseTestFixture
	{
		// TODO - Add relative layout tests
		// TODO: Port to new conventions

		public RelativeLayoutGalleryTests ()
		{
			ShouldResetPerFixture = false;
		}

		protected override void NavigateToGallery ()
		{
			App.NavigateToGallery (GalleryQueries.RelativeLayoutGalleryLegacy);
		}

		[Test]
		[Description ("All elements are present")]
		public void RelativeLayoutGalleryAllElementsPresent ()
		{
//			var elements = Enumerable.Range (0, 201).Select (x => x);
//			foreach (int element in elements)
//				App.ScrollDownForElement (q => q.Marked (element.ToString ()), 10);
		}

//		[Test]
//		[Description ("All elements are present - Landscape")]
//		public void RelativeLayoutGalleryAllElementsPresentLandscape ()
//		{
//			App.SetOrientationLandscape ();
//			App.Screenshot ("Rotated to Landscape");
//			RelativeLayoutGalleryAllElementsPresent ();
//			App.SetOrientationPortrait ();
//			App.Screenshot ("Rotated to Portrait");
//		}

	}
}
