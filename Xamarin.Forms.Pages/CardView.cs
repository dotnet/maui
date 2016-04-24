using System;
using Xamarin.Forms;
using Xamarin.Forms.Pages;

namespace Xamarin.Forms.Pages
{
	public class CardView : DataView
	{
		public static readonly BindableProperty TextProperty =
			BindableProperty.Create(nameof(Text), typeof(string), typeof(CardView), null, BindingMode.OneWay);

		public string Text
		{
			get { return (string)GetValue(TextProperty); }
			set { SetValue(TextProperty, value); }
		}

		public static readonly BindableProperty DetailProperty =
			BindableProperty.Create(nameof(Detail), typeof(string), typeof(CardView), null, BindingMode.OneWay);

		public string Detail
		{
			get { return (string)GetValue(DetailProperty); }
			set { SetValue(DetailProperty, value); }
		}

		public static readonly BindableProperty ImageSourceProperty =
			BindableProperty.Create(nameof(ImageSource), typeof(ImageSource), typeof(CardView), null, BindingMode.OneWay);

		public ImageSource ImageSource
		{
			get { return (ImageSource)GetValue(ImageSourceProperty); }
			set { SetValue(ImageSourceProperty, value); }
		}
	}
}

