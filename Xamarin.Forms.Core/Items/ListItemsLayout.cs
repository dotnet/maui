namespace Xamarin.Forms
{
	public class ListItemsLayout : ItemsLayout
	{
		// TODO hartez 2018/08/29 17:28:42 Consider changing this name to LinearItemsLayout; not everything using it is a list (e.g., Carousel)	
		public ListItemsLayout(ItemsLayoutOrientation orientation) : base(orientation)
		{
		}

		// TODO hartez 2018/05/31 15:56:23 Should these just be called Vertical and Horizontal (without List)?	
		public static readonly IItemsLayout VerticalList = new ListItemsLayout(ItemsLayoutOrientation.Vertical); 
		public static readonly IItemsLayout HorizontalList = new ListItemsLayout(ItemsLayoutOrientation.Horizontal);

		// TODO hartez 2018/08/29 20:31:54 Need something like these previous two, but as a carousel default	
	}
}