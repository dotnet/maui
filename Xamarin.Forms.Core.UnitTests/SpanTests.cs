using NUnit.Framework;

namespace Xamarin.Forms.Core.UnitTests
{
	[TestFixture]
	public class SpanTests : BaseTestFixture
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
			base.TearDown();
			Device.PlatformServices = null;
		}

		[Test]
		public void StyleApplied()
		{
			var pinkStyle = new Style(typeof(Span))
			{
				Setters = {
					new Setter { Property = Span.TextColorProperty, Value = Color.Pink },
				},
				Class = "pink",
				ApplyToDerivedTypes = true,
			};

			var span = new Span
			{
				Style = pinkStyle
			};

			var formattedText = new FormattedString();
			formattedText.Spans.Add(span);

			var label = new Label()
			{
				FormattedText = formattedText
			};

			new ContentView
			{
				Resources = new ResourceDictionary { pinkStyle },
				Content = label
			};

			Assert.AreEqual(Color.Pink, span.TextColor);
		}

		[Test]
		public void BindingApplied()
		{
			var vm = new ViewModel()
			{
				Text = "CheckBindingWorked"
			};

			var formattedText = new FormattedString();

			var label = new Label()
			{
				FormattedText = formattedText
			};

			var span = new Span();
			span.SetBinding(Span.TextProperty, "Text");

			formattedText.Spans.Add(span);

			label.BindingContext = vm;

			Assert.AreEqual(vm.Text, span.Text);
		}

		class ViewModel
		{
			public string Text { get; set; }
		}
	}
}