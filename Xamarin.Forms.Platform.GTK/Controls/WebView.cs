using Gtk;
using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Xamarin.Forms.Platform.GTK.Helpers;

namespace Xamarin.Forms.Platform.GTK.Controls
{
    // WebView definition for all platforms (Linux, macOS and Windows).
    public interface IWebView
    {
        string Uri { get; set; }
        void Navigate(string uri);
        void LoadHTML(string html, string baseUrl);
        bool CanGoBack();
        void GoBack();
        bool CanGoForward();
        void GoForward();
        void ExecuteScript(string script);
        event EventHandler LoadStarted;
        event EventHandler LoadFinished;
    }

    public class WebView : EventBox, IWebView
    {
        private GTKPlatform _platform;
        private WebViewWindows _webViewWindows;
        private WebViewLinux _webViewLinux;

        public event EventHandler LoadStarted;
        public event EventHandler LoadFinished;

        public string Uri
        {
            get
            {
                if (_platform == GTKPlatform.Windows)
                {
                    return _webViewWindows.WebBrowser.Url != null ? _webViewWindows.WebBrowser.Url.ToString() : string.Empty;
                }
                else
                {
                    return _webViewLinux.WebView.Uri;
                }
            }
            set
            {
                if (_platform == GTKPlatform.Windows)
                {
                    _webViewWindows.WebBrowser.Url = new Uri(value);
                }
                else
                {
                    _webViewLinux.WebView.LoadUri(value);
                }
            }
        }

        public WebView()
        {
            BuildWebView();
        }

        private void BuildWebView()
        {
            _platform = PlatformHelper.GetGTKPlatform();

            if (_platform == GTKPlatform.Windows)
            {
                _webViewWindows = new WebViewWindows();

                _webViewWindows.WebBrowser.Navigating += (sender, args) =>
                {
                    LoadStarted?.Invoke(this, args);
                };

                _webViewWindows.WebBrowser.Navigated += (sender, args) =>
                {
                    LoadFinished?.Invoke(this, args);
                };

                Add(_webViewWindows);
            }
            else
            {
                _webViewLinux = new WebViewLinux();

                _webViewLinux.WebView.LoadStarted += (sender, args) =>
                {
                    LoadStarted?.Invoke(this, args);
                };

                _webViewLinux.WebView.LoadFinished += (sender, args) =>
                {
                    LoadFinished?.Invoke(this, args);
                };

                Add(_webViewLinux);
            }
        }

        public void Navigate(string uri)
        {
            if (_platform == GTKPlatform.Windows)
            {
                _webViewWindows.Navigate(uri);
            }
            else
            {
                _webViewLinux.Navigate(uri);
            }
        }

        public void LoadHTML(string html, string baseUrl)
        {
            if (_platform == GTKPlatform.Windows)
            {
                _webViewWindows.LoadHTML(html, baseUrl);
            }
            else
            {
                _webViewLinux.LoadHTML(html, baseUrl);
            }
        }

        public bool CanGoBack()
        {
            if (_platform == GTKPlatform.Windows)
            {
                return _webViewWindows.WebBrowser.CanGoBack;
            }
            else
            {
                return _webViewLinux.WebView.CanGoBack();
            }
        }

        public void GoBack()
        {
            if (_platform == GTKPlatform.Windows)
            {
                _webViewWindows.WebBrowser.GoBack();
            }
            else
            {
                _webViewLinux.WebView.GoBack();
            }
        }

        public bool CanGoForward()
        {
            if (_platform == GTKPlatform.Windows)
            {
                return _webViewWindows.WebBrowser.CanGoForward;
            }
            else
            {
                return _webViewLinux.WebView.CanGoForward();
            }
        }

        public void GoForward()
        {
            if (_platform == GTKPlatform.Windows)
            {
                _webViewWindows.WebBrowser.GoForward();
            }
            else
            {
                 _webViewLinux.WebView.GoForward();
            }
        }

        public void ExecuteScript(string script)
        {
            if (_platform == GTKPlatform.Windows)
            {
                _webViewWindows.WebBrowser.DocumentText = script;
                _webViewWindows.WebBrowser.Document.InvokeScript(script);
            }
            else
            {
                _webViewLinux.WebView.ExecuteScript(script);
            }
        }
    }

    public class WebViewWindows : EventBox
    {
        [DllImport("libgdk-win32-2.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr gdk_win32_drawable_get_handle(IntPtr d);

        private WebBrowser _browser = null;

        /// <summary>
        /// Imported unmanaged function for setting the parent of a window.
        /// it's used for setting the parent of a WebBrowser.
        /// </summary>
        [DllImport("user32.dll", EntryPoint = "SetParent")]
        private static extern IntPtr SetParent([In] IntPtr hWndChild, [In] IntPtr hWndNewParent);

        public WebViewWindows()
        {
            BuildWebView();
        }

        public WebBrowser WebBrowser
        {
            get { return _browser; }
        }

        public void Navigate(string uri)
        {
            Uri uriResult;
            bool result = Uri.TryCreate(uri, UriKind.Absolute, out uriResult)
                && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);

            if (result)
            {
                _browser.Navigate(new Uri(uri));
            }
            else
            {
                string appPath = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetName().CodeBase);
                string filePath = System.IO.Path.Combine(appPath, uri);
                _browser.Url = new Uri(filePath);
            }
        }

        public void LoadHTML(string html, string baseUrl)
        {
            _browser.DocumentText = html;
            _browser.Update();
        }

        protected override void OnSizeAllocated(Gdk.Rectangle allocation)
        {
            base.OnSizeAllocated(allocation);

            if (IsRealized)
            {
                _browser.Bounds = 
                    new System.Drawing.Rectangle(allocation.X, allocation.Y, allocation.Width, allocation.Height);
            }
        }

        private void BuildWebView()
        {
            CreateWebView();

            var browserHandle = _browser.Handle;

            ScrolledWindow scroll = new ScrolledWindow
            {
                CanFocus = true,
                ShadowType = ShadowType.None
            };

            var drawingArea = new DrawingArea();

            IntPtr windowHandle;

            drawingArea.ExposeEvent += (s, a) =>
            {
                IntPtr test = drawingArea.GdkWindow.Handle;
                windowHandle = gdk_win32_drawable_get_handle(test);

                // Embedding Windows Browser control into a gtk widget.
                SetParent(browserHandle, windowHandle);
            };

            scroll.Add(drawingArea);

            Add(scroll);
            ShowAll();
        }

        private void CreateWebView()
        {
            _browser = new WebBrowser();
            _browser.ScriptErrorsSuppressed = true;
            _browser.AllowWebBrowserDrop = false;
        }
    }

    public class WebViewLinux : EventBox
    {
        private VBox _vbox = null;
        private WebKit.WebView _webview = null;

        public WebViewLinux()
        {
            BuildWebView();
        }

        public WebKit.WebView WebView
        {
            get { return _webview; }
        }
        
        public void Navigate(string uri)
        {
            _webview.Open(uri);
        }

        public void LoadHTML(string html, string baseUrl)
        {
            _webview.LoadHtmlString(html, baseUrl);
        }

        private void BuildWebView()
        {
            CreateWebView();

            ScrolledWindow scroll = new ScrolledWindow();
            scroll.AddWithViewport(_webview);

            _vbox = new VBox(false, 1);
            _vbox.PackStart(scroll, true, true, 0);

            Add(_vbox);
            ShowAll();
        }

        private void CreateWebView()
        {
            _webview = new WebKit.WebView();
            _webview.Editable = false;
        }
    }
}