using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Core.UnitTests;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public class Gh4516VM
	{
		public Uri[] Images { get; } = Array.Empty<Uri>();
	}

	public partial class Gh4516 : ContentPage
	{
		public Gh4516() => InitializeComponent();
		public Gh4516(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}

		[TestFixture]
		class Tests
		{
			[TestCase(true), TestCase(false)]
			public void BindingToEmptyCollection(bool useCompiledXaml)
			{
				Gh4516 layout = null;
				Assert.DoesNotThrow(() => layout = new Gh4516(useCompiledXaml) { BindingContext = new Gh4516VM() });
				Assert.That((layout.image.Source as FileImageSource).File, Is.EqualTo("foo.jpg"));
			}
		}
	}
}