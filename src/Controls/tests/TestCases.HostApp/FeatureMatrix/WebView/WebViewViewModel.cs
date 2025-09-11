using System;
using System.ComponentModel;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;
namespace Maui.Controls.Sample;

public class WebViewViewModel : INotifyPropertyChanged
{
	private WebViewSource _source;
	private CookieContainer _cookies;
	private bool _canGoBack;
	private bool _canGoForward;
	private bool _isVisible = true;
	private Shadow _shadow = null;
	private string _navigatingStatus;
	private string _navigatedStatus;
	private string _processTerminatedStatus;
	private string _jsEvaluationResult;
	private bool _isEventStatusLabelVisible = false;
	public bool IsPageLoaded { get; set; }
	public event PropertyChangedEventHandler PropertyChanged;
	public WebViewViewModel()
	{
		Source = new HtmlWebViewSource
		{
			Html = @"
            <html>
            <head>
                <title>HTML WebView Source</title>
            </head>
            <body style='font-family:sans-serif; padding:20px;'>
                <h1>WebView Feature Matrix</h1>
                <p>This page demonstrates various capabilities of the .NET MAUI WebView control, such as:</p>
                <ul>
                    <li>Rendering HTML content</li>
                    <li>Executing JavaScript</li>
                    <li>Cookie management</li>
                    <li>Back/Forward navigation</li>
                </ul>
                <h2>Test Content</h2>
                <p>
                    This is a longer body paragraph to help test the <strong>EvaluateJavaScript</strong> functionality 
                    and how it extracts body text. You can use this text to verify substring operations and test scrolling 
                    or formatting in the WebView.
                </p>
                <p>
                    Try interacting with navigation buttons, loading multiple pages, or checking cookie behavior.
                </p>
                <footer style='margin-top:40px; font-size:0.9em; color:gray;'>Generated for testing WebView features.</footer>
            </body>
            </html>",
		};
		GoBackCommand = new Command(OnGoBack, () => CanGoBack);
		GoForwardCommand = new Command(OnGoForward, () => CanGoForward);
		ReloadCommand = new Command(OnReload);
		EvaluateJavaScriptCommand = new Command(OnEvaluateJavaScript);
	}
	public void CopyWebViewStateFrom(WebViewViewModel oldViewModel)
	{
		WebViewReference = oldViewModel.WebViewReference;
		IsPageLoaded = oldViewModel.IsPageLoaded;
		CanGoBack = oldViewModel.CanGoBack;
		CanGoForward = oldViewModel.CanGoForward;
		JsEvaluationResult = oldViewModel.JsEvaluationResult;
	}
	public WebViewSource Source
	{
		get => _source;
		set
		{
			if (_source != value)
			{
				_source = value;
				JsEvaluationResult = string.Empty;
				OnPropertyChanged();
			}
		}
	}
	public CookieContainer Cookies
	{
		get => _cookies;
		set
		{
			if (_cookies != value)
			{
				_cookies = value;
				OnPropertyChanged();
				OnPropertyChanged(nameof(CookiesDisplay));
			}
		}
	}

