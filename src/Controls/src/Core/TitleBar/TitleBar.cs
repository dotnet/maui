using Microsoft.Maui.Graphics;
using System.Collections.Generic;

namespace Microsoft.Maui.Controls
{
	public partial class TitleBar : ContentView, ITitleBar
	{
		public static readonly BindableProperty IconProperty = BindableProperty.Create(nameof(Icon), typeof(ImageSource),
			typeof(TitleBar), null, propertyChanged: OnIconChanged);

		public static readonly BindableProperty LeadingContentProperty = BindableProperty.Create(nameof(LeadingContent), typeof(IView),
			typeof(TitleBar), null, propertyChanged: OnLeadingContentChanged);

		public static new readonly BindableProperty ContentProperty = BindableProperty.Create(nameof(Content), typeof(IView),
			typeof(TitleBar), null, propertyChanged: OnContentChanged);

		public static readonly BindableProperty TrailingContentProperty = BindableProperty.Create(nameof(TrailingContent), typeof(IView),
			typeof(TitleBar), null, propertyChanged: OnTrailingContentChanged);

		public static readonly BindableProperty TitleProperty = BindableProperty.Create(nameof(Title), typeof(string),
			typeof(TitleBar), null, propertyChanged: OnTitleChanged);

		public static readonly BindableProperty SubtitleProperty = BindableProperty.Create(nameof(Subtitle), typeof(string),
			typeof(TitleBar), null, propertyChanged: OnSubtitleChanged);

		public static readonly BindableProperty ForegroundColorProperty = BindableProperty.Create(nameof(ForegroundColor),
			typeof(Color), typeof(TitleBar), propertyChanged: OnForegroundChanged);

		public static readonly BindableProperty InactiveBackgroundColorProperty = BindableProperty.Create(nameof(InactiveBackgroundColor),
			typeof(Color), typeof(TitleBar), null);

		public static readonly BindableProperty InactiveForegroundColorProperty = BindableProperty.Create(nameof(InactiveForegroundColor),
			typeof(Color), typeof(TitleBar), null);

		public ImageSource Icon
		{
			get { return (ImageSource)GetValue(IconProperty); }
			set { SetValue(IconProperty, value); }
		}
		
		public IView? LeadingContent
		{
			get { return (View?)GetValue(LeadingContentProperty); }
			set { SetValue(LeadingContentProperty, value); }
		}

		public string Title
		{
			get { return (string)GetValue(TitleProperty); }
			set { SetValue(TitleProperty, value); }
		}

		public string Subtitle
		{
			get { return (string)GetValue(SubtitleProperty); }
			set { SetValue(SubtitleProperty, value); }
		}

		public new IView? Content
		{
			get { return (View?)GetValue(TitleBar.ContentProperty); }
			set { SetValue(TitleBar.ContentProperty, value); }
		}

		public IView? TrailingContent
		{
			get { return (View?)GetValue(TrailingContentProperty); }
			set { SetValue(TrailingContentProperty, value); }
		}

		public Color ForegroundColor
		{
			get { return (Color)GetValue(ForegroundColorProperty); }
			set { SetValue(ForegroundColorProperty, value); }
		}

		public Color InactiveBackgroundColor
		{
			get { return (Color)GetValue(InactiveBackgroundColorProperty); }
			set { SetValue(InactiveBackgroundColorProperty, value); }
		}

		public Color InactiveForegroundColor
		{
			get { return (Color)GetValue(InactiveForegroundColorProperty); }
			set { SetValue(InactiveForegroundColorProperty, value); }
		}

		public IList<IView> PassthroughElements { get; private set; }

		static void OnLeadingContentChanged(BindableObject bindable, object oldValue, object newValue)
		{
			var titlebar = (TitleBar)bindable;

			if (oldValue != null)
			{
				titlebar.ContentGrid.Remove(oldValue);

				if (oldValue is IView oldView)
				{
					titlebar.PassthroughElements.Remove(oldView);
				}
			}

			if (newValue is IView view)
			{
				titlebar.PassthroughElements.Add(view);
				titlebar.ContentGrid.Add(newValue);
				titlebar.ContentGrid.SetColumn(view, 0);
			}
		}

		static void OnIconChanged(BindableObject bindable, object oldValue, object newValue)
		{
			var titlebar = (TitleBar)bindable;
			if (newValue is ImageSource imageSource)
			{
				titlebar._iconImage.Source = imageSource;
				titlebar._iconImage.IsVisible = !imageSource.IsEmpty;
			}
		}

		static void OnTitleChanged(BindableObject bindable, object oldValue, object newValue)
		{
			var titlebar = (TitleBar)bindable;
			if (newValue is string text)
			{
				titlebar._titleLabel.Text = text;
				titlebar._titleLabel.IsVisible = !string.IsNullOrEmpty(text);
			}
		}

		static void OnSubtitleChanged(BindableObject bindable, object oldValue, object newValue)
		{
			var titlebar = (TitleBar)bindable;
			if (newValue is string text)
			{
				titlebar._subtitleLabel.Text = text;
				titlebar._subtitleLabel.IsVisible = !string.IsNullOrEmpty(text);
			}
		}

		static void OnContentChanged(BindableObject bindable, object oldValue, object newValue)
		{
			var titlebar = (TitleBar)bindable;

			if (oldValue != null)
			{
				titlebar.ContentGrid.Remove(oldValue);
				if (oldValue is IView oldView)
				{
					titlebar.PassthroughElements.Remove(oldView);
				}
			}

			if (newValue is IView view)
			{
				titlebar.PassthroughElements.Add(view);
				titlebar.ContentGrid.Add(newValue);
				titlebar.ContentGrid.SetColumn(view, 4);
			}
		}

