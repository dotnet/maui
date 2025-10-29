using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Core.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public class Bz43733Rd : ResourceDictionary
	{
		public Bz43733Rd()
		{
			Add("SharedText", "Foo");
		}
	}

	public partial class Bz43733 : ContentPage
	{
		public Bz43733()
		{
			InitializeComponent();
		}

		public Bz43733(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}
		public class Tests
		{
			[InlineData(true)]
			[Theory]
			[InlineData(false)]
			public void ThrowOnMissingDictionary(bool useCompiledXaml)
			{
				Application.Current = new MockApplication
				{
					Resources = new ResourceDictionary
					{
						new Bz43733Rd()
					}
				};
				var p = new Bz43733(useCompiledXaml);
				Assert.Equal("Foo", p.label.Text);
			}
		}
	}
}
