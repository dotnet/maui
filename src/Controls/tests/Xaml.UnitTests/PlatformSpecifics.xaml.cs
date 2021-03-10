using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific;
using NUnit.Framework;
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
		}

		[TestFixture]
		class Tests
		{
			[TestCase(true)]
			[TestCase(false)]
			public void PlatformSpecificPropertyIsSet(bool useCompiledXaml)
			{
				var layout = new PlatformSpecific(useCompiledXaml);
				Assert.AreEqual(layout.On<WindowsOS>().GetCollapseStyle(), CollapseStyle.Partial);
				Assert.AreEqual(layout.On<WindowsOS>().CollapsedPaneWidth(), 96d);
			}
		}
	}
}