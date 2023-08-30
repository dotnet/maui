using System;
using System.Collections.Generic;
using System.Globalization;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{

	public class BindingExpressionTests : BaseTestFixture
	{
		[Fact]
		public void Ctor()
		{
			string path = "Foo.Bar";
			var binding = new Binding(path);

			var be = new BindingExpression(binding, path);

			Assert.Same(binding, be.Binding);
			Assert.Equal(path, be.Path);
		}

		[Fact]
		public void CtorInvalid()
		{
			string path = "Foo.Bar";
			var binding = new Binding(path);

			Assert.Throws<ArgumentNullException>(() => new BindingExpression(binding, null));

			Assert.Throws<ArgumentNullException>(() => new BindingExpression(null, path));
		}

		[Fact]
		public void ApplyNull()
		{
			const string path = "Foo.Bar";
			var binding = new Binding(path);
			var be = new BindingExpression(binding, path);
			be.Apply(null, new MockBindable(), TextCell.TextProperty, SetterSpecificity.FromBinding);
		}

		// We only throw on invalid path features, if they give an invalid property
		// name, it won't have compiled in the first place or they misstyped.
		[InlineData("Foo.")]
		[InlineData("Foo[]")]
		[InlineData("Foo.Bar[]")]
		[InlineData("Foo[1")]
		[Theory]
		public void InvalidPaths(string path)
		{
			var fex = Assert.Throws<FormatException>(() =>
			{
				var binding = new Binding(path);
				new BindingExpression(binding, path);
			});

			Assert.False(String.IsNullOrWhiteSpace(fex.Message),
				"FormatException did not contain an explanation");
		}

		public static IEnumerable<object[]> ValidPathsData()
		{
			var paths = new List<string> { ".", "[1]", "[1 ]", ".[1]", ". [1]",
				"Foo", "Foo.Bar", "Foo. Bar", "Foo.Bar[1]",
				"Foo.Bar [1]" };

			foreach (var path in paths)
			{
				yield return new object[] { path, true, true };
				yield return new object[] { path, true, false };
				yield return new object[] { path, false, true };
				yield return new object[] { path, false, false };
			}
		}

		[Theory, MemberData(nameof(ValidPathsData))]
		public void ValidPaths(
			string path,
			bool spaceBefore,
			bool spaceAfter)
		{
			if (spaceBefore)
				path = " " + path;
			if (spaceAfter)
				path = path + " ";

			var binding = new Binding(path);
			_ = new BindingExpression(binding, path);
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

		public static IEnumerable<object[]> TryConvertWithNumbersAndCulturesCasesData()
		{
			foreach (var testCase in TryConvertWithNumbersAndCulturesCases)
			{
				yield return (object[])testCase;
			}
		}

		[Theory, MemberData(nameof(TryConvertWithNumbersAndCulturesCasesData))]
		public void TryConvertWithNumbersAndCultures(object inputString, CultureInfo culture, object expected)
		{
			CultureInfo.CurrentCulture = culture;
			BindingExpression.TryConvert(ref inputString, Entry.TextProperty, expected.GetType(), false);

			Assert.Equal(expected, inputString);
		}
	}
}
