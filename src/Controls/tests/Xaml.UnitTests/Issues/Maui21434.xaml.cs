using System;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;

using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui21434
{
	public Maui21434()
	{
		InitializeComponent();
	}

	public Maui21434(bool useCompiledXaml)
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
		// [TearDown] public void TearDown() => AppInfo.SetCurrent(null);

		[Theory]
			public void Method(bool useCompiledXaml)
		{
			var page = new Maui21434(useCompiledXaml);
			Assert.Equal("ParentText", page.ParentTextLabel?.Text);
			Assert.Equal("ChildText", page.ChildTextLabel?.Text);
		}
	}
}

public class ParentViewModel21434
{
	public string Text => "ParentText";
	public ChildViewModel21434 Child { get; } = new();
}

public class ChildViewModel21434
{
	public string Text => "ChildText";
}
