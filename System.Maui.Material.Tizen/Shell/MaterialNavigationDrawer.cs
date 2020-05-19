using ElmSharp;
using Tizen.NET.MaterialComponents;
using System.Maui.Platform.Tizen;

namespace System.Maui.Material.Tizen
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
