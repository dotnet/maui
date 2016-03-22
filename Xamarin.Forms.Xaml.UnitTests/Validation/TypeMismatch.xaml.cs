using System;

using Xamarin.Forms;

using NUnit.Framework;

namespace Xamarin.Forms.Xaml.UnitTests
{
	public partial class TypeMismatch : ContentPage
	{
		public TypeMismatch ()
		{
			InitializeComponent ();
		}

		public TypeMismatch (bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}

		[TestFixture]
		public class Tests
		{
			[TestCase (false)]
//			[TestCase (true)]
			public void ThrowsOnMismatchingType (bool useCompiledXaml)
			{
				Assert.Throws (new XamlParseExceptionConstraint (7, 16, m => m.StartsWith ("Cannot assign property", StringComparison.Ordinal)), () => new TypeMismatch (useCompiledXaml));
			}
		}
	}
}