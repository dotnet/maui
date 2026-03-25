using System.Net;
using System.Text.RegularExpressions;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific;
using Label = Microsoft.Maui.Controls.Label;
using WebView = Microsoft.Maui.Controls.WebView;

namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 3262, "Adding Cookies ability to a WebView...", isInternetRequired: true)]
	public class Issue3262 : TestContentPage // or TestFlyoutPage, etc ...
	{
		string _currentCookieValue;

		protected override void Init()
		{
			Label header = new Label
			{
				Text = "Cookies...",
				HorizontalOptions = LayoutOptions.Center
			};

			try
			{
				CookieContainer cookieContainer = new CookieContainer();
				string url = "https://dotnet.microsoft.com/apps/maui";
				Uri uri = new Uri(url, UriKind.RelativeOrAbsolute);

				Cookie cookie = new Cookie
				{
					Name = "TestCookie",
					Expires = DateTime.Now.AddDays(1),
					Value = "My Test Cookie...",
					Domain = uri.Host,
					Path = "/"
				};

				cookieContainer.Add(uri, cookie);

				WebView webView = new WebView
				{
					Source = url,
					HeightRequest = 200,
					WidthRequest = 300,
					Cookies = cookieContainer
				};

#if WINDOWS
				webView.On<Microsoft.Maui.Controls.PlatformConfiguration.Windows>().SetIsJavaScriptAlertEnabled(true);
#endif

				Action<string> cookieExpectation = null;
				var cookieResult = new Label()
				{
					Text = "Loading",
				};

				var successfullPageLoadLabel = new Label()
				{
					IsVisible = false,
					Text = "Page was loaded",
					AutomationId = "SuccessfullPageLoadLabel"
				};

				var successCookiesLabel = new Label()
				{
					IsVisible = false,
					Text = "Success",
					AutomationId = "SuccessCookiesLabel"
				};

				webView.Navigated += async (_, __) =>
				{
					successfullPageLoadLabel.IsVisible = true;
					_currentCookieValue = await webView.EvaluateJavaScriptAsync("document.cookie");
					cookieExpectation?.Invoke(_currentCookieValue);
					cookieExpectation = null;
				};

				Content = new StackLayout
				{
					Padding = new Thickness(20),
					Children =
					{
						header,
						webView,
						new Label()
						{
							Text = "Modify the Cookie Container"
						},
						new HorizontalStackLayout()
						{
							cookieResult,
							successfullPageLoadLabel,
							successCookiesLabel
						},
						new StackLayout()
						{
							Orientation = StackOrientation.Horizontal,
							Children =
							{
								new Button()
								{
									Text = "Empty",
									AutomationId = "EmptyAllCookies",
									Command = new Command(() =>
									{
										webView.Cookies = cookieContainer;
										cookieResult.Text = string.Empty;
										successCookiesLabel.IsVisible = false;
										cookieExpectation = (cookieValue) =>
										{
											if(cookieValue.Contains("TestCookie", StringComparison.OrdinalIgnoreCase))
											{
												cookieResult.Text = "Test Cookie Was not correctly cleared";
											}
											else
											{
												successCookiesLabel.IsVisible = true;
											}
										};

										foreach(Cookie c in webView.Cookies.GetCookies(uri))
										{
											if(c.Name.StartsWith("TestCookie"))
												c.Expired = true;
										}

										webView.Reload();
									})
								},
								new Button()
								{
									Text = "Null",
									AutomationId = "NullAllCookies",
									Command = new Command(() =>
									{
										var currentCookies = _currentCookieValue;
										cookieExpectation = (cookieValue) =>
										{
											if(Regex.Matches(_currentCookieValue, "TestCookie").Count != Regex.Matches(cookieValue, "TestCookie").Count)
											{
												cookieResult.Text = "Cookie Collection Incorrectly Modified";
											}
											else
											{
												successCookiesLabel.IsVisible = true;
											}
										};

										webView.Cookies = null;
										webView.Reload();
									})
								},
								new Button()
								{
									Text = "One",
									AutomationId = "OneCookie",
									Command = new Command(() =>
									{
										cookieResult.Text = String.Empty;
										successCookiesLabel.IsVisible = false;
										cookieExpectation = (cookieValue) =>
										{
											if(Regex.Matches(cookieValue, "TestCookie").Count > 1)
											{
												cookieResult.Text = "Too many cookies in the jar";
											}
											else
											{
												successCookiesLabel.IsVisible = true;
											}
										};

										cookieContainer = new CookieContainer();
										cookieContainer.Add(new Cookie
										{
											Name = $"TestCookie{cookieContainer.Count}",
											Expires = DateTime.Now.AddDays(1),
											Value = $"My Test Cookie {cookieContainer.Count}...",
											Domain = uri.Host,
											Path = "/"
										});

										webView.Cookies = cookieContainer;
										webView.Reload();
									})
								}
							}
						},
						new StackLayout()
						{
							Orientation = StackOrientation.Horizontal,
							Children =
							{
								new Button()
								{
									Text = "Additional",
									AutomationId = "AdditionalCookie",
									Command = new Command(() =>
									{
										webView.Cookies = cookieContainer;
										cookieResult.Text = String.Empty;
										successCookiesLabel.IsVisible = false;
										cookieContainer.Add(new Cookie
										{
											Name = $"TestCookie{cookieContainer.Count}",
											Expires = DateTime.Now.AddDays(1),
											Value = $"My Test Cookie {cookieContainer.Count}...",
											Domain = uri.Host,
											Path = "/"
										});

										int cookieCount = 0;
										foreach(Cookie testCookie in cookieContainer.GetCookies(uri))
											if(testCookie.Name.StartsWith("TestCookie"))
												cookieCount++;

										cookieExpectation = (cookieValue) =>
										{
											if(Regex.Matches(cookieValue, "TestCookie").Count != cookieCount)
											{
												cookieResult.Text = "Not enough cookies in the jar";
											}
											else
											{
												successCookiesLabel.IsVisible = true;
											}
										};

										webView.Reload();
									})
								},
								new Button()
								{
									Text = "Add Navigating",
									AutomationId = "ChangeDuringNavigating",
									Command = new Command(() =>
									{
										webView.Cookies = cookieContainer;
										var cookieToAdd = new Cookie
										{
											Name = $"TestCookie{cookieContainer.Count}",
											Expires = DateTime.Now.AddDays(1),
											Value = $"My Test Cookie {cookieContainer.Count}...",
											Domain = uri.Host,
											Path = "/"
										};

										EventHandler<WebNavigatingEventArgs> navigating = null;
										navigating = (_, __) =>
										{
											cookieContainer.Add(cookieToAdd);
										};

										cookieResult.Text = String.Empty;
										successCookiesLabel.IsVisible = false;
										cookieExpectation = (cookieValue) =>
										{
											if(cookieValue.Contains(cookieToAdd.Name, StringComparison.OrdinalIgnoreCase))
											{
												cookieResult.Text = "Cookie not added during navigating";
											}
											else
											{
												successCookiesLabel.IsVisible = true;
											}
										};

										webView.Reload();
									})
								},
							}
						},
						new Button()
						{
							Text = "Display all Cookies. You should see a cookie called 'TestCookie'",
							AutomationId = "DisplayAllCookies",
							Command = new Command(async () =>
							{
								var result = await webView.EvaluateJavaScriptAsync("document.cookie");
								await this.DisplayAlertAsync("cookie", result, "Cancel");
							})
						},
						new Button()
						{
							Text = "Load asset without cookies and app shouldn't crash",
							AutomationId = "PageWithoutCookies",
							Command = new Command(() =>
							{
								var previousCookies = webView.Cookies;
								webView.Cookies = null;
								webView.Source = "file:///android_asset/googlemapsearch.html";

								//Restore to the previous state
								webView.Cookies = previousCookies;
								webView.Source = url;
							})
						}
					}
				};
			}
			catch (Exception ex)
			{
				_ = ex.Message;
				throw;
			}
		}
	}
}