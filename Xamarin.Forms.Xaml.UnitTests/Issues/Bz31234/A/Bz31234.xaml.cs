using System;
using System.Collections.Generic;
using NUnit.Framework;
using Xamarin.Forms;

namespace Xamarin.Forms.Xaml.UnitTests.A
{
	public partial class Bz31234 : ContentPage
	{
		public Bz31234()
		{
			InitializeComponent();
		}

		public Bz31234(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}

		[TestFixture]
		public class Tests
		{
			[TestCase(true), TestCase(false)]
			public void ShouldPass(bool useCompiledXaml)
			{
				new Bz31234(useCompiledXaml);
				Assert.Pass();
			}
		}
	}
}