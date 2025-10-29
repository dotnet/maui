using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Core.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public class Gh4446Item
	{
		public string Id { get; set; }
		public string Text { get; set; }
		public string Description { get; set; }
	}

	public partial class Gh4446 : ContentPage
	{
		public Gh4446()
		{
			InitializeComponent();
		}

		public Gh4446(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}		class Tests
		{
			[InlineData(true), InlineData(false)]
			public void BindingThrowsOnWrongConverterParameter(bool useCompiledXaml)
			{
				Assert.DoesNotThrow(() => new Gh4446(useCompiledXaml) { BindingContext = new Gh4446Item { Text = null } });
			}
		}
	}
}
