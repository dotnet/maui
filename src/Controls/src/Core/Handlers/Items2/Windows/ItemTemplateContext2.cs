using System;

namespace Microsoft.Maui.Controls.Handlers.Items2;

internal class ItemTemplateContext2
{
	readonly WeakReference<BindableObject> _container;
	public DataTemplate? MauiDataTemplate { get; }
	public IMauiContext? MauiContext { get; }
	public object? Item { get; }
	public BindableObject? Container => _container.TryGetTarget(out var c) ? c : null;
	public double ItemHeight { get; }
	public double ItemWidth { get; }
	public Thickness ItemSpacing { get; }

	public bool IsHeader { get; }
	public bool IsFooter { get; }

	/// <summary>
	/// Gets whether this item should span the full width/height of the layout.
	/// Group headers and footers are full-span items in grid layouts.
	/// </summary>
	public bool IsFullSpan => IsHeader || IsFooter;

	public ItemTemplateContext2(DataTemplate mauiDataTemplate, object item, BindableObject container,
		double? height = null, double? width = null, Thickness? itemSpacing = null,
		bool isHeader = false, bool isFooter = false, IMauiContext? mauiContext = null)
	{
		MauiDataTemplate = mauiDataTemplate;
		Item = item;
		_container = new(container);
		MauiContext = mauiContext;

		if (height.HasValue)
			ItemHeight = height.Value;

		if (width.HasValue)
			ItemWidth = width.Value;

		if (itemSpacing.HasValue)
			ItemSpacing = itemSpacing.Value;

		IsHeader = isHeader;
		IsFooter = isFooter;
	}
}