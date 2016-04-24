using System;

namespace Xamarin.Forms.Controls
{
	public class AppLinkPageGallery  : ContentPage
	{
		public AppLinkPageGallery ()
		{
			_linkEntry = GetEntry ();
			_lbl = new Label {
				Text = "You are on a demo page via app url", IsVisible = ShowLabel 
			};

			var btnRegister = new Button { Text = "Index this Page", 
				Command = new Command (() => Application.Current.AppLinks.RegisterLink (LinkEntry))
			};
			var btnRemove = new Button { Text = "Remove this Page from index", 
				Command = new Command (() => Application.Current.AppLinks.DeregisterLink (LinkEntry))
			};

			var btnClearAll = new Button { Text = "Clear All Indexed Data", 
			//	Command = new Command (() => Application.Current.AppLinks.DeregisterAll ())
			};

			Content = new StackLayout { Children = { _lbl, btnRegister, btnRemove, btnClearAll } };
		}

		protected override void OnAppearing ()
		{
			LinkEntry.IsLinkActive = true;
		}

		protected override void OnDisappearing ()
		{
			LinkEntry.IsLinkActive = false;
		}

		public bool ShowLabel {
			get { 
				return _showlabel;
			}
			set { 
				_showlabel = value;
				_lbl.IsVisible = _showlabel;
			}
		}

		internal IAppLinkEntry LinkEntry {
			get { 
				return _linkEntry;
			}
		}

		bool _showlabel;
		IAppLinkEntry _linkEntry;
		Label _lbl;

		AppLinkEntry GetEntry ()
		{
			if (string.IsNullOrEmpty (Title))
				Title = "App Link Page Gallery";
	
			var type = GetType ().ToString ();
			var entry = new AppLinkEntry {
				Title = Title,
				Description =$"This is the page {Title} \nof Xamarin Forms Gallery",
				AppLinkUri = new Uri ($"http://{App.AppName}/gallery/{type}", UriKind.RelativeOrAbsolute),
				IsLinkActive = true,
				Thumbnail = ImageSource.FromFile ("seth.png")
			};

			entry.KeyValues.Add ("contentType", "GalleryPage");
			entry.KeyValues.Add ("appName",  App.AppName);
			entry.KeyValues.Add ("companyName", "Xamarin");

			return entry;
		}
	}
}

