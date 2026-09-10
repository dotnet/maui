using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	// FormattedString owns its Spans collection, so subscribing to that collection cannot outlive it.
	// A Span is different: the app can hold one, or share it between FormattedStrings, and a non-weak
	// PropertyChanged/PropertyChanging subscription then roots the FormattedString.
	public class FormattedStringWeakEventTests : BaseTestFixture
	{
		// https://github.com/dotnet/maui/issues/36289
		[Fact]
		public async Task RetainedSpanDoesNotRootFormattedString()
		{
			var shared = new Span { Text = "shared" };

			var reference = CreateFormattedString(shared);

			Assert.False(await reference.WaitForCollect(), "FormattedString should not be alive!");
			GC.KeepAlive(shared);
		}

		// A span removed from the collection must stop notifying entirely.
		[Fact]
		public async Task RemovedSpanDoesNotRootFormattedString()
		{
			var shared = new Span { Text = "shared" };

			var reference = CreateFormattedStringAndRemove(shared);

			Assert.False(await reference.WaitForCollect(), "FormattedString should not be alive after removal!");
			GC.KeepAlive(shared);
		}

		[Fact]
		public void SpanChangesStillRaisePropertyChanged()
		{
			var span = new Span { Text = "a" };
			var formatted = new FormattedString();
			formatted.Spans.Add(span);

			bool raised = false;
			formatted.PropertyChanged += (s, e) =>
			{
				if (e.PropertyName == nameof(FormattedString.Spans))
					raised = true;
			};

			span.Text = "b";

			Assert.True(raised);
			GC.KeepAlive(formatted);
		}

		[Fact]
		public void RemovedSpanNoLongerRaisesPropertyChanged()
		{
			var span = new Span { Text = "a" };
			var formatted = new FormattedString();
			formatted.Spans.Add(span);
			formatted.Spans.Remove(span);

			bool raised = false;
			formatted.PropertyChanged += (s, e) =>
			{
				if (e.PropertyName == nameof(FormattedString.Spans))
					raised = true;
			};

			span.Text = "b";

			Assert.False(raised);
			GC.KeepAlive(formatted);
		}

		[Fact]
		public void ClearDetachesSpans()
		{
			var span = new Span { Text = "a" };
			var formatted = new FormattedString();
			formatted.Spans.Add(span);
			formatted.Spans.Clear();

			bool raised = false;
			formatted.PropertyChanged += (s, e) =>
			{
				if (e.PropertyName == nameof(FormattedString.Spans))
					raised = true;
			};

			span.Text = "b";

			Assert.False(raised);
			GC.KeepAlive(formatted);
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		static WeakReference CreateFormattedString(Span span)
		{
			var formatted = new FormattedString();
			formatted.Spans.Add(span);
			return new WeakReference(formatted);
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		static WeakReference CreateFormattedStringAndRemove(Span span)
		{
			var formatted = new FormattedString();
			formatted.Spans.Add(span);
			formatted.Spans.Remove(span);
			return new WeakReference(formatted);
		}
	}
}
