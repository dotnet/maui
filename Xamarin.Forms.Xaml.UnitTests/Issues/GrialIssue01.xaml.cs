using System;
using System.Collections.Generic;
using NUnit.Framework;
using Xamarin.Forms;

namespace Xamarin.Forms.Xaml.UnitTests
{
	public partial class GrialIssue01 : ContentPage
	{
		public GrialIssue01()
		{
			InitializeComponent();
		}

		public GrialIssue01(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}

		[TestFixture]
		class Tests
		{
			[TestCase(true)]
			[TestCase(false)]
			public void ImplicitCastIsUsedOnFileImageSource(bool useCompiledXaml)
			{
				var layout = new GrialIssue01(useCompiledXaml);
				var res = (FileImageSource)layout.Resources["image"];

				Assert.AreEqual("path.png", res.File);
			}
		}
	}
}