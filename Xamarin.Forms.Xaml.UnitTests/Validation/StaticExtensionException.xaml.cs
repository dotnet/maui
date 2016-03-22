using System;
using System.Collections.Generic;

using Xamarin.Forms;

using NUnit.Framework;

namespace Xamarin.Forms.Xaml.UnitTests
{	
	public partial class StaticExtensionException : ContentPage
	{	
		public StaticExtensionException ()
		{
			InitializeComponent ();
		}

		public StaticExtensionException (bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}

		[TestFixture]
		public class Issue2115
		{
			[TestCase (false)]
			[TestCase (true)]
			public void xStaticThrowsMeaningfullException (bool useCompiledXaml)
			{
				Assert.Throws (new XamlParseExceptionConstraint (6, 34), () => new StaticExtensionException (useCompiledXaml));
			}
		}
	}
}