using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Core.UnitTests;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public partial class Issue25893 : ContentPage
	{
		public Issue25893()
		{
			InitializeComponent();
		}

		public Issue25893(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}

		[TestFixture]
		public class Tests
		{
			[TestCase(false)]
			[TestCase(true)]
			public void MenuFlyoutSubItemWithIcon(bool useCompiledXaml)
			{
				var page = new Issue25893(useCompiledXaml);
				var view = page.FindByName<ContentView>("ViewWithMenu");
				var contextFlyout = FlyoutBase.GetContextFlyout(view);
	
				Assert.AreEqual(contextFlyout.LogicalChildrenInternal.Count, 2);
			}
		}
	}
}