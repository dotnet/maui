namespace Microsoft.Maui.Controls
{
	/// <include file="../../../docs/Microsoft.Maui.Controls/StructuredItemsView.xml" path="Type[@FullName='Microsoft.Maui.Controls.StructuredItemsView']/Docs" />
	public class StructuredItemsView : ItemsView
	{
		/// <include file="../../../docs/Microsoft.Maui.Controls/StructuredItemsView.xml" path="//Member[@MemberName='HeaderProperty']/Docs" />
		public static readonly BindableProperty HeaderProperty =
			BindableProperty.Create(nameof(Header), typeof(object), typeof(ItemsView), null);

		/// <include file="../../../docs/Microsoft.Maui.Controls/StructuredItemsView.xml" path="//Member[@MemberName='Header']/Docs" />
		public object Header
		{
			get => GetValue(HeaderProperty);
			set => SetValue(HeaderProperty, value);
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/StructuredItemsView.xml" path="//Member[@MemberName='HeaderTemplateProperty']/Docs" />
		public static readonly BindableProperty HeaderTemplateProperty =
			BindableProperty.Create(nameof(HeaderTemplate), typeof(DataTemplate), typeof(ItemsView), null);

		/// <include file="../../../docs/Microsoft.Maui.Controls/StructuredItemsView.xml" path="//Member[@MemberName='HeaderTemplate']/Docs" />
		public DataTemplate HeaderTemplate
		{
			get => (DataTemplate)GetValue(HeaderTemplateProperty);
			set => SetValue(HeaderTemplateProperty, value);
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/StructuredItemsView.xml" path="//Member[@MemberName='FooterProperty']/Docs" />
		public static readonly BindableProperty FooterProperty =
			BindableProperty.Create(nameof(Footer), typeof(object), typeof(ItemsView), null);

		/// <include file="../../../docs/Microsoft.Maui.Controls/StructuredItemsView.xml" path="//Member[@MemberName='Footer']/Docs" />
		public object Footer
		{
			get => GetValue(FooterProperty);
			set => SetValue(FooterProperty, value);
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/StructuredItemsView.xml" path="//Member[@MemberName='FooterTemplateProperty']/Docs" />
		public static readonly BindableProperty FooterTemplateProperty =
			BindableProperty.Create(nameof(FooterTemplate), typeof(DataTemplate), typeof(ItemsView), null);

		/// <include file="../../../docs/Microsoft.Maui.Controls/StructuredItemsView.xml" path="//Member[@MemberName='FooterTemplate']/Docs" />
		public DataTemplate FooterTemplate
		{
			get => (DataTemplate)GetValue(FooterTemplateProperty);
			set => SetValue(FooterTemplateProperty, value);
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/StructuredItemsView.xml" path="//Member[@MemberName='ItemsLayoutProperty']/Docs" />
		public static readonly BindableProperty ItemsLayoutProperty = InternalItemsLayoutProperty;

		/// <include file="../../../docs/Microsoft.Maui.Controls/StructuredItemsView.xml" path="//Member[@MemberName='ItemsLayout']/Docs" />
		public IItemsLayout ItemsLayout
		{
			get => InternalItemsLayout;
			set => InternalItemsLayout = value;
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/StructuredItemsView.xml" path="//Member[@MemberName='ItemSizingStrategyProperty']/Docs" />
		public static readonly BindableProperty ItemSizingStrategyProperty =
			BindableProperty.Create(nameof(ItemSizingStrategy), typeof(ItemSizingStrategy), typeof(ItemsView));

		/// <include file="../../../docs/Microsoft.Maui.Controls/StructuredItemsView.xml" path="//Member[@MemberName='ItemSizingStrategy']/Docs" />
		public ItemSizingStrategy ItemSizingStrategy
		{
			get => (ItemSizingStrategy)GetValue(ItemSizingStrategyProperty);
			set => SetValue(ItemSizingStrategyProperty, value);
		}
	}
}