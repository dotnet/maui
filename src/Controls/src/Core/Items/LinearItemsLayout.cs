#nullable disable
using System;

namespace Microsoft.Maui.Controls
{
	/// <summary>
	/// An items layout that arranges items in a single row or column.
	/// </summary>
	/// <remarks>
	/// <see cref="LinearItemsLayout"/> is the default layout for <see cref="CollectionView"/> and displays items in a single continuous line.
	/// The <see cref="ItemSpacing"/> property controls the spacing between consecutive items.
	/// This class provides predefined static instances for common configurations like <see cref="Vertical"/> and <see cref="Horizontal"/>.
	/// </remarks>
	public class LinearItemsLayout : ItemsLayout
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="LinearItemsLayout"/> class with the specified orientation.
		/// </summary>
		/// <param name="orientation">The scroll orientation of the layout.</param>
		public LinearItemsLayout([Parameter("Orientation")] ItemsLayoutOrientation orientation) : base(orientation)
		{
		}

		/// <summary>
		/// Gets a default vertical linear items layout.
		/// </summary>
		/// <value>A <see cref="LinearItemsLayout"/> configured for vertical scrolling.</value>
		public static readonly IItemsLayout Vertical = CreateVerticalDefault();
		
		/// <summary>
		/// Gets a default horizontal linear items layout.
		/// </summary>
		/// <value>A <see cref="LinearItemsLayout"/> configured for horizontal scrolling.</value>
		public static readonly IItemsLayout Horizontal = CreateHorizontalDefault();

		/// <summary>
		/// Gets a default vertical linear items layout configured for carousel behavior with snap points.
		/// </summary>
		/// <value>A <see cref="LinearItemsLayout"/> configured for vertical carousel scrolling with mandatory single snap points centered.</value>
		public static readonly IItemsLayout CarouselVertical = CreateCarouselVerticalDefault();

		internal static readonly LinearItemsLayout CarouselDefault = CreateCarouselHorizontalDefault();

		/// <summary>Bindable property for <see cref="ItemSpacing"/>.</summary>
		public static readonly BindableProperty ItemSpacingProperty =
			BindableProperty.Create(nameof(ItemSpacing), typeof(double), typeof(LinearItemsLayout), default(double),
				validateValue: (bindable, value) => (double)value >= 0);

		/// <summary>
		/// Gets or sets the spacing between consecutive items in the layout.
		/// </summary>
		/// <value>The spacing between items in platform-specific units. Must be 0 or greater. The default is 0.</value>
		public double ItemSpacing
		{
			get => (double)GetValue(ItemSpacingProperty);
			set => SetValue(ItemSpacingProperty, value);
		}

		internal static LinearItemsLayout CreateVerticalDefault()
			=> new LinearItemsLayout(ItemsLayoutOrientation.Vertical);

		internal static LinearItemsLayout CreateHorizontalDefault()
			=> new LinearItemsLayout(ItemsLayoutOrientation.Horizontal);

		internal static LinearItemsLayout CreateCarouselVerticalDefault()
			=> new LinearItemsLayout(ItemsLayoutOrientation.Vertical)
			{
				SnapPointsType = SnapPointsType.MandatorySingle,
				SnapPointsAlignment = SnapPointsAlignment.Center
			};

		internal static LinearItemsLayout CreateCarouselHorizontalDefault()
			=> new LinearItemsLayout(ItemsLayoutOrientation.Horizontal)
			{
				SnapPointsType = SnapPointsType.MandatorySingle,
				SnapPointsAlignment = SnapPointsAlignment.Center
			};
	}
}