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
		// NOTE: xUnit uses constructor for setup. This may need manual conversion.
		// [SetUp]
		public void Setup()
		{
			Application.SetCurrentApplication(new MockApplication());
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		}

		// NOTE: xUnit uses IDisposable.Dispose() for teardown. This may need manual conversion.
		// [TearDown]
		public void TearDown()
		{
			AppInfo.SetCurrent(null);
		}

		[Theory]
			public void Method([InlineData(true, false)] bool useCompiledXaml)
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
