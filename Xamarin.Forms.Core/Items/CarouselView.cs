using Xamarin.Forms.Platform;

namespace Xamarin.Forms
{
	[RenderWith(typeof(_CarouselViewRenderer))]
	public class CarouselView : ItemsView
	{
		public CarouselView()
		{
			CollectionView.VerifyCollectionViewFlagEnabled(constructorHint: nameof(CarouselView));
			ItemsLayout = new ListItemsLayout(ItemsLayoutOrientation.Horizontal)
			{
				SnapPointsType = SnapPointsType.MandatorySingle,
				SnapPointsAlignment = SnapPointsAlignment.Center
			};
		}
	}
}