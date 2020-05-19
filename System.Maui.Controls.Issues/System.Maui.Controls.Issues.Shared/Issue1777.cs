using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.UITest;
using Xamarin.UITest.Queries;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve (AllMembers=true)]
	[Issue (IssueTracker.Github, 1777, "Adding picker items when picker is in a ViewCell breaks", PlatformAffected.WinPhone)]
	public class Issue1777 : TestContentPage
	{
		Picker _pickerTable = null;
		Picker _pickerNormal = null;
		string _pickerTableId = "pickerTableId";
		string _btnText = "do magic";

		protected override void Init ()
		{
			StackLayout stackLayout = new StackLayout();
			Content = stackLayout;

			var instructions = new Label
			{
				Text = $@"Tap the ""{_btnText}"" button. Then click on the picker inside the Table. The picker should display ""test 0"". If not, the test failed."
			};

			stackLayout.Children.Add(instructions);

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
			_pickerTable.AutomationId = _pickerTableId;
			_pickerTable.HorizontalOptions = LayoutOptions.FillAndExpand;
			contentView.Content = _pickerTable;

			Label label = new Label ();
			label.Text = "Normal";
			stackLayout.Children.Add (label);

			_pickerNormal = new Picker ();
			stackLayout.Children.Add (_pickerNormal);

			Button button = new Button ();
			button.Clicked += button_Clicked;
			button.Text = _btnText;
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

#if UITEST && __WINDOWS__
		[Test]
		public void Issue1777Test()
		{
			RunningApp.WaitForElement(q => q.Button(_btnText));
			RunningApp.Tap(q => q.Button(_btnText));
			RunningApp.Tap(q => q.Marked(_pickerTableId));
			RunningApp.WaitForElement(q => q.Marked("test 0"));
			RunningApp.Screenshot("Picker is displayed correctly in the ViewCell");
		}
#endif
    }
}
