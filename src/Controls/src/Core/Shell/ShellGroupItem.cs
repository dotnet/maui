#nullable disable
using System;

namespace Microsoft.Maui.Controls
{
	/// <summary>
	/// Base class for grouping Shell items such as <see cref="ShellItem"/> and <see cref="ShellSection"/>.
	/// </summary>
	public class ShellGroupItem : BaseShellItem
	{
		/// <summary>Bindable property for <see cref="FlyoutDisplayOptions"/>.</summary>
		public static readonly BindableProperty FlyoutDisplayOptionsProperty =
			BindableProperty.Create(nameof(FlyoutDisplayOptions), typeof(FlyoutDisplayOptions), typeof(ShellGroupItem), FlyoutDisplayOptions.AsSingleItem, BindingMode.OneTime, propertyChanged: OnFlyoutDisplayOptionsPropertyChanged);

		static void OnFlyoutDisplayOptionsPropertyChanged(BindableObject bindable, object oldValue, object newValue)
		{
			((Element)bindable).FindParentOfType<Shell>()?.SendFlyoutItemsChanged();
		}

		/// <summary>
		/// Gets or sets how this item is displayed in the flyout. This is a bindable property.
		/// </summary>
		public FlyoutDisplayOptions FlyoutDisplayOptions
		{
			get { return (FlyoutDisplayOptions)GetValue(FlyoutDisplayOptionsProperty); }
			set { SetValue(FlyoutDisplayOptionsProperty, value); }
		}

		internal virtual ShellElementCollection ShellElementCollection { get; }
	}
}