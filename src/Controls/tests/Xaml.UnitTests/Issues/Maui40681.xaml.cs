using System.Collections.Generic;
using Microsoft.Maui.Controls.Core.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public static class Maui40681Holder
{
	public static readonly BindableProperty ItemsProperty =
		BindableProperty.CreateAttached("Items", typeof(IList<View>), typeof(Maui40681Holder), null,
			defaultValueCreator: _ => new List<View>());

	public static IList<View> GetItems(BindableObject bindable) => (IList<View>)bindable.GetValue(ItemsProperty);

	public static void SetItems(BindableObject bindable, IList<View> value) => bindable.SetValue(ItemsProperty, value);
}

public class Maui40681View : ContentView
{
	public IList<View> Items { get; } = new List<View>();
}

public partial class Maui40681 : ContentPage
{
	public Maui40681() => InitializeComponent();

	[Collection("Xaml Inflation")]
	public class Tests : BaseTestFixture
	{
		[Theory]
		[XamlInflatorData]
		internal void SingleLayoutChildIsAddedToListProperty(XamlInflator inflator)
		{
			var page = new Maui40681(inflator);

			Assert.Equal(new View[] { page.attachedLayoutChild }, Maui40681Holder.GetItems(page.attachedLayoutHost));
			Assert.Equal(new View[] { page.attachedLabelChild }, Maui40681Holder.GetItems(page.attachedLabelHost));
			Assert.Equal(new View[] { page.attachedFirstGrid, page.attachedSecondGrid }, Maui40681Holder.GetItems(page.attachedTwoLayoutsHost));
			Assert.Equal(new View[] { page.clrLayoutChild }, page.clrLayoutHost.Items);
		}
	}
}
