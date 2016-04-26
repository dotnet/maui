using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.TestCasesPages
{
	[Preserve (AllMembers=true)]
	[Issue (IssueTracker.Github, 1777, "Adding picker items when picker is in a ViewCell breaks", PlatformAffected.WinPhone)]
	public class Issue1777 : ContentPage
	{
		Picker _pickerTable = null;
		Picker _pickerNormal = null;

		public Issue1777 ()
		{
			StackLayout stackLayout = new StackLayout();
			Content = stackLayout;

			TableView tableView = new TableView();
			stackLayout.Children.Add( tableView);

			TableRoot tableRoot = new TableRoot();
			tableView.Root = tableRoot;

			TableSection tableSection = new TableSection("Table");
			tableRoot.Add(tableSection);

			ViewCell viewCell = new ViewCell ();
			tableSection.Add (viewCell);

			ContentView contentView = new ContentView ();
			contentView.HorizontalOptions = LayoutOptions.FillAndExpand;
			viewCell.View = contentView;

			_pickerTable = new Picker ();
			_pickerTable.HorizontalOptions = LayoutOptions.FillAndExpand;
			contentView.Content = _pickerTable;

			Label label = new Label ();
			label.Text = "Normal";
			stackLayout.Children.Add (label);

			_pickerNormal = new Picker ();
			stackLayout.Children.Add (_pickerNormal);

			Button button = new Button ();
			button.Clicked += button_Clicked;
			button.Text = "do magic";
			stackLayout.Children.Add (button);

			//button_Clicked(button, EventArgs.Empty);
			_pickerTable.SelectedIndex = 0;
			_pickerNormal.SelectedIndex = 0;
		}

		void button_Clicked (object sender, EventArgs e)
		{
			_pickerTable.Items.Add ("test " + _pickerTable.Items.Count);
			_pickerNormal.Items.Add ("test " + _pickerNormal.Items.Count);
		}
	}
}
