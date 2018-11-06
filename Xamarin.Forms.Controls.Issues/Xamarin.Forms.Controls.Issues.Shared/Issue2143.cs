using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve (AllMembers=true)]
	[Issue (IssueTracker.Github, 2143, "Picker on windows phone",PlatformAffected.WinPhone)]
	public class Issue2143 : ContentPage
	{
		public Issue2143 ()
		{
			var table = GetTableView ();
			var list = GetListView ();
			Content = list;
		}

		static ListView GetListView ()
		{
			var listView = new ListView ();
			listView.ItemTemplate = new DataTemplate (typeof (PickerCell));
			listView.ItemsSource = new[] { "one", "two", "three" }; ;
			return listView;
		}

		static TableView GetTableView ()
		{
			var tableSection = new TableSection ("Picker");
			tableSection.Add (new PickerCell ());
			var root = new TableRoot ("Root");
			root.Add (tableSection);
			var table = new TableView (root);
			return table;
		}
	}

	internal class PickerCell : ViewCell
	{
		public PickerCell ()
		{
			var picker = new Picker { Title ="Select Level of Activity." };
			picker.Items.Add ("Sedentary");
			picker.Items.Add ("Moderate");
			picker.Items.Add ("Active");
			picker.Items.Add ("None");
			picker.SelectedIndex = 0;
			View = picker;
		}
	}

	
}
