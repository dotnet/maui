using System;
using System.Collections.Generic;
using Microsoft.Maui;
using Xunit;

namespace Microsoft.Maui.UnitTests
{
	public class WebViewRequestLifetimeTests
	{
		[Fact]
		public void StopDropsQueuedAndSubsequentCallbacks()
		{
			var queue = new Queue<Action>();
			var callbacks = 0;
			var completions = 0;
			var lifetime = new WebViewRequestLifetime(queue.Enqueue, () => completions++);

			lifetime.Invoke(() => callbacks++);
			lifetime.Stop();
			lifetime.Invoke(() => callbacks++, complete: true);
			while (queue.Count > 0)
				queue.Dequeue()();

			Assert.Equal(0, callbacks);
			Assert.Equal(1, completions);
		}

		[Fact]
		public void CompletionAndStopAreIdempotent()
		{
			var callbacks = 0;
			var completions = 0;
			var lifetime = new WebViewRequestLifetime(action => action(), () => completions++);

			lifetime.Invoke(() => callbacks++);
			lifetime.Invoke(() => callbacks++, complete: true);
			lifetime.Invoke(() => callbacks++);
			lifetime.Invoke(() => callbacks++, complete: true);
			lifetime.Stop();
			lifetime.Stop();

			Assert.Equal(2, callbacks);
			Assert.Equal(1, completions);
		}

		[Fact]
		public void ReentrantStopPreventsLaterResponseCallbacks()
		{
			var callbacks = 0;
			var lifetime = new WebViewRequestLifetime(action => action(), () => { });

			lifetime.Invoke(() => lifetime.Stop());
			lifetime.Invoke(() => callbacks++);
			lifetime.Invoke(() => callbacks++, complete: true);

			Assert.Equal(0, callbacks);
		}

		[Fact]
		public void StopBetweenHeadersAndBodyDropsBodyAndFinish()
		{
			var responseCallbacks = new List<string>();
			var lifetime = new WebViewRequestLifetime(action => action(), () => { });

			lifetime.Invoke(() => responseCallbacks.Add("headers"));
			lifetime.Stop();
			lifetime.Invoke(() => responseCallbacks.Add("body"));
			lifetime.Invoke(() => responseCallbacks.Add("finish"), complete: true);

			Assert.Equal(new[] { "headers" }, responseCallbacks);
		}

		[Fact]
		public void FailedTerminalCallbackStillReleasesTracking()
		{
			var completions = 0;
			var lifetime = new WebViewRequestLifetime(action => action(), () => completions++);

			Assert.Throws<InvalidOperationException>(() =>
				lifetime.Invoke(() => throw new InvalidOperationException(), complete: true));
			lifetime.Stop();

			Assert.Equal(1, completions);
		}
	}
}
