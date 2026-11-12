using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls.Internals;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{

	public class CommandTests : BaseTestFixture
	{
		[Fact]
		public void Constructor()
		{
			var cmd = new Command(() => { });
			Assert.True(cmd.CanExecute(null));
		}

		[Fact]
		public void ThrowsWithNullConstructor()
		{
			Assert.Throws<ArgumentNullException>(() => new Command((Action)null));
		}

		[Fact]
		public void ThrowsWithNullParameterizedConstructor()
		{
			Assert.Throws<ArgumentNullException>(() => new Command((Action<object>)null));
		}

		[Fact]
		public void ThrowsWithNullCanExecute()
		{
			Assert.Throws<ArgumentNullException>(() => new Command(() => { }, null));
		}

		[Fact]
		public void ThrowsWithNullParameterizedCanExecute()
		{
			Assert.Throws<ArgumentNullException>(() => new Command(o => { }, null));
		}

		[Fact]
		public void ThrowsWithNullExecuteValidCanExecute()
		{
			Assert.Throws<ArgumentNullException>(() => new Command(null, () => true));
		}

		[Fact]
		public void Execute()
		{
			bool executed = false;
			var cmd = new Command(() => executed = true);

			cmd.Execute(null);
			Assert.True(executed);
		}

		[Fact]
		public void ExecuteParameterized()
		{
			object executed = null;
			var cmd = new Command(o => executed = o);

			var expected = new object();
			cmd.Execute(expected);

			Assert.Equal(expected, executed);
		}

		[Fact]
		public void ExecuteWithCanExecute()
		{
			bool executed = false;
			var cmd = new Command(() => executed = true, () => true);

			cmd.Execute(null);
			Assert.True(executed);
		}

		[Theory, InlineData(true), InlineData(false)]
		public void CanExecute(bool expected)
		{
			bool canExecuteRan = false;
			var cmd = new Command(() => { }, () =>
			{
				canExecuteRan = true;
				return expected;
			});

			Assert.Equal(expected, cmd.CanExecute(null));
			Assert.True(canExecuteRan);
		}

		[Fact]
		public void ChangeCanExecute()
		{
			bool signaled = false;
			var cmd = new Command(() => { });

			cmd.CanExecuteChanged += (sender, args) => signaled = true;

			cmd.ChangeCanExecute();
			Assert.True(signaled);
		}

		[Fact]
		public void GenericThrowsWithNullExecute()
		{
			Assert.Throws<ArgumentNullException>(() => new Command<string>(null));
		}

		[Fact]
		public void GenericThrowsWithNullExecuteAndCanExecuteValid()
		{
			Assert.Throws<ArgumentNullException>(() => new Command<string>(null, s => true));
		}

		[Fact]
		public void GenericThrowsWithValidExecuteAndCanExecuteNull()
		{
			Assert.Throws<ArgumentNullException>(() => new Command<string>(s => { }, null));
		}

		[Fact]
		public void GenericExecute()
		{
			string result = null;
			var cmd = new Command<string>(s => result = s);

			cmd.Execute("Foo");
			Assert.Equal("Foo", result);
		}

		[Fact]
		public void GenericExecuteWithCanExecute()
		{
			string result = null;
			var cmd = new Command<string>(s => result = s, s => true);

			cmd.Execute("Foo");
			Assert.Equal("Foo", result);
		}

		[Theory, InlineData(true), InlineData(false)]
		public void GenericCanExecute(bool expected)
		{
			string result = null;
			var cmd = new Command<string>(s => { }, s =>
			{
				result = s;
				return expected;
			});

			Assert.Equal(expected, cmd.CanExecute("Foo"));
			Assert.Equal("Foo", result);
		}

		class FakeParentContext
		{
		}

		// ReSharper disable once ClassNeverInstantiated.Local
		class FakeChildContext
		{
		}

		[Fact]
		public void CanExecuteReturnsFalseIfParameterIsWrongReferenceType()
		{
			var command = new Command<FakeChildContext>(context => { }, context => true);

			Assert.False(command.CanExecute(new FakeParentContext()), "the parameter is of the wrong type");
		}

		[Fact]
		public void CanExecuteReturnsFalseIfParameterIsWrongValueType()
		{
			var command = new Command<int>(context => { }, context => true);

			Assert.False(command.CanExecute(10.5), "the parameter is of the wrong type");
		}

		[Fact]
		public void CanExecuteUsesParameterIfReferenceTypeAndSetToNull()
		{
			var command = new Command<FakeChildContext>(context => { }, context => true);

			Assert.True(command.CanExecute(null), "null is a valid value for a reference type");
		}

		[Fact]
		public void CanExecuteUsesParameterIfNullableAndSetToNull()
		{
			var command = new Command<int?>(context => { }, context => true);

			Assert.True(command.CanExecute(null), "null is a valid value for a Nullable<int> type");
		}

		[Fact]
		public void CanExecuteIgnoresParameterIfValueTypeAndSetToNull()
		{
			var command = new Command<int>(context => { }, context => true);

			Assert.False(command.CanExecute(null), "null is not a valid valid for int");
		}

		[Fact]
		public void ExecuteDoesNotRunIfParameterIsWrongReferenceType()
		{
			int executions = 0;
			var command = new Command<FakeChildContext>(context => executions += 1);

			command.Execute(new FakeParentContext()); // "the command should not execute, so no exception should be thrown"
			Assert.True(executions == 0, "the command should not have executed");
		}

		[Fact]
		public void ExecuteDoesNotRunIfParameterIsWrongValueType()
		{
			int executions = 0;
			var command = new Command<int>(context => executions += 1);

			command.Execute(10.5); // "the command should not execute, so no exception should be thrown"
			Assert.True(executions == 0, "the command should not have executed");
		}

		[Fact]
		public void ExecuteRunsIfReferenceTypeAndSetToNull()
		{
			int executions = 0;
			var command = new Command<FakeChildContext>(context => executions += 1);

			command.Execute(null); // "null is a valid value for a reference type"
			Assert.True(executions == 1, "the command should have executed");
		}

		[Fact]
		public void ExecuteRunsIfNullableAndSetToNull()
		{
			int executions = 0;
			var command = new Command<int?>(context => executions += 1);

			command.Execute(null); // "null is a valid value for a Nullable<int> type"
			Assert.True(executions == 1, "the command should have executed");
		}

		[Fact]
		public void ExecuteDoesNotRunIfValueTypeAndSetToNull()
		{
			int executions = 0;
			var command = new Command<int>(context => executions += 1);

			command.Execute(null); // "null is not a valid value for int"
			Assert.True(executions == 0, "the command should not have executed");
		}

		[Theory]
		[InlineData(typeof(Button), true)]
		[InlineData(typeof(Button), false)]
		[InlineData(typeof(RefreshView), true)]
		[InlineData(typeof(RefreshView), false)]
		[InlineData(typeof(TextCell), true)]
		[InlineData(typeof(TextCell), false)]
		[InlineData(typeof(ImageButton), true)]
		[InlineData(typeof(ImageButton), false)]
		[InlineData(typeof(MenuItem), true)]
		[InlineData(typeof(MenuItem), false)]
		[InlineData(typeof(SearchBar), true)]
		[InlineData(typeof(SearchBar), false)]
		[InlineData(typeof(SearchHandler), true)]
		[InlineData(typeof(SearchHandler), false)]
		[InlineData(typeof(BackButtonBehavior), true)]
		[InlineData(typeof(BackButtonBehavior), false)]
		[InlineData(typeof(SwipeItemView), true)]
		[InlineData(typeof(SwipeItemView), false)]
		public async Task CommandsSubscribedToCanExecuteCollect(Type controlType, bool useWeakEventHandler)
		{
			// Create a view model with a Command
			ICommand command;

			if (!useWeakEventHandler)
				command = new CommandWithoutWeakEventHandler();
			else
				command = new Command(() => { });

			List<WeakReference> weakReferences = new List<WeakReference>();

			// Create a button in a separate scope to ensure no references remain
			{
				var control = (BindableObject)Activator.CreateInstance(controlType);
				switch (control)
				{
					case Button b:
						b.Command = command;
						break;
					case RefreshView r:
						r.Command = command;
						break;
					case TextCell t:
						t.Command = command;
						break;
					case ImageButton i:
						i.Command = command;
						break;
					case MenuItem m:
						m.Command = command;
						break;
					case SearchBar s:
						s.SearchCommand = command;
						break;
					case SearchHandler sh:
						sh.Command = command;
						sh.ClearPlaceholderCommand = command;
						break;
					case BackButtonBehavior bbb:
						bbb.Command = command;
						break;
					case SwipeItemView sv:
						sv.Command = command;
						break;
				}

				// Create a weak reference to the button
				weakReferences.Add(new WeakReference(control));

				if (control is ICommandElement commandElement)
				{
					// Add weak references to the command and its cleanup tracker
					weakReferences.Add(new WeakReference(commandElement.CleanupTracker));
					weakReferences.Add(new WeakReference(commandElement.CleanupTracker.Proxy));
				}
				else if (control is SearchHandler searchHandler)
				{
					// Add weak references to the command and its cleanup tracker
					weakReferences.Add(new WeakReference(searchHandler.CommandSubscription));
					weakReferences.Add(new WeakReference(searchHandler.CommandSubscription.Proxy));
					weakReferences.Add(new WeakReference(searchHandler.ClearPlaceholderCommandSubscription));
					weakReferences.Add(new WeakReference(searchHandler.ClearPlaceholderCommandSubscription.Proxy));
				}

				await TestHelpers.Collect();
				await TestHelpers.Collect();

				// Make sure everything is still alive if the button is still in scope
				// We need to reference the button here again to keep it alive 
				// awaiting a Task appears to move us to a new scope and causes the button to be collected
				Assert.NotNull(control);

				foreach (var weakRef in weakReferences)
				{
					Assert.True(weakRef.IsAlive);
				}
			}

			foreach (var weakRef in weakReferences)
			{
				Assert.False(await weakRef.WaitForCollect());
			}
		}

		[Fact]
		public void CanExecuteDoesNotRefreshCollectedButtonBindings()
		{
			var command = new CommandWithoutWeakEventHandler();
			var liveButton = new Button { Command = command };
			using var entered = new ManualResetEventSlim();
			using var release = new ManualResetEventSlim();

			try
			{
				CreateFinalizerGate(entered, release);
				GC.Collect();
				Assert.True(entered.Wait(TimeSpan.FromSeconds(10)), "The finalizer gate did not start.");

				var abandonedButton = CreateAbandonedButton(command);
				GC.Collect();
				Assert.False(HasTarget(abandonedButton));

				command.CanExecuteResult = false;
				command.ChangeCanExecute();
				Assert.False(liveButton.IsEnabled);

				command.CanExecuteResult = true;
				command.ChangeCanExecute();
				Assert.True(liveButton.IsEnabled);
			}
			finally
			{
				release.Set();
				GC.WaitForPendingFinalizers();
				GC.KeepAlive(liveButton);
				GC.KeepAlive(command);
			}
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		static WeakReference<Button> CreateAbandonedButton(ICommand command)
		{
			var button = new Button { BindingContext = new NullCommandParameter() };
			button.SetBinding(Button.CommandParameterProperty,
				new TypedBinding<NullCommandParameter, object>(model => (model.Value, true), null, null));
			button.Command = command;
			return new WeakReference<Button>(button);
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		static bool HasTarget(WeakReference<Button> reference) => reference.TryGetTarget(out _);

		[MethodImpl(MethodImplOptions.NoInlining)]
		static void CreateFinalizerGate(ManualResetEventSlim entered, ManualResetEventSlim release)
		{
			var gate = new FinalizerGate(entered, release);
			GC.KeepAlive(gate);
		}

		class NullCommandParameter
		{
			public object Value => null;
		}

		sealed class FinalizerGate
		{
			readonly ManualResetEventSlim _entered;
			readonly ManualResetEventSlim _release;

			public FinalizerGate(ManualResetEventSlim entered, ManualResetEventSlim release)
			{
				_entered = entered;
				_release = release;
			}

			~FinalizerGate()
			{
				_entered.Set();
				_release.Wait(TimeSpan.FromSeconds(30));
			}
		}

		class CommandWithoutWeakEventHandler : ICommand
		{
			public event EventHandler CanExecuteChanged;

			public bool CanExecuteResult { get; set; } = true;

			public bool CanExecute(object parameter) => CanExecuteResult;

			public void Execute(object parameter) { }

			public void ChangeCanExecute() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
		}
	}
}