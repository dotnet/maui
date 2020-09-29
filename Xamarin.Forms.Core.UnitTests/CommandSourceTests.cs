using System;
using NUnit.Framework;

namespace Xamarin.Forms.Core.UnitTests
{
	public abstract class CommandSourceTests<T> : BaseTestFixture
		where T : BindableObject
	{
		[Test]
		public void TestCommand()
		{
			var source = CreateSource();

			bool executed = false;
			source.SetValue(CommandProperty, new Command(o =>
			{
				executed = true;
				Assert.AreEqual(source, o);
			}));

			source.SetValue(CommandParameterProperty, source);

			Activate(source);

			Assert.True(executed);
		}

		[Test]
		public void CommandCanExecuteModifiesEnabled([Values(true, false)] bool initial)
		{
			bool canExecute = initial;
			Command command;
			var source = CreateSource();
			source.SetValue(CommandProperty, command = new Command(() => { }, () => canExecute));

			Assert.AreEqual(canExecute, source.GetValue(IsEnabledProperty));

			canExecute = !initial;
			command.ChangeCanExecute();

			Assert.AreEqual(canExecute, source.GetValue(IsEnabledProperty));
		}

		[Test]
		public void ReenabledAfterCommandRemoved()
		{
			var source = CreateSource();
			source.SetValue(CommandProperty, new Command(() => { }, () => false));

			Assert.That(source.GetValue(IsEnabledProperty), Is.False);

			source.SetValue(CommandProperty, null);

			Assert.That(source.GetValue(IsEnabledProperty), Is.True);
		}

		[Test]
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

		[Test]
		public void CommandCanExecuteInvokedOnCommandSet()
		{
			bool fired = false;
			Func<bool> canExecute = () =>
			{
				fired = true;
				return true;
			};

			Assert.IsFalse(fired);
			var source = CreateSource();
			source.SetValue(CommandProperty, new Command(() => { }, canExecute));

			Assert.True(fired);
		}

		[Test]
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
			Assert.IsFalse(fired);
			source.SetValue(CommandParameterProperty, new object());
			Assert.True(fired);
		}

		[Test]
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

			Assert.That(fired, Is.True, "CanExecute was not called when the event was raised");

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

		[Test]
		public void EnabledUpdatesDoNotRemoveBindings()
		{
			var vm = new BoolViewModel { Toggle = true };
			var source = CreateSource();
			source.BindingContext = vm;
			source.SetBinding(IsEnabledProperty, "Toggle");

			Assert.That(source.GetValue(IsEnabledProperty), Is.True);

			source.SetValue(CommandProperty, new Command(() => { }));

			Assert.That(source.GetIsBound(IsEnabledProperty), Is.True);
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