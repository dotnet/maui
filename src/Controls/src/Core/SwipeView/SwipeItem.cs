#nullable disable
using System;
using System.ComponentModel;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	/// <summary>
	/// Represents a menu item displayed in a <see cref="SwipeView"/> when the view is swiped.
	/// </summary>
	public partial class SwipeItem : MenuItem, Controls.ISwipeItem, Maui.ISwipeItemMenuItem
	{
		/// <summary>Bindable property for <see cref="BackgroundColor"/>.</summary>
		public static readonly BindableProperty BackgroundColorProperty = BindableProperty.Create(nameof(BackgroundColor), typeof(Color), typeof(SwipeItem), null);

		/// <summary>Bindable property for <see cref="IsVisible"/>.</summary>
		public static readonly BindableProperty IsVisibleProperty = BindableProperty.Create(nameof(IsVisible), typeof(bool), typeof(SwipeItem), true, propertyChanged: OnIsVisibleChanged);

		/// <summary>Bindable property for <see cref="IconColor"/>.</summary>
		public static readonly BindableProperty IconColorProperty = BindableProperty.Create(nameof(IconColor), typeof(Color), typeof(SwipeItem), null);

		/// <summary>Bindable property for <see cref="TextColor"/>.</summary>
		public static readonly BindableProperty TextColorProperty = BindableProperty.Create(nameof(TextColor), typeof(Color), typeof(SwipeItem), null, propertyChanged: OnTextColorChanged);

		/// <summary>
		/// Gets or sets the background color of the swipe item. This is a bindable property.
		/// </summary>
		public Color BackgroundColor
		{
			get { return (Color)GetValue(BackgroundColorProperty); }
			set { SetValue(BackgroundColorProperty, value); }
		}

		/// <summary>
		/// Gets or sets a value indicating whether this swipe item is visible. This is a bindable property.
		/// </summary>
		public bool IsVisible
		{
			get { return (bool)GetValue(IsVisibleProperty); }
			set { SetValue(IsVisibleProperty, value); }
		}

		public event EventHandler<EventArgs> Invoked;

		/// <summary>
		/// Gets or sets the color used to tint <see cref="MenuItem.IconImageSource"/>. This is a bindable property.
		/// </summary>
		/// <remarks>
		/// When unset, a font icon is tinted by its own <see cref="FontImageSource.Color"/>, then by
		/// <see cref="TextColor"/>, then by a color contrasting <see cref="BackgroundColor"/>; image icons such as
		/// PNG and SVG render with their original colors. When set, this property tints font icons and image icons on
		/// Android, iOS, and MacCatalyst (including stream-based sources, since the resolved platform image is
		/// tinted), and font, file-based, and URI-based icons on Windows, and can be bound with
		/// <see cref="AppThemeBinding"/> to follow the current theme. Stream-based image icons on Windows and all
		/// icons on Tizen currently render with their original colors regardless of this property.
		/// </remarks>
		public Color IconColor
		{
			get { return (Color)GetValue(IconColorProperty); }
			set { SetValue(IconColorProperty, value); }
		}

		/// <summary>
		/// Gets or sets the color used for the swipe item's label text. This is a bindable property.
		/// </summary>
		/// <remarks>
		/// When unset, the label falls back to a color contrasting <see cref="BackgroundColor"/>, except when the
		/// item's icon is a <see cref="FontImageSource"/> that already specifies its own
		/// <see cref="FontImageSource.Color"/>, or when no <see cref="BackgroundColor"/> is set; in those cases the
		/// label keeps the platform default. When set, that color is used as the label color, and it can be bound
		/// with <see cref="AppThemeBinding"/> to follow the current theme.
		/// </remarks>
		public Color TextColor
		{
			get { return (Color)GetValue(TextColorProperty); }
			set { SetValue(TextColorProperty, value); }
		}

		Paint ISwipeItemMenuItem.Background => new SolidPaint(BackgroundColor);

		Color ISwipeItemMenuItem.IconColor => IconColor;

		Color ISwipeItemMenuItem.TextColor => TextColor;

		Visibility ISwipeItemMenuItem.Visibility => this.IsVisible ? Visibility.Visible : Visibility.Collapsed;

		static void OnTextColorChanged(BindableObject bindable, object oldValue, object newValue)
		{
			var swipeItem = (SwipeItem)bindable;

			// The same-key TextColor mapper update is already fired automatically by
			// Element.OnBindablePropertySet, so only the cross-key IconColor refresh is done here:
			// TextColor drives the icon tint for an untinted font icon (GetIconTintColor falls back
			// to GetTextColor only when IconColor is unset and the source is an IFontImageSource
			// without its own Color). Refresh the icon just in that case so its tint isn't left stale,
			// avoiding a needless image reload for image sources or an explicitly tinted icon.
			if (swipeItem.IconColor is null && swipeItem.IconImageSource is IFontImageSource { Color: null })
				swipeItem.Handler?.UpdateValue(nameof(ISwipeItemMenuItem.IconColor));
		}

		static void OnIsVisibleChanged(BindableObject bindable, object oldValue, object newValue)
		{
			var swipeItem = (SwipeItem)bindable;
			swipeItem.Handler?.UpdateValue(nameof(ISwipeItemMenuItem.Visibility));
		}

		void Maui.ISwipeItem.OnInvoked()
		{
			if (Command != null && Command.CanExecute(CommandParameter))
				Command.Execute(CommandParameter);

			OnClicked();
			Invoked?.Invoke(this, EventArgs.Empty);
		}

		void IImageSourcePart.UpdateIsLoading(bool isLoading)
		{
		}
	}
}