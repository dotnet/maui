namespace Xamarin.Forms
{
	public abstract class ItemsLayout : BindableObject, IItemsLayout
	{
		public ItemsLayoutOrientation Orientation { get; }

		protected ItemsLayout([Parameter("Orientation")] ItemsLayoutOrientation orientation)
		{
			Orientation = orientation;
		}

		public static readonly BindableProperty SnapPointsAlignmentProperty =
			BindableProperty.Create(nameof(SnapPointsAlignment), typeof(SnapPointsAlignment), typeof(ItemsLayout),
				SnapPointsAlignment.Start);

		public SnapPointsAlignment SnapPointsAlignment
		{
			get => (SnapPointsAlignment)GetValue(SnapPointsAlignmentProperty);
			set => SetValue(SnapPointsAlignmentProperty, value);
		}

		public static readonly BindableProperty SnapPointsTypeProperty =
			BindableProperty.Create(nameof(SnapPointsType), typeof(SnapPointsType), typeof(ItemsLayout),
				SnapPointsType.None);

		public SnapPointsType SnapPointsType
		{
			get => (SnapPointsType)GetValue(SnapPointsTypeProperty);
			set => SetValue(SnapPointsTypeProperty, value);
		}
	}
}