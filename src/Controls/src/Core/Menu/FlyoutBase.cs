#nullable disable
namespace Microsoft.Maui.Controls
{
	public abstract class FlyoutBase : Element, IFlyout
	{
		/// <summary>Bindable property for attached property <c>ContextFlyout</c>.</summary>
		/// <remarks>On Android and iOS, a context flyout opens with a long press.</remarks>
		public static readonly BindableProperty ContextFlyoutProperty = BindableProperty.CreateAttached("ContextFlyout", typeof(FlyoutBase), typeof(FlyoutBase), null,
			propertyChanged: (bo, oldV, newV) =>
			{
				bo.AddRemoveLogicalChildren(oldV, newV);
			});

		public static void SetContextFlyout(BindableObject b, FlyoutBase value)
		{
			b.SetValue(ContextFlyoutProperty, value);
		}

		public static FlyoutBase GetContextFlyout(BindableObject b)
		{
			return (FlyoutBase)b.GetValue(ContextFlyoutProperty);
		}
	}
}
