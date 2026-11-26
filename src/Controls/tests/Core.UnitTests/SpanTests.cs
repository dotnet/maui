using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{

	public class SpanTests : BaseTestFixture
	{
		[Fact]
		public void StyleApplied()
		{
			var pinkStyle = new Style(typeof(Span))
			{
				Setters = {
					new Setter { Property = Span.TextColorProperty, Value = Colors.Pink },
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

			Assert.Equal(Colors.Pink, span.TextColor);
		}

		[Fact]
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

			Assert.Equal(vm.Text, span.Text);
		}

		[Fact]
		public void TextDecorationColorDefaultsToNull()
		{
			var span = new Span();

			Assert.Null(span.TextDecorationColor);
		}

		[Fact]
		public void ChangingTextDecorationColorUpdatesLabelFormattedText()
		{
			var span = new Span { Text = "underlined", TextDecorations = TextDecorations.Underline };
			var label = new Label { FormattedText = new FormattedString { Spans = { span } } };

			var formattedTextChanged = false;
			label.PropertyChanged += (_, e) =>
			{
				if (e.PropertyName == nameof(Label.FormattedText))
					formattedTextChanged = true;
			};

			span.TextDecorationColor = Colors.Orange;

			Assert.Equal(Colors.Orange, span.TextDecorationColor);
			Assert.True(formattedTextChanged);
		}

		[Fact]
		public void TextDecorationThicknessDefaultsToFontThickness()
		{
			var span = new Span();

			Assert.Equal(-1d, span.TextDecorationThickness);
		}

		[Fact]
		public void ChangingTextDecorationThicknessUpdatesLabelFormattedText()
		{
			var span = new Span { Text = "underlined", TextDecorations = TextDecorations.Underline };
			var label = new Label { FormattedText = new FormattedString { Spans = { span } } };

			var formattedTextChanged = false;
			label.PropertyChanged += (_, e) =>
			{
				if (e.PropertyName == nameof(Label.FormattedText))
					formattedTextChanged = true;
			};

			span.TextDecorationThickness = 1;

			Assert.Equal(1d, span.TextDecorationThickness);
			Assert.True(formattedTextChanged);
		}

		class ViewModel
		{
			public string Text { get; set; }
		}
	}
}
