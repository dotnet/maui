using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Controls.Xaml;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public class Bz53318ListView : ListView
	{
	}

	[XamlCompilation(XamlCompilationOptions.Skip)]
	public partial class Bz53318 : ContentPage
	{
		public Bz53318()
		{
			InitializeComponent();
		}
		public public class Tests
		{
			[Fact]
			public void DoesCompilesArgsInsideDataTemplate()
			{
				Assert.DoesNotThrow(() => MockCompiler.Compile(typeof(Bz53318)));
			}
		}
	}
}