using System;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class ParentButton : Button
{
	public ParentButton() => InitializeComponent();

	EventHandler _myEvent;

	public event EventHandler MyEvent
	{
		add => _myEvent += value;
		remove => _myEvent -= value;
	}

	public int MyEventSubscriberCount => _myEvent.GetInvocationList().Length;

	private void ParentButton_OnClicked(object sender, EventArgs e) { }

	private void ParentButton_OnMyEvent(object sender, EventArgs e) { }
}