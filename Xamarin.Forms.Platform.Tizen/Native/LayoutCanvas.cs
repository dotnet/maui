using System;
using ElmSharp;
using System.Collections.Generic;
using System.Collections.Specialized;

using ELayout = ElmSharp.Layout;

namespace Xamarin.Forms.Platform.Tizen.Native
{
	public class LayoutCanvas : ELayout, IContainable<EvasObject>
	{
		readonly ObservableCollection<EvasObject> _children = new ObservableCollection<EvasObject>();
		Box _box;

		public LayoutCanvas(EvasObject parent) : base(parent)
		{
			SetTheme("layout", "elm_widget", "default");
			_box = new Box(parent);
			SetContent(_box);

			_children.CollectionChanged += (o, e) =>
			{
				if (e.Action == NotifyCollectionChangedAction.Add)
				{
					foreach (var v in e.NewItems)
					{
						var view = v as EvasObject;
						if (null != view)
						{
							OnAdd(view);
						}
					}
				}
				else if (e.Action == NotifyCollectionChangedAction.Remove)
				{
					foreach (var v in e.OldItems)
					{
						if (v is EvasObject view)
						{
							OnRemove(view);
						}
					}
				}
				else if (e.Action == NotifyCollectionChangedAction.Reset)
				{
					OnRemoveAll();
				}
			};
		}

		public event EventHandler<LayoutEventArgs> LayoutUpdated
		{
			add { _box.LayoutUpdated += value; }
			remove { _box.LayoutUpdated -= value; }
		}

		public new IList<EvasObject> Children
		{
			get
			{
				return _children;
			}
		}

		protected override void OnUnrealize()
		{
			foreach (var child in _children)
			{
				child.Unrealize();
			}

			base.OnUnrealize();
		}

		void OnAdd(EvasObject view)
		{
			_box.PackEnd(view);
		}

		void OnRemove(EvasObject view)
		{
			_box.UnPack(view);
		}

		void OnRemoveAll()
		{
			_box.UnPackAll();
		}
	}
}
