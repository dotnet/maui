namespace Xamarin.Forms
{
	public class StructuredItemsView : ItemsView
	{
		public static readonly BindableProperty HeaderProperty =
			BindableProperty.Create(nameof(Header), typeof(object), typeof(ItemsView), null);

		public object Header
		{
			get => GetValue(HeaderProperty);
			set => SetValue(HeaderProperty, value);
		}

		public static readonly BindableProperty HeaderTemplateProperty =
			BindableProperty.Create(nameof(HeaderTemplate), typeof(DataTemplate), typeof(ItemsView), null);

		public DataTemplate HeaderTemplate
		{
			get => (DataTemplate)GetValue(HeaderTemplateProperty);
			set => SetValue(HeaderTemplateProperty, value);
		}

		public static readonly BindableProperty FooterProperty =
			BindableProperty.Create(nameof(Footer), typeof(object), typeof(ItemsView), null);

		public object Footer
		{
			get => GetValue(FooterProperty);
			set => SetValue(FooterProperty, value);
		}

		public static readonly BindableProperty FooterTemplateProperty =
			BindableProperty.Create(nameof(FooterTemplate), typeof(DataTemplate), typeof(ItemsView), null);

		public DataTemplate FooterTemplate
		{
			get => (DataTemplate)GetValue(FooterTemplateProperty);
			set => SetValue(FooterTemplateProperty, value);
		}

		public static readonly BindableProperty ItemsLayoutProperty = InternalItemsLayoutProperty;

		public IItemsLayout ItemsLayout
		{
			get => InternalItemsLayout;
			set => InternalItemsLayout = value;
		}
	}
}