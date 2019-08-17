using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;
using System.Linq;
using Xamarin.Forms.PlatformConfiguration;
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;
using System.Threading;
using System.ComponentModel;


#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using Xamarin.Forms.Core.UITests;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.None, 0, "Shell Back Button Behavior Test",
		PlatformAffected.All)]
#if UITEST
	[NUnit.Framework.Category(UITestCategories.Shell)]
#endif
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

		protected override void Init()
		{
			AddContentPage(new BackButtonPage());
		}

		public class BackButtonPage : ContentPage
		{
			BackButtonBehavior behavior = new BackButtonBehavior();
			Entry _commandParameter;
			Label _commandResult = new Label() { AutomationId = CommandResultId };

			public BackButtonPage()
			{
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

				StackLayout layout = new StackLayout();

				layout.Children.Add(new Label()
				{
					Text = "Test setting different Back Button Behavior properties"
				});

				layout.Children.Add(new Button()
				{
					Text = "Toggle Behavior",
					Command = new Command(ToggleBehavior),
					AutomationId = ToggleBehaviorId

				});
				layout.Children.Add(new Button()
				{
					Text = "Toggle Command",
					Command = new Command(ToggleCommand),
					AutomationId = ToggleCommandId
				});

				layout.Children.Add(new Button()
				{
					Text = "Toggle Command Can Execute",
					Command = new Command(ToggleCommandIsEnabled),
					AutomationId = ToggleCommandCanExecuteId
				});

				layout.Children.Add(_commandParameter);
				layout.Children.Add(_commandResult);
				layout.Children.Add(new Button()
				{
					Text = "Toggle Text",
					Command = new Command(ToggleBackButtonText),
					AutomationId = ToggleTextId
				});
				layout.Children.Add(new Button()
				{
					Text = "Toggle Icon",
					Command = new Command(ToggleIcon),
					AutomationId = ToggleIconId
				});
				layout.Children.Add(new Button()
				{
					Text = "Toggle Is Enabled",
					Command = new Command(ToggleIsEnabled),
					AutomationId = ToggleIsEnabledId
				});

				layout.Children.Add(new Button()
				{
					Text = "Push Page",
					Command = new Command(PushPage),
					AutomationId = PushPageId
				});

				
				Content = layout;
				ToggleBehavior();
			}

			async void PushPage(object obj)
			{
				await Navigation.PushAsync(new BackButtonPage());
			}

			public void ToggleBehavior()
			{
				if (this.IsSet(Shell.BackButtonBehaviorProperty))
					this.ClearValue(Shell.BackButtonBehaviorProperty);
				else
					this.SetValue(Shell.BackButtonBehaviorProperty, behavior);
			}

			public void ToggleCommand()
			{
				if (behavior.Command == null)
					behavior.Command = new Command<string>(result =>
					{
						_commandResult.Text = result;
					}, (_) => canExecute);
				else
					behavior.ClearValue(BackButtonBehavior.CommandProperty);
			}

			bool canExecute = true;
			public void ToggleCommandIsEnabled()
			{
				canExecute = !canExecute;
				if (behavior.Command is Command command)
					command.ChangeCanExecute();
			}

			public void ToggleBackButtonText()
			{
				if (!String.IsNullOrWhiteSpace(behavior.TextOverride))
					behavior.ClearValue(BackButtonBehavior.TextOverrideProperty);
				else
					behavior.TextOverride = "Text";
			}

			public void ToggleIcon()
			{
				if (behavior.IsSet(BackButtonBehavior.IconOverrideProperty))
					behavior.ClearValue(BackButtonBehavior.IconOverrideProperty);
				else
					behavior.IconOverride = "coffee.png";

			}

			public void ToggleIsEnabled()
			{
				behavior.IsEnabled = !behavior.IsEnabled;
			}
		}


#if UITEST && (__IOS__ || __ANDROID__)
		[Test]
		public void CommandTest()
		{
			RunningApp.Tap(ToggleCommandId);
			RunningApp.EnterText(EntryCommandParameter, "parameter");
			ShowFlyout();

			var commandResult = RunningApp.WaitForElement(CommandResultId)[0].ReadText();

			Assert.AreEqual(commandResult, "parameter");
			RunningApp.EnterText(EntryCommandParameter, "canexecutetest");
			RunningApp.Tap(ToggleCommandCanExecuteId);

			commandResult = RunningApp.WaitForElement(CommandResultId)[0].ReadText();
			Assert.AreEqual(commandResult, "parameter");
		}
#endif
	}
}
