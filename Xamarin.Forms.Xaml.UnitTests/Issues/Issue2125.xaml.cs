using System;
using System.Collections.Generic;

using Xamarin.Forms;

using NUnit.Framework;

namespace Xamarin.Forms.Xaml.UnitTests
{	
	public partial class Issue2125 : ContentPage
	{	
		public Issue2125 ()
		{
			InitializeComponent ();
		}

		public Issue2125 (bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}

		[TestFixture]
		public class Tests
		{
			[TestCase (false)]
			[TestCase (true)]
			public void DuplicatexName (bool useCompiledXaml)
			{
				Assert.Throws (new XamlParseExceptionConstraint (5, 10), () => new Issue2125 (useCompiledXaml));
			}
		}
	}
}