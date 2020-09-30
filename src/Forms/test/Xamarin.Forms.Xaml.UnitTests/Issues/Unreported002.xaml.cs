using System;
using System.Collections.Generic;
using NUnit.Framework;
using Xamarin.Forms;

namespace Xamarin.Forms.Xaml.UnitTests
{
	public partial class Unreported002 : ContentPage
	{
		public Unreported002()
		{
			InitializeComponent();
		}

		public Unreported002(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}

		[TestFixture]
		public class Tests
		{
			[TestCase(false)]
			[TestCase(true)]
			public void TypeConvertersOnAttachedBP(bool useCompiledXaml)
			{
				var p = new Unreported002(useCompiledXaml);
				Assert.AreEqual(new Rectangle(0.5, 0.5, 1, -1), AbsoluteLayout.GetLayoutBounds(p.label));
			}
		}
	}
}