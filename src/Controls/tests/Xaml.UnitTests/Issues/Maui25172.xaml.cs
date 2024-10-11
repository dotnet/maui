using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

[XamlCompilation(XamlCompilationOptions.Skip)]
public partial class Maui25172 : CoreContentPage<VM25172>
{
    public Maui25172()
    {
        InitializeComponent();
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

        [TearDown] public void TearDown()
        {
            AppInfo.SetCurrent(null);
        }

        [Test]
        public void CompilationWithGenericBaseClassSucceeds()
        {
			MockCompiler.Compile(typeof(Maui25172));
        }
    }
}

public class VM25172 {}

public class CoreContentPage<T> : ContentPage
{
}
