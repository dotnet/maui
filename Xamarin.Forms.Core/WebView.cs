using System;
using System.ComponentModel;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Platform;

namespace Xamarin.Forms
{
	[RenderWith(typeof(_WebViewRenderer))]
	public class WebView : View, IWebViewController, IElementConfiguration<WebView>
	{
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

		public static readonly BindableProperty CanGoBackProperty = CanGoBackPropertyKey.BindableProperty;

		static readonly BindablePropertyKey CanGoForwardPropertyKey = BindableProperty.CreateReadOnly("CanGoForward", typeof(bool), typeof(WebView), false);

		public static readonly BindableProperty CanGoForwardProperty = CanGoForwardPropertyKey.BindableProperty;

		readonly Lazy<PlatformConfigurationRegistry<WebView>> _platformConfigurationRegistry;

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

		public bool CanGoForward
		{
			get { return (bool)GetValue(CanGoForwardProperty); }
		}

		[TypeConverter(typeof(WebViewSourceTypeConverter))]
		public WebViewSource Source
		{
			get { return (WebViewSource)GetValue(SourceProperty); }
			set { SetValue(SourceProperty, value); }
		}

		public void Eval(string script)
		{
			EventHandler<EvalRequested> handler = EvalRequested;
			handler?.Invoke(this, new EvalRequested(script));
		}

		public void GoBack()
		{
			EventHandler handler = GoBackRequested;
			if (handler != null)
				handler(this, EventArgs.Empty);
		}

		public void GoForward()
		{
			EventHandler handler = GoForwardRequested;
			if (handler != null)
				handler(this, EventArgs.Empty);
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

		event EventHandler<EvalRequested> IWebViewController.EvalRequested {
			add { EvalRequested += value; }
			remove { EvalRequested -= value; }
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public event EventHandler<EvalRequested> EvalRequested;

		[EditorBrowsable(EditorBrowsableState.Never)]
		public event EventHandler GoBackRequested;

		[EditorBrowsable(EditorBrowsableState.Never)]
		public event EventHandler GoForwardRequested;

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void SendNavigated(WebNavigatedEventArgs args)
		{
			Navigated?.Invoke(this, args);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void SendNavigating(WebNavigatingEventArgs args)
		{
			Navigating?.Invoke(this, args);
		}

		public IPlatformElementConfiguration<T, WebView> On<T>() where T : IConfigPlatform
		{
			return _platformConfigurationRegistry.Value.On<T>();
		}
	}
}