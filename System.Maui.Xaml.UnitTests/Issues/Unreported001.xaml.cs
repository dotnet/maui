using System;
using System.Collections.Generic;

using System.Maui;
using NUnit.Framework;

namespace System.Maui.Xaml.UnitTests
{
	public class U001Page : ContentPage
	{
		public U001Page ()
		{
			;
		}

	}

	public partial class Unreported001 : TabbedPage
	{
		public Unreported001 ()
		{
			InitializeComponent ();
		}

		public Unreported001 (bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}

		[TestFixture]
		public class Tests
		{
			[TestCase (false)]
			[TestCase (true)]
			public void DoesNotThrow (bool useCompiledXaml)
			{
				var p = new Unreported001 (useCompiledXaml);
				Assert.That (p.navpage.CurrentPage, Is.TypeOf<U001Page> ());
			}
		}
	}
}