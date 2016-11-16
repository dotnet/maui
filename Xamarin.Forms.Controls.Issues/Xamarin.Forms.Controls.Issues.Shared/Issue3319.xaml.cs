using System;
using System.Collections.Generic;

using Xamarin.Forms;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve (AllMembers = true)]
	[Issue (IssueTracker.Github, 3319, "[iOS] Clear and adding rows exception")]
	public partial class Issue3319 : TestContentPage
	{
		#if UITEST
		[Test]
		public void Issue3319Test ()
		{
			RunningApp.WaitForElement (q => q.Marked ("Will this repo work?"));
			
		}
		#endif

		FavoritesViewModel ViewModel {
			get { return BindingContext as FavoritesViewModel; }
		}

		public Issue3319 ()
		{
			#if APP
			InitializeComponent ();
			BindingContext = new FavoritesViewModel ();

			listView.SeparatorVisibility = SeparatorVisibility.Default;
			listView.SeparatorColor = Color.FromHex ("#ababab");

			listView.ItemTapped += (sender, args) => {
				if (listView.SelectedItem == null)
					return;

				//do nothing anyway
				return;
			};
			#endif

		}

		protected override void Init ()
		{
	
		}

#pragma warning disable 1998 // considered for removal
		public async void OnDelete (object sender, EventArgs e)
#pragma warning restore 1998
		{
			var mi = ((MenuItem)sender);
			if (mi.CommandParameter == null)
				return;

			var articlelistingitem = mi.CommandParameter;

			if (articlelistingitem != null)
				DisplayAlert ("Alert", "I'm not deleting just refreshing...", "Ok");
			ViewModel.LoadFavoritesCommand.Execute (null);
		}


		protected override void OnAppearing ()
		{
			base.OnAppearing ();
			if (ViewModel == null || !ViewModel.CanLoadMore || ViewModel.IsBusy)
				return;

			Device.BeginInvokeOnMainThread (() => {
				ViewModel.LoadFavoritesCommand.Execute (null);
			});
		}

		[Preserve (AllMembers = true)]
		public class FavoritesViewModel :BaseViewModelF
		{
			public ObservableCollection<ArticleListing> FavoriteArticles { get; set; }

			public FavoritesViewModel ()
			{
				Title = "Favorites";
				FavoriteArticles = new ObservableCollection<ArticleListing> ();
			}

			Command _loadFavoritesCommand;

			public Command LoadFavoritesCommand {
				get {
					return _loadFavoritesCommand ??
					(_loadFavoritesCommand = new Command (async () => {
						await ExecuteFavoritesCommand ();
					}, () => {
						return !IsBusy;
					}));
				}
			}

#pragma warning disable 1998 // considered for removal
			public async Task ExecuteFavoritesCommand ()
#pragma warning restore 1998
			{
				if (IsBusy)
					return;

				IsBusy = true;
				LoadFavoritesCommand.ChangeCanExecute ();
				FavoriteArticles.Clear ();
				var articles = new ObservableCollection<ArticleListing> {
					new ArticleListing {
						ArticleTitle = "Will this repo work?",
						AuthorString = "Ben Crispin",
						FormattedPostedDate = "7-28-2015"
					},
					new ArticleListing {
						ArticleTitle = "Xamarin Forms BugZilla",
						AuthorString = "Some Guy",
						FormattedPostedDate = "7-28-2015"
					}
				};
				var templist = new ObservableCollection<ArticleListing> ();
				foreach (var article in articles) {
					//templist.Add(article);
					FavoriteArticles.Add (article);
				}
				//FavoriteArticles = templist;
				OnPropertyChanged ("FavoriteArticles");
				IsBusy = false;
				LoadFavoritesCommand.ChangeCanExecute ();
			}

		}
	}

	public class BaseViewModelF : INotifyPropertyChanged
	{
		public BaseViewModelF ()
		{
		}

		string _title = string.Empty;
		public const string TitlePropertyName = "Title";

		/// <summary>
		/// Gets or sets the "Title" property
		/// </summary>
		/// <value>The title.</value>
		public string Title {
			get { return _title; }
			set { SetProperty (ref _title, value, TitlePropertyName); }
		}

		string _subTitle = string.Empty;
		/// <summary>
		/// Gets or sets the "Subtitle" property
		/// </summary>
		public const string SubtitlePropertyName = "Subtitle";

		public string Subtitle {
			get { return _subTitle; }
			set { SetProperty (ref _subTitle, value, SubtitlePropertyName); }
		}

		string _icon = null;
		/// <summary>
		/// Gets or sets the "Icon" of the viewmodel
		/// </summary>
		public const string IconPropertyName = "Icon";

		public string Icon {
			get { return _icon; }
			set { SetProperty (ref _icon, value, IconPropertyName); }
		}

		bool _isBusy;
		/// <summary>
		/// Gets or sets if the view is busy.
		/// </summary>
		public const string IsBusyPropertyName = "IsBusy";

		public bool IsBusy {
			get { return _isBusy; }
			set { SetProperty (ref _isBusy, value, IsBusyPropertyName); }
		}

		bool _canLoadMore = true;
		/// <summary>
		/// Gets or sets if we can load more.
		/// </summary>
		public const string CanLoadMorePropertyName = "CanLoadMore";

		public bool CanLoadMore {
			get { return _canLoadMore; }
			set { SetProperty (ref _canLoadMore, value, CanLoadMorePropertyName); }
		}

		protected void SetProperty<T> (
			ref T backingStore, T value,
			string propertyName,
			Action onChanged = null)
		{

			if (EqualityComparer<T>.Default.Equals (backingStore, value))
				return;

			backingStore = value;

			if (onChanged != null)
				onChanged ();

			OnPropertyChanged (propertyName);
		}

		#region INotifyPropertyChanged implementation

		public event PropertyChangedEventHandler PropertyChanged;

		#endregion

		public void OnPropertyChanged (string propertyName)
		{
			if (PropertyChanged == null)
				return;

			PropertyChanged (this, new PropertyChangedEventArgs (propertyName));
		}

	}

	[Preserve (AllMembers = true)]
	public class ArticleListing
	{
		public ArticleListing ()
		{
		}

		public string ArticleTitle { get; set; }

		public string ShortArticleTitle { get; set; }

		public string AuthorString { get; set; }

		public string FormattedPostedDate { get; set; }

		public string KickerName { get; set; }

		public string ArticleUrl { get; set; }
	}

	public class YearOloArticleList
	{
		public string Year { get; set; }

		public List<ArticleListing> ListArticleListing { get; set; }
	}
}

