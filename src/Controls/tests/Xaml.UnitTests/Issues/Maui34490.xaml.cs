using System;
using System.Collections.Generic;
using System.Windows.Input;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public class Maui34490ViewModel
{
	public List<Maui34490ItemModel> Items { get; set; }
	public ICommand SelectItemCommand { get; set; }
}

public class Maui34490ItemModel
{
	public string Name { get; set; }
}

public partial class Maui34490 : ContentPage
{
	public Maui34490() => InitializeComponent();

	[Collection("Issue")]
	public class Tests : IDisposable
	{
		public Tests() => AppInfo.SetCurrent(new MockAppInfo());
		public void Dispose() => AppInfo.SetCurrent(null);

		[Theory]
		[XamlInflatorData]
		internal void XReferenceSourceInDataTemplateShouldNotWarn(XamlInflator inflator)
		{
			if (inflator == XamlInflator.SourceGen)
			{
				var result = MockSourceGenerator.CreateMauiCompilation()
					.RunMauiSourceGenerator(typeof(Maui34490));
				// The path is resolved against ContentPage (x:Reference target), NOT Maui34490ItemModel (x:DataType).
				// BindingContext is 'object' on BindableObject, so SelectItemCommand warning on 'object' is expected,
				// but a warning mentioning Maui34490ItemModel would mean it's still resolving against the wrong type.
				Assert.DoesNotContain(result.Diagnostics, d => d.Id == "MAUIG2045" && d.GetMessage().Contains("Maui34490ItemModel", StringComparison.Ordinal));
			}

			var page = new Maui34490(inflator);
			Assert.NotNull(page);
		}
	}
}
