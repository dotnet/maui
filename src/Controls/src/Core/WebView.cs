using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Devices;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/WebView.xml" path="Type[@FullName='Microsoft.Maui.Controls.WebView']/Docs/*" />
	public partial class WebView : View, IWebViewController, IElementConfiguration<WebView>
	{
		/// <include file="../../docs/Microsoft.Maui.Controls/WebView.xml" path="//Member[@MemberName='SourceProperty']/Docs/*" />
		public static readonly BindableProperty SourceProperty = BindableProperty.Create("Source", typeof(WebViewSource), typeof(WebView), default(WebViewSource),
			propertyChanging: (bindable, oldvalue, newvalue) =>
			{
				var source = oldvalue as WebViewSource;
				if (source != null)
					source.SourceChanged -= ((WebView)bindable).OnSourceChanged;
			}, propertyChanged: (bindable, oldvalue, newvalue) =>
			{
				var source = newvalue as WebViewSource;
				var webview = (WebView)bindable;
				if (source != null)
				{
					source.SourceChanged += webview.OnSourceChanged;
					SetInheritedBindingContext(source, webview.BindingContext);
				}
			});

		static readonly BindablePropertyKey CanGoBackPropertyKey = BindableProperty.CreateReadOnly("CanGoBack", typeof(bool), typeof(WebView), false);

		/// <include file="../../docs/Microsoft.Maui.Controls/WebView.xml" path="//Member[@MemberName='CanGoBackProperty']/Docs/*" />
		public static readonly BindableProperty CanGoBackProperty = CanGoBackPropertyKey.BindableProperty;

		static readonly BindablePropertyKey CanGoForwardPropertyKey = BindableProperty.CreateReadOnly("CanGoForward", typeof(bool), typeof(WebView), false);

		/// <include file="../../docs/Microsoft.Maui.Controls/WebView.xml" path="//Member[@MemberName='CanGoForwardProperty']/Docs/*" />
		public static readonly BindableProperty CanGoForwardProperty = CanGoForwardPropertyKey.BindableProperty;

		/// <include file="../../docs/Microsoft.Maui.Controls/WebView.xml" path="//Member[@MemberName='CookiesProperty']/Docs/*" />
		public static readonly BindableProperty CookiesProperty = BindableProperty.Create(nameof(Cookies), typeof(CookieContainer), typeof(WebView), null);

		readonly Lazy<PlatformConfigurationRegistry<WebView>> _platformConfigurationRegistry;

		/// <include file="../../docs/Microsoft.Maui.Controls/WebView.xml" path="//Member[@MemberName='.ctor']/Docs/*" />
		public WebView()
		{
			_platformConfigurationRegistry = new Lazy<PlatformConfigurationRegistry<WebView>>(() => new PlatformConfigurationRegistry<WebView>(this));
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		bool IWebViewController.CanGoBack
		{
			get { return CanGoBack; }
			set { SetValue(CanGoBackPropertyKey, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/WebView.xml" path="//Member[@MemberName='CanGoBack']/Docs/*" />
		public bool CanGoBack
		{
			get { return (bool)GetValue(CanGoBackProperty); }
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		bool IWebViewController.CanGoForward
		{
			get { return CanGoForward; }
			set { SetValue(CanGoForwardPropertyKey, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/WebView.xml" path="//Member[@MemberName='CanGoForward']/Docs/*" />
		public bool CanGoForward
		{
			get { return (bool)GetValue(CanGoForwardProperty); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/WebView.xml" path="//Member[@MemberName='Cookies']/Docs/*" />
		public CookieContainer Cookies
		{
			get { return (CookieContainer)GetValue(CookiesProperty); }
			set { SetValue(CookiesProperty, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/WebView.xml" path="//Member[@MemberName='Source']/Docs/*" />
		[System.ComponentModel.TypeConverter(typeof(WebViewSourceTypeConverter))]
		public WebViewSource Source
		{
			get { return (WebViewSource)GetValue(SourceProperty); }
			set { SetValue(SourceProperty, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/WebView.xml" path="//Member[@MemberName='Eval']/Docs/*" />
		public void Eval(string script)
		{
			Handler?.Invoke(nameof(IWebView.Eval), script);
			_evalRequested?.Invoke(this, new EvalRequested(script));
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/WebView.xml" path="//Member[@MemberName='EvaluateJavaScriptAsync']/Docs/*" />
		public async Task<string> EvaluateJavaScriptAsync(string script)
		{
			if (script == null)
				return null;

			//make all the platforms mimic Android's implementation, which is by far the most complete.
			if (DeviceInfo.Platform != DevicePlatform.Android)
			{
				script = EscapeJsString(script);
				script = "try{JSON.stringify(eval('" + script + "'))}catch(e){'null'};";
			}

			string result;

			if (_evaluateJavaScriptRequested?.GetInvocationList().Length == 0)
			{
				// This is the WebViewRenderer subscribing to these requests; the handler stuff
				// doesn't use them.
				result = await _evaluateJavaScriptRequested?.Invoke(script);
			}
			else
			{
				// Use the handler command to evaluate the JS
				result = await Handler.InvokeAsync(nameof(IWebView.EvaluateJavaScriptAsync),
					new EvaluateJavaScriptAsyncRequest(script));
			}

			//if the js function errored or returned null/undefined treat it as null
			if (result == "null")
				result = null;

			//JSON.stringify wraps the result in literal quotes, we just want the actual returned result
			//note that if the js function returns the string "null" we will get here and not above
			else if (result != null)
				result = result.Trim('"');

			return result;
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/WebView.xml" path="//Member[@MemberName='GoBack']/Docs/*" />
		public void GoBack()
		{
			Handler?.Invoke(nameof(IWebView.GoBack));
			_goBackRequested?.Invoke(this, EventArgs.Empty);
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/WebView.xml" path="//Member[@MemberName='GoForward']/Docs/*" />
		public void GoForward()
		{
			Handler?.Invoke(nameof(IWebView.GoForward));
			_goForwardRequested?.Invoke(this, EventArgs.Empty);
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/WebView.xml" path="//Member[@MemberName='Reload']/Docs/*" />
		public void Reload()
		{
			Handler?.Invoke(nameof(IWebView.Reload));
			_reloadRequested?.Invoke(this, EventArgs.Empty);
		}

		public event EventHandler<WebNavigatedEventArgs> Navigated;

		public event EventHandler<WebNavigatingEventArgs> Navigating;

		protected override void OnBindingContextChanged()
		{
			base.OnBindingContextChanged();

			WebViewSource source = Source;
			if (source != null)
			{
				SetInheritedBindingContext(source, BindingContext);
			}
		}

		protected override void OnPropertyChanged(string propertyName)
		{
			if (propertyName == "BindingContext")
			{
				WebViewSource source = Source;
				if (source != null)
					SetInheritedBindingContext(source, BindingContext);
			}

			base.OnPropertyChanged(propertyName);
		}

		protected void OnSourceChanged(object sender, EventArgs e)
		{
			OnPropertyChanged(SourceProperty.PropertyName);
		}

		event EventHandler<EvalRequested> _evalRequested;
		event EventHandler<EvalRequested> IWebViewController.EvalRequested
		{
			add { _evalRequested += value; }
			remove { _evalRequested -= value; }
		}

		event EvaluateJavaScriptDelegate _evaluateJavaScriptRequested;
		event EvaluateJavaScriptDelegate IWebViewController.EvaluateJavaScriptRequested
		{
			add { _evaluateJavaScriptRequested += value; }
			remove { _evaluateJavaScriptRequested -= value; }
		}

		event EventHandler _goBackRequested;
		event EventHandler IWebViewController.GoBackRequested
		{
			add { _goBackRequested += value; }
			remove { _goBackRequested -= value; }
		}

		event EventHandler _goForwardRequested;
		event EventHandler IWebViewController.GoForwardRequested
		{
			add { _goForwardRequested += value; }
			remove { _goForwardRequested -= value; }
		}

		void IWebViewController.SendNavigated(WebNavigatedEventArgs args)
		{
			Navigated?.Invoke(this, args);
		}

		void IWebViewController.SendNavigating(WebNavigatingEventArgs args)
		{
			Navigating?.Invoke(this, args);
		}

		event EventHandler _reloadRequested;
		event EventHandler IWebViewController.ReloadRequested
		{
			add { _reloadRequested += value; }
			remove { _reloadRequested -= value; }
		}

		/// <inheritdoc/>
		public IPlatformElementConfiguration<T, WebView> On<T>() where T : IConfigPlatform
		{
			return _platformConfigurationRegistry.Value.On<T>();
		}

		static string EscapeJsString(string js)
		{
			if (js == null)
				return null;

			if (js.IndexOf("'", StringComparison.Ordinal) == -1)
				return js;

			//get every quote in the string along with all the backslashes preceding it
			var singleQuotes = Regex.Matches(js, @"(\\*?)'");

			var uniqueMatches = new List<string>();

			for (var i = 0; i < singleQuotes.Count; i++)
			{
				var matchedString = singleQuotes[i].Value;
				if (!uniqueMatches.Contains(matchedString))
				{
					uniqueMatches.Add(matchedString);
				}
			}

			uniqueMatches.Sort((x, y) => y.Length.CompareTo(x.Length));

			//escape all quotes from the script as well as add additional escaping to all quotes that were already escaped
			for (var i = 0; i < uniqueMatches.Count; i++)
			{
				var match = uniqueMatches[i];
				var numberOfBackslashes = match.Length - 1;
				var slashesToAdd = (numberOfBackslashes * 2) + 1;
				var replacementStr = "'".PadLeft(slashesToAdd + 1, '\\');
				js = Regex.Replace(js, @"(?<=[^\\])" + Regex.Escape(match), replacementStr);
			}

			return js;
		}
	}
}