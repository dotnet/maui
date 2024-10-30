using System.Diagnostics;
using System.Dynamic;
using AllTheLists.Models;

namespace AllTheLists.Views;

public partial class MonoProductListItem : ContentView
{
	public MonoProductListItem()
	{
		InitializeComponent();
		
	}

    protected override void OnBindingContextChanged()
    {
        base.OnBindingContextChanged();

		if(BindingContext is ProductDisplay)
		{
			BindingContext = ((ProductDisplay)BindingContext).Products[0];
		}
    }
}