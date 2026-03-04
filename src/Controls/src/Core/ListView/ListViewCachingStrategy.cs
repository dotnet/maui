using System;

namespace Microsoft.Maui.Controls
{
	/// <summary>Enumerates caching strategies for a ListView.</summary>
	[Flags]
	public enum ListViewCachingStrategy
	{
		/// <summary>Indicates that for every item in the List View's <see cref="Microsoft.Maui.Controls.ItemsView{T}.ItemsSource"/> property, a single unique element will be constructed from the DataTemplate.</summary>
		RetainElement = 0,
		/// <summary>Indicates that unneeded cells will have their binding contexts updated to that of a cell that is needed.</summary>
		RecycleElement = 1 << 0,
		/// <summary>Indicates that, in addition to the behavior specified by <see cref="Microsoft.Maui.Controls.ListViewCachingStrategy.RecycleElement"/>, <see cref="Microsoft.Maui.Controls.DataTemplate"/> objects that are selected by a <see cref="Microsoft.Maui.Controls.DataTemplateSelector"/> are cached by the data template type.</summary>
		RecycleElementAndDataTemplate = RecycleElement | 1 << 1,
	}
}