	public bool CanGoBack
	{
		get => _canGoBack;
		set
		{
			if (_canGoBack != value)
			{
				_canGoBack = value;
				OnPropertyChanged();
				((Command)GoBackCommand).ChangeCanExecute();
			}
		}
	}
	public bool CanGoForward
	{
		get => _canGoForward;
		set
		{
			if (_canGoForward != value)
			{
				_canGoForward = value;
				OnPropertyChanged();
				((Command)GoForwardCommand).ChangeCanExecute();
			}
		}
	}
	public bool IsVisible
	{
		get => _isVisible;
		set
		{
			if (_isVisible != value)
			{
				_isVisible = value;
				OnPropertyChanged();
			}
		}
	}
	public string NavigatingStatus
	{
		get => _navigatingStatus;
		set
		{
			if (_navigatingStatus != value)
			{
				if (!string.IsNullOrEmpty(value))
				{
					IsEventStatusLabelVisible = true;
				}
				_navigatingStatus = value;
				OnPropertyChanged();
			}
		}
	}
	public string NavigatedStatus
	{
		get => _navigatedStatus;
		set
		{
			if (_navigatedStatus != value)
			{
				if (!string.IsNullOrEmpty(value))
				{
					IsEventStatusLabelVisible = true;
				}
				_navigatedStatus = value;
				OnPropertyChanged();
			}
		}
	}
	public string ProcessTerminatedStatus
	{
		get => _processTerminatedStatus;
		set
		{
			if (_processTerminatedStatus != value)
			{
				if (!string.IsNullOrEmpty(value))
				{
					IsEventStatusLabelVisible = true;
				}
				_processTerminatedStatus = value;
				OnPropertyChanged();
			}
		}
	}
	public string JsEvaluationResult
	{
		get => _jsEvaluationResult;
		set
		{
			if (_jsEvaluationResult != value)
			{
				_jsEvaluationResult = value;
				OnPropertyChanged();
			}
		}
	}
	public bool IsEventStatusLabelVisible
	{
		get => _isEventStatusLabelVisible;
		set
		{
			if (_isEventStatusLabelVisible != value)
			{
				_isEventStatusLabelVisible = value;
				OnPropertyChanged();
			}
		}
	}
	public Shadow Shadow
	{
		get => _shadow;
		set { if (_shadow != value) { _shadow = value; OnPropertyChanged(); } }
	}
	public ICommand GoBackCommand { get; }
	public ICommand GoForwardCommand { get; }
	public ICommand ReloadCommand { get; }
	public ICommand EvaluateJavaScriptCommand { get; }
	public WebView WebViewReference { get; set; }
	private void OnGoBack()
	{
		WebViewReference?.GoBack();
	}
	private void OnGoForward()
	{
		WebViewReference?.GoForward();
	}
	private void OnReload()
	{
		WebViewReference?.Reload();
	}
	private async void OnEvaluateJavaScript()
	{
		if (WebViewReference != null)
		{
			try
			{
				var title = await WebViewReference.EvaluateJavaScriptAsync("document.title");
				if (string.IsNullOrEmpty(title))
				{
					var bodyText = await WebViewReference.EvaluateJavaScriptAsync("document.body.textContent.substring(0, 20)");
					JsEvaluationResult = $"JS Result: {bodyText}... (body content)";
				}
				else
				{
					JsEvaluationResult = $"JS Result: {title}";
				}
			}
			catch (Exception ex)
			{
				JsEvaluationResult = $"JS Error: {ex.Message}";
			}
		}
	}
	public void AddTestCookies()
	{
		var domain = GetCookieDomain();
		if (string.IsNullOrEmpty(domain))
			return;
		var cookieContainer = Cookies ?? new CookieContainer();
		var uri = new Uri($"https://{domain}");
		var cookie = new Cookie
		{
			Name = "DotNetMAUICookie",
			Value = "My cookie",
			Domain = uri.Host,
			Path = "/",
			Expires = DateTime.Now.AddDays(1)
		};
		try
		{
			cookieContainer.Add(uri, cookie);
		}
		catch
		{
			// Ignore if cookie is malformed
		}
		Cookies = cookieContainer;
	}
	public void ClearCookiesForCurrentSource()
	{
		Cookies = new CookieContainer();
	}
	private string GetCookieDomain()
	{
		if (Source is UrlWebViewSource urlSource && !string.IsNullOrEmpty(urlSource.Url))
		{
			var uri = new Uri(urlSource.Url);
			var domain = uri.Host;
			if (domain.StartsWith("www."))
				domain = domain.Substring(4);
			return domain;
		}
		if (Source is HtmlWebViewSource)
			return "localhost";
		return null;
	}
	public string CookiesDisplay
	{
		get
		{
			if (Cookies == null || Cookies.Count == 0)
				return "No cookies available.";
			try
			{
				var domain = GetDomainFromSource();
				var uri = new Uri($"https://{domain}");
				var cookieCollection = Cookies.GetCookies(uri);
				var visibleCookies = cookieCollection.Cast<Cookie>()
					.Where(c => !IsSystemCookie(c.Name))
					.ToList();
				if (visibleCookies.Count == 0)
					return $" No displayable cookies for domain: {domain}";
				var cookieText = string.Join("\n", visibleCookies.Select(c => $"{c.Name} = {c.Value}"));
				return $"Domain: {domain}\nCount: {visibleCookies.Count}\n{cookieText}";
			}
			catch (Exception ex)
			{
				return $" Error reading cookies: {ex.Message}";
			}
		}
	}
	private string GetDomainFromSource()
	{
		if (Source is UrlWebViewSource urlSource && !string.IsNullOrEmpty(urlSource.Url))
		{
			var uri = new Uri(urlSource.Url);
			var domain = uri.Host;
			return domain.StartsWith("www.") ? domain.Substring(4) : domain;
		}
		else if (Source is HtmlWebViewSource)
		{
			return "localhost";
		}
		return "unknown";
	}
	private bool IsSystemCookie(string name)
	{
		var excluded = new[] { "TestCookie", "SessionId" };
		return excluded.Contains(name, StringComparer.OrdinalIgnoreCase);
	}
	public void OnNavigating(object sender, WebNavigatingEventArgs e)
	{
		if (Source is HtmlWebViewSource)
		{
			NavigatingStatus = "Navigating to: Embedded HTML";
		}
		else if (Source is UrlWebViewSource)
		{
			try
			{
				var uri = new Uri(e.Url);
				NavigatingStatus = $"Navigating to: {uri.Host}";
			}
			catch
			{
				var shortUrl = e.Url?.Length > 40 ? e.Url.Substring(0, 40) + "..." : e.Url;
				NavigatingStatus = $"Navigating to: {shortUrl}";
			}
		}
		else
		{
			NavigatingStatus = "Navigating...";
		}
	}
	public void OnNavigated(object sender, WebNavigatedEventArgs e)
	{
		NavigatedStatus = $"Navigated: {e.Result}";
		if (WebViewReference != null)
		{
			CanGoBack = WebViewReference.CanGoBack;
			CanGoForward = WebViewReference.CanGoForward;
		}
	}
	public void OnProcessTerminated(object sender, EventArgs e)
	{
		ProcessTerminatedStatus = "WebView process terminated";
	}
	protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
}