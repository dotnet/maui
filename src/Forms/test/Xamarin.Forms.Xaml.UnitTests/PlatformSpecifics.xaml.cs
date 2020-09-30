using System;
using System.Collections.Generic;
using NUnit.Framework;
using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration;
using Xamarin.Forms.PlatformConfiguration.WindowsSpecific;
using WindowsOS = Xamarin.Forms.PlatformConfiguration.Windows;

namespace Xamarin.Forms.Xaml.UnitTests
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