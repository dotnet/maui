using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui31308
{
	public Maui31308() => InitializeComponent();


	public class Test
	{
		public Test()
		{
			Application.SetCurrentApplication(new MockApplication());
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		}

		[Theory]
		[Values]
		public void MissingResourceDictionaryValueIsReported(XamlInflator inflator)
		{
			var exception = Assert.Throws<XamlParseException>(() => new Maui31308(inflator));
			Assert.Equal("Position 6:38. StaticResource not found for key ThisKeyDoesNotExistInAnyResourceDictionary", exception.Message);
		}
	}
}
