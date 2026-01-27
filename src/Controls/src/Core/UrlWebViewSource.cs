#nullable disable
using System.ComponentModel;

namespace Microsoft.Maui.Controls
{
	/// <summary>
	/// A <see cref="WebViewSource"/> that loads content from a URL.
	/// </summary>
	public class UrlWebViewSource : WebViewSource
	{
		/// <summary>Bindable property for <see cref="Url"/>.</summary>
		public static readonly BindableProperty UrlProperty = BindableProperty.Create(nameof(Url), typeof(string), typeof(UrlWebViewSource), default(string),
			propertyChanged: (bindable, oldvalue, newvalue) => ((UrlWebViewSource)bindable).OnSourceChanged());

		/// <summary>
		/// Gets or sets the URL to load in the <see cref="WebView"/>. This is a bindable property.
		/// </summary>
		public string Url
		{
			get { return (string)GetValue(UrlProperty); }
			set { SetValue(UrlProperty, value); }
		}

		/// <inheritdoc/>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public override void Load(IWebViewDelegate renderer)
		{
			renderer.LoadUrl(Url);
		}
	}
}