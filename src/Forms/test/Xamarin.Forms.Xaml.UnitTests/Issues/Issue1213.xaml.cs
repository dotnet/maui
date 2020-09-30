using System;
using System.Collections.Generic;
using NUnit.Framework;
using Xamarin.Forms;

namespace Xamarin.Forms.Xaml.UnitTests
{
	public partial class Issue1213 : TabbedPage
	{
		public Issue1213()
		{
			InitializeComponent();
		}

		public Issue1213(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}

		[TestFixture]
		public class Tests
		{
			[TestCase(false)]
			[TestCase(true)]
			public void MultiPageAsContentPropertyAttribute(bool useCompiledXaml)
			{
				var page = new Issue1213(useCompiledXaml);
				Assert.AreEqual(2, page.Children.Count);
			}
		}
	}
}