using System;
using System.Collections.ObjectModel;
using NUnit.Framework;

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

	[TestFixture]
	class Tests
	{
		[Test]
		public void SupportsCrookedGenericScenarios([Values] XamlInflator inflator)
		{
			var p = new GenericCollections(inflator);
			Assert.AreEqual("Foo", (p.label0.GetValue(AttachedBP.AttachedBPProperty) as GenericCollection)[0]);
		}
	}
}