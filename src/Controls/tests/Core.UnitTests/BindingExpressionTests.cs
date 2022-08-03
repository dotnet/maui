using System;
using System.Globalization;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	[TestFixture]
	public class BindingExpressionTests : BaseTestFixture
	{
		[Test]
		public void Ctor()
		{
			string path = "Foo.Bar";
			var binding = new Binding(path);

			var be = new BindingExpression(binding, path);

			Assert.AreSame(binding, be.Binding);
			Assert.AreEqual(path, be.Path);
		}

		[Test]
		public void CtorInvalid()
		{
			string path = "Foo.Bar";
			var binding = new Binding(path);

			Assert.Throws<ArgumentNullException>(() => new BindingExpression(binding, null),
				"Allowed the path to eb null");

			Assert.Throws<ArgumentNullException>(() => new BindingExpression(null, path),
				"Allowed the binding to be null");
		}

		[Test]
		public void ApplyNull()
		{
			const string path = "Foo.Bar";
			var binding = new Binding(path);
			var be = new BindingExpression(binding, path);
			Assert.DoesNotThrow(() => be.Apply(null, new MockBindable(), TextCell.TextProperty));
		}

		// We only throw on invalid path features, if they give an invalid property
		// name, it won't have compiled in the first place or they misstyped.
		[TestCase("Foo.")]
		[TestCase("Foo[]")]
		[TestCase("Foo.Bar[]")]
		[TestCase("Foo[1")]
		public void InvalidPaths(string path)
		{
			var fex = Assert.Throws<FormatException>(() =>
			{
				var binding = new Binding(path);
				new BindingExpression(binding, path);
			});

			Assert.IsFalse(String.IsNullOrWhiteSpace(fex.Message),
				"FormatException did not contain an explanation");
		}

		[Test]
		public void ValidPaths(
			[Values (
				".", "[1]", "[1 ]", ".[1]", ". [1]",
				"Foo", "Foo.Bar", "Foo. Bar", "Foo.Bar[1]",
				"Foo.Bar [1]")]
			string path,
			[Values(true, false)] bool spaceBefore,
			[Values(true, false)] bool spaceAfter)
		{
			if (spaceBefore)
				path = " " + path;
			if (spaceAfter)
				path = path + " ";

			var binding = new Binding(path);
			Assert.DoesNotThrow(() => new BindingExpression(binding, path));
		}

		static object[] TryConvertWithNumbersAndCulturesCases => new object[]
		{
			new object[]{ "4.2", new CultureInfo("en"), 4.2m },
			new object[]{ "4,2", new CultureInfo("de"), 4.2m },
			new object[]{ "-4.2", new CultureInfo("en"), -4.2m },
			new object[]{ "-4,2", new CultureInfo("de"), -4.2m },

			new object[]{ "4.2", new CultureInfo("en"), new decimal?(4.2m)},
			new object[]{ "4,2", new CultureInfo("de"), new decimal?(4.2m) },
			new object[]{ "-4.2", new CultureInfo("en"), new decimal?(-4.2m)},
			new object[]{ "-4,2", new CultureInfo("de"), new decimal?(-4.2m) },

			new object[]{ "4.2", new CultureInfo("en"), 4.2d },
			new object[]{ "4,2", new CultureInfo("de"), 4.2d },
			new object[]{ "-4.2", new CultureInfo("en"), -4.2d },
			new object[]{ "-4,2", new CultureInfo("de"), -4.2d },

			new object[]{ "4.2", new CultureInfo("en"), new double?(4.2d)},
			new object[]{ "4,2", new CultureInfo("de"), new double?(4.2d) },
			new object[]{ "-4.2", new CultureInfo("en"), new double?(-4.2d)},
			new object[]{ "-4,2", new CultureInfo("de"), new double?(-4.2d) },

			new object[]{ "4.2", new CultureInfo("en"), 4.2f },
			new object[]{ "4,2", new CultureInfo("de"), 4.2f },
			new object[]{ "-4.2", new CultureInfo("en"), -4.2f },
			new object[]{ "-4,2", new CultureInfo("de"), -4.2f },

			new object[]{ "4.2", new CultureInfo("en"), new float?(4.2f)},
			new object[]{ "4,2", new CultureInfo("de"), new float?(4.2f) },
			new object[]{ "-4.2", new CultureInfo("en"), new float?(-4.2f)},
			new object[]{ "-4,2", new CultureInfo("de"), new float?(-4.2f) },

			new object[]{ "4.", new CultureInfo("en"), "4." },
			new object[]{ "4,", new CultureInfo("de"), "4," },
			new object[]{ "-0", new CultureInfo("en"), "-0" },
			new object[]{ "-0", new CultureInfo("de"), "-0" },
		};

		[TestCaseSource(nameof(TryConvertWithNumbersAndCulturesCases))]
		public void TryConvertWithNumbersAndCultures(object inputString, CultureInfo culture, object expected)
		{
			CultureInfo.CurrentCulture = culture;
			BindingExpression.TryConvert(ref inputString, Entry.TextProperty, expected.GetType(), false);

			Assert.AreEqual(expected, inputString);
		}
	}
}
