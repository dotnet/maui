using System.Collections.ObjectModel;
using System.Windows.Input;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 4314, "When ListView items is removed and it is empty, Xamarin Forms crash", PlatformAffected.iOS)]
public class Issue4314 : TestNavigationPage
{
	const string Success = "Success";

	MessagesViewModel viewModel;
	protected override void Init()
	{
		var page = new ContextActionsGallery(false, true, 2) { Title = "Swipe and delete both" };
		viewModel = page.BindingContext as MessagesViewModel;
		viewModel.Messages.CollectionChanged += (s, e) =>
		{
			if (viewModel.Messages.Count == 0)
			{
				Navigation.PushAsync(new ContentPage { Title = "Success", Content = new Label { Text = Success } });
			}
		};
		Navigation.PushAsync(page);
	}
}

internal class MessagesViewModel : ViewModelBase
{
	public MessagesViewModel(int messagesCount)
	{
		Messages = new ObservableCollection<MessageViewModel>(Enumerable.Range(0, messagesCount).Select(i =>
		{
			return new MessageViewModel { Subject = "Subject Line " + i, MessagePreview = "Lorem ipsum dolorem monkeys bonkers " + i };
		}));

		MessagingCenter.Subscribe<MessageViewModel, MessageViewModel>(this, "DeleteMessage", (vm, vm2) =>
		{
			Messages.Remove(vm);
		});
	}

	public ObservableCollection<MessageViewModel> Messages
	{
		get;
		private set;
	}
}

[Preserve(AllMembers = true)]
public class MessageViewModel : ViewModelBase
{
	public MessageViewModel()
	{
		Delete = new Command(() => MessagingCenter.Send(this, "DeleteMessage", this));
		Move = new Command(() => MessagingCenter.Send(this, "MoveMessage", this));
	}

	public string Subject
	{
		get;
		set;
	}

	public string MessagePreview
	{
		get;
		set;
	}

	public ICommand Delete
	{
		get;
		private set;
	}

	public ICommand Move
	{
		get;
		private set;
	}
}

internal class ContextActionsGallery : ContentPage
{
	class MessageCell : TextCell
	{
		public MessageCell()
		{
			this.SetBinding(TextProperty, "Subject");
			this.SetBinding(DetailProperty, "MessagePreview");

			var delete = new MenuItem { Text = "Delete", IsDestructive = true };
			delete.SetBinding(MenuItem.CommandProperty, "Delete");

			var mark = new MenuItem { Text = "Mark", IconImageSource = "calculator.png" };
			var move = new MenuItem { Text = "Move" };

			//move.Clicked += async (sender, e) => await Navigation.PopAsync();

			ContextActions.Add(mark);
			ContextActions.Add(delete);
			ContextActions.Add(move);

			var clear = new MenuItem { Text = "Clear Items" };
			clear.Clicked += (sender, args) => ContextActions.Clear();
			ContextActions.Add(clear);
		}
	}

	public ContextActionsGallery(bool tableView = false, bool hasUnevenRows = false, int messagesCount = 100)
	{
		BindingContext = new MessagesViewModel(messagesCount);

		View list;
		if (!tableView)
		{
			list = new ListView
			{
				HasUnevenRows = hasUnevenRows
			};
			list.SetBinding(ListView.ItemsSourceProperty, "Messages");
			((ListView)list).ItemTemplate = new DataTemplate(typeof(MessageCell));
		}
		else
		{
			var section = new TableSection();
			section.Add(new TextCell { Text = "I have no ContextActions", Detail = "Sup" });
			foreach (var msg in ((MessagesViewModel)BindingContext).Messages)
			{
				section.Add(new MessageCell { BindingContext = msg });
			}

			list = new TableView();
			((TableView)list).Root = new TableRoot { section };
		}

		Content = new StackLayout
		{
			Children = {
				new Label { Text = "Email" },
				list
			}
		};
	}
}