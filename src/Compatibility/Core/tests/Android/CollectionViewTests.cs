using System.Threading.Tasks;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Dispatching;
using NUnit.Framework;
using NUnit.Framework.Internal;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Android.UnitTests
{
	public class CollectionViewTests : PlatformTestFixture
	{
		[Issue(IssueTracker.Github, 9030, "[Bug] ClassNotFoundException when using snap points on API < 23")]
		[Test(Description = "CollectionView with SnapPointsType set should not crash")]
		public async Task SnapPointsDoNotCrashOnOlderAPIs()
		{
			var cv = new Microsoft.Maui.Controls.CollectionView();

			var itemsLayout = new LinearItemsLayout(ItemsLayoutOrientation.Vertical)
			{
				SnapPointsType = SnapPointsType.Mandatory
			};
			cv.ItemsLayout = itemsLayout;

			// Creating the renderer is enough to cause the ClassNotFoundException on older APIs
			await cv.Dispatcher.DispatchAsync(() => { GetRenderer(cv).Dispose(); });
		}
	}
}