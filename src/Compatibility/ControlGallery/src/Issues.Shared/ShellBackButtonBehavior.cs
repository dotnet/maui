using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;
using Microsoft.Maui.Graphics;


#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using Microsoft.Maui.Controls.Compatibility.UITests;
#endif

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.Issues
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

				StackLayout layout = new StackLayout();

				Button toggleFlyoutBehaviorButton = null;

				toggleFlyoutBehaviorButton = new Button()
				{
					Text = "Flyout Behavior: Flyout",
					Command = new Command((o) => ToggleFlyoutBehavior(o, toggleFlyoutBehaviorButton)),
					AutomationId = "ToggleFlyoutBehavior"
				};

				layout.Children.Add(toggleFlyoutBehaviorButton);

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


				Content = new ScrollView() { Content = layout };
				ToggleBehavior();
			}

			void ToggleFlyoutBehavior(object obj, Button btn)
			{
				var behavior = (int)(Shell.Current.FlyoutBehavior);
				behavior++;

				if (Enum.GetValues(typeof(FlyoutBehavior)).Length <= behavior)
					behavior = 0;

				Shell.Current.FlyoutBehavior = (FlyoutBehavior)behavior;
				btn.Text = $"Flyout Behavior: {(FlyoutBehavior)behavior}";

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
					behavior.TextOverride = "T3xt";
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


#if UITEST && (__SHELL__)
		[Test]
		public void CommandTest()
		{
			RunningApp.Tap(ToggleCommandId);
			RunningApp.EnterText(EntryCommandParameter, "parameter");
			ShowFlyout();

			// API 19 workaround
			var commandResult = RunningApp.QueryUntilPresent(() =>
			{
				ShowFlyout();
				if (RunningApp.WaitForElement(CommandResultId)[0].ReadText() == "parameter")
					return RunningApp.WaitForElement(CommandResultId);

				return null;
			})?.FirstOrDefault()?.ReadText();

			Assert.AreEqual("parameter", commandResult);
			RunningApp.EnterText(EntryCommandParameter, "canexecutetest");
			RunningApp.Tap(ToggleCommandCanExecuteId);

			commandResult = RunningApp.QueryUntilPresent(() =>
			{
				if (RunningApp.WaitForElement(CommandResultId)[0].ReadText() == "parameter")
					return RunningApp.WaitForElement(CommandResultId);

				return null;
			})?.FirstOrDefault()?.ReadText();

			Assert.AreEqual("parameter", commandResult);
		}

		[Test]
		public void CommandWorksWhenItsTheOnlyThingSet()
		{
			RunningApp.Tap(PushPageId);
			RunningApp.Tap(ToggleCommandId);
			RunningApp.EnterText(EntryCommandParameter, "parameter");

			// API 19 workaround
			var commandResult = RunningApp.QueryUntilPresent(() =>
			{

#if __ANDROID__
				TapBackArrow();
#else
				RunningApp.Tap("Page 0");
#endif

				if (RunningApp.WaitForElement(CommandResultId)[0].ReadText() == "parameter")
					return RunningApp.WaitForElement(CommandResultId);

				return null;
			})?.FirstOrDefault()?.ReadText();

			Assert.AreEqual(commandResult, "parameter");
		}

		[Test]
		public void BackButtonSetToTextStillNavigatesBack()
		{
			RunningApp.Tap(PushPageId);
			RunningApp.Tap(ToggleTextId);
			RunningApp.Tap("T3xt");
			RunningApp.WaitForNoElement(FlyoutOpen);
			RunningApp.WaitForElement("Page 0");
		}

		[Test]
		public void BackButtonSetToTextStillOpensFlyout()
		{
			RunningApp.Tap(ToggleTextId);

			RunningApp.Tap("T3xt");
			RunningApp.WaitForElement(FlyoutOpen);
		}

#if __ANDROID__
		[Test]
		public void FlyoutDisabledDoesntOpenFlyoutWhenSetToText()
		{
			RunningApp.WaitForElement("ToggleFlyoutBehavior");
			RunningApp.Tap("ToggleFlyoutBehavior");
			RunningApp.Tap("ToggleFlyoutBehavior");
			RunningApp.WaitForElement("Flyout Behavior: Disabled");
			RunningApp.Tap(ToggleTextId);
			RunningApp.Tap("T3xt");
			RunningApp.WaitForNoElement(FlyoutOpen);
		}
#else
		[Test]
		public void FlyoutDisabledDoesntOpenFlyoutWhenSetToText()
		{
			RunningApp.WaitForElement("ToggleFlyoutBehavior");
			RunningApp.Tap(ToggleTextId);
			RunningApp.WaitForElement("T3xt");
			RunningApp.Tap("ToggleFlyoutBehavior");
			RunningApp.WaitForElement("T3xt");
			RunningApp.Tap("ToggleFlyoutBehavior");
			RunningApp.WaitForElement("Flyout Behavior: Disabled");
			RunningApp.Tap("T3xt");
			RunningApp.WaitForNoElement(FlyoutOpen);
		}

		[Test]
		public void AutomationIdOnIconOverride()
		{
			RunningApp.WaitForElement("ToggleFlyoutBehavior");
			RunningApp.Tap(ToggleIconId);
			RunningApp.WaitForElement("CoffeeAutomation");
			RunningApp.Tap("ToggleFlyoutBehavior");
			RunningApp.WaitForElement("CoffeeAutomation");
			RunningApp.Tap("ToggleFlyoutBehavior");
			RunningApp.WaitForElement("Flyout Behavior: Disabled");
			RunningApp.Tap("CoffeeAutomation");
			RunningApp.WaitForNoElement(FlyoutOpen);
		}

#endif

#endif
	}
}
