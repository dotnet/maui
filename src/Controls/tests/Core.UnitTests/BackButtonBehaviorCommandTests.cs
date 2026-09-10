using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	// A command is normally owned by a view model that outlives the Shell page it is bound from, so a
	// direct CanExecuteChanged handler roots every BackButtonBehavior ever bound to that command.
	public class BackButtonBehaviorCommandTests : BaseTestFixture
	{
		// Note this uses a hand-rolled ICommand rather than Maui's own Command: Command backs
		// CanExecuteChanged with a WeakEventManager, so it never retains its subscribers and cannot
		// reproduce this. Most app-side RelayCommand implementations use a plain CLR event, which does.
		// https://github.com/dotnet/maui/issues/37635
		[Fact]
		public async Task SharedCommandDoesNotRootBehavior()
		{
			var shared = new PlainCommand();

			var reference = CreateBehavior(shared);

			Assert.False(await reference.WaitForCollect(), "BackButtonBehavior should not be alive!");
			GC.KeepAlive(shared);
		}

		// Guards that the weak subscription still delivers for a plain-event command.
		[Fact]
		public void IsEnabledTracksPlainCommandCanExecuteChanged()
		{
			var command = new PlainCommand { CanExecuteResult = false };
			var behavior = new BackButtonBehavior { Command = command };

			Assert.False(behavior.IsEnabled);

			command.CanExecuteResult = true;
			command.RaiseCanExecuteChanged();

			Assert.True(behavior.IsEnabled);
			GC.KeepAlive(behavior);
		}

		[Fact]
		public void IsEnabledReflectsCanExecuteWhenCommandIsSet()
		{
			var behavior = new BackButtonBehavior { Command = new Command(() => { }, () => false) };

			Assert.False(behavior.IsEnabled);
			GC.KeepAlive(behavior);
		}

		[Fact]
		public void IsEnabledTracksChangeCanExecute()
		{
			bool canExecute = false;
			var command = new Command(() => { }, () => canExecute);
			var behavior = new BackButtonBehavior { Command = command };

			Assert.False(behavior.IsEnabled);

			canExecute = true;
			command.ChangeCanExecute();

			Assert.True(behavior.IsEnabled);
			GC.KeepAlive(behavior);
		}

		[Fact]
		public void IsEnabledReevaluatesWhenCommandParameterChanges()
		{
			var command = new Command<string>(_ => { }, p => p == "yes");
			var behavior = new BackButtonBehavior { Command = command, CommandParameter = "no" };

			Assert.False(behavior.IsEnabled);

			behavior.CommandParameter = "yes";

			Assert.True(behavior.IsEnabled);
			GC.KeepAlive(behavior);
		}

		[Fact]
		public void ClearingCommandRestoresEnabled()
		{
			var behavior = new BackButtonBehavior { Command = new Command(() => { }, () => false) };
			Assert.False(behavior.IsEnabled);

			behavior.Command = null;

			Assert.True(behavior.IsEnabled);
			GC.KeepAlive(behavior);
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		static WeakReference CreateBehavior(ICommand command) =>
			new WeakReference(new BackButtonBehavior { Command = command });

		sealed class PlainCommand : ICommand
		{
			public bool CanExecuteResult { get; set; } = true;

			public event EventHandler CanExecuteChanged;

			public bool CanExecute(object parameter) => CanExecuteResult;

			public void Execute(object parameter) { }

			public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
		}
	}
}
