using System;
using System.ComponentModel;
using System.Linq;
using Android.Content;
using Android.Graphics;
using Android.Views;
using Android.Widget;
using AView = Android.Views.View;
using Xamarin.Forms.Internals;
using AImageButton = Android.Widget.ImageButton;

namespace Xamarin.Forms.Platform.Android
{
	public sealed class NavigationMenuRenderer : ViewRenderer
	{
		public NavigationMenuRenderer(Context context) : base(context)
		{
			AutoPackage = false;
		}

		[Obsolete("This constructor is obsolete as of version 2.5. Please use NavigationMenuRenderer(Context) instead.")]
		public NavigationMenuRenderer()
		{
			AutoPackage = false;
		}

		GridView GridView
		{
			get { return Control as GridView; }
		}

		NavigationMenu NavigationMenu
		{
			get { return Element as NavigationMenu; }
		}

		protected override AView CreateNativeControl()
		{
			return new GridView(Context);
		}

		protected override void OnElementChanged(ElementChangedEventArgs<View> e)
		{
			base.OnElementChanged(e);

			if (e.OldElement == null)
			{
				var grid = (GridView)CreateNativeControl();
				grid.SetVerticalSpacing(20);

				SetNativeControl(grid);
			}

			UpdateTargets();
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			switch (e.PropertyName)
			{
				case "Targets":
					UpdateTargets();
					break;
			}
		}

		protected override void OnSizeChanged(int w, int h, int oldw, int oldh)
		{
			GridView.NumColumns = w > h ? 3 : 2;
			base.OnSizeChanged(w, h, oldw, oldh);
		}

		void UpdateTargets()
		{
			GridView.Adapter = new MenuAdapter(NavigationMenu);
		}

		class MenuElementView : LinearLayout
		{
			readonly AImageButton _image;
			readonly TextView _label;
			string _icon;

			public MenuElementView(Context context) : base(context)
			{
				Orientation = Orientation.Vertical;
				_image = new AImageButton(context);
				_image.SetScaleType(ImageView.ScaleType.FitCenter);
				_image.Click += (object sender, EventArgs e) =>
				{
					if (OnSelected != null)
						OnSelected();
				};
				AddView(_image, new LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent) { Gravity = GravityFlags.Center });

				_label = new TextView(context) { TextAlignment = global::Android.Views.TextAlignment.Center, Gravity = GravityFlags.Center };
				AddView(_label, new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent));
			}

			public string Icon
			{
				get { return _icon; }
				set
				{
					_icon = value;
					Bitmap bitmap = Context.Resources.GetBitmap(_icon);
					_image.SetImageBitmap(bitmap);
				}
			}

			public string Name
			{
				get { return _label.Text; }
				set { _label.Text = value; }
			}

			public Action OnSelected { get; set; }
		}

		class MenuAdapter : BaseAdapter<Page>
		{
			readonly NavigationMenu _menu;

			INavigationMenuController MenuController => _menu;

			public MenuAdapter(NavigationMenu menu)
			{
				_menu = menu;
			}

			#region implemented abstract members of BaseAdapter

			public override Page this[int index]
			{
				get { return _menu.Targets.ElementAtOrDefault(index); }
			}

			#endregion

			public override AView GetView(int position, AView convertView, ViewGroup parent)
			{
				MenuElementView menuItem = convertView as MenuElementView ?? new MenuElementView(parent.Context);
				Page item = this[position];
				menuItem.Icon = item.Icon;
				menuItem.Name = item.Title;
				menuItem.OnSelected = () => MenuController.SendTargetSelected(item);
				return menuItem;
			}

			#region implemented abstract members of BaseAdapter

			public override long GetItemId(int position)
			{
				return 0;
			}

			public override int Count
			{
				get { return _menu.Targets.Count(); }
			}

			#endregion
		}
	}
}