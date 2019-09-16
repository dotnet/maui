using System.Collections.Generic;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Xaml;

namespace Xamarin.Forms.Controls.Issues
{
#if APP
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class Issue7519Xaml : ContentPage
	{
		public Issue7519Xaml()
		{
			InitializeComponent();

			var items = new List<_7519ItemModel>()
			{
				new _7519ItemModel {Name = "Item 1"},
				new _7519ItemModel {Name = "Item 2"},
				new _7519ItemModel {Name = "Item 3"},
				new _7519ItemModel {Name = "Item 4"},
				new _7519ItemModel {Name = "Item 5"},
				new _7519ItemModel {Name = "Item 6"},
				new _7519ItemModel {Name = "Item 7"},
			};

			BindingContext = new _7519Model { Items = items };
		}
	}

	[Preserve(AllMembers = true)]
	public class _7519Model
	{
		public List<_7519ItemModel> Items { get; set; }
	}

	[Preserve(AllMembers = true)]
	public class _7519ItemModel
	{
		public string Name { get; set; }
	}

#endif
}