		static void OnTrailingContentChanged(BindableObject bindable, object oldValue, object newValue)
		{
			var titlebar = (TitleBar)bindable;

			if (oldValue != null)
			{
				titlebar.ContentGrid.Remove(oldValue);
				if (oldValue is IView oldView)
				{
					titlebar.PassthroughElements.Remove(oldView);
				}
			}

			if (newValue is View view)
			{
				titlebar.PassthroughElements.Add(view);
				titlebar.ContentGrid.Add(newValue);
				titlebar.ContentGrid.SetColumn(view, 5);
			}
		}

		static void OnForegroundChanged(BindableObject bindable, object oldValue, object newValue)
		{
			var titlebar = (TitleBar)bindable;
			if (newValue is Color color)
			{
				titlebar._titleLabel.TextColor = color;
				titlebar._subtitleLabel.TextColor = color;
			}
		}

		Grid ContentGrid => (Grid)base.Content;
		readonly Image _iconImage;
		readonly Label _titleLabel;
		readonly Label _subtitleLabel;
		Color? _backgroundColor;

		static Color TextFillColorPrimaryLight = new(0, 0, 0, 228);
		static Color TextFillInactiveColorPrimaryLight = new(0, 0, 0, 135);

		static Color TextFillColorPrimaryDark = new(255, 255, 255, 255);
		static Color TextFillInactiveColorPrimaryDark = new(255, 255, 255, 114);

		public TitleBar()
		{
			PassthroughElements = new List<IView>();

			base.Content = new Grid
			{
				HorizontalOptions = LayoutOptions.Fill,
				ColumnDefinitions =
				{
					new ColumnDefinition(GridLength.Auto), // Leading content
					new ColumnDefinition(GridLength.Auto), // Icon content
					new ColumnDefinition(GridLength.Auto), // Title content
					new ColumnDefinition(GridLength.Auto), // Subtitle content
					new ColumnDefinition(GridLength.Star), // Content
					new ColumnDefinition(GridLength.Auto), // Trailing content
					new ColumnDefinition(150),			   // Min drag region + padding for system buttons
				}
			};

			_iconImage = new Image()
			{
				WidthRequest = 20,
				HeightRequest = 20,
				VerticalOptions = LayoutOptions.Center,
				IsVisible = false,
				Margin = new Thickness(16, 0, 0, 0)
			};
			ContentGrid.Add(_iconImage);
			ContentGrid.SetColumn(_iconImage, 1);

			_titleLabel = new Label()
			{
				Margin = new Thickness(16, 0),
				LineBreakMode = LineBreakMode.NoWrap,
				HorizontalOptions = LayoutOptions.Start,
				VerticalOptions = LayoutOptions.Center,
				MinimumWidthRequest = 48,
				FontSize = 12,
				IsVisible = false
			};
			ContentGrid.Add(_titleLabel);
			ContentGrid.SetColumn(_titleLabel, 2);

			_subtitleLabel = new Label()
			{
				Margin = new Thickness(0, 0, 16, 0),
				LineBreakMode = LineBreakMode.NoWrap,
				HorizontalOptions = LayoutOptions.Start,
				VerticalOptions = LayoutOptions.Center,
				MinimumWidthRequest = 48,
				FontSize = 12,
				Opacity = 0.8,
				IsVisible = false
			};
			ContentGrid.Add(_subtitleLabel);
			ContentGrid.SetColumn(_subtitleLabel, 3);
		}

		internal void NotifyWindowReady(Window window)
		{
			window.Activated += Window_Activated;
			window.Deactivated += Window_Deactivated;
		}

		internal void UnhookWindowEvents(Window window)
		{
			window.Activated -= Window_Activated;
			window.Deactivated -= Window_Deactivated;
		}

		private void Window_Activated(object? sender, System.EventArgs e)
		{
			if (ForegroundColor != null)
			{
				_titleLabel.TextColor = ForegroundColor;
				_subtitleLabel.TextColor = ForegroundColor;
			}
			else
			{
				_titleLabel.SetAppThemeColor(Label.TextColorProperty, TextFillColorPrimaryLight, TextFillColorPrimaryDark);
				_subtitleLabel.SetAppThemeColor(Label.TextColorProperty, TextFillColorPrimaryLight, TextFillColorPrimaryDark);
			}

			if (_backgroundColor != null)
			{
				BackgroundColor = _backgroundColor;
			}
		}

		private void Window_Deactivated(object? sender, System.EventArgs e)
		{
			if (InactiveForegroundColor != null)
			{
				_titleLabel.TextColor = InactiveForegroundColor;
				_subtitleLabel.TextColor = InactiveForegroundColor;
			}
			else
			{
				_titleLabel.SetAppThemeColor(Label.TextColorProperty, TextFillInactiveColorPrimaryLight, TextFillInactiveColorPrimaryDark);
				_subtitleLabel.SetAppThemeColor(Label.TextColorProperty, TextFillInactiveColorPrimaryLight, TextFillInactiveColorPrimaryDark);
			}

			if (BackgroundColor != null)
			{
				_backgroundColor = BackgroundColor;
			}

			if (InactiveBackgroundColor != null)
			{
				BackgroundColor = InactiveBackgroundColor;
			}
		}
	}
}
