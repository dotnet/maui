using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
#if UITEST
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.Github5000)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 4356, "[iOS] NSInternalInconsistencyException thrown when adding item to ListView after clearing bound ObservableCollection")]
	public partial class Issue4356 : TestContentPage
	{
#if UITEST
[Microsoft.Maui.Controls.Compatibility.UITests.FailsOnMauiAndroid]
[Microsoft.Maui.Controls.Compatibility.UITests.FailsOnMauiIOS]
		[Test]
		public void Issue4356Test()
		{
			RunningApp.WaitForElement(q => q.Marked("Will this repo work?"));
			RunningApp.WaitForElement(q => q.Marked("Remove item"));
			RunningApp.Tap(q => q.Marked("Remove item"));
			RunningApp.Tap(q => q.Marked("Remove item"));
			RunningApp.Tap(q => q.Marked("Add item"));
			RunningApp.WaitForElement(q => q.Marked("Added from Button Command"));
		}
#endif

		FavoritesViewModel ViewModel
		{
			get { return BindingContext as FavoritesViewModel; }
		}

		public Issue4356()
		{
#if APP
			InitializeComponent();
			BindingContext = new FavoritesViewModel();

			listView.SeparatorVisibility = SeparatorVisibility.Default;
			listView.SeparatorColor = Color.FromArgb("#ababab");

			listView.ItemTapped += (sender, args) =>
			{
				if (listView.SelectedItem == null)
					return;

				//do nothing anyway
				return;
			};
#endif

		}

		protected override void Init()
		{

		}

#pragma warning disable 1998 // considered for removal
		public async void OnDelete(object sender, EventArgs e)
#pragma warning restore 1998
		{
			var mi = ((MenuItem)sender);
			if (mi.CommandParameter == null)
				return;

			var articlelistingitem = mi.CommandParameter;

			if (articlelistingitem != null)
				await DisplayAlert("Alert", "I'm not deleting just refreshing...", "Ok");
			ViewModel.LoadFavoritesCommand.Execute(null);
		}


		protected override void OnAppearing()
		{
			base.OnAppearing();
			if (ViewModel == null || !ViewModel.CanLoadMore || ViewModel.IsBusy)
				return;

			Device.BeginInvokeOnMainThread(() =>
			{
				ViewModel.LoadFavoritesCommand.Execute(null);
			});
		}

		[Preserve(AllMembers = true)]
		public class FavoritesViewModel : BaseViewModelF
		{
			public ObservableCollection<ArticleListing> FavoriteArticles { get; set; }
			public int Count { get; private set; } = 0;
			readonly object _listLock = new object();

			public FavoritesViewModel()
			{
				Title = "Favorites";
				FavoriteArticles = new ObservableCollection<ArticleListing>();
				AddCommand = new Command(() =>
				{
					lock (_listLock)
					{
						FavoriteArticles.Add(new ArticleListing
						{
							ArticleTitle = "Added from Button Command",
							AuthorString = "Rui Marinho",
							FormattedPostedDate = "08-11-2018"
						});
					}
				});

				RemoveCommand = new Command(() =>
				{
					lock (_listLock)
					{
						if (FavoriteArticles.Count > 0)
						{
							FavoriteArticles.RemoveAt(FavoriteArticles.Count - 1);
							--Count;
						}
					}
				});

			}

			public Command AddCommand { get; }
			public Command RemoveCommand { get; }


			Command _loadFavoritesCommand;

			public Command LoadFavoritesCommand
			{
				get
				{
					return _loadFavoritesCommand ??
					(_loadFavoritesCommand = new Command(async () =>
					{
						await ExecuteFavoritesCommand();
					}, () =>
					{
						return !IsBusy;
					}));
				}
			}

#pragma warning disable 1998 // considered for removal
			public async Task ExecuteFavoritesCommand()
#pragma warning restore 1998
			{
				if (IsBusy)
					return;

				IsBusy = true;
				LoadFavoritesCommand.ChangeCanExecute();
				FavoriteArticles.Clear();
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
				var templist = new ObservableCollection<ArticleListing>();
				foreach (var article in articles)
				{
					//templist.Add(article);
					FavoriteArticles.Add(article);
				}
				//FavoriteArticles = templist;
				OnPropertyChanged("FavoriteArticles");
				IsBusy = false;
				LoadFavoritesCommand.ChangeCanExecute();
			}

		}
	}
}

