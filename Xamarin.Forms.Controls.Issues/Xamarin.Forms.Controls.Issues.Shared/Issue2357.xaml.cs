using System;
using System.Collections.Generic;

using Xamarin.Forms;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Linq;
using System.ComponentModel;
using System.Text;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;
#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using Xamarin.Forms.Core.UITests;
#endif

namespace Xamarin.Forms.Controls.Issues
{
#if APP
	[Preserve (AllMembers = true)]
	[Issue (IssueTracker.Github, 2357, "Webview waits to load the content until webviews on previous pages are loaded", PlatformAffected.iOS | PlatformAffected.Android)]
#if UITEST
	// this doesn't fail on Uwp but it leaves a browser window open and breaks later tests
	[Category(UITestCategories.UwpIgnore)]
#endif
	public partial class Issue2357 : MasterDetailPage
	{
		public Issue2357 ()
		{
			MasterViewModel = new MasterViewModel ();
			MasterViewModel.PageSelectionChanged += MasterViewModelOnPageSelectionChanged;
			BindingContext = MasterViewModel;

			Detail = new NavigationPage (new ContentPage {
				Title = "Home",
				Content = new Label {
					Text = "Hello, Forms !",
					VerticalOptions = LayoutOptions.CenterAndExpand,
					HorizontalOptions = LayoutOptions.CenterAndExpand
				}
			});
			InitializeComponent ();
		}

		public MasterViewModel MasterViewModel { get; set; }

		async void MasterViewModelOnPageSelectionChanged (object sender, NavigationEventArgs eventArgs)
		{
			Debug.WriteLine ("MasterViewModelOnPageSelectionChanged");
			IsPresented = false;
			var page = eventArgs.Page;
			await Detail.Navigation.PushAsync (page, true);
		}

		protected override async void OnAppearing ()
		{
			await TryInitializeMasterViewModel ();
			base.OnAppearing ();
		}

		protected override void OnDisappearing ()
		{
			//MasterViewModel.PageSelectionChanged -= MasterViewModelOnPageSelectionChanged;
			base.OnDisappearing ();
		}

		async Task TryInitializeMasterViewModel ()
		{
			while (true) {
				string errorMessage;
				try {
					await MasterViewModel.InitializeAsync ();
					break;
				} catch (Exception ex) {
					errorMessage = ex.Message;
				}

				if (!string.IsNullOrWhiteSpace (errorMessage)) {
					var retry = await DisplayAlert ("Error", errorMessage, "Retry", "Close Application");
					if (retry) {
						continue;
					}
			
				}

				break;
			}
		}

		protected void ListViewOnItemTapped (object sender, ItemTappedEventArgs e)
		{
			Debug.WriteLine ("ListViewOnItemTapped");

			if (((ListView)sender).SelectedItem == null)
				return;

			var menuItem = e.Item as MainMenuItem;

			if (menuItem != null) {
				switch (menuItem.MenuType) {
				case MenuType.Login:
					{
						break;
					}

				case MenuType.WebView:
					{
						var webViewViewModel = new WebViewViewModel (menuItem);
						MasterViewModel.CurrentDetailPage = new CustomWebView (webViewViewModel);
						break;
					}

				default:
					{
						//MenuType Standard
						break;
					}
				}

				((ListView)sender).SelectedItem = null;
			}
		}
	}

	internal class CustomWebView : ContentPage
	{
		WebView _titledWebView;

		public CustomWebView ()
		{
			_titledWebView = new WebView ();
			_titledWebView.SetBinding (WebView.SourceProperty, new Binding ("Url"));
			_titledWebView.Navigating += WebView_OnNavigating;
			this.SetBinding (TitleProperty, "Title");
			Content = _titledWebView;
		}

		public CustomWebView (WebViewViewModel webViewViewModel) : this ()
		{
			Debug.WriteLine ("New WebView");

			_titledWebView.BindingContext = webViewViewModel;
		}

