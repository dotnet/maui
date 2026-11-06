using System;
using Microsoft.Maui.Controls.StyleSheets;
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
		public void StyleClassAliasesExposeTheSameClasses()
		{
			var span = new Span();
			Assert.Null(span.StyleClass);
			Assert.Null(span.@class);

			var classes = new[] { "accent", "emphasis" };
			span.StyleClass = classes;
			Assert.Same(classes, span.@class);
			Assert.Same(classes, ((IStyleSelectable)span).Classes);

			var replacement = new[] { "warning" };
			span.@class = replacement;
			Assert.Same(replacement, span.StyleClass);
			Assert.Same(replacement, ((IStyleSelectable)span).Classes);
		}

		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public void ClassStyleAppliedBeforeOrAfterParenting(bool assignAfterParenting)
		{
			var span = new Span();
			if (!assignAfterParenting)
				span.StyleClass = new[] { "accent" };

			var label = new Label { FormattedText = new FormattedString { Spans = { span } } };
			var parent = new ContentView
			{
				Resources = new ResourceDictionary { CreateClassStyle("accent", Colors.Blue) },
				Content = label
			};

			if (assignAfterParenting)
				span.StyleClass = new[] { "accent" };

			Assert.Equal(Colors.Blue, span.TextColor);
		}

		[Fact]
		public void ClassesApplyIndependentlyAndCompose()
		{
			var accent = new Span { StyleClass = new[] { "accent" } };
			var warning = new Span { StyleClass = new[] { "warning" } };
			var emphasized = new Span { StyleClass = new[] { "accent", "emphasis" } };
			var label = new Label { FormattedText = new FormattedString { Spans = { accent, warning, emphasized } } };
			var parent = new ContentView
			{
				Resources = new ResourceDictionary
				{
					CreateClassStyle("accent", Colors.Blue),
					CreateClassStyle("warning", Colors.Red),
					new Style(typeof(Span))
					{
						Class = "emphasis",
						Setters = { new Setter { Property = Span.FontAttributesProperty, Value = FontAttributes.Bold } }
					}
				},
				Content = label
			};

			Assert.Equal(Colors.Blue, accent.TextColor);
			Assert.Equal(Colors.Red, warning.TextColor);
			Assert.Equal(Colors.Blue, emphasized.TextColor);
			Assert.Equal(FontAttributes.Bold, emphasized.FontAttributes);
			Assert.Equal(FontAttributes.None, accent.FontAttributes);
			Assert.Equal(FontAttributes.None, warning.FontAttributes);
		}

		[Theory]
		[InlineData(null)]
		[InlineData("")]
		[InlineData("missing")]
		[InlineData("warning")]
		public void ReplacingClassesRemovesPreviousSetters(string replacement)
		{
			var span = new Span { StyleClass = new[] { "accent" } };
			var accentStyle = CreateClassStyle("accent", Colors.Blue);
			accentStyle.Setters.Add(new Setter { Property = Span.FontAttributesProperty, Value = FontAttributes.Bold });
			var label = new Label { FormattedText = new FormattedString { Spans = { span } } };
			var parent = new ContentView
			{
				Resources = new ResourceDictionary
				{
					new Style(typeof(Span))
					{
						Setters = { new Setter { Property = Span.TextColorProperty, Value = Colors.Black } }
					},
					accentStyle,
					CreateClassStyle("warning", Colors.Red)
				},
				Content = label
			};
			Assert.Equal(Colors.Blue, span.TextColor);
			Assert.Equal(FontAttributes.Bold, span.FontAttributes);

			span.StyleClass = replacement == null ? null : replacement.Length == 0 ? Array.Empty<string>() : new[] { replacement };

			Assert.Equal(replacement == "warning" ? Colors.Red : Colors.Black, span.TextColor);
			Assert.Equal(FontAttributes.None, span.FontAttributes);
		}

		[Fact]
		public void ClassChangesPreserveExplicitStyleAndLocalValues()
		{
			var span = new Span
			{
				StyleClass = new[] { "accent" },
				Style = new Style(typeof(Span))
				{
					Setters = { new Setter { Property = Span.TextColorProperty, Value = Colors.Green } }
				}
			};
			var label = new Label { FormattedText = new FormattedString { Spans = { span } } };
			var parent = new ContentView
			{
				Resources = new ResourceDictionary
				{
					CreateClassStyle("accent", Colors.Blue),
					CreateClassStyle("warning", Colors.Red)
				},
				Content = label
			};

			Assert.Equal(Colors.Green, span.TextColor);
			span.StyleClass = new[] { "warning" };
			Assert.Equal(Colors.Green, span.TextColor);
			span.TextColor = Colors.Purple;
			span.StyleClass = null;
			Assert.Equal(Colors.Purple, span.TextColor);
			span.ClearValue(Span.TextColorProperty);
			Assert.Equal(Colors.Green, span.TextColor);
		}

		[Fact]
		public void ClassStyleUpdatesWhenResourcesAreReplacedOrSpanIsReparented()
		{
			var span = new Span { StyleClass = new[] { "accent" } };
			var label = new Label { FormattedText = new FormattedString { Spans = { span } } };
			var firstParent = new ContentView
			{
				Resources = new ResourceDictionary { CreateClassStyle("accent", Colors.Blue) },
				Content = label
			};
			Assert.Equal(Colors.Blue, span.TextColor);

			firstParent.Resources = new ResourceDictionary { CreateClassStyle("accent", Colors.Red) };
			Assert.Equal(Colors.Red, span.TextColor);

			firstParent.Content = null;
			var secondParent = new ContentView
			{
				Resources = new ResourceDictionary { CreateClassStyle("accent", Colors.Green) },
				Content = label
			};
			Assert.Equal(Colors.Green, span.TextColor);
		}

		[Fact]
		public void ClassStyleSupportsDerivedSpans()
		{
			var span = new StyledSpan { StyleClass = new[] { "accent" } };
			var style = CreateClassStyle("accent", Colors.Blue);
			style.ApplyToDerivedTypes = true;
			var parent = new ContentView
			{
				Resources = new ResourceDictionary { style },
				Content = new Label { FormattedText = new FormattedString { Spans = { span } } }
			};

			Assert.Equal(Colors.Blue, span.TextColor);
		}

		[Fact]
		public void ClassChangeNotifiesLabelFormattedText()
		{
			var span = new Span { StyleClass = new[] { "accent" } };
			var label = new Label { FormattedText = new FormattedString { Spans = { span } } };
			var parent = new ContentView
			{
				Resources = new ResourceDictionary
				{
					CreateClassStyle("accent", Colors.Blue),
					CreateClassStyle("warning", Colors.Red)
				},
				Content = label
			};
			var notifications = 0;
			label.PropertyChanged += (_, e) =>
			{
				if (e.PropertyName == nameof(Label.FormattedText))
					notifications++;
			};

			span.StyleClass = new[] { "warning" };

			Assert.Equal(Colors.Red, span.TextColor);
			Assert.True(notifications > 0);
		}

		static Style CreateClassStyle(string name, Color color) => new Style(typeof(Span))
		{
			Class = name,
			Setters = { new Setter { Property = Span.TextColorProperty, Value = color } }
		};

		class StyledSpan : Span
		{
		}

		class ViewModel
		{
			public string Text { get; set; }
		}
	}
}
