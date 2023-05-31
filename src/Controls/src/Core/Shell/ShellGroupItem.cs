#nullable disable
using System;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../../docs/Microsoft.Maui.Controls/ShellGroupItem.xml" path="Type[@FullName='Microsoft.Maui.Controls.ShellGroupItem']/Docs/*" />
	public class ShellGroupItem : BaseShellItem
	{
		/// <summary>Bindable property for <see cref="FlyoutDisplayOptions"/>.</summary>
		public static readonly BindableProperty FlyoutDisplayOptionsProperty =
			BindableProperty.Create(nameof(FlyoutDisplayOptions), typeof(FlyoutDisplayOptions), typeof(ShellGroupItem), FlyoutDisplayOptions.AsSingleItem, BindingMode.OneTime, propertyChanged: OnFlyoutDisplayOptionsPropertyChanged);

		static void OnFlyoutDisplayOptionsPropertyChanged(BindableObject bindable, object oldValue, object newValue)
		{
			((Element)bindable).FindParentOfType<Shell>()?.SendFlyoutItemsChanged();
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/ShellGroupItem.xml" path="//Member[@MemberName='FlyoutDisplayOptions']/Docs/*" />
		public FlyoutDisplayOptions FlyoutDisplayOptions
		{
			get { return (FlyoutDisplayOptions)GetValue(FlyoutDisplayOptionsProperty); }
			set { SetValue(FlyoutDisplayOptionsProperty, value); }
		}

		internal virtual ShellElementCollection ShellElementCollection { get; }
	}
}