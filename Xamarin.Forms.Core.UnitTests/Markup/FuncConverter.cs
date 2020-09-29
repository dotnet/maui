using System.Globalization;
using NUnit.Framework;

namespace Xamarin.Forms.Markup.UnitTests
{
	[TestFixture(true)]
	[TestFixture(false)]
	public class FuncConverter : MarkupBaseTestFixture
	{
		public FuncConverter(bool withExperimentalFlag) : base(withExperimentalFlag) { }

		[Test]
		public void FullyTypedTwoWayWithParamAndCulture() => AssertExperimental(() =>
		{
			CultureInfo convertCulture = null, convertBackCulture = null;
			var expectedCulture = System.Threading.Thread.CurrentThread.CurrentUICulture;

			var converter = new FuncConverter<bool, Color, double>(
				(isRed, alpha, culture) => { convertCulture = culture; return (isRed ? Color.Red : Color.Green).MultiplyAlpha(alpha); },
				(color, alpha, culture) => { convertBackCulture = culture; return color == Color.Red.MultiplyAlpha(alpha); }
			).AssertConvert(true, 0.5, Color.Red.MultiplyAlpha(0.5), twoWay: true, culture: expectedCulture)
			 .AssertConvert(false, 0.2, Color.Green.MultiplyAlpha(0.2), twoWay: true, culture: expectedCulture);

			Assert.That(convertCulture, Is.EqualTo(expectedCulture));
			Assert.That(convertBackCulture, Is.EqualTo(expectedCulture));

			Assert.That(converter.Convert(null, typeof(object), null, CultureInfo.InvariantCulture), Is.EqualTo(Color.Green.MultiplyAlpha(default(double))));
			Assert.That(converter.ConvertBack(null, typeof(object), null, CultureInfo.InvariantCulture), Is.EqualTo(default(bool)));
		});

		[Test]
		public void FullyTypedTwoWayWithParam() => AssertExperimental(() =>
		{
			var converter = new FuncConverter<bool, Color, double>(
				(isRed, alpha) => (isRed ? Color.Red : Color.Green).MultiplyAlpha(alpha),
				(color, alpha) => color == Color.Red.MultiplyAlpha(alpha)
			).AssertConvert(true, 0.5, Color.Red.MultiplyAlpha(0.5), twoWay: true)
			 .AssertConvert(false, 0.2, Color.Green.MultiplyAlpha(0.2), twoWay: true);

			Assert.That(converter.Convert(null, typeof(object), null, CultureInfo.InvariantCulture), Is.EqualTo(Color.Green.MultiplyAlpha(default(double))));
			Assert.That(converter.ConvertBack(null, typeof(object), null, CultureInfo.InvariantCulture), Is.EqualTo(default(bool)));
		});

		[Test]
		public void FullyTypedTwoWay() => AssertExperimental(() =>
		{
			var converter = new FuncConverter<bool, Color, object>(
				isRed => isRed ? Color.Red : Color.Green,
				color => color == Color.Red
			).AssertConvert(true, Color.Red, twoWay: true)
			 .AssertConvert(false, Color.Green, twoWay: true);

			Assert.That(converter.Convert(null, typeof(object), null, CultureInfo.InvariantCulture), Is.EqualTo(Color.Green));
			Assert.That(converter.ConvertBack(null, typeof(object), null, CultureInfo.InvariantCulture), Is.EqualTo(default(bool)));
		});

		[Test]
		public void FullyTypedOneWayWithParam() => AssertExperimental(() =>
		{
			new FuncConverter<bool, Color, double>(
				(isRed, alpha) => (isRed ? Color.Red : Color.Green).MultiplyAlpha(alpha)
			).AssertConvert(true, 0.5, Color.Red.MultiplyAlpha(0.5))
			 .AssertConvert(false, 0.2, Color.Green.MultiplyAlpha(0.2));
		});

		[Test]
		public void FullyTypedOneWay() => AssertExperimental(() =>
		{
			new FuncConverter<bool, Color, object>(
				isRed => isRed ? Color.Red : Color.Green
			).AssertConvert(true, Color.Red)
			 .AssertConvert(false, Color.Green);
		});

