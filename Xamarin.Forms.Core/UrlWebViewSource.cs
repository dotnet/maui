namespace Xamarin.Forms
{
	public class UrlWebViewSource : WebViewSource
	{
		public static readonly BindableProperty UrlProperty = BindableProperty.Create("Url", typeof(string), typeof(UrlWebViewSource), default(string),
			propertyChanged: (bindable, oldvalue, newvalue) => ((UrlWebViewSource)bindable).OnSourceChanged());

		public string Url
		{
			get { return (string)GetValue(UrlProperty); }
			set { SetValue(UrlProperty, value); }
		}

		internal override void Load(IWebViewDelegate renderer)
		{
			renderer.LoadUrl(Url);
		}
	}
}