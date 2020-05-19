using System;
using System.Collections.Generic;

using System.Maui;
using NUnit.Framework;

namespace System.Maui.Xaml.UnitTests
{
	public partial class Issue3106 : ContentPage
	{
		public Issue3106 ()
		{
			InitializeComponent ();
		}
		public Issue3106 (bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}

		[TestFixture]
		public class Tests
		{
			[TestCase (false)]
			[TestCase (true)]
			public void NewDoesNotThrow (bool useCompiledXaml)
			{
				var p = new Issue3106 (useCompiledXaml);
			}
		}
	}
}

