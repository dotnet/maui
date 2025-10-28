using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Core.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public class Gh6176VM
	{
	}

	public class Gh6176Base<TVM> : ContentPage where TVM : class
	{
		public TVM ViewModel => BindingContext as TVM;
		protected void ShowMenu(object sender, EventArgs e) { }
	}

	public partial class Gh6176
	{
		public Gh6176() => InitializeComponent();
		public Gh6176(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}		class Tests
		{
			[Theory]
			public void Method([InlineData(false, true)] bool useCompiledXaml)
			{
				var layout = new Gh6176(useCompiledXaml);
			}
		}
	}
}
