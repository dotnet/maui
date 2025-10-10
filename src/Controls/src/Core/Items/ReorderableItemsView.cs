#nullable disable
using System;
using System.ComponentModel;

namespace Microsoft.Maui.Controls
{
	/// <summary>
	/// A <see cref="GroupableItemsView"/> that supports reordering of items through user interaction.
	/// </summary>
	/// <remarks>
	/// This class extends <see cref="GroupableItemsView"/> to provide reordering capabilities.
	/// Use <see cref="CanReorderItems"/> to enable or disable reordering functionality.
	/// When items are grouped, use <see cref="CanMixGroups"/> to control whether items can be moved between groups.
	/// </remarks>
	public class ReorderableItemsView : GroupableItemsView
	{
		/// <summary>
		/// Occurs when a reorder operation has been completed.
		/// </summary>
		/// <remarks>
		/// This event is raised after the user has successfully reordered an item and the operation is complete.
		/// It can be used to update the underlying data source or perform other actions in response to the reordering.
		/// </remarks>
		public event EventHandler ReorderCompleted;

		/// <summary>Bindable property for <see cref="CanMixGroups"/>.</summary>
		public static readonly BindableProperty CanMixGroupsProperty = BindableProperty.Create(nameof(CanMixGroups), typeof(bool), typeof(ReorderableItemsView), false);

		/// <summary>
		/// Gets or sets a value indicating whether items from different groups can be mixed together during reordering.
		/// </summary>
		/// <value><see langword="true"/> if items can be moved between groups during reordering; otherwise, <see langword="false"/>. The default is <see langword="false"/>.</value>
		/// <remarks>
		/// When <see langword="true"/>, items can be dragged and dropped between different groups during reordering operations.
		/// When <see langword="false"/>, items can only be reordered within their own group.
		/// This property is only meaningful when the items view is grouped and <see cref="CanReorderItems"/> is <see langword="true"/>.
		/// </remarks>
		public bool CanMixGroups
		{
			get { return (bool)GetValue(CanMixGroupsProperty); }
			set { SetValue(CanMixGroupsProperty, value); }
		}

		/// <summary>Bindable property for <see cref="CanReorderItems"/>.</summary>
		public static readonly BindableProperty CanReorderItemsProperty = BindableProperty.Create(nameof(CanReorderItems), typeof(bool), typeof(ReorderableItemsView), false);

		/// <summary>
		/// Gets or sets a value indicating whether items in the collection can be reordered by the user.
		/// </summary>
		/// <value><see langword="true"/> if items can be reordered through user interaction (such as drag and drop); otherwise, <see langword="false"/>. The default is <see langword="false"/>.</value>
		/// <remarks>
		/// When enabled, users can typically drag and drop items to reorder them within the collection.
		/// The specific interaction method (drag and drop, etc.) depends on the platform implementation.
		/// When an item is successfully reordered, the <see cref="ReorderCompleted"/> event is raised.
		/// </remarks>
		public bool CanReorderItems
		{
			get { return (bool)GetValue(CanReorderItemsProperty); }
			set { SetValue(CanReorderItemsProperty, value); }
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void SendReorderCompleted() => ReorderCompleted?.Invoke(this, EventArgs.Empty);
	}
}
