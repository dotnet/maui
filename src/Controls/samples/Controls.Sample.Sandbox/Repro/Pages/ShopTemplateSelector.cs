using AllTheLists.Models;

namespace AllTheLists.Pages;

public class ShopTemplateSelector : DataTemplateSelector
{
    public DataTemplate MonoTemplate { get; set; }
    public DataTemplate DuoTemplate { get; set; }
    public DataTemplate LoadingMoreTemplate { get; set; }

    protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
    {
        ProductDisplay productDisplay = (ProductDisplay)item;
        if(productDisplay.IsLoading)
        {
            return LoadingMoreTemplate;
        }

        return ((ProductDisplay)item).Products.Count < 2 ? MonoTemplate : DuoTemplate;
    }
}
