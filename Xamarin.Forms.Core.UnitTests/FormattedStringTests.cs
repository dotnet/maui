using System;
using System.Collections.ObjectModel;
using NUnit.Framework;

namespace Xamarin.Forms.Core.UnitTests
{
	[TestFixture]
	public class FormattedStringTests : BaseTestFixture
	{
		[SetUp]
		public override void Setup()
		{
			base.Setup();
			Device.PlatformServices = new MockPlatformServices();
		}

		[TearDown]
		public override void TearDown()
		{
			base.Setup();
			Device.PlatformServices = null;
		}

		[Test]
		public void NullSpansNotAllowed()
		{
			var fs = new FormattedString();
			Assert.That(() => fs.Spans.Add(null), Throws.InstanceOf<ArgumentNullException>());

			fs = new FormattedString();
			fs.Spans.Add(new Span());

			Assert.That(() =>
			{
				fs.Spans[0] = null;
			}, Throws.InstanceOf<ArgumentNullException>());
		}

		[Test]
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

			Assert.That(spansChanged, Is.True);
		}

		[Test]
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

			Assert.That(spansChanged, Is.False);
		}

		[Test]
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

			Assert.That(spansChanged, Is.True);
		}

		[Test]
		public void ImplicitStringConversion()
		{
			string original = "fubar";
			FormattedString fs = original;
			Assert.That(fs, Is.Not.Null);
			Assert.That(fs.Spans.Count, Is.EqualTo(1));
			Assert.That(fs.Spans[0], Is.Not.Null);
			Assert.That(fs.Spans[0].Text, Is.EqualTo(original));
		}

		[Test]
		public void ImplicitStringConversionNull()
		{
			string original = null;
			FormattedString fs = original;
			Assert.That(fs, Is.Not.Null);
			Assert.That(fs.Spans.Count, Is.EqualTo(1));
			Assert.That(fs.Spans[0], Is.Not.Null);
			Assert.That(fs.Spans[0].Text, Is.EqualTo(original));
		}
	}
}