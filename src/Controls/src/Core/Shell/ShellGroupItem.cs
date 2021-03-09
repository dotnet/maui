using System;

namespace Microsoft.Maui.Controls
{
	public class ShellGroupItem : BaseShellItem
	{
		public static readonly BindableProperty FlyoutDisplayOptionsProperty =
			BindableProperty.Create(nameof(FlyoutDisplayOptions), typeof(FlyoutDisplayOptions), typeof(ShellGroupItem), FlyoutDisplayOptions.AsSingleItem, BindingMode.OneTime, propertyChanged: OnFlyoutDisplayOptionsPropertyChanged);

		static void OnFlyoutDisplayOptionsPropertyChanged(BindableObject bindable, object oldValue, object newValue)
		{
			((Element)bindable).FindParentOfType<Shell>()?.SendFlyoutItemsChanged();
		}

		public FlyoutDisplayOptions FlyoutDisplayOptions
		{
			get { return (FlyoutDisplayOptions)GetValue(FlyoutDisplayOptionsProperty); }
			set { SetValue(FlyoutDisplayOptionsProperty, value); }
		}

		internal virtual ShellElementCollection ShellElementCollection { get; }
	}
}