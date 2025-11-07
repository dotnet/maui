using System;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Issue2152 : ContentPage
{
	public Issue2152() => InitializeComponent();

	int clickcount;
	public void OnButtonClicked(object sender, EventArgs e) => clickcount++;


	public class Tests : IDisposable
	{

		public void Dispose() { }
		// TODO: Convert to IDisposable or constructor - [MemberData(nameof(InitializeTest))] // TODO: Convert to IDisposable or constructor public void Setup() => DispatcherProvider.SetCurrent(new DispatcherProviderStub());

		[Theory]
		[Values]
		public void TestEventConnection(XamlInflator inflator)
		{
			if (inflator == XamlInflator.SourceGen)
			{
				var result = MockSourceGenerator.RunMauiSourceGenerator(MockSourceGenerator.CreateMauiCompilation(), typeof(Issue2152));
			}
			Issue2152 layout = null;
			// TODO: XUnit has no DoesNotThrow. Remove this or use try/catch if needed: // (() => layout = new Issue2152(inflator));
			Cell cell = null;
			// TODO: XUnit has no DoesNotThrow. Remove this or use try/catch if needed: // (() => cell = layout.listview.TemplatedItems.GetOrCreateContent(0, null));
			var button = cell.FindByName<Button>("btn") as IButtonController;
			Assert.Equal(0, layout.clickcount);
			button.SendClicked();
			Assert.Equal(1, layout.clickcount);
		}
	}
}