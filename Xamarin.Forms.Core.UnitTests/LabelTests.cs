using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using NUnit.Framework;
using NUnit.Framework.Constraints;

namespace Xamarin.Forms.Core.UnitTests
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
		public void AssignToFontStructUpdatesFontFamily(
			[Values(NamedSize.Default, NamedSize.Large, NamedSize.Medium, NamedSize.Small, NamedSize.Micro)] NamedSize size,
			[Values(FontAttributes.None, FontAttributes.Bold, FontAttributes.Italic, FontAttributes.Bold | FontAttributes.Italic)] FontAttributes attributes)
		{
			var label = new Label();
			double startSize = label.FontSize;
			var startAttributes = label.FontAttributes;

			bool firedSizeChanged = false;
			bool firedAttributesChanged = false;
			label.PropertyChanged += (sender, args) =>
			{
				if (args.PropertyName == Label.FontSizeProperty.PropertyName)
					firedSizeChanged = true;
				if (args.PropertyName == Label.FontAttributesProperty.PropertyName)
					firedAttributesChanged = true;
			};

			label.Font = Font.OfSize("Testing123", size).WithAttributes(attributes);

			Assert.AreEqual(Device.GetNamedSize(size, typeof(Label), true), label.FontSize);
			Assert.AreEqual(attributes, label.FontAttributes);
			Assert.AreEqual(startSize != label.FontSize, firedSizeChanged);
			Assert.AreEqual(startAttributes != label.FontAttributes, firedAttributesChanged);
		}

		[Test]
		public void AssignToFontFamilyUpdatesFont()
		{
			var label = new Label();

			label.FontFamily = "CrazyFont";
			Assert.AreEqual(label.Font, Font.OfSize("CrazyFont", label.FontSize));
		}

		[Test]
		public void AssignToFontSizeUpdatesFont()
		{
			var label = new Label();

			label.FontSize = 1000;
			Assert.AreEqual(label.Font, Font.SystemFontOfSize(1000));
		}

		[Test]
		public void AssignedToFontSizeUpdatesFontDouble()
		{
			var label = new Label();

			label.FontSize = 10.7;
			Assert.AreEqual(label.Font, Font.SystemFontOfSize(10.7));
		}

		[Test]
		public void AssignedToFontSizeDouble()
		{
			var label = new Label();

			label.FontSize = 10.7;
			Assert.AreEqual(label.FontSize, 10.7);
		}


		[Test]
		public void AssignToFontAttributesUpdatesFont()
		{
			var label = new Label();

			label.FontAttributes = FontAttributes.Italic | FontAttributes.Bold;
			Assert.AreEqual(label.Font, Font.SystemFontOfSize(label.FontSize, FontAttributes.Bold | FontAttributes.Italic));
		}

		[Test]
		public void LabelResizesWhenFontChanges()
		{
			Device.PlatformServices = new MockPlatformServices(getNativeSizeFunc: (ve, w, h) =>
			{
				var l = (Label)ve;
				return new SizeRequest(new Size(l.Font.FontSize, l.Font.FontSize));
			});

			var label = new Label { IsPlatformEnabled = true };

			Assert.AreEqual(label.Font.FontSize, label.GetSizeRequest(double.PositiveInfinity, double.PositiveInfinity).Request.Width);

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

			label.SetValue(Label.FontProperty, Font.SystemFontOfSize(1.0), fromStyle: true);
			Assert.AreEqual(2.0, label.FontSize);
		}

		[Test]
		public void ChangingHorizontalTextAlignmentFiresXAlignChanged()
		{
			var label = new Label() { HorizontalTextAlignment = TextAlignment.Center };

			var xAlignFired = false;
			var horizontalTextAlignmentFired = false;

			label.PropertyChanged += (sender, args) =>
			{
				if (args.PropertyName == "XAlign")
				{
					xAlignFired = true;
				}
				else if (args.PropertyName == Label.HorizontalTextAlignmentProperty.PropertyName)
				{
					horizontalTextAlignmentFired = true;
				}
			};

			label.HorizontalTextAlignment = TextAlignment.End;

			Assert.True(xAlignFired);
			Assert.True(horizontalTextAlignmentFired);
		}

		[Test]
		public void ChangingVerticalTextAlignmentFiresYAlignChanged()
		{
			var label = new Label() { VerticalTextAlignment = TextAlignment.Center };

			var yAlignFired = false;
			var verticalTextAlignmentFired = false;

			label.PropertyChanged += (sender, args) =>
			{
				if (args.PropertyName == "YAlign")
				{
					yAlignFired = true;
				}
				else if (args.PropertyName == Label.VerticalTextAlignmentProperty.PropertyName)
				{
					verticalTextAlignmentFired = true;
				}
			};

			label.VerticalTextAlignment = TextAlignment.End;

			Assert.True(yAlignFired);
			Assert.True(verticalTextAlignmentFired);
		}

		[Test]
		public void EntryCellXAlignBindingMatchesHorizontalTextAlignmentBinding()
		{
			var vm = new ViewModel();
			vm.HorizontalAlignment = TextAlignment.Center;

			var labelXAlign = new Label() { BindingContext = vm };
			labelXAlign.SetBinding(Label.XAlignProperty, new Binding("HorizontalAlignment"));

			var labelHorizontalTextAlignment = new Label() { BindingContext = vm };
			labelHorizontalTextAlignment.SetBinding(Label.HorizontalTextAlignmentProperty, new Binding("HorizontalAlignment"));

			Assert.AreEqual(TextAlignment.Center, labelXAlign.XAlign);
			Assert.AreEqual(TextAlignment.Center, labelHorizontalTextAlignment.HorizontalTextAlignment);

			vm.HorizontalAlignment = TextAlignment.End;

			Assert.AreEqual(TextAlignment.End, labelXAlign.XAlign);
			Assert.AreEqual(TextAlignment.End, labelHorizontalTextAlignment.HorizontalTextAlignment);
		}

		[Test]
		public void EntryCellYAlignBindingMatchesVerticalTextAlignmentBinding()
		{
			var vm = new ViewModel();
			vm.VerticalAlignment = TextAlignment.Center;

			var labelYAlign = new Label() { BindingContext = vm };
			labelYAlign.SetBinding(Label.YAlignProperty, new Binding("VerticalAlignment"));

			var labelVerticalTextAlignment = new Label() { BindingContext = vm };
			labelVerticalTextAlignment.SetBinding(Label.VerticalTextAlignmentProperty, new Binding("VerticalAlignment"));

			Assert.AreEqual(TextAlignment.Center, labelYAlign.YAlign);
			Assert.AreEqual(TextAlignment.Center, labelVerticalTextAlignment.VerticalTextAlignment);

			vm.VerticalAlignment = TextAlignment.End;

			Assert.AreEqual(TextAlignment.End, labelYAlign.YAlign);
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