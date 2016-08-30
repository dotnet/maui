using System;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Platform;

namespace Xamarin.Forms
{
	[RenderWith(typeof(_WebViewRenderer))]
	public class WebView : View, IElementConfiguration<WebView>
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

		public bool CanGoBack
		{
			get { return (bool)GetValue(CanGoBackProperty); }
			internal set { SetValue(CanGoBackPropertyKey, value); }
		}

		public bool CanGoForward
		{
			get { return (bool)GetValue(CanGoForwardProperty); }
			internal set { SetValue(CanGoForwardPropertyKey, value); }
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

		internal event EventHandler<EvalRequested> EvalRequested;

		internal event EventHandler GoBackRequested;

		internal event EventHandler GoForwardRequested;

		internal void SendNavigated(WebNavigatedEventArgs args)
		{
			EventHandler<WebNavigatedEventArgs> handler = Navigated;
			if (handler != null)
				handler(this, args);
		}

		internal void SendNavigating(WebNavigatingEventArgs args)
		{
			EventHandler<WebNavigatingEventArgs> handler = Navigating;
			if (handler != null)
				handler(this, args);
		}

		public IPlatformElementConfiguration<T, WebView> On<T>() where T : IConfigPlatform
		{
			return _platformConfigurationRegistry.Value.On<T>();
		}
	}
}