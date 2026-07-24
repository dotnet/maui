#nullable disable
using System;
using System.ComponentModel;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.Controls
{
	/// <summary>
	/// Represents a menu item displayed in a <see cref="SwipeView"/> when the view is swiped.
	/// </summary>
	[ElementHandler(typeof(SwipeItemMenuItemHandler))]
	public partial class SwipeItem : MenuItem, Controls.ISwipeItem, Maui.ISwipeItemMenuItem
	{
		/// <summary>Bindable property for <see cref="BackgroundColor"/>.</summary>
		public static readonly BindableProperty BackgroundColorProperty = BindableProperty.Create(nameof(BackgroundColor), typeof(Color), typeof(SwipeItem), null);

		/// <summary>Bindable property for <see cref="IsVisible"/>.</summary>
		public static readonly BindableProperty IsVisibleProperty = BindableProperty.Create(nameof(IsVisible), typeof(bool), typeof(SwipeItem), true);

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

		Paint ISwipeItemMenuItem.Background => new SolidPaint(BackgroundColor);

		Visibility ISwipeItemMenuItem.Visibility => this.IsVisible ? Visibility.Visible : Visibility.Collapsed;

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