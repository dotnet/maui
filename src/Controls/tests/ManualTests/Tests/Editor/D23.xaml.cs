using System.Collections.ObjectModel;
using Microsoft.Maui.ManualTests.Categories;

namespace Microsoft.Maui.ManualTests.Tests.RefreshView;

[Test(
	id: "D23",
	title: "Screen is partially hidden when showing the virtual keyboard.",
	category: Category.Editor)]
public partial class D23 : ContentPage
{
	readonly ObservableCollection<string> _messages = new();

	int _counter = 0;

	public D23()
	{
		InitializeComponent();

		entryMessage.Completed += async (s, e) => await SendMessageAsync();
		listViewMessages.ItemsSource = _messages;
	}

	async Task SendMessageAsync()
	{
		// simulates sending messages
		await Task.Delay(200);

		AppendMessage($"{DateTime.Now:HH:mm:ss}: {++_counter} {Message}");

		Message = string.Empty;
	}

	private void AppendMessage(string message)
	{
		_messages.Add(message);
		listViewMessages.ScrollTo(_messages.Last(), ScrollToPosition.End);
	}

	string Message
	{
		get => entryMessage.Text;
		set => entryMessage.Text = value;
	}
}
