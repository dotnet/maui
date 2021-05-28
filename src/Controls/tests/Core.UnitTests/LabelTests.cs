using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Graphics;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	[TestFixture]
	public class LabelTests : BaseTestFixture
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
		public void TextAndAttributedTextMutuallyExclusive()
		{
			var label = new Label();
			Assert.IsNull(label.Text);
			Assert.IsNull(label.FormattedText);

			label.Text = "Foo";
			Assert.AreEqual("Foo", label.Text);
			Assert.IsNull(label.FormattedText);

			var fs = new FormattedString();
			label.FormattedText = fs;
			Assert.IsNull(label.Text);
			Assert.AreSame(fs, label.FormattedText);

			label.Text = "Foo";
			Assert.AreEqual("Foo", label.Text);
			Assert.IsNull(label.FormattedText);
		}

		[Test]
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
			Assert.IsTrue(fired);

			fired = false;
			label.TextTransform = TextTransform.Lowercase;
			Assert.IsTrue(fired);

			fired = false;
			label.TextTransform = TextTransform.Uppercase;
			Assert.IsTrue(fired);

			fired = false;
			label.TextTransform = TextTransform.None;
			Assert.IsTrue(fired);

			var fs = new FormattedString();

			fired = false;
			label.FormattedText = fs;
			Assert.IsTrue(fired);

			fired = false;
			fs.Spans.Add(new Span { Text = "bar" });
			Assert.IsTrue(fired);
		}

		[Test]
		public void AssignedToFontSizeDouble()
		{
			var label = new Label();

			label.FontSize = 10.7;
			Assert.AreEqual(label.FontSize, 10.7);
		}

		[Test]
		public void LabelResizesWhenFontChanges()
		{
			Device.PlatformServices = new MockPlatformServices(getNativeSizeFunc: (ve, w, h) =>
			{
				var l = (Label)ve;
				return new SizeRequest(new Size(l.FontSize, l.FontSize));
			});

			var label = new Label { IsPlatformEnabled = true };

			Assert.AreEqual(label.FontSize, label.GetSizeRequest(double.PositiveInfinity, double.PositiveInfinity).Request.Width);

			bool fired = false;

			label.MeasureInvalidated += (sender, args) =>
			{
				Assert.AreEqual(25, label.GetSizeRequest(double.PositiveInfinity, double.PositiveInfinity).Request.Width);
				fired = true;
			};


			label.FontSize = 25;

			Assert.True(fired);
		}

		[Test]
		public void FontSizeConverterTests()
		{
			var converter = new FontSizeConverter();
			Assert.AreEqual(12, converter.ConvertFromInvariantString("12"));
			Assert.AreEqual(10.7, converter.ConvertFromInvariantString("10.7"));
		}

		[Test]
		public void FontSizeCanBeSetFromStyle()
		{
			var label = new Label();

			Assert.AreEqual(10.0, label.FontSize);

			label.SetValue(Label.FontSizeProperty, 1.0, true);
			Assert.AreEqual(1.0, label.FontSize);
		}

		[Test]
		public void ManuallySetFontSizeNotOverridenByStyle()
		{
			var label = new Label();
			Assume.That(label.FontSize, Is.EqualTo(10.0));

			label.SetValue(Label.FontSizeProperty, 2.0, false);
			Assert.AreEqual(2.0, label.FontSize);

			label.SetValue(Label.FontSizeProperty, 1.0, true);
			Assert.AreEqual(2.0, label.FontSize);
		}

		[Test]
		public void ManuallySetFontSizeNotOverridenByFontSetInStyle()
		{
			var label = new Label();
			Assume.That(label.FontSize, Is.EqualTo(10.0));

			label.SetValue(Label.FontSizeProperty, 2.0);
			Assert.AreEqual(2.0, label.FontSize);
		}

		[Test]
		public void EntryCellXAlignBindingMatchesHorizontalTextAlignmentBinding()
		{
			var vm = new ViewModel();
			vm.HorizontalAlignment = TextAlignment.Center;

			var labelHorizontalTextAlignment = new Label() { BindingContext = vm };
			labelHorizontalTextAlignment.SetBinding(Label.HorizontalTextAlignmentProperty, new Binding("HorizontalAlignment"));

			Assert.AreEqual(TextAlignment.Center, labelHorizontalTextAlignment.HorizontalTextAlignment);

			vm.HorizontalAlignment = TextAlignment.End;

			Assert.AreEqual(TextAlignment.End, labelHorizontalTextAlignment.HorizontalTextAlignment);
		}

		[Test]
		public void EntryCellYAlignBindingMatchesVerticalTextAlignmentBinding()
		{
			var vm = new ViewModel();
			vm.VerticalAlignment = TextAlignment.Center;

			var labelVerticalTextAlignment = new Label() { BindingContext = vm };
			labelVerticalTextAlignment.SetBinding(Label.VerticalTextAlignmentProperty, new Binding("VerticalAlignment"));

			Assert.AreEqual(TextAlignment.Center, labelVerticalTextAlignment.VerticalTextAlignment);

			vm.VerticalAlignment = TextAlignment.End;

			Assert.AreEqual(TextAlignment.End, labelVerticalTextAlignment.VerticalTextAlignment);
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

