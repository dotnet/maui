using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 30950, "Deleting Items Causes Other Expanded Items to Collapse Unexpectedly", PlatformAffected.iOS | PlatformAffected.macOS)]
	public partial class Issue30950 : ContentPage
	{
		public Issue30950()
		{
			InitializeComponent();
			BindingContext = new Issue30950ViewModel();
		}
	}

	public class Issue16094SwipeMessage
	{
		public string Title { get; set; }
		public string Description { get; set; }
	}

	public class Issue30950ViewModel : BindableObject
	{
		ObservableCollection<Issue16094SwipeMessage> _messages;

		public Issue30950ViewModel()
		{
			Messages = new ObservableCollection<Issue16094SwipeMessage>();
			LoadMessages();
		}

		public ObservableCollection<Issue16094SwipeMessage> Messages
		{
			get { return _messages; }
			set
			{
				_messages = value;
				OnPropertyChanged();
			}
		}

		public ICommand FavouriteCommand => new Command<Issue16094SwipeMessage>(OnFavourite);
		public ICommand DeleteCommand => new Command<Issue16094SwipeMessage>(OnDelete);

		void LoadMessages()
		{
			for (int i = 0; i < 10; i++)
			{
				Messages!.Add(new Issue16094SwipeMessage
				{
					Title = $"Item {i + 1}",
					Description = $"Description for item {i + 1}. Swipe left for favorite, right for delete."
				});
			}
		}

		void OnFavourite(Issue16094SwipeMessage message)
		{
			Application.Current?.MainPage?.DisplayAlert("Issue30950Alert", $"Favourited: {message?.Title}", "OK");
		}

		void OnDelete(Issue16094SwipeMessage message)
		{
			if (message != null && Messages != null)
			{
				Messages.Remove(message);
				
				Application.Current?.MainPage?.DisplayAlert("Issue30950Alert",
					$"Deleted: {message?.Title}. Other expanded swipe items should remain open.", "OK");
			}
		}
	}
}