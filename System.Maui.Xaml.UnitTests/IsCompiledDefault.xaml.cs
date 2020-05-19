using System;
using System.Collections.Generic;

using Xamarin.Forms;
using NUnit.Framework;

namespace Xamarin.Forms.Xaml.UnitTests
{
	public partial class IsCompiledDefault : ContentPage
	{
		public IsCompiledDefault ()
		{
			InitializeComponent ();
		}

		public IsCompiledDefault (bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}

		[TestFixture]
		public class Tests
		{
			[TestCase (false)]
			[TestCase (true)]
			public void IsCompiled (bool useCompiledXaml)
			{
				var layout = new IsCompiledDefault (useCompiledXaml);
				Assert.AreEqual (true, typeof (IsCompiledDefault).IsCompiled ());
			}
		}
	}
}