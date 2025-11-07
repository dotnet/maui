using System;
using System.Collections.ObjectModel;
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


	public class Tests
	{
		[Theory]
		[Values]
		public void SupportsCrookedGenericScenarios(XamlInflator inflator)
		{
			var p = new GenericCollections(inflator);
			Assert.Equal("Foo", (p.label0.GetValue(AttachedBP.AttachedBPProperty) as GenericCollection)[0]);
		}
	}
}