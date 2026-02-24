using System.Collections;

namespace Microsoft.Maui.Controls.Handlers.Items2
{
	/// <summary>
	/// Represents a single group as an item in the outer ItemsView for grouped grid mode.
	/// <para>
	/// Instead of flattening all groups into individual items (as <see cref="GroupedItemTemplateCollection2"/> does),
	/// each group becomes a single element in the ItemsView's source. The <see cref="ItemFactory"/> uses this context
	/// to build a native panel containing: GroupHeader + Grid (with items) + GroupFooter.
	/// </para>
	/// <para>
	/// This enables the architecture: <c>ItemsView.Layout = StackLayout</c> (virtualizes groups)
	/// with a native Grid panel inside each group for grid item arrangement — no custom
	/// MeasureOverride/ArrangeOverride needed.
	/// </para>
	/// </summary>
	internal class GroupGridContext
	{
		/// <summary>
		/// The group object. Used as BindingContext for group header/footer templates.
		/// Also implements IList for accessing group items.
		/// </summary>
		public object Group { get; }

		/// <summary>
		/// The items within this group, cast from the group object.
		/// </summary>
		public IList Items { get; }

		/// <summary>
		/// The item template (or DataTemplateSelector) for rendering individual items within the grid.
		/// </summary>
		public DataTemplate? ItemTemplate { get; }

		/// <summary>
		/// Template for the group header, rendered above the grid.
		/// </summary>
		public DataTemplate? GroupHeaderTemplate { get; }

		/// <summary>
		/// Template for the group footer, rendered below the grid.
		/// </summary>
		public DataTemplate? GroupFooterTemplate { get; }

		/// <summary>
		/// The parent container (CollectionView) for template binding context propagation.
		/// </summary>
		public BindableObject Container { get; }

		/// <summary>
		/// The MAUI context for converting MAUI views to native elements.
		/// </summary>
		public IMauiContext? MauiContext { get; }

		public GroupGridContext(
			object group,
			DataTemplate? itemTemplate,
			DataTemplate? groupHeaderTemplate,
			DataTemplate? groupFooterTemplate,
			BindableObject container,
			IMauiContext? mauiContext = null)
		{
			Group = group;
			Items = group as IList ?? new System.Collections.Generic.List<object>();
			ItemTemplate = itemTemplate;
			GroupHeaderTemplate = groupHeaderTemplate;
			GroupFooterTemplate = groupFooterTemplate;
			Container = container;
			MauiContext = mauiContext;
		}
	}
}
