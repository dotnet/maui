using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation.Peers;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using WThickness = Microsoft.UI.Xaml.Thickness;

namespace Microsoft.Maui.Platform
{
	public class MauiButton : Button
	{
		public MauiButton()
		{
			Content = new DefaultMauiButtonContent();

			VerticalAlignment = VerticalAlignment.Stretch;
			HorizontalAlignment = HorizontalAlignment.Stretch;
		}

		protected override AutomationPeer OnCreateAutomationPeer()
		{
			return new MauiButtonAutomationPeer(this);
		}
	}

	internal class DefaultMauiButtonContent : Grid
	{
		readonly Image _image;
		readonly TextBlock _textBlock;
		int _imgColumn = -1;
		int _imgRow = -1;

		public DefaultMauiButtonContent()
		{
			RowDefinitions.Add(new RowDefinition { Height = UI.Xaml.GridLength.Auto });
			RowDefinitions.Add(new RowDefinition { Height = UI.Xaml.GridLength.Auto });

			ColumnDefinitions.Add(new ColumnDefinition { Width = UI.Xaml.GridLength.Auto });
			ColumnDefinitions.Add(new ColumnDefinition { Width = UI.Xaml.GridLength.Auto });

			HorizontalAlignment = HorizontalAlignment.Center;
			VerticalAlignment = VerticalAlignment.Center;
			Margin = new WThickness(0);

			_image = new Image
			{
				VerticalAlignment = VerticalAlignment.Center,
				HorizontalAlignment = HorizontalAlignment.Center,
				Stretch = Stretch.None,
				Margin = new WThickness(0),
				Visibility = UI.Xaml.Visibility.Collapsed,
			};

			_textBlock = new TextBlock
			{
				VerticalAlignment = VerticalAlignment.Center,
				HorizontalAlignment = HorizontalAlignment.Center,
				Margin = new WThickness(0),
				Visibility = UI.Xaml.Visibility.Collapsed,
			};

			Children.Add(_image);
			Children.Add(_textBlock);

			SizeChanged += DefaultMauiButtonContent_SizeChanged;

			LayoutImageLeft(0);
		}

		private void DefaultMauiButtonContent_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			// Ensure that images in buttons do not take up more than half the width/height of the
			// button when we also have text to display. This ensures that we have enough
			// space to show a reasonable amount of text.
			if (_imgColumn != -1 &&
				_textBlock.Visibility == UI.Xaml.Visibility.Visible &&
				ColumnDefinitions[_imgColumn].Width.IsAuto)
			{
				if (_image.ActualWidth > e.NewSize.Width / 2.0)
				{
					ColumnDefinitions[_imgColumn].Width = new UI.Xaml.GridLength(1, UI.Xaml.GridUnitType.Star);
				}
			}

			if (_imgRow != -1 &&
				_textBlock.Visibility == UI.Xaml.Visibility.Visible &&
				RowDefinitions[_imgRow].Height.IsAuto)
			{
				if (_image.ActualHeight > e.NewSize.Height / 2.0)
				{
					RowDefinitions[_imgRow].Height = new UI.Xaml.GridLength(1, UI.Xaml.GridUnitType.Star);
				}
			}
		}

		public void LayoutImageLeft(double spacing)
		{
			_imgColumn = 0;

			SetupHorizontalLayout(spacing);

			Grid.SetColumn(_image, 0);
			Grid.SetColumn(_textBlock, 1);

			ColumnDefinitions[0].Width = new UI.Xaml.GridLength(0, UI.Xaml.GridUnitType.Auto);
			ColumnDefinitions[1].Width = new UI.Xaml.GridLength(1, UI.Xaml.GridUnitType.Star);
		}

		public void LayoutImageRight(double spacing)
		{
			_imgColumn = 1;

			SetupHorizontalLayout(spacing);

			Grid.SetColumn(_image, 1);
			Grid.SetColumn(_textBlock, 0);

			ColumnDefinitions[0].Width = new UI.Xaml.GridLength(1, UI.Xaml.GridUnitType.Star);
			ColumnDefinitions[1].Width = new UI.Xaml.GridLength(0, UI.Xaml.GridUnitType.Auto);
		}

		public void LayoutImageTop(double spacing)
		{
			_imgRow = 0;

			SetupVerticalLayout(spacing);

			Grid.SetRow(_image, 0);
			Grid.SetRow(_textBlock, 1);

			RowDefinitions[0].Height = new UI.Xaml.GridLength(0, UI.Xaml.GridUnitType.Auto);
			RowDefinitions[1].Height = new UI.Xaml.GridLength(1, UI.Xaml.GridUnitType.Star);
		}

		public void LayoutImageBottom(double spacing)
		{
			_imgRow = 1;

			SetupVerticalLayout(spacing);

			Grid.SetRow(_image, 1);
			Grid.SetRow(_textBlock, 0);

			RowDefinitions[0].Height = new UI.Xaml.GridLength(1, UI.Xaml.GridUnitType.Star);
			RowDefinitions[1].Height = new UI.Xaml.GridLength(0, UI.Xaml.GridUnitType.Auto);
		}

		double AdjustSpacing(double spacing)
		{
			if (_image.Visibility == UI.Xaml.Visibility.Collapsed
				|| _textBlock.Visibility == UI.Xaml.Visibility.Collapsed)
			{
				return 0;
			}

			return spacing;
		}

		void SetupHorizontalLayout(double spacing)
		{
			_imgRow = -1;

			RowSpacing = 0;
			ColumnSpacing = AdjustSpacing(spacing);

			RowDefinitions[0].Height = new UI.Xaml.GridLength(1, UI.Xaml.GridUnitType.Star);
			RowDefinitions[1].Height = new UI.Xaml.GridLength(1, UI.Xaml.GridUnitType.Star);

			Grid.SetRow(_image, 0);
			Grid.SetRowSpan(_image, 2);
			Grid.SetColumnSpan(_image, 1);

			Grid.SetRow(_textBlock, 0);
			Grid.SetRowSpan(_textBlock, 2);
			Grid.SetColumnSpan(_textBlock, 1);

		}

		void SetupVerticalLayout(double spacing)
		{
			_imgColumn = -1;

			ColumnSpacing = 0;
			RowSpacing = AdjustSpacing(spacing);

			RowDefinitions[0].Height = UI.Xaml.GridLength.Auto;
			RowDefinitions[1].Height = UI.Xaml.GridLength.Auto;

			ColumnDefinitions[0].Width = new UI.Xaml.GridLength(1, UI.Xaml.GridUnitType.Star);
			ColumnDefinitions[1].Width = new UI.Xaml.GridLength(1, UI.Xaml.GridUnitType.Star);

			Grid.SetRowSpan(_image, 1);
			Grid.SetColumn(_image, 0);
			Grid.SetColumnSpan(_image, 2);

			Grid.SetRowSpan(_textBlock, 1);
			Grid.SetColumn(_textBlock, 0);
			Grid.SetColumnSpan(_textBlock, 2);
		}
	}
}