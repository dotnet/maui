using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation.Peers;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Windows.Security.Credentials.UI;
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

			LayoutImageLeft(0);
		}

		public void LayoutImageLeft(double spacing)
		{
			SetupHorizontalLayout(spacing);

			Grid.SetColumn(_image, 0);
			Grid.SetColumn(_textBlock, 1);

			ColumnDefinitions[0].Width = new UI.Xaml.GridLength(1, UI.Xaml.GridUnitType.Auto);
			ColumnDefinitions[1].Width = new UI.Xaml.GridLength(1, UI.Xaml.GridUnitType.Star);
		}

		public void LayoutImageRight(double spacing)
		{
			SetupHorizontalLayout(spacing);

			Grid.SetColumn(_image, 1);
			Grid.SetColumn(_textBlock, 0);

			ColumnDefinitions[0].Width = new UI.Xaml.GridLength(1, UI.Xaml.GridUnitType.Star);
			ColumnDefinitions[1].Width = new UI.Xaml.GridLength(1, UI.Xaml.GridUnitType.Auto);
		}

		public void LayoutImageTop(double spacing)
		{
			SetupVerticalLayout(spacing);

			Grid.SetRow(_image, 0);
			Grid.SetRow(_textBlock, 1);

			RowDefinitions[0].Height = new UI.Xaml.GridLength(0, UI.Xaml.GridUnitType.Auto);
			RowDefinitions[1].Height = new UI.Xaml.GridLength(1, UI.Xaml.GridUnitType.Star);
		}

		public void LayoutImageBottom(double spacing)
		{
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