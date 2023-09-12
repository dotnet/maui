using System;
using System.Collections.ObjectModel;
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
	}
}