		[Test]
		public void FullyTypedBackOnlyWithParam() => AssertExperimental(() =>
		{
			new FuncConverter<bool, Color, double>(
				null,
				(color, alpha) => color == Color.Red.MultiplyAlpha(alpha)
			).AssertConvert(true, 0.5, Color.Red.MultiplyAlpha(0.5), backOnly: true)
			 .AssertConvert(false, 0.2, Color.Green.MultiplyAlpha(0.2), backOnly: true);
		});

		[Test]
		public void FullyTypedBackOnly() => AssertExperimental(() =>
		{
			new FuncConverter<bool, Color, object>(
				null,
				color => color == Color.Red
			).AssertConvert(true, Color.Red, backOnly: true)
			 .AssertConvert(false, Color.Green, backOnly: true);
		});

		[Test]
		public void TwoWay() => AssertExperimental(() =>
		{
			new FuncConverter<bool, Color>(
				isRed => isRed ? Color.Red : Color.Green,
				color => color == Color.Red
			).AssertConvert(true, Color.Red, twoWay: true)
			 .AssertConvert(false, Color.Green, twoWay: true);
		});

		[Test]
		public void OneWay() => AssertExperimental(() =>
		{
			new FuncConverter<bool, Color>(
				isRed => isRed ? Color.Red : Color.Green
			).AssertConvert(true, Color.Red)
			 .AssertConvert(false, Color.Green);
		});

		[Test]
		public void BackOnly() => AssertExperimental(() =>
		{
			new FuncConverter<bool, Color>(
				null,
				color => color == Color.Red
			).AssertConvert(true, Color.Red, backOnly: true)
			 .AssertConvert(false, Color.Green, backOnly: true);
		});

		[Test]
		public void TypedSourceTwoWay() => AssertExperimental(() =>
		{
			new FuncConverter<bool>(
				isRed => isRed ? Color.Red : Color.Green,
				color => (Color)color == Color.Red
			).AssertConvert(true, Color.Red, twoWay: true)
			 .AssertConvert(false, Color.Green, twoWay: true);
		});

		[Test]
		public void TypedSourceOneWay() => AssertExperimental(() =>
		{
			new FuncConverter<bool>(
				isRed => isRed ? Color.Red : Color.Green
			).AssertConvert(true, Color.Red)
			 .AssertConvert(false, Color.Green);
		});

		[Test]
		public void TypedSourceBackOnly() => AssertExperimental(() =>
		{
			new FuncConverter<bool>(
				null,
				color => (Color)color == Color.Red
			).AssertConvert(true, (object)Color.Red, backOnly: true)
			 .AssertConvert(false, (object)Color.Green, backOnly: true);
		});

		[Test]
		public void UntypedTwoWay() => AssertExperimental(() =>
		{
			new Markup.FuncConverter(
				isRed => (bool)isRed ? Color.Red : Color.Green,
				color => (Color)color == Color.Red
			).AssertConvert((object)true, (object)Color.Red, twoWay: true)
			 .AssertConvert((object)false, (object)Color.Green, twoWay: true);
		});

		[Test]
		public void UntypedOneWay() => AssertExperimental(() =>
		{
			new Markup.FuncConverter(
				isRed => (bool)isRed ? Color.Red : Color.Green
			).AssertConvert((object)true, (object)Color.Red)
			 .AssertConvert((object)false, (object)Color.Green);
		});

		[Test]
		public void UntypedBackOnly() => AssertExperimental(() =>
		{
			new Markup.FuncConverter(
				null,
				color => (Color)color == Color.Red
			).AssertConvert((object)true, (object)Color.Red, backOnly: true)
			 .AssertConvert((object)false, (object)Color.Green, backOnly: true);
		});

		[Test]
		public void ToStringConverter() => AssertExperimental(() =>
		{
			new ToStringConverter("Converted {0}")
				.AssertConvert((object)3, "Converted 3");
		});

		[Test]
		public void ToStringConverterDefault() => AssertExperimental(() =>
		{
			new ToStringConverter()
				.AssertConvert((object)3, "3");
		});

		[Test]
		public void NotConverter()
		{
			if (withExperimentalFlag)
			{
				Markup.NotConverter.Instance // Ensure instance create path covered
					.AssertConvert(true, false, twoWay: true)
					.AssertConvert(false, true, twoWay: true);

				Markup.NotConverter.Instance // Ensure instance reuse path covered
					.AssertConvert(true, false, twoWay: true)
					.AssertConvert(false, true, twoWay: true);
			}
			else
			{
				AssertExperimental(() => { var _ = new NotConverter(); });
			}
		}
	}
}