using System.Collections.Generic;
using System.Collections.Specialized;
using ElmSharp;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Tizen.Native
{
	public class EvasFormsCanvas : EvasBox, IContainable<EvasObject>
	{
		public EvasFormsCanvas(EvasObject parent) : base(parent)
		{
			Initilize();
		}

		readonly ObservableCollection<EvasObject> _children = new ObservableCollection<EvasObject>();

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

		void Initilize()
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
