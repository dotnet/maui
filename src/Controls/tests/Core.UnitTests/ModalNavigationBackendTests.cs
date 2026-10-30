#nullable disable
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.Platform;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	[CollectionDefinition(nameof(ModalNavigationBackendTests), DisableParallelization = true)]
	public class ModalNavigationBackendCollection { }

	// The modal navigation manager of the platform-less build delegates to an optional backend (the GTK backend sets it).
	[Collection(nameof(ModalNavigationBackendTests))]
	public class ModalNavigationBackendTests : BaseTestFixture
	{
		sealed class RecordingBackend : IModalNavigationBackend
		{
			readonly Window _target;

			public RecordingBackend(Window target) => _target = target;

			public List<string> Calls { get; } = new();

			public Task PushAsync(Window window, Page modal, Page previous, bool animated)
			{
				if (window == _target)
					Calls.Add($"push {modal.Title} over {previous.Title} animated={animated}");
				return Task.CompletedTask;
			}

			public Task PopAsync(Window window, Page popped, Page next, bool animated)
			{
				if (window == _target)
					Calls.Add($"pop {popped.Title} revealing {next.Title} animated={animated}");
				return Task.CompletedTask;
			}
		}

		[Fact]
		public async Task PushAndPopAreForwardedToTheBackend()
		{
			var root = new Page { Title = "root" };
			var first = new Page { Title = "first" };
			var second = new Page { Title = "second" };
			var window = new TestWindow(root);
			// A modal page is only presented once the window and its page have handlers.
			window.Handler = new WindowHandlerStub();
			root.Handler = new HandlerStub();
			var backend = new RecordingBackend(window);
			ModalNavigationBackend.Current = backend;

			try
			{
				await window.Navigation.PushModalAsync(first, false);
				await window.Navigation.PushModalAsync(second, true);
				await window.Navigation.PopModalAsync(true);
				await window.Navigation.PopModalAsync(false);

				Assert.Equal(new[]
				{
					"push first over root animated=False",
					"push second over first animated=True",
					"pop second revealing first animated=True",
					"pop first revealing root animated=False",
				}, backend.Calls);
			}
			finally
			{
				ModalNavigationBackend.Current = null;
			}
		}

		[Fact]
		public async Task WithoutABackendModalPagesAreOnlyTracked()
		{
			Assert.Null(ModalNavigationBackend.Current);

			var window = new TestWindow(new Page());
			var modal = new Page();
			await window.Navigation.PushModalAsync(modal);
			Assert.Equal(modal, window.Navigation.ModalStack[0]);

			Assert.Equal(modal, await window.Navigation.PopModalAsync());
		}
	}
}
