using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using WBrush = Windows.UI.Xaml.Media.Brush;

using WContentPresenter = Windows.UI.Xaml.Controls.ContentPresenter;

namespace Xamarin.Forms.Platform.UWP
{
	public class FormsButton : Windows.UI.Xaml.Controls.Button
	{
		public static readonly DependencyProperty BorderRadiusProperty = DependencyProperty.Register(nameof(BorderRadius), typeof(int), typeof(FormsButton),
			new PropertyMetadata(default(int), OnBorderRadiusChanged));

		public static readonly DependencyProperty BackgroundColorProperty = DependencyProperty.Register(nameof(BackgroundColor), typeof(WBrush), typeof(FormsButton),
			new PropertyMetadata(default(WBrush), OnBackgroundColorChanged));

		WContentPresenter _contentPresenter;
		Windows.UI.Xaml.Controls.Grid _rootGrid;

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
			_rootGrid = GetTemplateChild("RootGrid") as Windows.UI.Xaml.Controls.Grid;

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
			Background = Color.Transparent.ToBrush();
		}

		void UpdateBorderRadius()
		{
			var radius = BorderRadius == -1 ? 0 : BorderRadius;
			var cornerRadius = new Windows.UI.Xaml.CornerRadius(radius);
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

			if(Content is TextBlock tb)
			{
				tb.CharacterSpacing = CharacterSpacing;
			}

			if (Content is StackPanel sp)
			{
				foreach (var item in sp.Children)
				{
					if (item is TextBlock textBlock)
					{
						textBlock.CharacterSpacing = CharacterSpacing;
					}
				}
			}
		}
	}
}