#nullable disable
using System;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Handlers;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	[CollectionDefinition(nameof(GesturePlatformBackendTests), DisableParallelization = true)]
	public class GesturePlatformBackendCollection { }

	// The gesture manager of the platform-less build delegates to an optional backend factory (the GTK backend sets it).
	[Collection(nameof(GesturePlatformBackendTests))]
	public class GesturePlatformBackendTests
	{
		sealed class Connection : IDisposable
		{
			public int Disposed { get; private set; }

			public void Dispose() => Disposed++;
		}

		[Fact]
		public void WithoutABackendNothingIsAttached()
		{
			Assert.Null(GesturePlatformBackend.Factory);

			var manager = new GesturePlatformManager(new ButtonHandler());
			manager.Dispose();
		}

		[Fact]
		public void TheFactoryCreatesTheConnectionAndDisposeReleasesItOnce()
		{
			var target = new ButtonHandler();
			var connection = new Connection();
			IViewHandler seen = null;

			// Answers only for the handler under test, so views of other tests keep the default behaviour.
			GesturePlatformBackend.Factory = handler =>
			{
				if (handler != target)
					return null;
				seen = handler;
				return connection;
			};

			try
			{
				var manager = new GesturePlatformManager(target);
				Assert.Same(target, seen);
				Assert.Equal(0, connection.Disposed);

				manager.Dispose();
				manager.Dispose();
				Assert.Equal(1, connection.Disposed);

				// A handler the backend does not handle gets no connection.
				new GesturePlatformManager(new ButtonHandler()).Dispose();
				Assert.Equal(1, connection.Disposed);
			}
			finally
			{
				GesturePlatformBackend.Factory = null;
			}
		}
	}
}
