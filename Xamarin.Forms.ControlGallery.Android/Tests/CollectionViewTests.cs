using NUnit.Framework;
using NUnit.Framework.Internal;
using Xamarin.Forms.CustomAttributes;

namespace Xamarin.Forms.ControlGallery.Android.Tests
{
	public class CollectionViewTests : PlatformTestFixture 
	{
		[Issue(IssueTracker.Github, 9030, "[Bug] ClassNotFoundException when using snap points on API < 23")]
		[Test(Description = "CollectionView with SnapPointsType set should not crash")]
		public void SnapPointsDoNotCrashOnOlderAPIs()
		{
			var cv = new CollectionView();

			var itemsLayout = new LinearItemsLayout(ItemsLayoutOrientation.Vertical)
			{
				SnapPointsType = SnapPointsType.Mandatory
			};
			cv.ItemsLayout = itemsLayout;

			// Creating the renderer is enough to cause the ClassNotFoundException on older APIs
			GetRenderer(cv).Dispose();
		}
	}
}