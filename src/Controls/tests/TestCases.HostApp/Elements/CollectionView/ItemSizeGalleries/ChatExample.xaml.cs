using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.CollectionViewGalleries.ItemSizeGalleries
{
	public partial class ChatExample : ContentPage
	{
		Random _random = new Random(DateTime.Now.Millisecond);
		ChatExampleViewModel _vm = new ChatExampleViewModel();

		public ChatExample()
		{
			InitializeComponent();
			BindingContext = _vm;

			AppendRandomSizedItem.Clicked += AppendRandomChatMessage;
			Clear.Clicked += ClearMessages;
			Lots.Clicked += LotsOfMessages;
		}

		void AppendRandomChatMessage(object sender, EventArgs e)
		{
			_vm.ChatMessages.Add(GenerateRandomMessage());
		}

		void LotsOfMessages(object sender, EventArgs e)
		{
			var newVm = new ChatExampleViewModel(GenerateMessages(1000));
			_vm = newVm;
			BindingContext = _vm;
		}

		void ClearMessages(object sender, EventArgs e)
		{
			_vm.ChatMessages.Clear();
		}

		ChatMessage GenerateRandomMessage()
		{
			const string lorem = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut "
				+ "labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip "
				+ "ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat "
				+ "nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.";

			var local = _random.Next(0, 2) == 1;

			var textLength = _random.Next(0, lorem.Length - 1);

			var text = lorem.Substring(0, textLength);

			return new ChatMessage(text, local);
		}

		IEnumerable<ChatMessage> GenerateMessages(int count)
		{
			for (int n = 0; n < count; n++)
			{
				yield return GenerateRandomMessage();
			}
		}
	}

	public class ChatExampleViewModel
	{
		public ObservableCollection<ChatMessage> ChatMessages { get; }

		public ChatExampleViewModel()
		{
			ChatMessages = new ObservableCollection<ChatMessage>();
		}

		public ChatExampleViewModel(IEnumerable<ChatMessage> chatMessages)
		{
			ChatMessages = new ObservableCollection<ChatMessage>(chatMessages);
		}
	}

	public class ChatMessage
	{
		public bool IsLocal { get; set; }
		public string Text { get; set; }

		public ChatMessage(string text) : this(text, true) { }

		public ChatMessage(string text, bool isLocal)
		{
			IsLocal = isLocal;
			Text = text;
		}
	}

	class ChatTemplateSelector : DataTemplateSelector
	{
		public DataTemplate LocalTemplate { get; set; }
		public DataTemplate RemoteTemplate { get; set; }

		protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
		{
			if (item is ChatMessage message)
			{
				if (message.IsLocal)
				{
					return LocalTemplate!;
				}

				return RemoteTemplate!;
			}

			throw new ArgumentOutOfRangeException();
		}
	}
}