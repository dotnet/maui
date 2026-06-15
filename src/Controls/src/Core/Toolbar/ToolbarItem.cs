#nullable disable
using System;
using Microsoft.Maui.Graphics;

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

	/// <summary>Bindable property for <see cref="BadgeText"/>.</summary>
	public static readonly BindableProperty BadgeTextProperty = BindableProperty.Create(
		nameof(BadgeText), typeof(string), typeof(ToolbarItem), default(string));

	/// <summary>Bindable property for <see cref="BadgeColor"/>.</summary>
	public static readonly BindableProperty BadgeColorProperty = BindableProperty.Create(
		nameof(BadgeColor), typeof(Color), typeof(ToolbarItem), default(Color));

	/// <summary>Bindable property for <see cref="BadgeTextColor"/>.</summary>
	public static readonly BindableProperty BadgeTextColorProperty = BindableProperty.Create(
		nameof(BadgeTextColor), typeof(Color), typeof(ToolbarItem), default(Color));

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
	/// Gets or sets the badge text displayed on this toolbar item.
	/// Set to a non-empty string to show a text/count badge, an empty string to show a dot indicator, or <see langword="null"/> to hide the badge.
	/// This is a bindable property.
	/// </summary>
	/// <remarks>
	/// <para>
	/// Badge rendering varies by platform:
	/// </para>
	/// <list type="bullet">
	/// <item><description><b>Android</b>: Uses Material Design <c>BadgeDrawable</c> via <c>BadgeUtils</c>. Supports numeric and text badges on primary toolbar items. Empty string shows a small dot indicator.</description></item>
	/// <item><description><b>iOS/MacCatalyst</b>: Uses the native <c>UIBarButtonItem.badge</c> API introduced in iOS 26. On earlier iOS versions, the badge is silently ignored. Empty string shows a dot indicator.</description></item>
	/// <item><description><b>Windows</b>: Uses WinUI <c>InfoBadge</c> overlaid on the toolbar button. Numeric values display as counts; non-numeric text and empty string display as a dot indicator.</description></item>
	/// </list>
	/// <para>
	/// Badges are only displayed on primary toolbar items (items with <see cref="Order"/> set to <see cref="ToolbarItemOrder.Primary"/> or <see cref="ToolbarItemOrder.Default"/>).
	/// Secondary (overflow) items do not display badges.
	/// </para>
	/// </remarks>
	public string BadgeText
	{
		get => (string)GetValue(BadgeTextProperty);
		set => SetValue(BadgeTextProperty, value);
	}

	/// <summary>
	/// Gets or sets the background color of the badge displayed on this toolbar item.
	/// When set to <see langword="null"/>, the platform default badge color is used.
	/// This is a bindable property.
	/// </summary>
	/// <remarks>
	/// This property is only effective when <see cref="BadgeText"/> is set to a non-empty value.
	/// </remarks>
	public Color BadgeColor
	{
		get => (Color)GetValue(BadgeColorProperty);
		set => SetValue(BadgeColorProperty, value);
	}

	/// <summary>
	/// Gets or sets the foreground (text) color of the badge displayed on this toolbar item.
	/// When set to <see langword="null"/>, the platform default text color is used (typically white).
	/// This is a bindable property.
	/// </summary>
	/// <remarks>
	/// <para>
	/// Platform support:
	/// </para>
	/// <list type="bullet">
	/// <item><description><b>Android</b>: Maps to <c>BadgeDrawable.BadgeTextColor</c>.</description></item>
	/// <item><description><b>iOS/MacCatalyst 26+</b>: Maps to <c>UIBarButtonItemBadge.ForegroundColor</c>.</description></item>
	/// <item><description><b>Windows</b>: Maps to <c>InfoBadge.Foreground</c>.</description></item>
	/// </list>
	/// </remarks>
	public Color BadgeTextColor
	{
		get => (Color)GetValue(BadgeTextColorProperty);
		set => SetValue(BadgeTextColorProperty, value);
	}
}
