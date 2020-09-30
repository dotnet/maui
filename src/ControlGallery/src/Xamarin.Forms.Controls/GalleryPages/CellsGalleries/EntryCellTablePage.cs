namespace Xamarin.Forms.Controls
{
	public class EntryCellTablePage : ContentPage
	{
		public EntryCellTablePage()
		{
			Title = "EntryCell Table Gallery - Legacy";

			if (Device.RuntimePlatform == Device.iOS && Device.Idiom == TargetIdiom.Tablet)
				Padding = new Thickness(0, 0, 0, 60);

			int timesEntered = 1;

			var entryCell = new EntryCell { Label = "Enter text", Placeholder = "I am a placeholder" };
			entryCell.Completed += (sender, args) =>
			{
				((EntryCell)sender).Label = "Entered: " + timesEntered;
				timesEntered++;
			};

			var tableSection = new TableSection("Section One") {
				new EntryCell { Label = "disabled", Placeholder = "disabled", IsEnabled = false },
				new EntryCell { Label = "Text 2" },
				new EntryCell { Label = "Text 3", Placeholder = "Placeholder 2" },
				new EntryCell { Label = "Text 4" },
				new EntryCell { Label = "Text 5", Placeholder = "Placeholder 3" },
				new EntryCell { Label = "Text 6" },
				new EntryCell { Label = "Text 7", Placeholder = "Placeholder 4" },
				new EntryCell { Label = "Text 8" },
				new EntryCell { Label = "Text 9", Placeholder = "Placeholder 5" },
				new EntryCell { Label = "Text 10" },
				new EntryCell { Label = "Text 11", Placeholder = "Placeholder 6" },
				new EntryCell { Label = "Text 12" },
				new EntryCell { Label = "Text 13", Placeholder = "Placeholder 7" },
				new EntryCell { Label = "Text 14" },
				new EntryCell { Label = "Text 15", Placeholder = "Placeholder 8" },
				new EntryCell { Label = "Text 16" },
				entryCell
			};

			var tableSectionTwo = new TableSection("Section Two") {
				new EntryCell { Label = "Text 17", Placeholder = "Placeholder 9" },
				new EntryCell { Label = "Text 18" },
				new EntryCell { Label = "Text 19", Placeholder = "Placeholder 10" },
				new EntryCell { Label = "Text 20" },
				new EntryCell { Label = "Text 21", Placeholder = "Placeholder 11" },
				new EntryCell { Label = "Text 22" },
				new EntryCell { Label = "Text 23", Placeholder = "Placeholder 12" },
				new EntryCell { Label = "Text 24" },
				new EntryCell { Label = "Text 25", Placeholder = "Placeholder 13" },
				new EntryCell { Label = "Text 26" },
				new EntryCell { Label = "Text 27", Placeholder = "Placeholder 14" },
				new EntryCell { Label = "Text 28" },
				new EntryCell { Label = "Text 29", Placeholder = "Placeholder 15" },
				new EntryCell { Label = "Text 30" },
				new EntryCell { Label = "Text 31", Placeholder = "Placeholder 16" },
				new EntryCell { Label = "Text 32" },
			};

			var keyboards = new TableSection("Keyboards") {
				new EntryCell { Label = "Chat", Keyboard = Keyboard.Chat },
				new EntryCell { Label = "Default", Keyboard = Keyboard.Default },
				new EntryCell { Label = "Email", Keyboard = Keyboard.Email },
				new EntryCell { Label = "Numeric", Keyboard = Keyboard.Numeric },
				new EntryCell { Label = "Telephone", Keyboard = Keyboard.Telephone },
				new EntryCell { Label = "Text", Keyboard = Keyboard.Text },
				new EntryCell { Label = "Url", Keyboard = Keyboard.Url }
			};

			var root = new TableRoot("Text Cell table") {
				tableSection,
				tableSectionTwo,
				keyboards
			};

			var table = new TableView
			{
				AutomationId = CellTypeList.CellTestContainerId,
				Root = root
			};

			Content = table;
		}
	}
}