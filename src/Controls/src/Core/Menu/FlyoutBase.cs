#nullable disable
namespace Microsoft.Maui.Controls
{
	public abstract class FlyoutBase : Element, IFlyout
	{
		/// <summary>Bindable property for attached property <c>ContextFlyout</c>.</summary>
		public static readonly BindableProperty ContextFlyoutProperty = BindableProperty.CreateAttached("ContextFlyout", typeof(FlyoutBase), typeof(FlyoutBase), null,
			propertyChanged: (bo, oldV, newV) =>
			{
				bo.AddRemoveLogicalChildren(oldV, newV);
			});

		/// <summary>Sets the context menu that is shown for an element.</summary>
		/// <param name="b">The element that shows the context menu.</param>
		/// <param name="value">The <see cref="MenuFlyout"/> to show, or <see langword="null"/> to remove the context menu.</param>
		/// <remarks>
		/// The menu opens with the platform's context menu gesture: a long press on iOS and Android, a secondary
		/// (right) click on Mac Catalyst, Windows and Android with a mouse, and Shift+F10 or the menu key on
		/// Windows and Android. Its items are read every time it opens, so bindings, <see cref="MenuItem.IsEnabled"/>
		/// and commands that can't execute are always reflected. Elements without a context menu aren't affected.
		/// </remarks>
		public static void SetContextFlyout(BindableObject b, FlyoutBase value)
		{
			b.SetValue(ContextFlyoutProperty, value);
		}

		/// <summary>Gets the context menu that is shown for an element.</summary>
		/// <param name="b">The element to get the context menu of.</param>
		/// <returns>The context menu, or <see langword="null"/> when the element doesn't have one.</returns>
		public static FlyoutBase GetContextFlyout(BindableObject b)
		{
			return (FlyoutBase)b.GetValue(ContextFlyoutProperty);
		}
	}
}
