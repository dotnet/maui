using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation.Peers;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using WThickness = Microsoft.UI.Xaml.Thickness;
using Microsoft.UI.Xaml.Media.Imaging;

namespace Microsoft.Maui.Platform
{
	public class MauiButton : Button
	{
		public MauiButton()
		{
			Content = new DefaultMauiButtonContent();

			VerticalAlignment = VerticalAlignment.Stretch;
			HorizontalAlignment = HorizontalAlignment.Stretch;

			SizeChanged += MauiButton_SizeChanged;
		}

		private void MauiButton_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			if (Content is DefaultMauiButtonContent defaultButtonContent)
			{
				if (double.IsNaN(Width) && double.IsNaN(Height))
				{
					// No size is set, allow image to be native size
					defaultButtonContent.SetImageStretch(UI.Xaml.Media.Stretch.None);
				}
				else
				{
					// Tell image to resize to fit
					defaultButtonContent.SetImageStretch(UI.Xaml.Media.Stretch.Uniform);
				}

				if (double.IsNaN(Width))
				{
					// If the width is NaN, we don't want to limit the width of the image
					defaultButtonContent.LimitImageWidth(double.PositiveInfinity);
				}
				else
				{
					var maxWidth = Math.Max(0, ActualWidth - (Padding.Left + Padding.Right) - defaultButtonContent.ColumnSpacing);
					defaultButtonContent.LimitImageWidth(maxWidth);
				}

				if (double.IsNaN(Height))
				{
					// If the height is NaN, we don't want to limit the height of the image
					defaultButtonContent.LimitImageHeight(double.PositiveInfinity);
				}
				else
				{
					var maxHeight = Math.Max(0, ActualHeight - (Padding.Top + Padding.Bottom) - defaultButtonContent.RowSpacing);
					defaultButtonContent.LimitImageHeight(maxHeight);
				}
			}
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

		int _imgRow = -1;
		int _imgColumn = -1;

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
				VerticalAlignment = VerticalAlignment.Stretch,
				HorizontalAlignment = HorizontalAlignment.Stretch,
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

			LayoutImageLeft(0);
		}

		internal void SetImageStretch(UI.Xaml.Media.Stretch stretch)
		{
			_image.Stretch = stretch;
		}

		/// <summary>
		/// Limit the width of the image to the specified value
		/// Setting the value to double.PositiveInfinity will remove the width limit
		/// </summary>
		/// <param name="width"></param>
		internal void LimitImageWidth(double width)
		{
			if (_imgColumn == -1)
			{
				return;
			}

			if (double.IsInfinity(width))
			{
				ColumnDefinitions[_imgColumn].Width = UI.Xaml.GridLength.Auto;
			}
			else
			{
				var maxWidth = width;
				if (_image.Source is BitmapImage bitmap)
				{
					// Image isn't loaded yet, wait for it to load to get the width
					if (bitmap.PixelWidth == 0)
					{
						// Make sure that the column width is not larger than the image width or clamped width
						// For some unknown reason, setting the ColumnDefinition MaxWidth doesn't work
						bitmap.ImageOpened += ImageOpened;
						void ImageOpened(object sender, RoutedEventArgs _)
						{
							bitmap.ImageOpened -= ImageOpened;
							maxWidth = Math.Min(bitmap.PixelWidth, maxWidth);
							ColumnDefinitions[_imgColumn].Width = new UI.Xaml.GridLength(maxWidth, UI.Xaml.GridUnitType.Pixel);
						};
					}
					else
					{
						maxWidth = Math.Min(bitmap.PixelWidth, maxWidth);
					}
				}
				ColumnDefinitions[_imgColumn].Width = new UI.Xaml.GridLength(maxWidth, UI.Xaml.GridUnitType.Pixel);
			}
		}

		/// <summary>
		/// Limit the height of the image to the specified value
		/// Setting the value to double.PositiveInfinity will remove the height limit
		/// </summary>
		/// <param name="height"></param>
		internal void LimitImageHeight(double height)
		{
			if (_imgRow == -1)
			{
				return;
			}

			if (double.IsInfinity(height))
			{
				RowDefinitions[_imgRow].Height = UI.Xaml.GridLength.Auto;
			}
			else
			{
				var maxHeight = height;
				if (_image.Source is BitmapImage bitmap)
				{
					// Image isn't loaded yet, wait for it to load to get the height
					if (bitmap.PixelHeight == 0)
					{
						// Make sure that the row height is not larger than the image height or clamped height
						// For some unknown reason, setting the RowDefinition MaxHeight doesn't work
						bitmap.ImageOpened += ImageOpened;
						void ImageOpened(object sender, RoutedEventArgs _) 
						{
							bitmap.ImageOpened -= ImageOpened;
							maxHeight = Math.Min(bitmap.PixelHeight, maxHeight);
							RowDefinitions[_imgRow].Height = new UI.Xaml.GridLength(maxHeight, UI.Xaml.GridUnitType.Pixel);
						};
					}
					else
					{
						maxHeight = Math.Min(bitmap.PixelHeight, maxHeight);
					}
				}
				RowDefinitions[_imgRow].Height = new UI.Xaml.GridLength(maxHeight, UI.Xaml.GridUnitType.Pixel);
			}
		}

		public void LayoutImageLeft(double spacing)
		{
			_imgColumn = 0;

			SetupHorizontalLayout(spacing);

			Grid.SetColumn(_image, 0);
			Grid.SetColumn(_textBlock, 1);

			ColumnDefinitions[0].Width = UI.Xaml.GridLength.Auto;
			ColumnDefinitions[1].Width = new UI.Xaml.GridLength(1, UI.Xaml.GridUnitType.Star);
		}

		public void LayoutImageRight(double spacing)
		{
			_imgColumn = 1;

			SetupHorizontalLayout(spacing);

			Grid.SetColumn(_image, 1);
			Grid.SetColumn(_textBlock, 0);

			ColumnDefinitions[0].Width = new UI.Xaml.GridLength(1, UI.Xaml.GridUnitType.Star);
			ColumnDefinitions[1].Width = UI.Xaml.GridLength.Auto;
		}

		public void LayoutImageTop(double spacing)
		{
			_imgRow = 0;

			SetupVerticalLayout(spacing);

			Grid.SetRow(_image, 0);
			Grid.SetRow(_textBlock, 1);

			RowDefinitions[0].Height = UI.Xaml.GridLength.Auto;
			RowDefinitions[1].Height = new UI.Xaml.GridLength(1, UI.Xaml.GridUnitType.Star);
		}

		public void LayoutImageBottom(double spacing)
		{
			_imgRow = 1;

			SetupVerticalLayout(spacing);

			Grid.SetRow(_image, 1);
			Grid.SetRow(_textBlock, 0);

			RowDefinitions[0].Height = new UI.Xaml.GridLength(1, UI.Xaml.GridUnitType.Star);
			RowDefinitions[1].Height = UI.Xaml.GridLength.Auto;
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