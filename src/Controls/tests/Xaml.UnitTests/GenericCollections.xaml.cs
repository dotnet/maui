using System;
using System.Collections.ObjectModel;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public class AttachedBP
{
	public static readonly BindableProperty AttachedBPProperty = BindableProperty.CreateAttached(
		"AttachedBP",
		typeof(GenericCollection),
		typeof(AttachedBP),
		null);

	public static GenericCollection GetAttachedBP(BindableObject bindable)
	{
		throw new NotImplementedException();
	}
}

public class GenericCollection : ObservableCollection<object>
{
}

public partial class GenericCollections : ContentPage
{
	public GenericCollections() => InitializeComponent();


	public class Tests : IDisposable
	{
		public Tests()
		{
			Application.SetCurrentApplication(new MockApplication());
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		}

		public void Dispose()
		{
			AppInfo.SetCurrent(null);
			DispatcherProvider.SetCurrent(null);
			Application.SetCurrentApplication(null);
		}

		[Theory]
		[Values]
		public void SupportsCrookedGenericScenarios(XamlInflator inflator)
		{
			var p = new GenericCollections(inflator);
			Assert.Equal("Foo", (p.label0.GetValue(AttachedBP.AttachedBPProperty) as GenericCollection)[0]);
		}
	}
}