using System;
using System.Collections.Generic;
using NUnit.Framework;
using Xamarin.Forms;

namespace Xamarin.Forms.Xaml.UnitTests
{
	[ContentProperty("Content")]
	public class MockBindableForArray : View
	{
		public object Content { get; set; }
	}

	public partial class XArray : MockBindableForArray
	{
		public XArray()
		{
			InitializeComponent();
		}

		public XArray(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}

		[TestFixture]
		public class Tests
		{
			[TestCase(false)]
			[TestCase(true)]
			public void SupportsXArray(bool useCompiledXaml)
			{
				var layout = new XArray(useCompiledXaml);
				var array = layout.Content;
				Assert.NotNull(array);
				Assert.That(array, Is.TypeOf<string[]>());
				Assert.AreEqual(2, ((string[])layout.Content).Length);
				Assert.AreEqual("Hello", ((string[])layout.Content)[0]);
				Assert.AreEqual("World", ((string[])layout.Content)[1]);
			}
		}
	}
}