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
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui31308
{
	public Maui31308() => InitializeComponent();

	[TestFixture]
	class Test
	{
		[SetUp]
		public void Setup()
		{
			Application.SetCurrentApplication(new MockApplication());
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		}

		[TearDown] public void TearDown() => AppInfo.SetCurrent(null);

		[Test]
		public void MissingResourceDictionaryValueIsReported([Values] XamlInflator inflator)
		{
			var exception = Assert.Catch<XamlParseException>(() => new Maui31308(inflator));
			Assert.AreEqual("Position 6:38. StaticResource not found for key ThisKeyDoesNotExistInAnyResourceDictionary", exception.Message);
		}
	}
}
