using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public partial class Bz46921 : ContentPage
	{
		public Bz46921()
		{
			InitializeComponent();
		}

		public Bz46921(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}		class Tests
		{
			[InlineData(true)]
			[InlineData(false)]
			public void MultipleWaysToCreateAThicknessResource(bool useCompiledXaml)
			{
				var page = new Bz46921(useCompiledXaml);
				foreach (var resname in new string[] { "thickness0", "thickness1", "thickness2", "thickness3", })
				{
					var resource = page.Resources[resname];
					Assert.IsType<Thickness>(resource);
					var thickness = (Thickness)resource;
					Assert.Equal(new Thickness(4, 20, 4, 20), thickness);

				}
			}
		}
	}
}