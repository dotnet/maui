using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.UnitTests;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui20616
{
	public Maui20616()
	{
		InitializeComponent();
		BindingContext = new ViewModel20616<string> { Value = "Foo" };
	}

	public Maui20616(bool useCompiledXaml)
	{
		//this stub will be replaced at compile time
	}

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
		public void XDataTypeCanBeGeneric([Values(false, true)] bool useCompiledXaml)
		{
			var page = new Maui20616(useCompiledXaml);

			page.LabelA.BindingContext = new ViewModel20616<string> { Value = "ABC" };
			Assert.AreEqual("ABC", page.LabelA.Text);

			if (useCompiledXaml)
			{
				var binding = page.LabelA.GetContext(Label.TextProperty).Bindings.GetValue();
				Assert.That(binding, Is.TypeOf<TypedBinding<ViewModel20616<string>, string>>());
			}

			page.LabelB.BindingContext = new ViewModel20616<ViewModel20616<bool>> { Value = new ViewModel20616<bool> { Value = true } };
			Assert.AreEqual("True", page.LabelB.Text);

			if (useCompiledXaml)
			{
				var binding = page.LabelB.GetContext(Label.TextProperty).Bindings.GetValue();
				Assert.That(binding, Is.TypeOf<TypedBinding<ViewModel20616<ViewModel20616<bool>>, bool>>());
			}

			Assert.AreEqual(typeof(ViewModel20616<bool>), page.Resources["ViewModelBool"]);
			Assert.AreEqual(typeof(ViewModel20616<ViewModel20616<string>>), page.Resources["NestedViewModel"]);
		}
	}
}

public class ViewModel20616<T>
{
	public required T Value { get; init; }
}