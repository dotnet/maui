using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Xamarin.Forms.Core.UnitTests
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
	}
}