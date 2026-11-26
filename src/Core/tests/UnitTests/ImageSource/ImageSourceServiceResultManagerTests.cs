using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Maui.UnitTests.ImageSource
{
	[Category(TestCategory.Core, TestCategory.ImageSource)]
	public class ImageSourceServiceResultManagerTests
	{
		[Fact]
		public void CompletingSupersededLoadKeepsCurrentLoadCancellable()
		{
			var manager = new ImageSourceServiceResultManager();

			var tokenA = manager.BeginLoad();
			var tokenB = manager.BeginLoad();
			Assert.True(tokenA.IsCancellationRequested);

			manager.CompleteLoad(null, tokenA);

			Assert.True(manager.IsLoading);
			Assert.Equal(tokenB, manager.Token);

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
			Assert.False(staleResult.IsDisposed);

			manager.CompleteLoad(currentResult, tokenB);
			Assert.False(manager.IsLoading);

			manager.BeginLoad();

			Assert.True(staleResult.IsDisposed);
			Assert.True(currentResult.IsDisposed);
		}

		[Fact]
		public async Task SupersededLoadsCanCompleteWhileNewLoadsBegin()
		{
			var manager = new ImageSourceServiceResultManager();
			var staleTokens = new System.Collections.Concurrent.BlockingCollection<CancellationToken>();

			var completer = Task.Run(() =>
			{
				foreach (var token in staleTokens.GetConsumingEnumerable())
				{
					manager.CompleteLoad(new DisposableResult(), token);
				}
			});

			for (int i = 0; i < 20000; i++)
			{
				staleTokens.Add(manager.BeginLoad());
			}

			staleTokens.CompleteAdding();
			await completer;

			var lastToken = manager.BeginLoad();
			manager.CompleteLoad(null, lastToken);

			Assert.False(manager.IsLoading);
		}

		[Fact]
		public void ResetKeepsLoadStartedWhileDisposingTheCurrentResult()
		{
			var manager = new ImageSourceServiceResultManager();
			var reentrantToken = default(CancellationToken);
			var result = new DisposableResult(() => reentrantToken = manager.BeginLoad());

			var tokenA = manager.BeginLoad();
			manager.CompleteLoad(result, tokenA);

			manager.Reset();

			Assert.True(result.IsDisposed);
			Assert.True(manager.IsLoading);
			Assert.Equal(reentrantToken, manager.Token);
			Assert.False(reentrantToken.IsCancellationRequested);

			manager.CompleteLoad(null, reentrantToken);

			Assert.False(manager.IsLoading);
		}

		class DisposableResult : IDisposable
		{
			readonly Action _onDispose;

			public DisposableResult(Action onDispose = null) => _onDispose = onDispose;

			public bool IsDisposed { get; private set; }

			public void Dispose()
			{
				if (IsDisposed)
					return;

				IsDisposed = true;
				_onDispose?.Invoke();
			}
		}
	}
}
