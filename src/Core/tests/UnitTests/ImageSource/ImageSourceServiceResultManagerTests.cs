using System;
using Xunit;

namespace Microsoft.Maui.UnitTests.ImageSource
{
	[Category(TestCategory.Core, TestCategory.ImageSource)]
	public class ImageSourceServiceResultManagerTests
	{
		// https://github.com/dotnet/maui/issues/40421
		[Fact]
		public void CompletingSupersededLoadKeepsCurrentLoadCancellable()
		{
			var manager = new ImageSourceServiceResultManager();

			var tokenA = manager.BeginLoad();
			var tokenB = manager.BeginLoad();
			Assert.True(tokenA.IsCancellationRequested);

			// The cancelled load A finishes after B started.
			manager.CompleteLoad(null, tokenA);

			Assert.True(manager.IsLoading);
			Assert.Equal(tokenB, manager.Token);

			// Starting C must cancel B.
			var tokenC = manager.BeginLoad();

			Assert.True(tokenB.IsCancellationRequested);
			Assert.False(tokenC.IsCancellationRequested);
		}

		[Fact]
		public void CompletingCurrentLoadFinishesIt()
		{
			var manager = new ImageSourceServiceResultManager();

			var token = manager.BeginLoad();
			manager.CompleteLoad(null, token);

			Assert.False(manager.IsLoading);
		}

		[Fact]
		public void SupersededResultIsReleasedWhenTheNextLoadBegins()
		{
			var manager = new ImageSourceServiceResultManager();
			var staleResult = new DisposableResult();
			var currentResult = new DisposableResult();

			var tokenA = manager.BeginLoad();
			var tokenB = manager.BeginLoad();

			manager.CompleteLoad(staleResult, tokenA);
			// The superseded result may still be displayed until the current load replaces it.
			Assert.False(staleResult.IsDisposed);

			manager.CompleteLoad(currentResult, tokenB);
			Assert.False(manager.IsLoading);

			manager.BeginLoad();

			Assert.True(staleResult.IsDisposed);
			Assert.True(currentResult.IsDisposed);
		}

		class DisposableResult : IDisposable
		{
			public bool IsDisposed { get; private set; }

			public void Dispose() => IsDisposed = true;
		}
	}
}
