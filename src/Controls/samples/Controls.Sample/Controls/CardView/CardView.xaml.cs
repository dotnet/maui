using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample.Controls
{
	public partial class CardView : ContentView
	{
		public static readonly BindableProperty CardTitleProperty =
			BindableProperty.Create(nameof(CardTitle), typeof(string), typeof(CardView), string.Empty);

		public static readonly BindableProperty CardDescriptionProperty =
			BindableProperty.Create(nameof(CardDescription), typeof(string), typeof(CardView), string.Empty);

		public static readonly BindableProperty BorderColorProperty =
			BindableProperty.Create(nameof(BorderColor), typeof(Color), typeof(CardView), Colors.DarkGray);

		public static readonly BindableProperty CardColorProperty =
			BindableProperty.Create(nameof(CardColor), typeof(Color), typeof(CardView), Colors.White);

		public static readonly BindableProperty IconImageSourceProperty =
			BindableProperty.Create(nameof(IconImageSource), typeof(ImageSource), typeof(CardView), default(ImageSource));

		public static readonly BindableProperty IconBackgroundColorProperty =
			BindableProperty.Create(nameof(IconBackgroundColor), typeof(Color), typeof(CardView), Colors.LightGray);

		public string CardTitle
		{
			get => (string)GetValue(CardTitleProperty);
			set => SetValue(CardTitleProperty, value);
		}

		public string CardDescription
		{
			get => (string)GetValue(CardDescriptionProperty);
			set => SetValue(CardDescriptionProperty, value);
		}

		public Color BorderColor
		{
			get => (Color)GetValue(BorderColorProperty);
			set => SetValue(BorderColorProperty, value);
		}

		public Color CardColor
		{
			get => (Color)GetValue(CardColorProperty);
			set => SetValue(CardColorProperty, value);
		}

		public ImageSource IconImageSource
		{
			get => (ImageSource)GetValue(IconImageSourceProperty);
			set => SetValue(IconImageSourceProperty, value);
		}

		public Color IconBackgroundColor
		{
			get => (Color)GetValue(IconBackgroundColorProperty);
			set => SetValue(IconBackgroundColorProperty, value);
		}

		public CardView()
		{
			InitializeComponent();

			Card.BindingContext = this;
		}
	}
}