#nullable disable
using System.ComponentModel;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/UrlWebViewSource.xml" path="Type[@FullName='Microsoft.Maui.Controls.UrlWebViewSource']/Docs/*" />
	public class UrlWebViewSource : WebViewSource
	{
		/// <summary>Bindable property for <see cref="Url"/>.</summary>
		public static readonly BindableProperty UrlProperty = BindableProperty.Create(nameof(Url), typeof(string), typeof(UrlWebViewSource), default(string),
			propertyChanged: (bindable, oldvalue, newvalue) => ((UrlWebViewSource)bindable).OnSourceChanged());

		/// <include file="../../docs/Microsoft.Maui.Controls/UrlWebViewSource.xml" path="//Member[@MemberName='Url']/Docs/*" />
		public string Url
		{
			get { return (string)GetValue(UrlProperty); }
			set { SetValue(UrlProperty, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/UrlWebViewSource.xml" path="//Member[@MemberName='Load']/Docs/*" />
		[EditorBrowsable(EditorBrowsableState.Never)]
		public override void Load(IWebViewDelegate renderer)
		{
			renderer.LoadUrl(Url);
		}
	}
}