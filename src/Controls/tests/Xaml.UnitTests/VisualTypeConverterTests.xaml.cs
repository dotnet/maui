using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Core.UnitTests;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public partial class VisualTypeConverterTests : ContentPage
	{
		public VisualTypeConverterTests()
		{
			InitializeComponent();
		}

		public VisualTypeConverterTests(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}

		[TestFixture]
		public class Tests
		{
			[TestCase(false)]
			[TestCase(true)]
			public void VisualAreConverted(bool useCompiledXaml)
			{
				Controls.Internals.Registrar.Registered.RegisterVisual(typeof(VisualTypeConverterTestsVisual));

				var page = new VisualTypeConverterTests(useCompiledXaml);
				Assert.That(page.Visual, Is.TypeOf<VisualTypeConverterTestsVisual>());
			}
		}

		public class VisualTypeConverterTestsVisual : IVisual
		{
			public VisualTypeConverterTestsVisual() { }
		}
	}
}