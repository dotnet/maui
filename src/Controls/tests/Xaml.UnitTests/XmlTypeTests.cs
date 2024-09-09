using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	[TestFixture]
	public class XmlTypeTests : BaseTestFixture
	{
		[Test]
		public void TestXmlTypeEquality()
		{
			// Arrange
			var type1 = new XmlType("http://example.com", "Type1", typeArguments: null);
			var type2 = new XmlType("http://example.com", "Type1", typeArguments: null);

			// Act
			var result = type1.Equals(type2);

			// Assert
			Assert.IsTrue(result);
		}

		[Test]
		public void TestXmlTypeInequality()
		{
			// Arrange
			var type1 = new XmlType("http://example.com", "Type1", typeArguments: null);
			var type2 = new XmlType("http://example.com", "Type2", typeArguments: null);

			// Act
			var result = type1.Equals(type2);

			// Assert
			Assert.IsFalse(result);
		}

		[Test]
		public void TestXmlTypeEqualityWithTypeArgs()
		{
			// Arrange
			var typeArg = new XmlType("http://example.com", "TypeArg", typeArguments: null);
			var type1 = new XmlType("http://example.com", "Type1", typeArguments: [typeArg]);
			var type2 = new XmlType("http://example.com", "Type1", typeArguments: [typeArg]);

			// Act
			var result = type1.Equals(type2);

			// Assert
			Assert.IsTrue(result);
		}

		[Test]
		public void TestXmlTypeInequalityWithSameTypeArgs()
		{
			// Arrange
			var typeArg = new XmlType("http://example.com", "TypeArg", typeArguments: null);
			var type1 = new XmlType("http://example.com", "Type1", typeArguments: [typeArg]);
			var type2 = new XmlType("http://example.com", "Type2", typeArguments: [typeArg]);

			// Act
			var result = type1.Equals(type2);

			// Assert
			Assert.IsFalse(result);
		}


		[Test]
		public void TestXmlTypeInequalityWithDifferentTypeArgs()
		{
			// Arrange
			var typeArg1 = new XmlType("http://example.com", "TypeArg1", typeArguments: null);
			var typeArg2 = new XmlType("http://example.com", "TypeArg2", typeArguments: null);
			var type1 = new XmlType("http://example.com", "Type1", typeArguments: [typeArg1]);
			var type2 = new XmlType("http://example.com", "Type1", typeArguments: [typeArg2]);

			// Act
			var result = type1.Equals(type2);

			// Assert
			Assert.IsFalse(result);
		}

		[Test]
		public void TestXmlTypeInequalityWithAndWithoutTypeArgs()
		{
			// Arrange
			var typeArg = new XmlType("http://example.com", "TypeArg", typeArguments: null);
			var type1 = new XmlType("http://example.com", "Type1", typeArguments: [typeArg]);
			var type2 = new XmlType("http://example.com", "Type1", typeArguments: null);

			// Act
			var result = type1.Equals(type2);

			// Assert
			Assert.IsFalse(result);
		}
	}
}
