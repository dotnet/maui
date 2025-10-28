using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public partial class Issue2152 : ContentPage
	{
		public Issue2152()
		{
			InitializeComponent();
		}

		public Issue2152(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}

		int clickcount;
		public void OnButtonClicked(object sender, EventArgs e)
		{
			clickcount++;
		}		public class Tests
		{
			// NOTE: xUnit uses constructor for setup. This may need manual conversion.
		// [SetUp] public void Setup() => DispatcherProvider.SetCurrent(new DispatcherProviderStub());
			// NOTE: xUnit uses IDisposable.Dispose() for teardown. This may need manual conversion.
		// [TearDown] public void TearDown() => DispatcherProvider.SetCurrent(null);

			[InlineData(false)]
			[InlineData(true)]
			public void TestEventConnection(bool useCompiledXaml)
			{
				Issue2152 layout = null;
				Assert.DoesNotThrow(() => layout = new Issue2152(useCompiledXaml));
				Cell cell = null;
				Assert.DoesNotThrow(() => cell = layout.listview.TemplatedItems.GetOrCreateContent(0, null));
				var button = cell.FindByName<Button>("btn") as IButtonController;
				Assert.Equal(0, layout.clickcount);
				button.SendClicked();
				Assert.Equal(1, layout.clickcount);
			}
		}
	}
}