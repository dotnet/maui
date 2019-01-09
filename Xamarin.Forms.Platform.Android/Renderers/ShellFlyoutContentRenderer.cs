using Android.Content;
using Android.Graphics.Drawables;
using Android.Support.Design.Widget;
using Android.Views;
using System;
using System.Collections.Generic;
using static Android.Support.Design.Widget.NavigationView;
using AView = Android.Views.View;

namespace Xamarin.Forms.Platform.Android
{
	public class ShellFlyoutContentRenderer : NavigationView, IShellFlyoutContentRenderer, IOnNavigationItemSelectedListener
	{
		#region IShellFlyoutContentRenderer

		AView IShellFlyoutContentRenderer.AndroidView => this;

		#endregion IShellFlyoutContentRenderer

		AView _headerView;
		readonly Dictionary<IMenuItem, Element> _lookupTable = new Dictionary<IMenuItem, Element>();
		IShellContext _shellContext;
		bool _disposed;

		public ShellFlyoutContentRenderer(IShellContext shellContext, Context context) : base(context)
		{
			_shellContext = shellContext;

			((IShellController)_shellContext.Shell).StructureChanged += OnShellStructureChanged;

			SetNavigationItemSelectedListener(this);

			BuildMenu();

			_headerView = new ContainerView(context, ((IShellController)shellContext.Shell).FlyoutHeader)
			{
				MatchWidth = true
			};

			AddHeaderView(_headerView);
		}

		bool IOnNavigationItemSelectedListener.OnNavigationItemSelected(IMenuItem menuItem)
		{
			if (_lookupTable.TryGetValue(menuItem, out var element))
			{
				((IShellController)_shellContext.Shell).OnFlyoutItemSelected(element);
				return true;
			}
			return false;
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);

			if (disposing && !_disposed)
			{
				_disposed = true;

				((IShellController)_shellContext.Shell).StructureChanged -= OnShellStructureChanged;
				_lookupTable.Clear();
				_headerView.Dispose();
				_headerView = null;
				_shellContext = null;
			}
		}

		void BuildMenu()
		{
			_lookupTable.Clear();

			var menu = Menu;
			menu.Clear();

			var groups = ((IShellController)_shellContext.Shell).GenerateFlyoutGrouping();

			int gid = 0;
			int id = 0;

			foreach (var group in groups)
			{
				foreach (var element in group)
				{
					string title = null;
					ImageSource icon = null;
					if (element is BaseShellItem shellItem)
					{
						title = shellItem.Title;
						icon = shellItem.FlyoutIcon;
					}
					else if (element is MenuItem menuItem)
					{
						title = menuItem.Text;
						icon = menuItem.Icon;
					}

					var item = menu.Add(gid, id++, 0, new Java.Lang.String(title));
					if (icon != null)
						SetMenuItemIcon(item, icon);
				}
				gid++;
			}
		}

		void OnShellStructureChanged(object sender, EventArgs e)
		{
			BuildMenu();
		}

		async void SetMenuItemIcon(IMenuItem menuItem, ImageSource source)
		{
			var drawable = await Context.GetFormsDrawable(source);
			menuItem.SetIcon(drawable);
			drawable?.Dispose();
		}
	}
}