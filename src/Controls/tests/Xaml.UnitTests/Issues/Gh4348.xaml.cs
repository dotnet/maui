using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Core.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public class Gh4348VM : ObservableCollection<string>
	{
		public Gh4348VM()
		{
			Add("foo");
			Add("bar");
		}
	}

	public partial class Gh4348 : ContentPage
	{
		public Gh4348()
		{
			InitializeComponent();
		}

		public Gh4348(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}		class Tests
		{
			[InlineData(true), InlineData(false)]
			public void GenericBaseClassResolution(bool useCompiledXaml)
			{
				var layout = new Gh4348(useCompiledXaml) { BindingContext = new Gh4348VM() };
				Assert.Equal("2", layout.labelCount.Text);
			}
		}
	}
}
