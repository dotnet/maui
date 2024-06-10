#nullable disable
using System.ComponentModel;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/HtmlWebViewSource.xml" path="Type[@FullName='Microsoft.Maui.Controls.HtmlWebViewSource']/Docs/*" />
	public class HtmlWebViewSource : WebViewSource
	{
		/// <summary>Bindable property for <see cref="Html"/>.</summary>
		public static readonly BindableProperty HtmlProperty = BindableProperty.Create(nameof(Html), typeof(string), typeof(HtmlWebViewSource), default(string),
			propertyChanged: (bindable, oldvalue, newvalue) => ((HtmlWebViewSource)bindable).OnSourceChanged());

		/// <summary>Bindable property for <see cref="BaseUrl"/>.</summary>
		public static readonly BindableProperty BaseUrlProperty = BindableProperty.Create(nameof(BaseUrl), typeof(string), typeof(HtmlWebViewSource), default(string),
			propertyChanged: (bindable, oldvalue, newvalue) => ((HtmlWebViewSource)bindable).OnSourceChanged());

		/// <include file="../../docs/Microsoft.Maui.Controls/HtmlWebViewSource.xml" path="//Member[@MemberName='BaseUrl']/Docs/*" />
		public string BaseUrl
		{
			get { return (string)GetValue(BaseUrlProperty); }
			set { SetValue(BaseUrlProperty, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/HtmlWebViewSource.xml" path="//Member[@MemberName='Html']/Docs/*" />
		public string Html
		{
			get { return (string)GetValue(HtmlProperty); }
			set { SetValue(HtmlProperty, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/HtmlWebViewSource.xml" path="//Member[@MemberName='Load']/Docs/*" />
		[EditorBrowsable(EditorBrowsableState.Never)]
		public override void Load(IWebViewDelegate renderer)
		{
			renderer.LoadHtml(Html, BaseUrl);
		}
	}
}