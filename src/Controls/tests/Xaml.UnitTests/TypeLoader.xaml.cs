using System;
using Microsoft.Maui.Controls.Core.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class TypeLoader : ContentPage
{
	public TypeLoader() => InitializeComponent();

	public class Tests : IDisposable
	{
		public Tests()
		{
			Application.Current = new MockApplication();
		}

		public void Dispose()
		{
			Application.Current = null;
		}
		
		[Theory]
		[Values]
		public void LoadTypeFromXmlns(XamlInflator inflator)
		{
			TypeLoader layout = new TypeLoader(inflator);
			Assert.NotNull(layout.customview0);
			Assert.IsType<CustomView>(layout.customview0);
		}

		[Theory]
		[Values]
		public void LoadTypeFromXmlnsWithoutAssembly(XamlInflator inflator)
		{
			TypeLoader layout = new TypeLoader(inflator);
			Assert.NotNull(layout.customview1);
			Assert.IsType<CustomView>(layout.customview1);
		}
	}
}