using System;

namespace Microsoft.Maui.Controls.Handlers.Items2;

/// <summary>
/// Pairs a data item with its <see cref="DataTemplate"/> and layout metadata.
/// Used by the WinUI ItemsView/ItemsRepeater as the data context for each realized element.
/// </summary>
internal class ItemTemplateContext2
{
	readonly WeakReference<BindableObject> _container;

	/// <summary>The data template used to create the visual element for this item.</summary>
	public DataTemplate? MauiDataTemplate { get; }

	/// <summary>The MAUI context used for platform view creation.</summary>
	public IMauiContext? MauiContext { get; }

	/// <summary>The actual data item from the items source.</summary>
	public object? Item { get; }

	/// <summary>The parent container (e.g., the ItemsView). Held via a weak reference to avoid leaks.</summary>
	public BindableObject? Container => _container.TryGetTarget(out var c) ? c : null;

	/// <summary>The desired item height for uniform sizing strategies.</summary>
	public double ItemHeight { get; }

	/// <summary>The desired item width for uniform sizing strategies.</summary>
	public double ItemWidth { get; }

	/// <summary>The spacing between items.</summary>
	public Thickness ItemSpacing { get; }

	/// <summary>Whether this context represents a group header.</summary>
	public bool IsHeader { get; }

	/// <summary>Whether this context represents a group footer.</summary>
	public bool IsFooter { get; }

	public ItemTemplateContext2(DataTemplate mauiDataTemplate, object item, BindableObject container,
		double? height = null, double? width = null, Thickness? itemSpacing = null,
		bool isHeader = false, bool isFooter = false, IMauiContext? mauiContext = null)
	{
		MauiDataTemplate = mauiDataTemplate;
		Item = item;
		_container = new(container);
		MauiContext = mauiContext;
		ItemHeight = height ?? 0;
		ItemWidth = width ?? 0;
		ItemSpacing = itemSpacing ?? default;

		IsHeader = isHeader;
		IsFooter = isFooter;
	}
}