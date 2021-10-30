using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Graphics;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using WBrush = Microsoft.UI.Xaml.Media.Brush;

using WContentPresenter = Microsoft.UI.Xaml.Controls.ContentPresenter;

namespace Microsoft.Maui.Controls.Compatibility.Platform.UWP
{
	[PortHandler]
	public class FormsButton : Microsoft.UI.Xaml.Controls.Button
	{
		public static readonly DependencyProperty BorderRadiusProperty = DependencyProperty.Register(nameof(BorderRadius), typeof(int), typeof(FormsButton),
			new PropertyMetadata(default(int), OnBorderRadiusChanged));

		public static readonly DependencyProperty BackgroundColorProperty = DependencyProperty.Register(nameof(BackgroundColor), typeof(WBrush), typeof(FormsButton),
			new PropertyMetadata(default(WBrush), OnBackgroundColorChanged));

		WContentPresenter _contentPresenter;
		Microsoft.UI.Xaml.Controls.Grid _rootGrid;

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
			_rootGrid = GetTemplateChild("RootGrid") as Microsoft.UI.Xaml.Controls.Grid;

			UpdateBackgroundColor();
			UpdateBorderRadius();
		}

		static void OnBackgroundColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			((FormsButton)d).UpdateBackgroundColor();
		}

		static void OnBorderRadiusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			((FormsButton)d).UpdateBorderRadius();
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
			
			if(_rootGrid != null)
				_rootGrid.CornerRadius = cornerRadius;
		}

		public void UpdateCharacterSpacing(int characterSpacing)
		{
			CharacterSpacing = characterSpacing;

			if (_contentPresenter != null)
				_contentPresenter.CharacterSpacing = CharacterSpacing;

			var textBlock = GetTextBlock(Content);
			
			if (textBlock != null)
				textBlock.CharacterSpacing = CharacterSpacing;

		}

		public TextBlock GetTextBlock(object content)
		{
			if (content is TextBlock tb)
			{
				return tb;
			}

			if (content is StackPanel sp)
			{
				foreach (var item in sp.Children)
				{
					if (item is TextBlock textBlock)
					{
						return textBlock;
					}
				}
			}

			return null;
		}
	}
}
