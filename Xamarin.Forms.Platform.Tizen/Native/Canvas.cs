using System.Collections.Generic;
using System.Collections.Specialized;
using ElmSharp;

namespace Xamarin.Forms.Platform.Tizen.Native
{
	/// <summary>
	/// A Canvas provides a class which can be a container for other controls.
	/// </summary>
	/// <remarks>
	/// This class is used as a container view for Layouts from Xamarin.Forms.Platform.Tizen framework.
	/// It is used for implementing xamarin pages and layouts.
	/// </remarks>
	public class Canvas : Box, IContainable<EvasObject>
	{
		/// <summary>
		/// The list of Views.
		/// </summary>
		readonly ObservableCollection<EvasObject> _children = new ObservableCollection<EvasObject>();

		/// <summary>
		/// Initializes a new instance of the <see cref="Xamarin.Forms.Platform.Tizen.Native.Canvas"/> class.
		/// </summary>
		/// <remarks>Canvas doesn't support replacing its children, this will be ignored.</remarks>
		/// <param name="parent">Parent of this instance.</param>
		public Canvas(EvasObject parent) : base(parent)
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

		/// <summary>
		/// Gets list of native elements that are placed in the canvas.
		/// </summary>
		public new IList<EvasObject> Children
		{
			get
			{
				return _children;
			}
		}

		/// <summary>
		/// Provides destruction for native element and contained elements.
		/// </summary>
		protected override void OnUnrealize()
		{
			foreach (var child in _children)
			{
				child.Unrealize();
			}

			base.OnUnrealize();
		}

		/// <summary>
		/// Adds a new child to a container.
		/// </summary>
		/// <param name="view">Native element which will be added</param>
		void OnAdd(EvasObject view)
		{
			PackEnd(view);
		}

		/// <summary>
		/// Removes a child from a container.
		/// </summary>
		/// <param name="view">Child element to be removed from canvas</param>
		void OnRemove(EvasObject view)
		{
			UnPack(view);
		}

		/// <summary>
		/// Removes all children from a canvas.
		/// </summary>
		void OnRemoveAll()
		{
			UnPackAll();
		}
	}
}
