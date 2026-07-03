using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{

	public class FormattedStringTests : BaseTestFixture
	{
		[Fact]
		public void NullSpansNotAllowed()
		{
			var fs = new FormattedString();
			Assert.Throws<ArgumentNullException>(() => fs.Spans.Add(null));

			fs = new FormattedString();
			fs.Spans.Add(new Span());

			Assert.Throws<ArgumentNullException>(() =>
			{
				fs.Spans[0] = null;
			});
		}

		[Fact]
		public void SpanChangeTriggersSpansPropertyChange()
		{
			var span = new Span();
			var fs = new FormattedString();
			fs.Spans.Add(span);

			bool spansChanged = false;
			fs.PropertyChanged += (s, e) =>
			{
				if (e.PropertyName == "Spans")
					spansChanged = true;
			};

			span.Text = "New text";

			Assert.True(spansChanged);
		}

		[Fact]
		public void SpanChangesUnsubscribes()
		{
			var span = new Span();
			var fs = new FormattedString();
			fs.Spans.Add(span);
			fs.Spans.Remove(span);

			bool spansChanged = false;
			fs.PropertyChanged += (s, e) =>
			{
				if (e.PropertyName == "Spans")
					spansChanged = true;
			};

			span.Text = "New text";

			Assert.False(spansChanged);
		}

		[Fact]
		public void AddingSpanTriggersSpansPropertyChange()
		{
			var span = new Span();
			var fs = new FormattedString();

			bool spansChanged = false;
			fs.PropertyChanged += (s, e) =>
			{
				if (e.PropertyName == "Spans")
					spansChanged = true;
			};

			fs.Spans.Add(span);

			Assert.True(spansChanged);
		}

		[Fact]
		public void ImplicitStringConversion()
		{
			string original = "fubar";
			FormattedString fs = original;
			Assert.NotNull(fs);
			Assert.Single(fs.Spans);
			Assert.NotNull(fs.Spans[0]);
			Assert.Equal(fs.Spans[0].Text, original);
		}

		[Fact]
		public void ImplicitStringConversionNull()
		{
			string original = null;
			FormattedString fs = original;
			Assert.NotNull(fs);
			Assert.Single(fs.Spans);
			Assert.NotNull(fs.Spans[0]);
			Assert.Equal(fs.Spans[0].Text, original);
		}

		[Fact, Category(TestCategory.Memory)]
		public async Task LabelIsNotKeptAliveBySharedFormattedText()
		{
			// A long-lived/shared FormattedString (e.g. from a view-model or
			// App.Resources) that is assigned to a Label wires three *strong*
			// subscriptions from the FormattedString back to the Label:
			//   formattedString.PropertyChanging  += label.OnFormattedTextChanging
			//   formattedString.PropertyChanged   += label.OnFormattedTextChanged
			//   formattedString.SpansCollectionChanged += label.Span_CollectionChanged
			// These are only unwired when the Label's FormattedText is reassigned,
			// never when the Label alone is collected. If the FormattedString
			// outlives the Label, those strong delegates keep the Label alive.
			var formattedString = new FormattedString();
			formattedString.Spans.Add(new Span { Text = "Hello" });

			WeakReference CreateReference()
			{
				var label = new Label { FormattedText = formattedString };
				return new(label);
			}

			WeakReference reference = CreateReference();

			await TestHelpers.Collect();

			Assert.False(await reference.WaitForCollect(), "Label should not be alive!");

			// Ensure the shared FormattedString isn't collected during the test
			GC.KeepAlive(formattedString);
		}

		[Fact, Category(TestCategory.Memory)]
		public async Task LabelIsNotKeptAliveBySpanGestureRecognizers()
		{
			// A Span exposes a GestureRecognizers collection, and the Label wires a
			// subscription to each Span's GestureRecognizersCollectionChanged so it can
			// forward gesture changes. If that subscription is strong, a long-lived/shared
			// FormattedString (whose Spans carry gesture recognizers) keeps every Label it
			// was ever assigned to alive. This verifies the Span -> Label path is weak too.
			var formattedString = new FormattedString();
			var span = new Span { Text = "Tap me" };
			span.GestureRecognizers.Add(new TapGestureRecognizer());
			formattedString.Spans.Add(span);

			WeakReference CreateReference()
			{
				var label = new Label { FormattedText = formattedString };
				return new(label);
			}

			WeakReference reference = CreateReference();

			await TestHelpers.Collect();

			Assert.False(await reference.WaitForCollect(), "Label should not be alive!");

			// Ensure the shared FormattedString (and its Span) isn't collected during the test
			GC.KeepAlive(formattedString);
		}
	}
}