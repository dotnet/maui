using System;
using System.Collections.Generic;
using NUnit.Framework;
using Xamarin.Forms;

namespace Xamarin.Forms.Xaml.UnitTests
{
	public partial class Issue1199 : ContentPage
	{
		public Issue1199()
		{
			InitializeComponent();
		}

		public Issue1199(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}

		[TestFixture]
		class Tests
		{
			[TestCase(true)]
			[TestCase(false)]
			public void AllowCreationOfTypesFromString(bool useCompiledXaml)
			{
				var layout = new Issue1199(useCompiledXaml);
				var res = (Color)layout.Resources["AlmostSilver"];

				Assert.AreEqual(Color.FromHex("#FFCCCCCC"), res);
			}
		}
	}
}