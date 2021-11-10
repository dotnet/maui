using System;

namespace Maui.Controls.Sample.Pages
{
	public partial class WebViewPage
	{
		public WebViewPage()
		{
			InitializeComponent();
		}

		void OnEvalClicked(object sender, EventArgs args)
		{
			MauiWebView.Eval("alert('Eval')");
		}
	}
}