using System.Collections.ObjectModel;
using System.Windows.Input;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Xaml;

namespace Microsoft.Maui.Controls.ControlGallery.GalleryPages.SwipeViewGalleries
{
	[Preserve(AllMembers = true)]
	[XamlCompilation(XamlCompilationOptions.Skip)]
	public partial class SwipeBindableLayoutGallery : ContentPage
	{
		public SwipeBindableLayoutGallery()
		{
			InitializeComponent();
			BindingContext = new SwipeViewGalleryViewModel();

			MessagingCenter.Subscribe<SwipeViewGalleryViewModel>(this, "favourite", sender => { DisplayAlert("SwipeView", "Favourite", "Ok"); });
			MessagingCenter.Subscribe<SwipeViewGalleryViewModel>(this, "delete", sender => { DisplayAlert("SwipeView", "Delete", "Ok"); });
		}
	}

	[Preserve(AllMembers = true)]
	public class Message
	{
		public string Title { get; set; }
		public string SubTitle { get; set; }
		public string Description { get; set; }
		public string Date { get; set; }
	}

	[Preserve(AllMembers = true)]
	public class SwipeViewGalleryViewModel : BindableObject
	{
		ObservableCollection<Message> _messages;

		public SwipeViewGalleryViewModel()
		{
			Messages = new ObservableCollection<Message>();
			LoadMessages();
		}

		public ObservableCollection<Message> Messages
		{
			get { return _messages; }
			set
			{
				_messages = value;
				OnPropertyChanged();
			}
		}

		public ICommand FavouriteCommand => new Command(OnFavourite);
		public ICommand DeleteCommand => new Command(OnDelete);
		public ICommand TapCommand => new Command(OnTap);

		void LoadMessages()
		{
			for (int i = 0; i < 100; i++)
			{
				Messages.Add(new Message { Title = $"Lorem ipsum {i + 1}", SubTitle = "Lorem ipsum dolor sit amet", Date = "Yesterday", Description = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua." });
			}
		}

		void OnFavourite()
		{
			MessagingCenter.Send(this, "favourite");
		}

		void OnDelete()
		{
			MessagingCenter.Send(this, "delete");
		}

		void OnTap()
		{
			MessagingCenter.Send(this, "tap");
		}
	}
}