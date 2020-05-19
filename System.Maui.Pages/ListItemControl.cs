namespace Xamarin.Forms.Pages
{
	public class ListItemControl : DataView
	{
		public static readonly BindableProperty TitleProperty = BindableProperty.Create(nameof(Title), typeof(string), typeof(ListItemControl), default(string));

		public static readonly BindableProperty DetailProperty = BindableProperty.Create(nameof(Detail), typeof(string), typeof(ListItemControl), default(string));

		public static readonly BindableProperty ImageSourceProperty = BindableProperty.Create(nameof(ImageSource), typeof(ImageSource), typeof(ListItemControl), default(ImageSource));

		public static readonly BindableProperty PlaceHolderImageSourceProperty = BindableProperty.Create(nameof(PlaceholderImageSource), typeof(ImageSource), typeof(ListItemControl), default(ImageSource));

		public static readonly BindableProperty AspectProperty = BindableProperty.Create(nameof(Aspect), typeof(Aspect), typeof(ListItemControl), default(Aspect));

		public Aspect Aspect
		{
			get { return (Aspect)GetValue(AspectProperty); }
			set { SetValue(AspectProperty, value); }
		}

		public string Detail
		{
			get { return (string)GetValue(DetailProperty); }
			set { SetValue(DetailProperty, value); }
		}

		public ImageSource ImageSource
		{
			get { return (ImageSource)GetValue(ImageSourceProperty); }
			set { SetValue(ImageSourceProperty, value); }
		}

		public ImageSource PlaceholderImageSource
		{
			get { return (ImageSource)GetValue(PlaceHolderImageSourceProperty); }
			set { SetValue(PlaceHolderImageSourceProperty, value); }
		}

		public string Title
		{
			get { return (string)GetValue(TitleProperty); }
			set { SetValue(TitleProperty, value); }
		}
	}
}