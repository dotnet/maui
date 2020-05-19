using System;
using System.Collections.Generic;

using System.Maui;
using NUnit.Framework;
using System.Maui.PlatformConfiguration;
using System.Maui.PlatformConfiguration.WindowsSpecific;

using WindowsOS = System.Maui.PlatformConfiguration.Windows;

namespace System.Maui.Xaml.UnitTests
{
	public partial class PlatformSpecific : MasterDetailPage
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