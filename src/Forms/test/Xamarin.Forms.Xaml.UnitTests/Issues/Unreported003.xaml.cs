using System;
using System.Collections.Generic;
using NUnit.Framework;
using Xamarin.Forms;

namespace Xamarin.Forms.Xaml.UnitTests
{
	public partial class Unreported003 : ContentPage
	{
		public Unreported003()
		{
			InitializeComponent();
		}

		public Unreported003(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}

		[TestFixture]
		class Tests
		{
			[TestCase(true), TestCase(false)]
			public void AllowCtorArgsForValueTypes(bool useCompiledXaml)
			{
				var page = new Unreported003(useCompiledXaml);
			}
		}
	}
}