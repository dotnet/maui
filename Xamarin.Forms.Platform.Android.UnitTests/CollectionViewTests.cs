using System.Threading.Tasks;
using NUnit.Framework;
using NUnit.Framework.Internal;
using Xamarin.Forms.CustomAttributes;

namespace Xamarin.Forms.Platform.Android.UnitTests
{
	public class CollectionViewTests : PlatformTestFixture 
	{
		[Issue(IssueTracker.Github, 9030, "[Bug] ClassNotFoundException when using snap points on API < 23")]
		[Test(Description = "CollectionView with SnapPointsType set should not crash")]
		public async Task SnapPointsDoNotCrashOnOlderAPIs()
		{
			var cv = new Xamarin.Forms.CollectionView();

			var itemsLayout = new LinearItemsLayout(ItemsLayoutOrientation.Vertical)
			{
				SnapPointsType = SnapPointsType.Mandatory
			};
			cv.ItemsLayout = itemsLayout;

			// Creating the renderer is enough to cause the ClassNotFoundException on older APIs
			await Device.InvokeOnMainThreadAsync(() => { GetRenderer(cv).Dispose(); });
		}
	}
}