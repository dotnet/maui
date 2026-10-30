using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Xunit;

namespace Microsoft.Maui.UnitTests
{
	[CollectionDefinition(nameof(PlatformViewBackendTests), DisableParallelization = true)]
	public class PlatformViewBackendCollection { }

	[Category(TestCategory.Core)]
	[Collection(nameof(PlatformViewBackendTests))]
	public class PlatformViewBackendTests
	{
		// Answers only for the handler under test, so handlers of tests running concurrently in other collections keep
		// the default (no backend) behaviour.
		class RecordingBackend : IPlatformViewBackend
		{
			readonly IViewHandler _target;

			public RecordingBackend(IViewHandler target) => _target = target;

			public Rect? Arranged { get; private set; }
			public int SetupCount { get; private set; }
			public int RemoveCount { get; private set; }

			public Size GetDesiredSize(IViewHandler handler, double widthConstraint, double heightConstraint) =>
				handler == _target ? new Size(widthConstraint / 2, 42) : Size.Zero;

			public void PlatformArrange(IViewHandler handler, Rect frame)
			{
				if (handler == _target)
					Arranged = frame;
			}

			public void SetupContainer(IViewHandler handler)
			{
				if (handler != _target)
					return;
				SetupCount++;
				((ViewHandler)handler).SetContainerView(new object());
			}

			public void RemoveContainer(IViewHandler handler)
			{
				if (handler != _target)
					return;
				RemoveCount++;
				((ViewHandler)handler).SetContainerView(null);
			}
		}

		[Fact]
		public void WithoutBackendLayoutIsANoOp()
		{
			var handler = new ViewHandlerStub();

			Assert.Equal(Size.Zero, handler.GetDesiredSize(100, 100));
			handler.PlatformArrange(new Rect(1, 2, 3, 4));
			handler.HasContainer = true;
			Assert.Null(handler.ContainerView);
		}

		[Fact]
		public void BackendReceivesMeasureArrangeAndContainerCalls()
		{
			var handler = new ViewHandlerStub();
			var backend = new RecordingBackend(handler);
			PlatformViewBackend.Current = backend;
			try
			{
				Assert.Equal(new Size(50, 42), handler.GetDesiredSize(100, 100));

				handler.PlatformArrange(new Rect(1, 2, 3, 4));
				Assert.Equal(new Rect(1, 2, 3, 4), backend.Arranged);

				handler.HasContainer = true;
				Assert.Equal(1, backend.SetupCount);
				Assert.NotNull(handler.ContainerView);

				handler.HasContainer = false;
				Assert.Equal(1, backend.RemoveCount);
				Assert.Null(handler.ContainerView);
			}
			finally
			{
				PlatformViewBackend.Current = null;
			}
		}
	}
}
