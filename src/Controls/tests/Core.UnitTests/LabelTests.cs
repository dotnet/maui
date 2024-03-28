using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Maui;
using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{

	public class LabelTests : BaseTestFixture
	{
		[Fact]
		public void TextAndAttributedTextMutuallyExclusive()
		{
			var label = new Label();
			Assert.Null(label.Text);
			Assert.Null(label.FormattedText);

			label.Text = "Foo";
			Assert.Equal("Foo", label.Text);
			Assert.Null(label.FormattedText);

			var fs = new FormattedString();
			label.FormattedText = fs;
			Assert.Null(label.Text);
			Assert.Same(fs, label.FormattedText);

			label.Text = "Foo";
			Assert.Equal("Foo", label.Text);
			Assert.Null(label.FormattedText);
		}

		[Fact]
		public void InvalidateMeasureWhenTextChanges()
		{
			var label = new Label();

			bool fired;
			label.MeasureInvalidated += (sender, args) =>
			{
				fired = true;
			};

			fired = false;
			label.Text = "Foo";
			Assert.True(fired);

			fired = false;
			label.TextTransform = TextTransform.Lowercase;
			Assert.True(fired);

			fired = false;
			label.TextTransform = TextTransform.Uppercase;
			Assert.True(fired);

			fired = false;
			label.TextTransform = TextTransform.None;
			Assert.True(fired);

			var fs = new FormattedString();

			fired = false;
			label.FormattedText = fs;
			Assert.True(fired);

			fired = false;
			fs.Spans.Add(new Span { Text = "bar" });
			Assert.True(fired);
		}

		[Fact]
		public void AssignedToFontSizeDouble()
		{
			var label = new Label();

			label.FontSize = 10.7;
			Assert.Equal(10.7, label.FontSize);
		}

		[Fact]
		public void LabelResizesWhenFontChanges()
		{
			MockPlatformSizeService.Current.GetPlatformSizeFunc = (ve, w, h) =>
			{
				var l = (Label)ve;
				return new SizeRequest(new Size(l.FontSize, l.FontSize));
			};

			var label = new Label { IsPlatformEnabled = true };

			Assert.Equal(label.FontSize, label.Measure(double.PositiveInfinity, double.PositiveInfinity).Request.Width);

			bool fired = false;

			label.MeasureInvalidated += (sender, args) =>
			{
				Assert.Equal(25, label.Measure(double.PositiveInfinity, double.PositiveInfinity).Request.Width);
				fired = true;
			};


			label.FontSize = 25;

			Assert.True(fired);
		}

		[Fact]
		public void FontSizeConverterTests()
		{
			var converter = new FontSizeConverter();
			Assert.Equal(12d, converter.ConvertFromInvariantString("12"));
			Assert.Equal(10.7, converter.ConvertFromInvariantString("10.7"));
		}

		[Fact]
		public void FontSizeCanBeSetFromStyle()
		{
			var label = new Label();

			Assert.Equal(label.GetDefaultFontSize(), label.FontSize);

			label.SetValue(Label.FontSizeProperty, 1.0, new SetterSpecificity(SetterSpecificity.StyleImplicit, 0, 0, 0));
			Assert.Equal(1.0, label.FontSize);
		}

		[Fact]
		public void ManuallySetFontSizeNotOverridenByStyle()
		{
			var label = new Label();
			Assert.Equal(label.FontSize, label.GetDefaultFontSize());

			label.SetValue(Label.FontSizeProperty, 2.0);
			Assert.Equal(2.0, label.FontSize);

			label.SetValue(Label.FontSizeProperty, 1.0, new SetterSpecificity(SetterSpecificity.StyleImplicit, 0, 0, 0));
			Assert.Equal(2.0, label.FontSize);
		}

		[Fact]
		public void ManuallySetFontSizeNotOverridenByFontSetInStyle()
		{
			var label = new Label();
			Assert.Equal(label.FontSize, label.GetDefaultFontSize());

			label.SetValue(Label.FontSizeProperty, 2.0);
			Assert.Equal(2.0, label.FontSize);
		}

		[Fact]
		public void EntryCellXAlignBindingMatchesHorizontalTextAlignmentBinding()
		{
			var vm = new ViewModel();
			vm.HorizontalAlignment = TextAlignment.Center;

			var labelHorizontalTextAlignment = new Label() { BindingContext = vm };
			labelHorizontalTextAlignment.SetBinding(Label.HorizontalTextAlignmentProperty, new Binding("HorizontalAlignment"));

			Assert.Equal(TextAlignment.Center, labelHorizontalTextAlignment.HorizontalTextAlignment);

			vm.HorizontalAlignment = TextAlignment.End;

			Assert.Equal(TextAlignment.End, labelHorizontalTextAlignment.HorizontalTextAlignment);
		}

		[Fact]
		public void EntryCellYAlignBindingMatchesVerticalTextAlignmentBinding()
		{
			var vm = new ViewModel();
			vm.VerticalAlignment = TextAlignment.Center;

			var labelVerticalTextAlignment = new Label() { BindingContext = vm };
			labelVerticalTextAlignment.SetBinding(Label.VerticalTextAlignmentProperty, new Binding("VerticalAlignment"));

			Assert.Equal(TextAlignment.Center, labelVerticalTextAlignment.VerticalTextAlignment);

			vm.VerticalAlignment = TextAlignment.End;

			Assert.Equal(TextAlignment.End, labelVerticalTextAlignment.VerticalTextAlignment);
		}

		[Fact]
		public void OriginalLabelTextNotUpdatingAfterBindingIsSet()
		{
			var label = new Label() { Text = "sassifrass" };
			label.BindingContext = 1;
			label.SetBinding(Label.TextProperty, ".");
			Assert.Equal("1", label.Text);
		}

		[Fact]
		public void SpansInheritFormat ()
		{
			var label = new Label() {
				BackgroundColor = Colors.White,
				CharacterSpacing = 1,
				FontAttributes = FontAttributes.None,
				FontAutoScalingEnabled = true,
				FontFamily = "OpenSansRegular",
				FontSize = 16.0,
				LineHeight = 1,
				TextColor = Colors.Black,
				TextDecorations = TextDecorations.None,
				TextTransform = TextTransform.None,
			};
			var span1 = new Span() {
				Text = "one",
			};
			var span2 = new Span() {
				BackgroundColor = Colors.Black,
				CharacterSpacing = 2,
				FontAttributes = FontAttributes.Bold,
				FontAutoScalingEnabled = false,
				FontFamily = "OpenSansSemibold",
				FontSize = 24.0,
				LineHeight = 2,
				TextColor = Colors.White,
				TextDecorations = TextDecorations.Underline,
				TextTransform = TextTransform.Uppercase,
				Text = "two",
			};
			var formattedText = new FormattedString();
			formattedText.Spans.Add(span1);
			formattedText.Spans.Add(span2);
			label.FormattedText = formattedText;

			Assert.Equal(Colors.White, span1.BackgroundColor);
			Assert.Equal(1, span1.CharacterSpacing);
			Assert.Equal(FontAttributes.None, span1.FontAttributes);
			Assert.True(span1.FontAutoScalingEnabled);
			Assert.Equal("OpenSansRegular", span1.FontFamily);
			Assert.Equal(16.0, span1.FontSize);
			Assert.Equal(1, span1.LineHeight);
			Assert.Equal(Colors.Black, span1.TextColor);
			Assert.Equal(TextDecorations.None, span1.TextDecorations);
			Assert.Equal(TextTransform.None, span1.TextTransform);


			Assert.Equal(Colors.Black, span2.BackgroundColor);
			Assert.Equal(2, span2.CharacterSpacing);
			Assert.Equal(FontAttributes.Bold, span2.FontAttributes);
			Assert.False(span2.FontAutoScalingEnabled);
			Assert.Equal("OpenSansSemibold", span2.FontFamily);
			Assert.Equal(24.0, span2.FontSize);
			Assert.Equal(2, span2.LineHeight);
			Assert.Equal(Colors.White, span2.TextColor);
			Assert.Equal(TextDecorations.Underline, span2.TextDecorations);
			Assert.Equal(TextTransform.Uppercase, span2.TextTransform);
		}

		sealed class ViewModel : INotifyPropertyChanged
		{
			TextAlignment horizontalAlignment;
			TextAlignment verticalAlignment;

			public TextAlignment HorizontalAlignment
			{
				get { return horizontalAlignment; }
				set
				{
					horizontalAlignment = value;
					OnPropertyChanged();
				}
			}

			public TextAlignment VerticalAlignment
			{
				get { return verticalAlignment; }
				set
				{
					verticalAlignment = value;
					OnPropertyChanged();
				}
			}

			public event PropertyChangedEventHandler PropertyChanged;

			void OnPropertyChanged([CallerMemberName] string propertyName = null)
			{
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
			}
		}
	}
}

