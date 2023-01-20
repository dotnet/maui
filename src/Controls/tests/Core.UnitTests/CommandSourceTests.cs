using System;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	public abstract class CommandSourceTests<T> : BaseTestFixture
		where T : BindableObject
	{
		[Fact]
		public void TestCommand()
		{
			var source = CreateSource();

			bool executed = false;
			source.SetValue(CommandProperty, new Command(o =>
			{
				executed = true;
				Assert.Equal(source, o);
			}));

			source.SetValue(CommandParameterProperty, source);

			Activate(source);

			Assert.True(executed);
		}

		[Theory, InlineData(true), InlineData(false)]
		public void CommandCanExecuteModifiesEnabled(bool initial)
		{
			bool canExecute = initial;
			Command command;
			var source = CreateSource();
			source.SetValue(CommandProperty, command = new Command(() => { }, () => canExecute));

			Assert.Equal(canExecute, source.GetValue(IsEnabledProperty));

			canExecute = !initial;
			command.ChangeCanExecute();

			Assert.Equal(canExecute, source.GetValue(IsEnabledProperty));
		}

		[Fact]
		public void ReenabledAfterCommandRemoved()
		{
			var source = CreateSource();
			source.SetValue(CommandProperty, new Command(() => { }, () => false));

			Assert.False((bool)source.GetValue(IsEnabledProperty));

			source.SetValue(CommandProperty, null);

			Assert.True((bool)source.GetValue(IsEnabledProperty));
		}

		[Fact]
		public void CommandUnhooksOnNull()
		{
			bool canExecute = false;
			Command command;
			var source = CreateSource();

			bool raised = false;
			source.SetValue(CommandProperty, command = new Command(() => { }, () =>
			{
				raised = true;
				return canExecute;
			}));

			raised = false;
			source.SetValue(CommandProperty, null);

			canExecute = true;
			command.ChangeCanExecute();

			Assert.False(raised);
		}

		[Fact]
		public void CommandCanExecuteInvokedOnCommandSet()
		{
			bool fired = false;
			Func<bool> canExecute = () =>
			{
				fired = true;
				return true;
			};

			Assert.False(fired);
			var source = CreateSource();
			source.SetValue(CommandProperty, new Command(() => { }, canExecute));

			Assert.True(fired);
		}

		[Fact]
		public void CommandCanExecuteInvokedOnCommandParameterSet()
		{
			bool fired;
			Func<bool> canExecute = () =>
			{
				fired = true;
				return true;
			};

			var source = CreateSource();
			source.SetValue(CommandProperty, new Command(() => { }, canExecute));

			fired = false;
			Assert.False(fired);
			source.SetValue(CommandParameterProperty, new object());
			Assert.True(fired);
		}

		[Fact]
		public void CommandCanExecuteInvokedOnChange()
		{
			bool fired;
			Func<bool> canExecute = () =>
			{
				fired = true;
				return true;
			};

			var cmd = new Command(() => { }, canExecute);

			var source = CreateSource();
			source.SetValue(CommandProperty, cmd);

			fired = false;

			cmd.ChangeCanExecute();

			Assert.True(fired, "CanExecute was not called when the event was raised");

			// Preserve source from GC during the test in Release mode
			GC.KeepAlive(source);
		}

		class BoolViewModel
			: MockViewModel
		{
			bool toggle;

			public bool Toggle
			{
				get { return toggle; }
				set
				{
					if (toggle == value)
						return;

					toggle = value;
					OnPropertyChanged();
				}
			}
		}

		[Fact]
		public void CommandCanExecuteUpdatesEnabled()
		{
			var source = CreateSource();

			bool result = false;

			var bindingContext = new
			{
				Command = new Command(() => { }, () => result)
			};

			source.SetBinding(CommandProperty, "Command");
			source.BindingContext = bindingContext;

			Assert.False((bool)source.GetValue(IsEnabledProperty));

			result = true;

			bindingContext.Command.ChangeCanExecute();

			Assert.True((bool)source.GetValue(IsEnabledProperty));
		}

		[Fact]
		public void EnabledUpdatesDoNotRemoveBindings()
		{
			var vm = new BoolViewModel { Toggle = true };
			var source = CreateSource();
			source.BindingContext = vm;
			source.SetBinding(IsEnabledProperty, "Toggle");

			Assert.True((bool)source.GetValue(IsEnabledProperty));

			source.SetValue(CommandProperty, new Command(() => { }));

			Assert.True(source.GetIsBound(IsEnabledProperty));
		}

		[Theory]
		[InlineData(true, true, true)]
		[InlineData(true, false, false)]
		[InlineData(false, true, false)]
		[InlineData(false, false, false)]
		public void EnabledAndCommandAreCorrect(bool viewEnabled, bool commandEnabled, bool expectedEnabled)
		{
			var source = CreateSource();
			source.SetValue(IsEnabledProperty, viewEnabled);
			source.SetValue(CommandProperty, new Command(() => { }, () => commandEnabled));

			Assert.Equal(expectedEnabled, (bool)source.GetValue(IsEnabledProperty));
		}

		[Theory]
		[InlineData(true, true, false)]
		[InlineData(true, false, false)]
		[InlineData(false, true, true)]
		[InlineData(false, false, false)]
		public void EnabledUpdatesDoNotOverrideCommand(bool viewEnabled, bool commandEnabled, bool expectedEnabled)
		{
			var source = CreateSource();
			source.SetValue(IsEnabledProperty, viewEnabled);
			source.SetValue(CommandProperty, new Command(() => { }, () => commandEnabled));

			source.SetValue(IsEnabledProperty, !viewEnabled);

			Assert.Equal(expectedEnabled, (bool)source.GetValue(IsEnabledProperty));
		}

		[Theory]
		[InlineData(true, true, false)]
		[InlineData(true, false, true)]
		[InlineData(false, true, false)]
		[InlineData(false, false, false)]
		public void CommandUpdatesDoNotOverrideEnabled(bool viewEnabled, bool commandEnabled, bool expectedEnabled)
		{
			var source = CreateSource();

			source.SetValue(IsEnabledProperty, viewEnabled);
			var command = new Command(() => { }, () => commandEnabled);
			source.SetValue(CommandProperty, command);

			commandEnabled = !commandEnabled;
			command.ChangeCanExecute();

			Assert.Equal(expectedEnabled, (bool)source.GetValue(IsEnabledProperty));
		}

		protected abstract T CreateSource();

		protected abstract void Activate(T source);

		protected abstract BindableProperty IsEnabledProperty
		{
			get;
		}

		protected abstract BindableProperty CommandProperty
		{
			get;
		}

		protected abstract BindableProperty CommandParameterProperty
		{
			get;
		}
	}
}
