using System;
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
			var label = MockPlatformSizeService.Sub<Label>((ve, w, h) =>
			{
				var l = (Label)ve;
				return new SizeRequest(new Size(l.FontSize, l.FontSize));
			});

			Assert.Equal(label.FontSize, label.Measure(double.PositiveInfinity, double.PositiveInfinity, MeasureFlags.None).Request.Width);

			bool fired = false;

			label.MeasureInvalidated += (sender, args) =>
			{
				Assert.Equal(25, label.Measure(double.PositiveInfinity, double.PositiveInfinity, MeasureFlags.None).Request.Width);
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

		// A shared FormattedString must not prevent Label GC after the Label's handler is disconnected.

		[Fact]
		public void SharedFormattedStringDoesNotPreventLabelGC()
		{
			var sharedFs = new FormattedString();
			sharedFs.Spans.Add(new Span { Text = "Hello " });
			sharedFs.Spans.Add(new Span { Text = "World", FontAttributes = FontAttributes.Bold });

			var weakRef = CreateLabelWithSharedFormattedString(sharedFs);

			RunGCLoop();

			Assert.False(weakRef.IsAlive,
				"Label should be GC-eligible after handler disconnect even when a shared FormattedString is still alive.");
			GC.KeepAlive(sharedFs);
		}

		[Fact]
		public void SharedFormattedStringWithGestureSpansDoesNotPreventLabelGC()
		{
			// Spans with gesture recognizers create an additional per-span retention path
			// via GestureRecognizersCollectionChanged. This test verifies that chain is also broken.
			var sharedFs = new FormattedString();
			var span = new Span { Text = "Tap me" };
			span.GestureRecognizers.Add(new TapGestureRecognizer());
			sharedFs.Spans.Add(span);

			var weakRef = CreateLabelWithSharedFormattedString(sharedFs);

			RunGCLoop();

			Assert.False(weakRef.IsAlive,
				"Label should be GC-eligible even when shared FormattedString has spans with gesture recognizers.");
			GC.KeepAlive(sharedFs);
		}

		[Fact]
		public void MultipleLabelsWithSharedFormattedStringAreAllGCEligible()
		{
			var sharedFs = new FormattedString();
			sharedFs.Spans.Add(new Span { Text = "Shared text" });

			var weakRefs = CreateMultipleLabelsWithSharedFormattedString(sharedFs, count: 5);

			RunGCLoop();

			for (int i = 0; i < weakRefs.Length; i++)
			{
				Assert.False(weakRefs[i].IsAlive,
					$"Label[{i}] should be GC-eligible after handler disconnect even when a shared FormattedString is still alive.");
			}
			GC.KeepAlive(sharedFs);
		}

		[Fact]
		public void LiveLabelIsNotCollectedWhenHandlerStillAttached()
		{
			// Sanity check: a Label whose handler is still attached must NOT be collected.
			var sharedFs = new FormattedString();
			sharedFs.Spans.Add(new Span { Text = "Shared" });

			var liveLabel = new Label { FormattedText = sharedFs };
			liveLabel.Handler = new HandlerStub();
			var liveRef = new WeakReference(liveLabel);

			var deadRef = CreateLabelWithSharedFormattedString(sharedFs);

			RunGCLoop();

			Assert.False(deadRef.IsAlive, "Disconnected Label should be collected.");
			Assert.True(liveRef.IsAlive, "Label still attached to a handler must NOT be collected.");
			GC.KeepAlive(liveLabel);
		}

		// Runs multiple GC cycles to improve collection reliability across GC generations.
		static void RunGCLoop(int iterations = 3)
		{
			for (int i = 0; i < iterations; i++)
			{
				GC.Collect();
				GC.WaitForPendingFinalizers();
				GC.Collect();
			}
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		static WeakReference CreateLabelWithSharedFormattedString(FormattedString sharedFs)
		{
			var label = new Label { FormattedText = sharedFs };
			// Simulate full page lifecycle: attach handler (page push) then disconnect (page pop).
			label.Handler = new HandlerStub();
			label.Handler = null;
			return new WeakReference(label);
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		static WeakReference[] CreateMultipleLabelsWithSharedFormattedString(FormattedString sharedFs, int count)
		{
			var refs = new WeakReference[count];
			for (int i = 0; i < count; i++)
			{
				var label = new Label { FormattedText = sharedFs };
				label.Handler = new HandlerStub();
				label.Handler = null;
				refs[i] = new WeakReference(label);
			}
			return refs;
		}
	}
}

