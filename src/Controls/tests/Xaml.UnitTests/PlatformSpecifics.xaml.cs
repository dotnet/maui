using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific;
using Xunit;
using WindowsOS = Microsoft.Maui.Controls.PlatformConfiguration.Windows;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public partial class PlatformSpecific : FlyoutPage
	{
		public PlatformSpecific()
		{
			InitializeComponent();
		}

		public PlatformSpecific(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}		class Tests
		{
			[InlineData(true)]
			[InlineData(false)]
			public void PlatformSpecificPropertyIsSet(bool useCompiledXaml)
			{
				var layout = new PlatformSpecific(useCompiledXaml);
				Assert.Equal(layout.On<WindowsOS>().GetCollapseStyle(), CollapseStyle.Partial);
				Assert.Equal(layout.On<WindowsOS>().CollapsedPaneWidth(), 96d);
			}
		}
	}
}