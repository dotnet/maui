using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Linq;
using Xamarin.Forms;

namespace Xamarin.Forms.Controls
{
	public partial class BlogPage : ContentPage
	{

		private BlogFeedViewModel ViewModel
		{
			get { return BindingContext as BlogFeedViewModel; }
		}

		public BlogPage()
		{
			InitializeComponent();
			BindingContext = new BlogFeedViewModel();

			listView.ItemTapped += (sender, args) =>
			{
				if (listView.SelectedItem == null)
					return;
				this.Navigation.PushAsync(new BlogDetailsView(listView.SelectedItem as FeedItem));
				listView.SelectedItem = null;
			};
		}

		protected override void OnAppearing()
		{
			base.OnAppearing();
			if (ViewModel == null || !ViewModel.CanLoadMore || ViewModel.IsBusy || ViewModel.FeedItems.Count > 0)
				return;

			ViewModel.LoadItemsCommand.Execute(null);
		}
	}


	public class BlogDetailsView : BaseView
	{
		public BlogDetailsView(FeedItem item)
		{
			BindingContext = item;
			var webView = new WebView
			{
				VerticalOptions = LayoutOptions.FillAndExpand,
				HorizontalOptions = LayoutOptions.FillAndExpand
			};
			webView.Source = new HtmlWebViewSource
			{
				Html = item.Description
			};
			Content = new StackLayout
			{
				Children =
		{
		  webView
		}
			};
			var share = new ToolbarItem
			{
				Icon = "ic_share.png",
				Text = "Share",
				//Command = new Command(() => CrossShare.Current
				//  .Share("Be sure to read @shanselman's " + item.Title + " " + item.Link))
			};

			ToolbarItems.Add(share);
		}
	}


	public class BlogFeedViewModel : HBaseViewModel
	{
		public BlogFeedViewModel()
		{
			Title = "Blog";
			Icon = "blog.png";
		}

		private ObservableCollection<FeedItem> feedItems = new ObservableCollection<FeedItem>();

		/// <summary>
		/// gets or sets the feed items
		/// </summary>
		public ObservableCollection<FeedItem> FeedItems
		{
			get { return feedItems; }
			set { feedItems = value; OnPropertyChanged(); }
		}

		private FeedItem selectedFeedItem;
		/// <summary>
		/// Gets or sets the selected feed item
		/// </summary>
		public FeedItem SelectedFeedItem
		{
			get { return selectedFeedItem; }
			set
			{
				selectedFeedItem = value;
				OnPropertyChanged();
			}
		}

		private Command loadItemsCommand;
		/// <summary>
		/// Command to load/refresh items
		/// </summary>
		public Command LoadItemsCommand
		{
			get { return loadItemsCommand ?? (loadItemsCommand = new Command(async () => await ExecuteLoadItemsCommand())); }
		}

		private async Task ExecuteLoadItemsCommand()
		{
			if (IsBusy)
				return;

			IsBusy = true;
			var error = false;
			try
			{
				var responseString = string.Empty;
				//TODO: get feed and 
				//using (var httpClient = new HttpClient())
				//{
				//	var feed = "http://feeds.hanselman.com/ScottHanselman";
				//	responseString = await httpClient.GetStringAsync(feed);
				//}

				FeedItems.Clear();
				var items = await ParseFeed(responseString);
				foreach (var item in items)
				{
					FeedItems.Add(item);
				}
			}
			catch
			{
				error = true;
			}

			if (error)
			{
				var page = new ContentPage();
				await page.DisplayAlert("Error", "Unable to load blog.", "OK");

			}

			IsBusy = false;
		}



		/// <summary>
		/// Parse the RSS Feed
		/// </summary>
		/// <param name="rss"></param>
		/// <returns></returns>
		private async Task<List<FeedItem>> ParseFeed(string rss)
		{
			return await Task.Run(() =>
				{
					var xdoc = XDocument.Parse(rss);
					var id = 0;
					return (from item in xdoc.Descendants("item")
							select new FeedItem
							{
								Title = (string)item.Element("title"),
								Description = (string)item.Element("description"),
								Link = (string)item.Element("link"),
								PublishDate = (string)item.Element("pubDate"),
								Category = (string)item.Element("category"),
								Id = id++
							}).ToList();
				});
		}

		/// <summary>
		/// Gets a specific feed item for an Id
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public FeedItem GetFeedItem(int id)
		{
			return FeedItems.FirstOrDefault(i => i.Id == id);
		}
	}

	public class FeedItem : INotifyPropertyChanged
	{



		public string Description { get; set; }
		public string Link { get; set; }
		private string publishDate;
		public string PublishDate
		{
			get { return publishDate; }
			set
			{
				DateTime time;
				if (DateTime.TryParse(value, out time))
					publishDate = time.ToLocalTime().ToString("D");
				else
					publishDate = value;
			}
		}
		public string Author { get; set; }
		public string AuthorEmail { get; set; }
		public int Id { get; set; }
		public string CommentCount { get; set; }
		public string Category { get; set; }

		public string Mp3Url { get; set; }

		private string title;
		public string Title
		{
			get
			{
				return title;
			}
			set
			{
				title = value;

			}
		}

		private string caption;

		public string Caption
		{
			get
			{
				if (!string.IsNullOrWhiteSpace(caption))
					return caption;


				//get rid of HTML tags
				caption = Regex.Replace(Description, "<[^>]*>", string.Empty);


				//get rid of multiple blank lines
				caption = Regex.Replace(caption, @"^\s*$\n", string.Empty, RegexOptions.Multiline);

				caption = caption.Substring(0, caption.Length < 200 ? caption.Length : 200).Trim() + "...";
				return caption;
			}
		}

		public string Length { get; set; }

		private bool showImage = true;

		public bool ShowImage
		{
			get { return showImage; }
			set { showImage = value; }
		}

		private string image = @"https://secure.gravatar.com/avatar/70148d964bb389d42547834e1062c886?s=60&r=x&d=http%3a%2f%2fd1iqk4d73cu9hh.cloudfront.net%2fcomponents%2fimg%2fuser-icon.png";

		/// <summary>
		/// When we set the image, mark show image as true
		/// </summary>
		public string Image
		{
			get { return image; }
			set
			{
				image = value;
				showImage = true;
			}

		}

		private string firstImage;
		public string FirstImage
		{
			get
			{
				if (!string.IsNullOrWhiteSpace(firstImage))
					return firstImage;


				var regx = new Regex("http://([\\w+?\\.\\w+])+([a-zA-Z0-9\\~\\!\\@\\#\\$\\%\\^\\&amp;\\*\\(\\)_\\-\\=\\+\\\\\\/\\?\\.\\:\\;\\'\\,]*)?.(?:jpg|bmp|gif|png)", RegexOptions.IgnoreCase);
				var matches = regx.Matches(Description);

				if (matches.Count == 0)
					firstImage = ScottHead;
				else
					firstImage = matches[0].Value;

				return firstImage;
			}
		}

		public ImageSource FirstImageSource
		{
			get
			{
				var image = FirstImage;
				return UriImageSource.FromUri(new Uri(image));
			}
		}

		public string ScottHead { get { return "http://www.hanselman.com/images/photo-scott-tall.jpg"; } }

		private decimal progress = 0.0M;
		public decimal Progress
		{
			get { return progress; }
			set { progress = value; OnPropertyChanged("Progress"); }
		}


		public event PropertyChangedEventHandler PropertyChanged;
		private void OnPropertyChanged(string name)
		{
			if (PropertyChanged == null)
				return;
			PropertyChanged(this, new PropertyChangedEventArgs(name));
		}
	}
}
