using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Graphics;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public partial class Gh16293 : ContentPage
	{
		public Gh16293() => InitializeComponent();
		public Gh16293(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}

		[TestFixture]
		class Tests
		{
			[TestCase(true), TestCase(false)]
			public void ShouldResolveNested(bool useCompiledXaml)
			{
				if (useCompiledXaml)
					Assert.DoesNotThrow(() => MockCompiler.Compile(typeof(Gh16293)));

				var layout = new Gh16293(useCompiledXaml);
				Assert.That(layout.Label1.Text, Is.EqualTo("LibraryConstant"));
				Assert.That(layout.Label2.Text, Is.EqualTo("NestedLibraryConstant"));
			}

			[TestCase(true), TestCase(false)]
			public void ShouldResolveInternalNested(bool useCompiledXaml)
			{
				if (useCompiledXaml)
					Assert.DoesNotThrow(() => MockCompiler.Compile(typeof(Gh16293)));

				var layout = new Gh16293(useCompiledXaml);
				Assert.That(layout.Label3.Text, Is.EqualTo("InternalLibraryConstant"));
				Assert.That(layout.Label4.Text, Is.EqualTo("InternalNestedLibraryConstant"));
			}
		}
	}
}
