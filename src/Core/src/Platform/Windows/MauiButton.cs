#nullable enable
using Microsoft.Maui.Graphics;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using WBrush = Microsoft.UI.Xaml.Media.Brush;
using WContentPresenter = Microsoft.UI.Xaml.Controls.ContentPresenter;
using WImage = Microsoft.UI.Xaml.Controls.Image;

namespace Microsoft.Maui
{
	// This is needed by WinUI because of 
	// https://github.com/microsoft/microsoft-ui-xaml/issues/2698#issuecomment-648751713
	[Microsoft.UI.Xaml.Data.Bindable]
	public class MauiButton : Button
	{
		public static readonly DependencyProperty BorderRadiusProperty =
			DependencyProperty.Register(nameof(BorderRadius), typeof(int), typeof(MauiButton),
				new PropertyMetadata(default(int), OnBorderRadiusChanged));

		public static readonly DependencyProperty BackgroundColorProperty =
			DependencyProperty.Register(nameof(BackgroundColor), typeof(WBrush), typeof(MauiButton),
				new PropertyMetadata(default(WBrush), OnBackgroundColorChanged));

		WContentPresenter? _contentPresenter;
		Grid? _rootGrid;

		public WBrush BackgroundColor
		{
			get
			{
				return (WBrush)GetValue(BackgroundColorProperty);
			}
			set
			{
				SetValue(BackgroundColorProperty, value);
			}
		}

		public int BorderRadius
		{
			get
			{
				return (int)GetValue(BorderRadiusProperty);
			}
			set
			{
				SetValue(BorderRadiusProperty, value);
			}
		}

		protected override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			_contentPresenter = GetTemplateChild("ContentPresenter") as WContentPresenter;
			_rootGrid = GetTemplateChild("RootGrid") as Grid;

			UpdateBackgroundColor();
			UpdateBorderRadius();
		}

		static void OnBackgroundColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			((MauiButton)d).UpdateBackgroundColor();
		}

		static void OnBorderRadiusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			((MauiButton)d).UpdateBorderRadius();
		}

		void UpdateBackgroundColor()
		{
			if (BackgroundColor == null)
				BackgroundColor = Background;

			if (_contentPresenter != null)
				_contentPresenter.Background = BackgroundColor;

			Background = new UI.Xaml.Media.SolidColorBrush(UI.Colors.Transparent);
		}

		void UpdateBorderRadius()
		{
			var radius = BorderRadius == -1 ? 0 : BorderRadius;
			var cornerRadius = WinUIHelpers.CreateCornerRadius(radius);

			if (_contentPresenter != null)
				_contentPresenter.CornerRadius = cornerRadius;

			if (_rootGrid != null)
				_rootGrid.CornerRadius = cornerRadius;
		}

		public void UpdateCharacterSpacing(double characterSpacing)
		{
			CharacterSpacing = characterSpacing.ToEm();
			
			if (_contentPresenter != null)
				_contentPresenter.CharacterSpacing = CharacterSpacing;

			var textBlock = GetTextBlock();

			if (textBlock != null)
				textBlock.CharacterSpacing = CharacterSpacing;
		}

		public void UpdateBorderColor(Color borderColor)
		{
			BorderBrush = (borderColor != null) ? ColorExtensions.ToNative(borderColor) : (WBrush)Application.Current.Resources["ButtonBorderThemeBrush"];
		}

		public void UpdateBorderWidth(double borderWidth)
		{
			BorderThickness = WinUIHelpers.CreateThickness(borderWidth);
		}

		public void UpdateCornerRadius(int cornerRadius)
		{
			BorderRadius = cornerRadius;
		}
				
		public TextBlock? GetTextBlock() => GetContent<TextBlock?>();

		public WImage? GetImage() => GetContent<WImage?>();

		internal T? GetContent<T>()
		{
			if (Content is T t)
			{
				return t;
			}

			if (Content is StackPanel sp)
			{
				foreach (var item in sp.Children)
				{
					if (item is T tChild)
					{
						return tChild;
					}
				}
			}

			return default;
		}
	}
}
