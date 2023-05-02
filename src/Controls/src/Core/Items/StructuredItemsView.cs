#nullable disable
namespace Microsoft.Maui.Controls
{
	/// <include file="../../../docs/Microsoft.Maui.Controls/StructuredItemsView.xml" path="Type[@FullName='Microsoft.Maui.Controls.StructuredItemsView']/Docs/*" />
	public class StructuredItemsView : ItemsView
	{
		/// <summary>Bindable property for <see cref="Header"/>.</summary>
		public static readonly BindableProperty HeaderProperty =
			BindableProperty.Create(nameof(Header), typeof(object), typeof(ItemsView), null);

		/// <include file="../../../docs/Microsoft.Maui.Controls/StructuredItemsView.xml" path="//Member[@MemberName='Header']/Docs/*" />
		public object Header
		{
			get => GetValue(HeaderProperty);
			set => SetValue(HeaderProperty, value);
		}

		/// <summary>Bindable property for <see cref="HeaderTemplate"/>.</summary>
		public static readonly BindableProperty HeaderTemplateProperty =
			BindableProperty.Create(nameof(HeaderTemplate), typeof(DataTemplate), typeof(ItemsView), null);

		/// <include file="../../../docs/Microsoft.Maui.Controls/StructuredItemsView.xml" path="//Member[@MemberName='HeaderTemplate']/Docs/*" />
		public DataTemplate HeaderTemplate
		{
			get => (DataTemplate)GetValue(HeaderTemplateProperty);
			set => SetValue(HeaderTemplateProperty, value);
		}

		/// <summary>Bindable property for <see cref="Footer"/>.</summary>
		public static readonly BindableProperty FooterProperty =
			BindableProperty.Create(nameof(Footer), typeof(object), typeof(ItemsView), null);

		/// <include file="../../../docs/Microsoft.Maui.Controls/StructuredItemsView.xml" path="//Member[@MemberName='Footer']/Docs/*" />
		public object Footer
		{
			get => GetValue(FooterProperty);
			set => SetValue(FooterProperty, value);
		}

		/// <summary>Bindable property for <see cref="FooterTemplate"/>.</summary>
		public static readonly BindableProperty FooterTemplateProperty =
			BindableProperty.Create(nameof(FooterTemplate), typeof(DataTemplate), typeof(ItemsView), null);

		/// <include file="../../../docs/Microsoft.Maui.Controls/StructuredItemsView.xml" path="//Member[@MemberName='FooterTemplate']/Docs/*" />
		public DataTemplate FooterTemplate
		{
			get => (DataTemplate)GetValue(FooterTemplateProperty);
			set => SetValue(FooterTemplateProperty, value);
		}

		/// <summary>Bindable property for <see cref="ItemsLayout"/>.</summary>
		public static readonly BindableProperty ItemsLayoutProperty = InternalItemsLayoutProperty;

		/// <include file="../../../docs/Microsoft.Maui.Controls/StructuredItemsView.xml" path="//Member[@MemberName='ItemsLayout']/Docs/*" />
		public IItemsLayout ItemsLayout
		{
			get => InternalItemsLayout;
			set => InternalItemsLayout = value;
		}

		/// <summary>Bindable property for <see cref="ItemSizingStrategy"/>.</summary>
		public static readonly BindableProperty ItemSizingStrategyProperty =
			BindableProperty.Create(nameof(ItemSizingStrategy), typeof(ItemSizingStrategy), typeof(ItemsView));

		/// <include file="../../../docs/Microsoft.Maui.Controls/StructuredItemsView.xml" path="//Member[@MemberName='ItemSizingStrategy']/Docs/*" />
		public ItemSizingStrategy ItemSizingStrategy
		{
			get => (ItemSizingStrategy)GetValue(ItemSizingStrategyProperty);
			set => SetValue(ItemSizingStrategyProperty, value);
		}
	}
}