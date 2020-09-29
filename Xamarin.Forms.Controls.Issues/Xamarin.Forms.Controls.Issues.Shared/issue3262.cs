using System;
using System.Net;
using System.Text.RegularExpressions;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;
using Xamarin.Forms.PlatformConfiguration;
using Xamarin.Forms.PlatformConfiguration.WindowsSpecific;

#if UITEST
using Xamarin.Forms.Core.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
#if UITEST
	[Category(UITestCategories.ManualReview)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 3262, "Adding Cookies ability to a WebView...")]
	public class Issue3262 : TestContentPage // or TestFlyoutPage, etc ...
	{
		string _currentCookieValue;

		protected override void Init()
		{
			Label header = new Label
			{
				Text = "Cookies...",
				FontSize = Device.GetNamedSize(NamedSize.Medium, typeof(Label)),
				HorizontalOptions = LayoutOptions.Center
			};

			try
			{
				CookieContainer cookieContainer = new CookieContainer();
				string url = "https://dotnet.microsoft.com/apps/xamarin";
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
					HorizontalOptions = LayoutOptions.FillAndExpand,
					VerticalOptions = LayoutOptions.FillAndExpand,
					Cookies = cookieContainer
				};
				webView.On<Windows>().SetIsJavaScriptAlertEnabled(true);

				Action<string> cookieExpectation = null;
				var cookieResult = new Label()
				{
					Text = "Loading",
					AutomationId = "CookieResult"
				};

				webView.Navigating += (_, __) =>
				{
					if (cookieExpectation != null)
						cookieResult.Text = "Navigating";
				};

				webView.Navigated += async (_, __) =>
				{
					if (cookieResult.Text == "Loading")
						cookieResult.Text = "Loaded";

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
						cookieResult,
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
										cookieResult.Text = String.Empty;
										cookieExpectation = (cookieValue) =>
										{
											if(cookieValue.Contains("TestCookie"))
											{
												cookieResult.Text = "Test Cookie Was not correctly cleared";
											}
											else
											{
												cookieResult.Text = "Success";
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
												cookieResult.Text = "Success";
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
										cookieExpectation = (cookieValue) =>
										{
											if(Regex.Matches(cookieValue, "TestCookie").Count > 1)
											{
												cookieResult.Text = "Too many cookies in the jar";
											}
											else
											{
												cookieResult.Text = "Success";
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
												cookieResult.Text = "Success";
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
										cookieExpectation = (cookieValue) =>
										{
											if(cookieValue.Contains(cookieToAdd.Name))
											{
												cookieResult.Text = "Cookie not added during navigating";
											}
											else
											{
												cookieResult.Text = "Success";
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
								await this.DisplayAlert("cookie", result, "Cancel");
							})
						},
						new Button()
						{
							Text = "Load asset without cookies and app shouldn't crash",
							AutomationId = "PageWithoutCookies",
							Command = new Command(() =>
							{
								webView.Cookies = null;
								webView.Source = "file:///android_asset/googlemapsearch.html";
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

#if UITEST

		[Test]
		public void LoadingPageWithoutCookiesSpecifiedDoesntCrash()
		{
			RunningApp.Tap("PageWithoutCookies");
			RunningApp.WaitForElement("PageWithoutCookies");
		}

		[Test]
		public void ChangeDuringNavigating()
		{
			RunningApp.WaitForElement("Loaded");
			// add a couple cookies
			RunningApp.Tap("ChangeDuringNavigating");
			RunningApp.WaitForElement("Success");
			RunningApp.Tap("ChangeDuringNavigating");
			RunningApp.WaitForElement("Success");
		}

		[Test]
		public void AddAdditionalCookieToWebView()
		{
			RunningApp.WaitForElement("Loaded");
			// add a couple cookies
			RunningApp.Tap("AdditionalCookie");
			RunningApp.WaitForElement("Success");
			RunningApp.Tap("AdditionalCookie");
			RunningApp.WaitForElement("Success");
		}

		[Test]
		public void SetToOneCookie()
		{
			RunningApp.WaitForElement("Loaded");
			RunningApp.Tap("OneCookie");
			RunningApp.WaitForElement("Success");
		}

		[Test]
		public void SetCookieContainerToNullDisablesCookieManagement()
		{
			RunningApp.WaitForElement("Loaded");
			// add a cookie to verify said cookie remains
			RunningApp.Tap("AdditionalCookie");
			RunningApp.WaitForElement("Success");
			RunningApp.Tap("NullAllCookies");
			RunningApp.WaitForElement("Success");
		}

		[Test]
		public void RemoveAllTheCookiesIAdded()
		{
			RunningApp.WaitForElement("Loaded");
			// add a cookie so you can remove a cookie
			RunningApp.Tap("AdditionalCookie");
			RunningApp.WaitForElement("Success");
			RunningApp.Tap("EmptyAllCookies");
			RunningApp.WaitForElement("Success");
		}
#endif
	}
}