		static void WebView_OnNavigating (object sender, WebNavigatingEventArgs e)
		{
			Debug.WriteLine ("OS: " + Device.RuntimePlatform + " Current Url: " + GetSourceUrl (((WebView)sender).Source) + "Destination Url: " + e.Url + " " + DateTime.Now);

			if (e.Url.IsValidAbsoluteUrl ()) {
				var destinationUri = new Uri (e.Url);
				var sourceUri = GetSourceUrl (((WebView)sender).Source);
				if (sourceUri.HasSameHost (destinationUri)) {
					if (destinationUri == sourceUri) {
						//Do nothing. This happens on webview load
						Debug.WriteLine ("WebView_OnNavigating Same URI");
						return;
					}

					//If it reaches here, A link could have been clicked.
					e.Cancel = true;
					Debug.WriteLine ("WebView_OnNavigating Same Host but different Uri");
				} else {
					//if external link is clicked
					Debug.WriteLine ("WebView_OnNavigating, DIfferent Uri, so open in Native Browser");
					e.Cancel = true;
					Device.OpenUri (new Uri (e.Url));    
				}
			}
		}

		static Uri GetSourceUrl (WebViewSource source)
		{
			Debug.Assert (source != null, "source cannot be null.");

			var urlWebViewSource = source as UrlWebViewSource;
			if (urlWebViewSource != null) {
				if (urlWebViewSource.Url.IsValidAbsoluteUrl ()) {
					return new Uri (urlWebViewSource.Url);
				}
			}

			throw new InvalidOperationException ("WebViewSource is Invalid. Only UrlWebViewSource is accepted.");
		}
	}

	public static class UriExtensions
	{
		public static bool HasSameHost (this Uri sourceUri, Uri destinationUri, UriFormat uriFormat = UriFormat.Unescaped)
		{
			Debug.Assert (sourceUri != null, "sourceUri cannot be null.");
			Debug.Assert (destinationUri != null, "destinationUri cannot be null.");

			return destinationUri.GetComponents (UriComponents.Host, uriFormat) ==
			sourceUri.GetComponents (UriComponents.Host, uriFormat);
		}
	}

	public static class StringExtensions
	{
		public static bool IsValidAbsoluteUrl (this string stringValue)
		{
			Uri result;
			return !string.IsNullOrWhiteSpace (stringValue) && Uri.TryCreate (stringValue, UriKind.Absolute, out result) && (result.Scheme == "http" || result.Scheme == "https");
		}
	}
	public delegate void PageSelectionChanged (object sender, NavigationEventArgs e);

	public class MasterViewModel : ViewModelBase1
	{
		public static event PageSelectionChanged PageSelectionChanged;

		ObservableCollection<MainMenuItem> _mainMenuItems;
		Page _currentDetailPage;

		public MasterViewModel ()
		{
			_mainMenuItems = new ObservableCollection<MainMenuItem> (Enumerable.Empty<MainMenuItem> ());
		}

#pragma warning disable 1998 // considered for removal
		public async Task InitializeAsync ()
#pragma warning restore 1998
		{
			var items = new List<MainMenuItem> ();
			items.Add (new MainMenuItem {
				Title = "SHORT",
				MenuType = MenuType.WebView,
				Uri = new Uri ("http://api.morgans.bluearc-uat.com/mobile/SamplePage.aspx?page=Portfolio")
			});
			items.Add (new MainMenuItem {
				Title = "LONG",
				MenuType = MenuType.WebView,
				Uri = new Uri ("http://api.morgans.bluearc-uat.com/mobile/SamplePage.aspx?page=long")
			});
		
			MainMenuItems = new ObservableCollection<MainMenuItem> (items);
		}

		public ObservableCollection<MainMenuItem> MainMenuItems {
			get { return _mainMenuItems; }
			set {
				_mainMenuItems = value;
				OnPropertyChanged ("MainMenuItems");
			}
		}

