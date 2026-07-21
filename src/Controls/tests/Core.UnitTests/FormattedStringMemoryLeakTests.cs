using System;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	public class FormattedStringMemoryLeakTests : BaseTestFixture
	{
		/// <summary>
		/// Verifies that a shared, long-lived <see cref="Span"/> added to a
		/// <see cref="FormattedString"/> does not retain that <see cref="FormattedString"/>.
		/// Reproduces issue #36517: the per-span PropertyChanging/PropertyChanged subscription
		/// was non-weak, so the span's event invocation list strongly rooted the FormattedString
		/// (and any Label owning it) until the span itself was collected.
		/// </summary>
		[Fact, Category(TestCategory.Memory)]
		public async Task SharedSpanDoesNotLeakFormattedString()
		{
			// A shared / long-lived span that outlives the FormattedString (e.g. reused across
			// pages, held by a view-model, or stored in a static).
			var sharedSpan = new Span { Text = "shared" };

			WeakReference CreateFormattedStringReference()
			{
				var fs = new FormattedString();
				fs.Spans.Add(sharedSpan);
				return new WeakReference(fs);
			}

			var reference = CreateFormattedStringReference();

			Assert.False(await reference.WaitForCollect(), "FormattedString should be collected once the only remaining reference is the shared Span, but it was retained by the non-weak Span.PropertyChanged/PropertyChanging subscription.");

			// Keep the shared span alive for the duration of the test.
			GC.KeepAlive(sharedSpan);
		}
	}
}
