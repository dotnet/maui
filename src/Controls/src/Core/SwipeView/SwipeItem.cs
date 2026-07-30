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
		public static readonly BindableProperty IconColorProperty = BindableProperty.Create(nameof(IconColor), typeof(Color), typeof(SwipeItem), null, propertyChanged: OnIconColorChanged);

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
		/// When unset, a <see cref="FontImageSource"/> uses its own <see cref="FontImageSource.Color"/> and falls
		/// back to a color contrasting <see cref="BackgroundColor"/>, while image icons such as PNG and SVG render
		/// with their original colors. When set, this property tints font, file-based, and URI-based image icons on
		/// Android, iOS, MacCatalyst, and Windows, and can be bound with <see cref="AppThemeBinding"/> to follow the
		/// current theme. Stream-based image icons on Windows and all icons on Tizen currently render with their
		/// original colors regardless of this property.
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
		/// When unset, the label falls back to a color contrasting <see cref="BackgroundColor"/>.
		/// When set, that color is used as the label color, and it can be bound with
		/// <see cref="AppThemeBinding"/> to follow the current theme.
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

		static void OnIconColorChanged(BindableObject bindable, object oldValue, object newValue)
		{
			var swipeItem = (SwipeItem)bindable;
			swipeItem.Handler?.UpdateValue(nameof(ISwipeItemMenuItem.IconColor));
		}

		static void OnTextColorChanged(BindableObject bindable, object oldValue, object newValue)
		{
			var swipeItem = (SwipeItem)bindable;
			swipeItem.Handler?.UpdateValue(nameof(ISwipeItemMenuItem.TextColor));
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