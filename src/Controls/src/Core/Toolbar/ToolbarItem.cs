#nullable disable
using System;

namespace Microsoft.Maui.Controls;

/// <summary>
/// An item in a toolbar or displayed on a Page.
/// </summary>
/// <remarks>Any changes made to the properties of the toolbar item after it has been added will be ignored.</remarks>
public class ToolbarItem : MenuItem
{
	static readonly BindableProperty OrderProperty = BindableProperty.Create(nameof(Order), typeof(ToolbarItemOrder), typeof(ToolbarItem), ToolbarItemOrder.Default, validateValue: (bo, o) =>
	{
		var order = (ToolbarItemOrder)o;
		return order == ToolbarItemOrder.Default || order == ToolbarItemOrder.Primary || order == ToolbarItemOrder.Secondary;
	});

	static readonly BindableProperty PriorityProperty = BindableProperty.Create(nameof(Priority), typeof(int), typeof(ToolbarItem), 0);

	/// <summary>Bindable property for <see cref="IsVisible"/>.</summary>
	public static readonly BindableProperty IsVisibleProperty = BindableProperty.Create(nameof(IsVisible), typeof(bool), typeof(ToolbarItem), true, propertyChanged: OnIsVisibleChanged);

	/// <summary>
	/// Constructs and initializes a new instance of the ToolbarItem class.
	/// </summary>
	public ToolbarItem()
	{
	}

	/// <summary>
	/// Constructs and initializes a new instance of the ToolbarItem class.
	/// </summary>
	/// <param name="name">The name to use for this toolbar item.</param>
	/// <param name="icon">The icon to use for this toolbar item.</param>
	/// <param name="activated">The action to invoke when this toolbar item is activated.</param>
	/// <param name="order">Determines if this toolbar item is visible on the <see cref="Toolbar"/> directly or in a collapsed menu.</param>
	/// <param name="priority">Determines the order of this toolbar item.</param>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="activated"/> is <see langword="null"/>.</exception>
	public ToolbarItem(string name, string icon, Action activated, ToolbarItemOrder order = ToolbarItemOrder.Default, int priority = 0)
	{
		if (activated is null)
		{
			throw new ArgumentNullException(nameof(activated));
		}

		Text = name;
		IconImageSource = icon;
		Clicked += (s, e) => activated();
		Order = order;
		Priority = priority;
	}

	/// <summary>
	/// Gets or sets a value that indicates on which of the primary, secondary, or default toolbar surfaces to display this ToolbarItem element.
	/// This is a bindable property.
	/// </summary>
	public ToolbarItemOrder Order
	{
		get { return (ToolbarItemOrder)GetValue(OrderProperty); }
		set { SetValue(OrderProperty, value); }
	}

	/// <summary>
	/// Gets or sets the priority of this ToolbarItem element. This is a bindable property.
	/// </summary>
	public int Priority
	{
		get { return (int)GetValue(PriorityProperty); }
		set { SetValue(PriorityProperty, value); }
	}

	/// <summary>
	/// Gets or sets a value that indicates whether this ToolbarItem is visible.
	/// This is a bindable property.
	/// </summary>
	public bool IsVisible
	{
		get { return (bool)GetValue(IsVisibleProperty); }
		set { SetValue(IsVisibleProperty, value); }
	}

	static void OnIsVisibleChanged(BindableObject bindable, object oldValue, object newValue)
	{
		// Platform-specific toolbar rendering will handle filtering invisible items
		// when the toolbar is refreshed
	}
}
