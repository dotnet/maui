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
		/// <para>
		/// The menu opens with the platform's context menu gesture: a long press on iOS and Android, a secondary
		/// (right) click on Mac Catalyst, Windows and Android with a mouse, Shift+F10 or the menu key on Windows,
		/// and holding Enter or the D-pad center on a focused control on Android.
		/// Its items are read every time it opens, so bindings, <see cref="MenuItem.IsEnabled"/> and commands that
		/// can't execute are always reflected. Elements without a context menu aren't affected.
		/// </para>
		/// <para>
		/// A long press is shared with other behavior the way each platform shares it. On iOS the context menu
		/// takes the long press over, including from a <see cref="LongPressGestureRecognizer"/>. On Android the
		/// pressed view's own long-press handler comes first (a <see cref="LongPressGestureRecognizer"/> or
		/// <see cref="DragGestureRecognizer"/> on it), then the context menu, which comes before tooltips, and the
		/// menus of the element's ancestors are added after its own, like native Android context menus.
		/// Reordering a <see cref="CollectionView"/> keeps its long press on both. The menu still opens with a
		/// secondary click or the keyboard.
		/// </para>
		/// <para>
		/// Text inputs (<see cref="Entry"/>, <see cref="Editor"/>, <see cref="SearchBar"/>) keep their own text
		/// selection, like in native apps: the items of their context menu, or of an ancestor's, are added to the
		/// system's text menus instead. That is the edit menu on iOS and Mac Catalyst (iOS 16 and later; on earlier
		/// versions the context menu opens on long press), and the selection toolbar and the context menu of a
		/// secondary click on Android.
		/// </para>
		/// <para>
		/// The menu is the platform's own context menu, so it has the platform's limitations:
		/// <list type="bullet">
		/// <item><description>Android: items have no icons, as native Android context menus don't show them, so
		/// <see cref="MenuItem.IconImageSource"/> is ignored. The menu opens where the element was pressed (at the
		/// center of a focused control for the keyboard) on Android 7.0 and later, and as a dialog on earlier
		/// versions and when opened by an accessibility service like TalkBack. Separators are drawn as dividers on
		/// Android 9.0 and later.</description></item>
		/// <item><description>Android text toolbars: the items show as plain text actions, without icons, dividers
		/// or nested sub menus.</description></item>
		/// <item><description>iOS and Mac Catalyst: icons can be file or font images
		/// (<see cref="FileImageSource"/>, <see cref="FontImageSource"/>); other image sources aren't shown.</description></item>
		/// </list>
		/// </para>
		/// <para>
		/// Set the <c>MauiEnableMobileContextFlyouts</c> MSBuild property to <see langword="false"/> to turn off
		/// context menus on iOS and Android and the text menu items on iOS, Mac Catalyst and Android.
		/// </para>
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
