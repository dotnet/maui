namespace Microsoft.Maui.Controls
{
	/// <include file="../../../docs/Microsoft.Maui.Controls/ItemsLayout.xml" path="Type[@FullName='Microsoft.Maui.Controls.ItemsLayout']/Docs" />
	public abstract class ItemsLayout : BindableObject, IItemsLayout
	{
		/// <include file="../../../docs/Microsoft.Maui.Controls/ItemsLayout.xml" path="//Member[@MemberName='Orientation']/Docs" />
		public ItemsLayoutOrientation Orientation { get; }

		protected ItemsLayout([Parameter("Orientation")] ItemsLayoutOrientation orientation)
		{
			Orientation = orientation;
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/ItemsLayout.xml" path="//Member[@MemberName='SnapPointsAlignmentProperty']/Docs" />
		public static readonly BindableProperty SnapPointsAlignmentProperty =
			BindableProperty.Create(nameof(SnapPointsAlignment), typeof(SnapPointsAlignment), typeof(ItemsLayout),
				SnapPointsAlignment.Start);

		/// <include file="../../../docs/Microsoft.Maui.Controls/ItemsLayout.xml" path="//Member[@MemberName='SnapPointsAlignment']/Docs" />
		public SnapPointsAlignment SnapPointsAlignment
		{
			get => (SnapPointsAlignment)GetValue(SnapPointsAlignmentProperty);
			set => SetValue(SnapPointsAlignmentProperty, value);
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/ItemsLayout.xml" path="//Member[@MemberName='SnapPointsTypeProperty']/Docs" />
		public static readonly BindableProperty SnapPointsTypeProperty =
			BindableProperty.Create(nameof(SnapPointsType), typeof(SnapPointsType), typeof(ItemsLayout),
				SnapPointsType.None);

		/// <include file="../../../docs/Microsoft.Maui.Controls/ItemsLayout.xml" path="//Member[@MemberName='SnapPointsType']/Docs" />
		public SnapPointsType SnapPointsType
		{
			get => (SnapPointsType)GetValue(SnapPointsTypeProperty);
			set => SetValue(SnapPointsTypeProperty, value);
		}
	}
}
