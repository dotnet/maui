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
		public async Task FormattedStringDoesNotLeak()
		{
			// Long-lived span, like one shared from a view-model or App.Resources.
			// Adding it to a FormattedString subscribes FormattedString to the
			// span's PropertyChanged/PropertyChanging events and makes FormattedString
			// the span's logical parent. If those references aren't weak, the shared
			// span keeps every FormattedString it was added to alive.
			var span = new Span { Text = "Hello" };

			WeakReference CreateReference()
			{
				var fs = new FormattedString();
				fs.Spans.Add(span);
				return new(fs);
			}

			WeakReference reference = CreateReference();

			await TestHelpers.Collect();

			Assert.False(await reference.WaitForCollect(), "FormattedString should not be alive!");

			// Ensure the shared Span isn't collected during the test
			GC.KeepAlive(span);
		}
	}
}