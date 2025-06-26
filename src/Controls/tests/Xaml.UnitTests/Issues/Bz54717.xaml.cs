using System;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public partial class Bz54717 : ContentPage
	{
		public Bz54717()
		{
			InitializeComponent();
		}

		public Bz54717(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}		class Tests
		{			public void TearDown()
			{
				Application.Current = null;
			}

			[Theory]
			[InlineData(true)]
			[Theory]
			[InlineData(false)]
			public void FooBz54717(bool useCompiledXaml)
			{
				Application.Current = new MockApplication
				{
					Resources = new ResourceDictionary {
						{"Color1", Colors.Red},
						{"Color2", Colors.Blue},
					}
				};
				var layout = new Bz54717(useCompiledXaml);
				Assert.Equal(1, layout.Resources.Count);
				var array = layout.Resources["SomeColors"] as Color[];
				Assert.Equal(Colors.Red, array[0]);
				Assert.Equal(Colors.Blue, array[1]);
			}
		}
	}
}
