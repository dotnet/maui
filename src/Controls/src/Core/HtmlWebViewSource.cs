#nullable disable
using System.ComponentModel;

namespace Microsoft.Maui.Controls
{
	/// <summary>A WebViewSource bound to an HTML-formatted string.</summary>
	public class HtmlWebViewSource : WebViewSource
	{
		/// <summary>Bindable property for <see cref="Html"/>.</summary>
		public static readonly BindableProperty HtmlProperty = BindableProperty.Create(nameof(Html), typeof(string), typeof(HtmlWebViewSource), default(string),
			propertyChanged: (bindable, oldvalue, newvalue) => ((HtmlWebViewSource)bindable).OnSourceChanged());

		/// <summary>Bindable property for <see cref="BaseUrl"/>.</summary>
		public static readonly BindableProperty BaseUrlProperty = BindableProperty.Create(nameof(BaseUrl), typeof(string), typeof(HtmlWebViewSource), default(string),
			propertyChanged: (bindable, oldvalue, newvalue) => ((HtmlWebViewSource)bindable).OnSourceChanged());

		/// <summary>The base URL for the source HTML document. This is a bindable property.</summary>
		public string BaseUrl
		{
			get { return (string)GetValue(BaseUrlProperty); }
			set { SetValue(BaseUrlProperty, value); }
		}

		/// <summary>The HTML content. This is a bindable property.</summary>
		public string Html
		{
			get { return (string)GetValue(HtmlProperty); }
			set { SetValue(HtmlProperty, value); }
		}

		/// <summary>Loads the specified <paramref name="renderer"/> with the current base URL and HTML.</summary>
		/// <param name="renderer">The renderer into which to load html content.</param>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public override void Load(IWebViewDelegate renderer)
		{
			renderer.LoadHtml(Html, BaseUrl);
		}
	}
}