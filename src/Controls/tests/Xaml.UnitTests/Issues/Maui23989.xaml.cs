using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Dispatching;

using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.UnitTests;
using NUnit.Framework;
using Microsoft.Maui.Controls.Internals;
using System;
using System.Collections.Generic;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui23989
{
    public Maui23989()
    {
        InitializeComponent();
    }

    public Maui23989(bool useCompiledXaml)
    {
        //this stub will be replaced at compile time
    }

    [TestFixture]
    public class Test
    {
        [SetUp]
        public void Setup()
        {
            Application.SetCurrentApplication(new MockApplication());
            DispatcherProvider.SetCurrent(new DispatcherProviderStub());
        }

        [TearDown] public void TearDown() => AppInfo.SetCurrent(null);

        [Test]
        public void ItemDisplayBindingWithoutDataTypeFails([Values(false, true)] bool useCompiledXaml)
        {
            if (useCompiledXaml)
                Assert.Throws(new BuildExceptionConstraint(12, 13, s => s.Contains("0045", StringComparison.Ordinal)), ()=> MockCompiler.Compile(typeof(Maui23989), null, true));

            var layout = new Maui23989(useCompiledXaml);
            //without x:DataType, bindings aren't compiled
            Assert.That(layout.picker0.ItemDisplayBinding, Is.TypeOf<Binding>());
            if (useCompiledXaml)
                Assert.That(layout.picker1.ItemDisplayBinding, Is.TypeOf<TypedBinding<MockItemViewModel, string>>());
            else
                Assert.That(layout.picker1.ItemDisplayBinding, Is.TypeOf<Binding>());

            layout.BindingContext = new MockViewModel{
                Items = new List<MockItemViewModel> { 
                    new MockItemViewModel { Title = "item1" },
                    new MockItemViewModel { Title = "item2" },
                    new MockItemViewModel { Title = "item3" },
                }.ToArray()
            };

            Assert.That(layout.picker0.Items[0], Is.EqualTo("item1"));
            Assert.That(layout.picker1.Items[0], Is.EqualTo("item1"));
            
        }
    }
}
