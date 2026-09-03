using System.Collections.Generic;
using System.Collections.Specialized;
using ElmSharp;
using Tizen.NET.MaterialComponents;
using Microsoft.Maui.Controls.Platform.Tizen.Native;

namespace Microsoft.Maui.Controls.Compatibility.Material.Tizen.Native
{
	public class MCanvas : MCard, IContainable<EvasObject>
	{
		readonly ObservableCollection<EvasObject> _children = new ObservableCollection<EvasObject>();

		public MCanvas(EvasObject parent) : base(parent)
		{
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
						var view = v as EvasObject;
						if (null != view)
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
			PackEnd(view);
		}

		void OnRemove(EvasObject view)
		{
			UnPack(view);
		}

		void OnRemoveAll()
		{
			UnPackAll();
		}
	}
}
