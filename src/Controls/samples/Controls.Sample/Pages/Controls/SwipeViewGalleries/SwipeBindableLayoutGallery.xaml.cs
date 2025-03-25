using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Xaml;

namespace Maui.Controls.Sample.Pages.SwipeViewGalleries
{
	[Preserve(AllMembers = true)]
	[XamlCompilation(XamlCompilationOptions.Skip)]
	public partial class SwipeBindableLayoutGallery : ContentPage
	{
		public SwipeBindableLayoutGallery()
		{
			InitializeComponent();
			BindingContext = new SwipeViewGalleryViewModel();

			WeakReferenceMessenger.Default.Register<SwipeViewGalleryViewModel, string>(this, "favourite", (_, sender) => { DisplayAlertAsync("SwipeView", "Favourite", "Ok"); });
			WeakReferenceMessenger.Default.Register<SwipeViewGalleryViewModel, string>(this, "delete", (_, sender) => { DisplayAlertAsync("SwipeView", "Delete", "Ok"); });
		}
	}

	[Preserve(AllMembers = true)]
	public class Message
	{
		public string? Title { get; set; }
		public string? SubTitle { get; set; }
		public string? Description { get; set; }
		public string? Date { get; set; }
	}

	[Preserve(AllMembers = true)]
	public class SwipeViewGalleryViewModel : BindableObject
	{
		ObservableCollection<Message>? _messages;

		public SwipeViewGalleryViewModel()
		{
			Messages = new ObservableCollection<Message>();
			LoadMessages();
		}

		public ObservableCollection<Message>? Messages
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
				Messages!.Add(new Message { Title = $"Lorem ipsum {i + 1}", SubTitle = "Lorem ipsum dolor sit amet", Date = "Yesterday", Description = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua." });
			}
		}

		void OnFavourite()
		{
			WeakReferenceMessenger.Default.Send(this, "favourite");
		}

		void OnDelete()
		{
			WeakReferenceMessenger.Default.Send(this, "delete");
		}

		void OnTap()
		{
			WeakReferenceMessenger.Default.Send(this, "tap");
		}
	}
}