using ElmSharp;
using Tizen.NET.MaterialComponents;
using Xamarin.Forms.Platform.Tizen;

namespace Xamarin.Forms.Material.Tizen
{
	public class MaterialNavigationDrawer : MNavigationDrawer, INavigationDrawer
	{
		public MaterialNavigationDrawer(EvasObject parent) : base(parent)
		{
		}

		EvasObject INavigationDrawer.NavigationView
		{
			get => NavigationView;
			set => NavigationView = value as MNavigationView;
		}
	}
}
