#nullable disable
namespace Microsoft.Maui.Controls
{
	/// <summary>
	/// Base class for layouts that arrange items in collection and carousel views.
	/// </summary>
	/// <remarks>
	/// <see cref="ItemsLayout"/> is the abstract base class for item arrangement strategies used in <see cref="CollectionView"/> and <see cref="CarouselView"/>.
	/// It defines common properties like <see cref="Orientation"/>, <see cref="SnapPointsType"/>, and <see cref="SnapPointsAlignment"/>
	/// that control how items are laid out and how scrolling behaves.
	/// Concrete implementations include <see cref="LinearItemsLayout"/> and <see cref="GridItemsLayout"/>.
	/// </remarks>
	public abstract class ItemsLayout : BindableObject, IItemsLayout
	{
		/// <summary>
		/// Gets the orientation of the items layout.
		/// </summary>
		/// <value>An <see cref="ItemsLayoutOrientation"/> value indicating whether items flow vertically or horizontally.</value>
		public ItemsLayoutOrientation Orientation { get; }

		internal ItemsUpdatingScrollMode ItemsUpdatingScrollMode { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="ItemsLayout"/> class with the specified orientation.
		/// </summary>
		/// <param name="orientation">The orientation of the layout.</param>
		protected ItemsLayout([Parameter("Orientation")] ItemsLayoutOrientation orientation)
		{
			Orientation = orientation;
		}

		/// <summary>Bindable property for <see cref="SnapPointsAlignment"/>.</summary>
		public static readonly BindableProperty SnapPointsAlignmentProperty =
			BindableProperty.Create(nameof(SnapPointsAlignment), typeof(SnapPointsAlignment), typeof(ItemsLayout),
				SnapPointsAlignment.Start);

		/// <summary>
		/// Gets or sets how items align to snap points when scrolling stops.
		/// </summary>
		/// <value>A <see cref="SnapPointsAlignment"/> value. The default is <see cref="SnapPointsAlignment.Start"/>.</value>
		/// <remarks>
		/// This property only applies when <see cref="SnapPointsType"/> is not <see cref="SnapPointsType.None"/>.
		/// It controls whether items snap to the start, center, or end of the visible area.
		/// </remarks>
		public SnapPointsAlignment SnapPointsAlignment
		{
			get => (SnapPointsAlignment)GetValue(SnapPointsAlignmentProperty);
			set => SetValue(SnapPointsAlignmentProperty, value);
		}

		/// <summary>Bindable property for <see cref="SnapPointsType"/>.</summary>
		public static readonly BindableProperty SnapPointsTypeProperty =
			BindableProperty.Create(nameof(SnapPointsType), typeof(SnapPointsType), typeof(ItemsLayout),
				SnapPointsType.None);

		/// <summary>
		/// Gets or sets the snap points behavior when scrolling.
		/// </summary>
		/// <value>A <see cref="SnapPointsType"/> value. The default is <see cref="SnapPointsType.None"/>.</value>
		/// <remarks>
		/// Snap points cause scrolling to "snap" to specific item positions, creating a paging-like effect.
		/// Set this to <see cref="SnapPointsType.Mandatory"/> or <see cref="SnapPointsType.MandatorySingle"/> to enable snapping behavior.
		/// </remarks>
		public SnapPointsType SnapPointsType
		{
			get => (SnapPointsType)GetValue(SnapPointsTypeProperty);
			set => SetValue(SnapPointsTypeProperty, value);
		}
	}
}
