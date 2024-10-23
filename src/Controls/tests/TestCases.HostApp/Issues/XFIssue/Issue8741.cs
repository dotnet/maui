using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 8741, "[Bug] [Shell] [Android] ToolbarItem Enabled/Disabled behavior does not work for Shell apps", PlatformAffected.Android)]
public class Issue8741 : TestShell
{
	protected override void Init()
	{
		var page = CreateContentPage();
		var toolbarItem = new ToolbarItem
		{
			Text = "Add",
			AutomationId = "Add"
		};

		toolbarItem.SetBinding(MenuItem.CommandProperty, "ToolbarTappedCommand");
		page.ToolbarItems.Add(toolbarItem);

		var button = new Button
		{
			Text = "Toggle Enabled/Disabled",
			AutomationId = "ToggleEnabled"
		};

		button.SetBinding(Button.CommandProperty, "ChangeToggleCommand");
		var label = new Label();
		label.SetBinding(Label.TextProperty, "EnabledText");

		var clickCount = new Label();
		clickCount.AutomationId = "ClickCount";
		clickCount.SetBinding(Label.TextProperty, "ClickCount");

		page.Content =
			new StackLayout
			{
				label,
				clickCount,
				button
			};

		BindingContext = new ViewModelIssue8741();
	}

	public class ViewModelIssue8741 : INotifyPropertyChanged
	{
		bool _canAddNewItem;
		int _clickCount;

		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		public bool Enabled
		{
			get => _canAddNewItem;
			set
			{
				_canAddNewItem = value;
				OnPropertyChanged(nameof(Enabled));
				ToolbarTappedCommand.ChangeCanExecute();
			}
		}

		public int ClickCount
		{
			get
			{
				return _clickCount;
			}
			set
			{
				_clickCount = value;
				OnPropertyChanged(nameof(ClickCount));
			}
		}

		public string EnabledText { get; set; }
		public Command ChangeToggleCommand { get; set; }
		public Command ToolbarTappedCommand { get; set; }

		public ViewModelIssue8741()
		{
			ChangeToggleCommand = new Command(ChangeToggle);
			ToolbarTappedCommand = new Command(ToolbarTapped, () => Enabled);
			EnabledText = Enabled ? "Enabled" : "Disabled";
		}

		void ToolbarTapped()
		{
			ClickCount++;
		}

		void ChangeToggle()
		{
			Enabled = !Enabled;
			EnabledText = Enabled ? "Enabled" : "Disabled";
			OnPropertyChanged(nameof(EnabledText));
		}
	}
}
