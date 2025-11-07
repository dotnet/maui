using System;
using Microsoft.Maui.Controls.Core.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class TypeLoader : ContentPage
{
	public TypeLoader() => InitializeComponent();

	public class Tests
	{
		// TODO: Convert to IDisposable or constructor - [MemberData(nameof(InitializeTest))] // TODO: Convert to IDisposable or constructor public void SetUp() => Application.Current = new MockApplication();

		[Theory]
		[Values]
		public void LoadTypeFromXmlns()
		{
			TypeLoader layout = null;
			// TODO: XUnit has no DoesNotThrow. Remove this or use try/catch if needed: // (() => layout = new TypeLoader(inflator));
			Assert.NotNull(layout.customview0);
			Assert.IsType<CustomView>(layout.customview0);
		}

		[Theory]
		[Values]
		public void LoadTypeFromXmlnsWithoutAssembly()
		{
			TypeLoader layout = null;
			// TODO: XUnit has no DoesNotThrow. Remove this or use try/catch if needed: // (() => layout = new TypeLoader(inflator));
			Assert.NotNull(layout.customview1);
			Assert.IsType<CustomView>(layout.customview1);
		}
	}
}