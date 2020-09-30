using System;
using System.Collections.Generic;
using NUnit.Framework;
using Xamarin.Forms;

namespace Xamarin.Forms.Xaml.UnitTests
{
	[XamlCompilation(XamlCompilationOptions.Skip)]
	public partial class StaticExtensionException : ContentPage
	{
		public StaticExtensionException()
		{
			InitializeComponent();
		}

		public StaticExtensionException(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}

		[TestFixture]
		public class Issue2115
		{
			[TestCase(false)]
			public void xStaticThrowsMeaningfullException(bool useCompiledXaml)
			{
				Assert.Throws(new XamlParseExceptionConstraint(6, 34), () => new StaticExtensionException(useCompiledXaml));
			}
		}
	}
}