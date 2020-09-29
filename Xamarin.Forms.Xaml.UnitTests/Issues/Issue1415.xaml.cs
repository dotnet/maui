using System;
using System.Collections.Generic;
using NUnit.Framework;
using Xamarin.Forms;

namespace Xamarin.Forms.Xaml.UnitTests
{
	public partial class Issue1415 : ContentPage
	{
		public Issue1415()
		{
			InitializeComponent();
		}

		public Issue1415(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}

		[TestFixture]
		public class Tests
		{
			[TestCase(false)]
			[TestCase(true)]
			public void NestedMarkupExtension(bool useCompiledXaml)
			{
				var page = new Issue1415(useCompiledXaml);
				var label = page.FindByName<Label>("label");
				Assert.NotNull(label);
				label.BindingContext = "foo";
				Assert.AreEqual("oof", label.Text);
			}
		}
	}
}