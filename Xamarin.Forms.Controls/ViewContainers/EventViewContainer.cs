using System;

namespace Xamarin.Forms.Controls
{
	internal class EventViewContainer<T> : ViewContainer<T>
		where T : View
	{
		string _formsMember;
		Label _eventLabel;
		int _numberOfTimesFired;

		public EventViewContainer(Enum formsMember, T view) : base(formsMember, view)
		{
			_numberOfTimesFired = 0;

			_formsMember = formsMember.ToString();

			_eventLabel = new Label
			{
				AutomationId = formsMember + "EventLabel",
				Text = "Event: " + _formsMember + " (none)"
			};

			ContainerLayout.Children.Add(_eventLabel);
		}

		public void EventFired()
		{
			_numberOfTimesFired++;
			_eventLabel.Text = "Event: " + _formsMember + " (fired " + _numberOfTimesFired + ")";
		}
	}
}