		public Page CurrentDetailPage {
			get { return _currentDetailPage; }
			set {
				_currentDetailPage = value;

				var handler = PageSelectionChanged;
				if (handler != null) {
					handler (null, new NavigationEventArgs (value));
				}
			}
		}
	}

	public class WebViewViewModel : ViewModelBase1
	{
		string _title;
		string _url;

		public WebViewViewModel (MainMenuItem menuItem)
		{
			Debug.WriteLine ("New WebViewViewModel");
			_title = menuItem.Title;
			_url = menuItem.Uri.AbsoluteUri;
		}

		public string Title {
			get { return _title; }
			set {
				_title = value;
				OnPropertyChanged ("Title");
			}
		}

		public string Url {
			get { return _url; }
			set {
				Debug.WriteLine ("WebViewViewModel Url Changed");
				_url = value;
				OnPropertyChanged ("Url");
			}
		}
	}

	public interface IMenuService
	{
		Task<IEnumerable<MainMenuItem>> GetMenuItemsAsync ();
	}

	public class MainMenuItem
	{
		public object Id { get; set; }

		public MenuType MenuType { get; set; }

		public string Title { get; set; }

		public Uri Uri { get; set; }
	}

	public enum MenuType
	{
		Login,
		WebView,
		Standard
	}

	public class ViewModelBase1 : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		/// <summary>
		/// Event for when IsBusy changes
		/// </summary>
		public event EventHandler IsBusyChanged;

		/// <summary>
		/// Event for when IsValid changes
		/// </summary>
		public event EventHandler IsValidChanged;

		readonly List<string> _errors = new List<string> ();
		bool _isBusy;

		/// <summary>
		/// Default constructor
		/// </summary>
		public ViewModelBase1 ()
		{
			//Make sure validation is performed on startup
			Validate ();
		}

		/// <summary>
		/// Returns true if the current state of the ViewModel is valid
		/// </summary>
		public bool IsValid {
			get { return _errors.Count == 0; }
		}

		/// <summary>
		/// A list of errors if IsValid is false
		/// </summary>
		protected List<string> Errors {
			get { return _errors; }
		}

		/// <summary>
		/// An aggregated error message
		/// </summary>
		public string Error {
			get {
				return _errors.Aggregate (new StringBuilder (), (b, s) => b.AppendLine (s)).ToString ().Trim ();
			}
		}

		/// <summary>
		/// Protected method for validating the ViewModel
		/// - Fires PropertyChanged for IsValid and Errors
		/// </summary>
		protected void Validate ()
		{
			OnPropertyChanged ("IsValid");
			OnPropertyChanged ("Errors");

			var method = IsValidChanged;
			if (method != null)
				method (this, EventArgs.Empty);
		}

		/// <summary>
		/// Other viewmodels should call this when overriding Validate, to validate each property
		/// </summary>
		/// <param name="validate">Func to determine if a value is valid</param>
		/// <param name="error">The error message to use if not valid</param>
		protected void ValidateProperty (Func<bool> validate, string error)
		{
			if (validate ()) {
				if (!Errors.Contains (error))
					Errors.Add (error);
			} else {
				Errors.Remove (error);
			}
		}

		/// <summary>
		/// Value indicating if a spinner should be shown
		/// </summary>
		public bool IsBusy {
			get { return _isBusy; }
			set {
				if (_isBusy != value) {
					_isBusy = value;

					OnPropertyChanged ("IsBusy");
					OnIsBusyChanged ();
				}
			}
		}

		/// <summary>
		/// Other viewmodels can override this if something should be done when busy
		/// </summary>
		protected void OnIsBusyChanged ()
		{
			var ev = IsBusyChanged;
			if (ev != null) {
				ev (this, EventArgs.Empty);
			}
		}

		protected void OnPropertyChanged (string name)
		{
			var ev = PropertyChanged;
			if (ev != null) {
				ev (this, new PropertyChangedEventArgs (name));
			}
		}
	}
#endif
}

