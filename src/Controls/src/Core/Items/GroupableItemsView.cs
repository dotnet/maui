#nullable disable
namespace Microsoft.Maui.Controls
{
	/// <include file="../../../docs/Microsoft.Maui.Controls/GroupableItemsView.xml" path="Type[@FullName='Microsoft.Maui.Controls.GroupableItemsView']/Docs/*" />
	public class GroupableItemsView : SelectableItemsView
	{
		/// <summary>Bindable property for <see cref="IsGrouped"/>.</summary>
		public static readonly BindableProperty IsGroupedProperty =
			BindableProperty.Create(nameof(IsGrouped), typeof(bool), typeof(GroupableItemsView), false);

		/// <include file="../../../docs/Microsoft.Maui.Controls/GroupableItemsView.xml" path="//Member[@MemberName='IsGrouped']/Docs/*" />
		public bool IsGrouped
		{
			get => (bool)GetValue(IsGroupedProperty);
			set => SetValue(IsGroupedProperty, value);
		}

		/// <summary>Bindable property for <see cref="GroupHeaderTemplate"/>.</summary>
		public static readonly BindableProperty GroupHeaderTemplateProperty =
			BindableProperty.Create(nameof(GroupHeaderTemplate), typeof(DataTemplate), typeof(GroupableItemsView), default(DataTemplate));

		/// <include file="../../../docs/Microsoft.Maui.Controls/GroupableItemsView.xml" path="//Member[@MemberName='GroupHeaderTemplate']/Docs/*" />
		public DataTemplate GroupHeaderTemplate
		{
			get => (DataTemplate)GetValue(GroupHeaderTemplateProperty);
			set => SetValue(GroupHeaderTemplateProperty, value);
		}

		/// <summary>Bindable property for <see cref="GroupFooterTemplate"/>.</summary>
		public static readonly BindableProperty GroupFooterTemplateProperty =
			BindableProperty.Create(nameof(GroupFooterTemplate), typeof(DataTemplate), typeof(GroupableItemsView), default(DataTemplate));

		/// <include file="../../../docs/Microsoft.Maui.Controls/GroupableItemsView.xml" path="//Member[@MemberName='GroupFooterTemplate']/Docs/*" />
		public DataTemplate GroupFooterTemplate
		{
			get => (DataTemplate)GetValue(GroupFooterTemplateProperty);
			set => SetValue(GroupFooterTemplateProperty, value);
		}
	}
}
