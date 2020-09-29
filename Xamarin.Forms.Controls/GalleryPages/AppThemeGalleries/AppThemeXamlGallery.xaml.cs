using System;
using System.Globalization;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Xaml;

namespace Xamarin.Forms.Controls.GalleryPages.AppThemeGalleries
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class AppThemeXamlGallery : ContentPage
	{
		public AppThemeXamlGallery()
		{
			InitializeComponent();
		}
	}

	[Preserve(AllMembers = true)]
	class FooConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var val = value as string;
			return val == "1" ? Color.Green : Color.Red;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotSupportedException("Only one way bindings are supported with this converter");
		}
	}

	//public class CustomControl : ContentView
	//   {
	//       public static readonly BindableProperty TextColorProperty = BindableProperty.Create(nameof(TextColor), typeof(Color), typeof(CustomControl), (Color)new AppThemeColor() { Light = Color.Red, Dark = Color.Green });
	//       public static readonly BindableProperty TextProperty = BindableProperty.Create(nameof(Text), typeof(string), typeof(CustomControl));
	//       public static readonly BindableProperty BoxColorProperty = BindableProperty.Create(nameof(BoxColor), typeof(Color), typeof(CustomControl), Color.Yellow);

	//       private StackLayout layout = new StackLayout();
	//       public CustomControl()
	//       {
	//           var label = new Label();
	//           label.SetBinding(Label.TextProperty, new Binding(nameof(Text), source: this));
	//           label.SetBinding(Label.TextColorProperty, new Binding(nameof(TextColor), source: this, mode: BindingMode.TwoWay));
	//           label.SetDynamicResource(Label.TextColorProperty, "MyColor");
	//           this.layout.Children.Add(label);

	//           var box = new BoxView();
	//           box.SetBinding(BoxView.ColorProperty, new Binding(nameof(BoxColor), source: this, mode: BindingMode.TwoWay));
	//           box.WidthRequest = 24;
	//           box.HeightRequest = 24;
	//           this.layout.Children.Add(box);

	//           this.layout.Orientation = StackOrientation.Horizontal;
	//           this.Content = this.layout;
	//       }

	//       public Color TextColor
	//       {
	//           get { return (Color)this.GetValue(TextColorProperty); }
	//           set { this.SetValue(TextColorProperty, value); }
	//       }

	//       public string Text
	//       {
	//           get { return (string)this.GetValue(TextProperty); }
	//           set { this.SetValue(TextProperty, value); }
	//       }

	//       public Color BoxColor
	//       {
	//           get { return (Color)this.GetValue(BoxColorProperty); }
	//           set { this.SetValue(BoxColorProperty, value); }
	//       }
	//   } 
}