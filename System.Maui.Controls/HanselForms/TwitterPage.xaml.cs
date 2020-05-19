using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Xamarin.Forms.Controls
{
	public partial class TwitterPage : ContentPage
	{

		private TwitterViewModel ViewModel
		{
			get { return BindingContext as TwitterViewModel; }
		}
		public TwitterPage()
		{
			InitializeComponent();
			BindingContext = new TwitterViewModel();


			listView.ItemTapped += (sender, args) =>
			{
				if (listView.SelectedItem == null)
					return;
				var tweet = listView.SelectedItem as Tweet;
				this.Navigation.PushAsync(new WebsiteView("http://m.twitter.com/shanselman/status/" + tweet.StatusID, tweet.Date));
				listView.SelectedItem = null;
			};
		}


		protected override void OnAppearing()
		{
			base.OnAppearing();
			if (ViewModel == null || !ViewModel.CanLoadMore || ViewModel.IsBusy || ViewModel.Tweets.Count > 0)
				return;

			ViewModel.LoadTweetsCommand.Execute(null);
		}
	}

	public class TwitterViewModel : HBaseViewModel
	{

		public ObservableCollection<Tweet> Tweets { get; set; }

		public TwitterViewModel()
		{
			Title = "Twitter";
			Icon = "slideout.png";
			Tweets = new ObservableCollection<Tweet>();

		}

		private Command loadTweetsCommand;

		public Command LoadTweetsCommand
		{
			get
			{
				return loadTweetsCommand ??
				  (loadTweetsCommand = new Command(async () =>
				  {
					  await ExecuteLoadTweetsCommand();
				  }, () =>
				  {
					  return !IsBusy;
				  }));
			}
		}

		public async Task ExecuteLoadTweetsCommand()
		{
			if (IsBusy)
				return;

			IsBusy = true;
			LoadTweetsCommand.ChangeCanExecute();
			var error = false;
			try
			{

				Tweets.Clear();
				//var auth = new ApplicationOnlyAuthorizer()
				//{
				//	CredentialStore = new InMemoryCredentialStore
				//	{
				//		ConsumerKey = "ZTmEODUCChOhLXO4lnUCEbH2I",
				//		ConsumerSecret = "Y8z2Wouc5ckFb1a0wjUDT9KAI6DUat5tFNdmIkPLl8T4Nyaa2J",
				//	},
				//};
				//await auth.AuthorizeAsync();

				//var twitterContext = new TwitterContext(auth);

				//var queryResponse = await
				//  (from tweet in twitterContext.Status
				//   where tweet.Type == StatusType.User &&
				//	 tweet.ScreenName == "shanselman" &&
				//	 tweet.Count == 100 &&
				//	 tweet.IncludeRetweets == true &&
				//	 tweet.ExcludeReplies == true
				//   select tweet).ToListAsync();

				//var tweets =
				//  (from tweet in queryResponse
				//   select new Tweet
				//   {
				//	   StatusID = tweet.StatusID,
				//	   ScreenName = tweet.User.ScreenNameResponse,
				//	   Text = tweet.Text,
				//	   CurrentUserRetweet = tweet.CurrentUserRetweet,
				//	   CreatedAt = tweet.CreatedAt,
				//	   Image = tweet.RetweetedStatus != null && tweet.RetweetedStatus.User != null ?
				//					  tweet.RetweetedStatus.User.ProfileImageUrl.Replace("http://", "https://") : (tweet.User.ScreenNameResponse == "shanselman" ? "scott159.png" : tweet.User.ProfileImageUrl.Replace("http://", "https://"))
				//   }).ToList();
				//foreach (var tweet in tweets)
				//{
				//	Tweets.Add(tweet);
				//}

				//if (Device.OS == TargetPlatform.iOS)
				//{
				//	// only does anything on iOS, for the Watch
				//	//	DependencyService.Get<ITweetStore>().Save(tweets);
				//}



			}
			catch
			{
				error = true;
			}

			if (error)
			{
				var page = new ContentPage();
				await page.DisplayAlert("Error", "Unable to load tweets.", "OK");
			}

			IsBusy = false;
			LoadTweetsCommand.ChangeCanExecute();
		}
	}

	public class Tweet
	{
		public Tweet()
		{
		}


		public ulong StatusID { get; set; }

		public string ScreenName { get; set; }

		public string Text { get; set; }

		//[JsonIgnore]
		public string Date { get { return CreatedAt.ToString("g"); } }
		//[JsonIgnore]
		public string RTCount { get { return CurrentUserRetweet == 0 ? string.Empty : CurrentUserRetweet + " RT"; } }

		public string Image { get; set; }

		public DateTime CreatedAt
		{
			get;
			set;
		}

		public ulong CurrentUserRetweet
		{
			get;
			set;
		}
	}

	public interface ITweetStore
	{
		void Save(System.Collections.Generic.List<Tweet> tweets);
		//System.Collections.Generic.List<Hanselman.Shared.Tweet> Load ();
	}
}
