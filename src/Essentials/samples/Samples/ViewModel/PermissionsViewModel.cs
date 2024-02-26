using System.Collections.Generic;
using Microsoft.Maui.ApplicationModel;
using Samples.Model;

namespace Samples.ViewModel
{
	public class PermissionsViewModel : BaseViewModel
	{
		public List<PermissionItem> PermissionItems =>
			new List<PermissionItem>
			{
				new PermissionItem("Battery", new Permissions.Battery()),
				new PermissionItem("Calendar (Read)", new Permissions.CalendarRead()),
				new PermissionItem("Calendar (Write)", new Permissions.CalendarWrite()),
				new PermissionItem("Camera", new Permissions.Camera()),
				new PermissionItem("Contacts (Read)", new Permissions.ContactsRead()),
				new PermissionItem("Contacts (Write)", new Permissions.ContactsWrite()),
				new PermissionItem("Flashlight", new Permissions.Flashlight()),
				new PermissionItem("Launch Apps", new Permissions.LaunchApp()),
				new PermissionItem("Location (Always)", new Permissions.LocationAlways()),
				new PermissionItem("Location (Only When In Use)", new Permissions.LocationWhenInUse()),
				new PermissionItem("Maps", new Permissions.Maps()),
				new PermissionItem("Media Library", new Permissions.Media()),
				new PermissionItem("Microphone", new Permissions.Microphone()),
				new PermissionItem("Network State", new Permissions.NetworkState()),
				new PermissionItem("Phone", new Permissions.Phone()),
				new PermissionItem("Photos", new Permissions.Photos()),
				new PermissionItem("Photos AddOnly", new Permissions.PhotosAddOnly()),
				new PermissionItem("Post Notification", new Permissions.PostNotifications()),
				new PermissionItem("Reminders", new Permissions.Reminders()),
				new PermissionItem("Sensors", new Permissions.Sensors()),
				new PermissionItem("SMS", new Permissions.Sms()),
				new PermissionItem("Speech", new Permissions.Speech()),
				new PermissionItem("Storage (Read)", new Permissions.StorageRead()),
				new PermissionItem("Storage (Write)", new Permissions.StorageWrite()),
				new PermissionItem("Vibrate", new Permissions.Vibrate())
			};
	}
}
