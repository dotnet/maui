using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui11467 : ParentButton
{
	public Maui11467() => InitializeComponent();

	public Maui11467(bool useCompiledXaml)
	{
		//this stub will be replaced at compile time
	}

	[TestFixture]
	class Test
	{
		[SetUp] public void Setup() => AppInfo.SetCurrent(new MockAppInfo());
		[TearDown] public void TearDown() => AppInfo.SetCurrent(null);

		[Test]
		public void EventHandlerFromBaseType([Values(false, true)] bool useCompiledXaml)
		{
			if (useCompiledXaml)
				MockCompiler.Compile(typeof(Maui11467));

			// Used to throw:
			// XamlParseException : Position 5:9. No method ParentButton_OnClicked with correct signature found on type Microsoft.Maui.Controls.Xaml.UnitTests.Maui11467
			var button = new Maui11467(useCompiledXaml);
			Assert.That(button.MyEventSubscriberCount, Is.EqualTo(1));
		}
	}
}