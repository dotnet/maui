namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.None, 0, "Shell Back Button Behavior Test",
	PlatformAffected.All)]
public class ShellBackButtonBehavior : TestShell
{
	const string EntryCommandParameter = "EntryCommandParameter";
	const string ToggleBehaviorId = "ToggleBehaviorId";
	const string ToggleCommandId = "ToggleCommandId";
	const string ToggleCommandCanExecuteId = "ToggleCommandCanExecuteId";
	const string ToggleIconId = "ToggleIconId";
	const string ToggleIsEnabledId = "ToggleIsEnabledId";
	const string ToggleTextId = "ToggleTextId";
	const string CommandResultId = "CommandResult";
	const string PushPageId = "PushPageId";
	const string FlyoutOpen = "Flyout Open";

	protected override void Init()
	{
		AddContentPage(new BackButtonPage());
		Items.Add(new MenuItem() { Text = "Flyout Open", AutomationId = "Flyout Open" });
	}

	public class BackButtonPage : ContentPage
	{
		BackButtonBehavior behavior = new BackButtonBehavior();
		Entry _commandParameter;
		Label _commandResult = new Label()
		{
			AutomationId = CommandResultId,
			BackgroundColor = Colors.LightBlue,
			Text = "Label"
		};

		public BackButtonPage()
		{
			Title = $"Page {Shell.Current?.Navigation?.NavigationStack?.Count ?? 0}";
			_commandParameter = new Entry()
			{
				Placeholder = "Command Parameter",
				AutomationId = EntryCommandParameter
			};

			_commandParameter.TextChanged += (_, __) =>
			{
				if (String.IsNullOrWhiteSpace(_commandParameter.Text))
					behavior.ClearValue(BackButtonBehavior.CommandParameterProperty);
				else
					behavior.CommandParameter = _commandParameter.Text;
			};

			Button toggleFlyoutBehaviorButton = null;

			toggleFlyoutBehaviorButton = new Button()
			{
				Text = "Flyout Behavior: Flyout",
				Command = new Command((o) => ToggleFlyoutBehavior(o, toggleFlyoutBehaviorButton)),
				AutomationId = "ToggleFlyoutBehavior"
			};

			StackLayout layout = new StackLayout()
			{
				toggleFlyoutBehaviorButton,
				new Label()
				{
					Text = "Test setting different Back Button Behavior properties"
				},
				new Button()
				{
					Text = "Toggle Behavior",
					Command = new Command(ToggleBehavior),
					AutomationId = ToggleBehaviorId

				},
				new Button()
				{
					Text = "Toggle Command",
					Command = new Command(ToggleCommand),
					AutomationId = ToggleCommandId
				},
				new Button()
				{
					Text = "Toggle Command Can Execute",
					Command = new Command(ToggleCommandIsEnabled),
					AutomationId = ToggleCommandCanExecuteId
				},
				_commandParameter,
				_commandResult,
				new Button()
				{
					Text = "Toggle Text",
					Command = new Command(ToggleBackButtonText),
					AutomationId = ToggleTextId
				},
				new Button()
				{
					Text = "Toggle Icon",
					Command = new Command(ToggleIcon),
					AutomationId = ToggleIconId
				},
				new Button()
				{
					Text = "Toggle Is Enabled",
					Command = new Command(ToggleIsEnabled),
					AutomationId = ToggleIsEnabledId
				},
				new Button()
				{
					Text = "Push Page",
					Command = new Command(PushPage),
					AutomationId = PushPageId
				},
			};

			Content = new Microsoft.Maui.Controls.ScrollView() { Content = layout };
			ToggleBehavior();
		}

		void ToggleFlyoutBehavior(object obj, Button btn)
		{
			var behavior = (int)(Shell.Current.FlyoutBehavior);
			behavior++;

			if (Enum.GetValues(typeof(FlyoutBehavior)).Length <= behavior)
			{
				behavior = 0;
			}

			Shell.Current.FlyoutBehavior = (FlyoutBehavior)behavior;
			btn.Text = $"Flyout Behavior: {(FlyoutBehavior)behavior}";

		}

		async void PushPage(object obj)
		{
			await Navigation.PushAsync(new BackButtonPage());
		}

		public void ToggleBehavior()
		{
			if (IsSet(Shell.BackButtonBehaviorProperty))
			{
				ClearValue(Shell.BackButtonBehaviorProperty);
			}
			else
			{
				SetValue(Shell.BackButtonBehaviorProperty, behavior);
			}
		}

		public void ToggleCommand()
		{
			if (behavior.Command == null)
			{
				behavior.Command = new Command<string>(result =>
				{
					_commandResult.Text = result;
				}, (_) => canExecute);
			}
			else
			{
				behavior.ClearValue(BackButtonBehavior.CommandProperty);
			}
		}

		bool canExecute = true;
		public void ToggleCommandIsEnabled()
		{
			canExecute = !canExecute;
			if (behavior.Command is Command command)
			{
				command.ChangeCanExecute();
			}
		}

		public void ToggleBackButtonText()
		{
			if (!String.IsNullOrWhiteSpace(behavior.TextOverride))
			{
				behavior.ClearValue(BackButtonBehavior.TextOverrideProperty);
			}
			else
			{
				behavior.TextOverride = "T3xt";
			}
		}

		public void ToggleIcon()
		{
			if (behavior.IsSet(BackButtonBehavior.IconOverrideProperty))
			{
				behavior.ClearValue(BackButtonBehavior.IconOverrideProperty);
			}
			else
			{
				behavior.IconOverride = new FileImageSource()
				{
					File = "coffee.png",
					AutomationId = "CoffeeAutomation"
				};
			}

		}

		public void ToggleIsEnabled()
		{
			behavior.IsEnabled = !behavior.IsEnabled;
		}
	}
}
