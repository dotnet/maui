using System.ComponentModel;

namespace Xamarin.Forms
{
	public class HtmlWebViewSource : WebViewSource
	{
		public static readonly BindableProperty HtmlProperty = BindableProperty.Create("Html", typeof(string), typeof(HtmlWebViewSource), default(string),
			propertyChanged: (bindable, oldvalue, newvalue) => ((HtmlWebViewSource)bindable).OnSourceChanged());

		public static readonly BindableProperty BaseUrlProperty = BindableProperty.Create("BaseUrl", typeof(string), typeof(HtmlWebViewSource), default(string),
			propertyChanged: (bindable, oldvalue, newvalue) => ((HtmlWebViewSource)bindable).OnSourceChanged());

		public string BaseUrl
		{
			get { return (string)GetValue(BaseUrlProperty); }
			set { SetValue(BaseUrlProperty, value); }
		}

		public string Html
		{
			get { return (string)GetValue(HtmlProperty); }
			set { SetValue(HtmlProperty, value); }
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public override void Load(IWebViewDelegate renderer)
		{
			renderer.LoadHtml(Html, BaseUrl);
		}
	}
}