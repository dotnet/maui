//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Android.App;
//using Android.Content;
//using Android.Content.PM;
//using Android.OS;
//using Android.Renderscripts;
//using Android.Runtime;
//using Android.Views;
//using Android.Widget;
//using Microsoft.Maui.Controls.ControlGallery.Issues;

using System;

namespace Microsoft.Maui.Controls.ControlGallery
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