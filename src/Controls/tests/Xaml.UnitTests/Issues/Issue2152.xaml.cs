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
		public Tests() => DispatcherProvider.SetCurrent(new DispatcherProviderStub());

		public void Dispose() => DispatcherProvider.SetCurrent(null);

		[Theory]
		[Values]
		public void TestEventConnection(XamlInflator inflator)
		{
			if (inflator == XamlInflator.SourceGen)
			{
				var result = MockSourceGenerator.RunMauiSourceGenerator(MockSourceGenerator.CreateMauiCompilation(), typeof(Issue2152));
			}
			Issue2152 layout = null;
			Assert.Null(Record.Exception(() => layout = new Issue2152(inflator)));
			Cell cell = null;
			Assert.Null(Record.Exception(() => cell = layout.listview.TemplatedItems.GetOrCreateContent(0, null)));
			var button = cell.FindByName<Button>("btn") as IButtonController;
			Assert.Equal(0, layout.clickcount);
			button.SendClicked();
			Assert.Equal(1, layout.clickcount);
		}
	}
}