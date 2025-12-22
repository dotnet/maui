#nullable disable
using System;
using System.ComponentModel;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/SwipeItem.xml" path="Type[@FullName='Microsoft.Maui.Controls.SwipeItem']/Docs/*" />
	public partial class SwipeItem : MenuItem, Controls.ISwipeItem, Maui.ISwipeItemMenuItem
	{
		/// <summary>Bindable property for <see cref="BackgroundColor"/>.</summary>
		public static readonly BindableProperty BackgroundColorProperty = BindableProperty.Create(nameof(BackgroundColor), typeof(Color), typeof(SwipeItem), null,
			propertyChanged: OnBackgroundColorPropertyChanged);

		static void OnBackgroundColorPropertyChanged(BindableObject bindable, object oldValue, object newValue)
		{
			if (bindable is SwipeItem swipeItem)
				swipeItem.Handler?.UpdateValue(nameof(ISwipeItemMenuItem.Background));
		}

		/// <summary>Bindable property for <see cref="IsVisible"/>.</summary>
		public static readonly BindableProperty IsVisibleProperty = BindableProperty.Create(nameof(IsVisible), typeof(bool), typeof(SwipeItem), true);

		/// <include file="../../docs/Microsoft.Maui.Controls/SwipeItem.xml" path="//Member[@MemberName='BackgroundColor']/Docs/*" />
		public Color BackgroundColor
		{
			get { return (Color)GetValue(BackgroundColorProperty); }
			set { SetValue(BackgroundColorProperty, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/SwipeItem.xml" path="//Member[@MemberName='IsVisible']/Docs/*" />
		public bool IsVisible
		{
			get { return (bool)GetValue(IsVisibleProperty); }
			set { SetValue(IsVisibleProperty, value); }
		}

		public event EventHandler<EventArgs> Invoked;

		Paint ISwipeItemMenuItem.Background
		{
			get
			{
				if (BackgroundColor.IsNotDefault())
					return new SolidPaint(BackgroundColor);

				return null;
			}
		}

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

		private protected override void OnHandlerChangingCore(HandlerChangingEventArgs args)
		{
			base.OnHandlerChangingCore(args);

			if (Application.Current is null)
				return;

			if (args.NewHandler is null || args.OldHandler is not null)
				Application.Current.RequestedThemeChanged -= OnRequestedThemeChanged;
			if (args.NewHandler is not null && args.OldHandler is null)
				Application.Current.RequestedThemeChanged += OnRequestedThemeChanged;
		}

		private void OnRequestedThemeChanged(object sender, AppThemeChangedEventArgs e)
		{
			// Force refresh/re-evaluate AppTheme binding
			this.RefreshPropertyValue(BackgroundColorProperty, BackgroundColor);
		}
	}
}