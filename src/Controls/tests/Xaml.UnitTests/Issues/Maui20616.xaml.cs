using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
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
            if (useCompiledXaml)
            {
                MockCompiler.Compile(typeof(Maui20616));
            }
            else
            {
                new Maui20616(useCompiledXaml: false);
            }
        }
    }
}

public class ViewModel20616<T>
{
    public required T Value { get; init; }
}