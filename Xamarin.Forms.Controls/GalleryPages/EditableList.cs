using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Linq;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls
{
	internal class MessagesViewModel : ViewModelBase
	{
		public MessagesViewModel(int messagesCount)
		{
			Messages = new ObservableCollection<MessageViewModel> (Enumerable.Range (0, messagesCount).Select (i => {
				return new MessageViewModel { Subject = "Subject Line " + i, MessagePreview = "Lorem ipsum dolorem monkeys bonkers " + i };
			}));

			MessagingCenter.Subscribe<MessageViewModel, MessageViewModel> (this, "DeleteMessage", (vm, vm2) => {
				Messages.Remove (vm);
			});
		}

		public ObservableCollection<MessageViewModel> Messages
		{
			get;
			private set;
		}
	}

	[Preserve (AllMembers = true)]
	public class MessageViewModel : ViewModelBase
	{
		public MessageViewModel()
		{
			Delete = new Command (() => MessagingCenter.Send (this, "DeleteMessage", this));
			Move = new Command (() => MessagingCenter.Send (this, "MoveMessage", this));
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
				this.SetBinding (TextProperty, "Subject");
				this.SetBinding (DetailProperty, "MessagePreview");

				var delete = new MenuItem { Text = "Delete", IsDestructive = true };
				delete.SetBinding (MenuItem.CommandProperty, "Delete");

				var mark = new MenuItem { Text = "Mark",  Icon = "calculator.png" };
				var move = new MenuItem { Text = "Move" };

				//move.Clicked += async (sender, e) => await Navigation.PopAsync();

				ContextActions.Add (mark);
				ContextActions.Add (delete);
				ContextActions.Add (move);

				var clear = new MenuItem { Text = "Clear Items" };
				clear.Clicked += (sender, args) => ContextActions.Clear();
				ContextActions.Add (clear);
			}
		}

		public ContextActionsGallery (bool tableView = false, bool hasUnevenRows = false, int messagesCount = 100)
		{
			BindingContext = new MessagesViewModel(messagesCount);

			View list;
			if (!tableView) {
				list = new ListView
				{
					HasUnevenRows = hasUnevenRows
				};
				list.SetBinding (ListView.ItemsSourceProperty, "Messages");
				((ListView)list).ItemTemplate = new DataTemplate (typeof (MessageCell));
			} else {
				var section = new TableSection();
				section.Add (new TextCell { Text = "I have no ContextActions", Detail = "Sup" });
				foreach (var msg in ((MessagesViewModel) BindingContext).Messages) {
					section.Add (new MessageCell { BindingContext = msg });
				}

				list = new TableView();
				((TableView)list).Root = new TableRoot { section };
			}

			Content = new StackLayout {
				Children = {
					new Label { Text = "Email" },
					list
				}
			};
		}
	}
}
