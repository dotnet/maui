using System;
using System.Collections.Generic;
using NUnit.Framework;
using Xamarin.Forms;

namespace Xamarin.Forms.Xaml.UnitTests
{
	[XamlCompilation(XamlCompilationOptions.Skip)]
	public partial class IsCompiledSkip : ContentPage
	{
		public IsCompiledSkip()
		{
			InitializeComponent();
		}

		public IsCompiledSkip(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}

		[TestFixture]
		public class Tests
		{
			[TestCase(false)]
			[TestCase(true)]
			public void IsCompiled(bool useCompiledXaml)
			{
				var layout = new IsCompiledDefault(useCompiledXaml);
				Assert.AreEqual(false, typeof(IsCompiledSkip).IsCompiled());
			}
		}
	}
}