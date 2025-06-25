using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui25172 : CoreContentPage<VM25172>
{
	public Maui25172()
	{
		InitializeComponent();
	}

	public Maui25172(bool useCompiledXaml)
	{
		//this stub will be replaced at compile time
	}	class Test
	{
		[SetUp]
		public void Setup()
		{
			Application.SetCurrentApplication(new MockApplication());
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		}		public void TearDown()
		{
			AppInfo.SetCurrent(null);
		}

		[Fact]
		public void CompilationWithGenericBaseClassSucceeds([Values(true, false)] bool useCompiledXaml)
		{
			if (useCompiledXaml)
			{
				MockCompiler.Compile(typeof(Maui25172));
			}

			var page = new Maui25172(useCompiledXaml);
			Assert.True(typeof(CoreContentPage<VM25172>).IsAssignableFrom(page.GetType()));
		}
	}
}

public class VM25172 { }

public class CoreContentPage<T> : ContentPage
{
}
