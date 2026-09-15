#nullable disable
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using Microsoft.Maui;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls
{
	/// <summary>An element that can respond to gestures.</summary>
	public class GestureElement : Element, ISpatialElement, IGestureRecognizers
	{
		readonly GestureRecognizerCollection _gestureRecognizers = new GestureRecognizerCollection();
		readonly WeakEventManager _gestureWeakEventManager = new WeakEventManager();

		internal event NotifyCollectionChangedEventHandler GestureRecognizersCollectionChanged
		{
			add => _gestureWeakEventManager.AddEventHandler(value, nameof(GestureRecognizersCollectionChanged));
			remove => _gestureWeakEventManager.RemoveEventHandler(value, nameof(GestureRecognizersCollectionChanged));
		}

		/// <summary>Creates a new <see cref="Microsoft.Maui.Controls.GestureElement"/> object with default values.</summary>
		public GestureElement()
		{
			_gestureRecognizers.CollectionChanged += (sender, args) =>
			{
				void AddItems()
				{
					foreach (IElementDefinition item in args.NewItems.OfType<IElementDefinition>())
					{
						ValidateGesture(item as IGestureRecognizer);
						item.Parent = this;
					}
				}

				void RemoveItems()
				{
					foreach (IElementDefinition item in args.OldItems.OfType<IElementDefinition>())
						item.Parent = null;
				}

				switch (args.Action)
				{
					case NotifyCollectionChangedAction.Add:
						AddItems();
						break;
					case NotifyCollectionChangedAction.Remove:
						RemoveItems();
						break;
					case NotifyCollectionChangedAction.Replace:
						AddItems();
						RemoveItems();
						break;
					case NotifyCollectionChangedAction.Reset:
						foreach (IElementDefinition item in _gestureRecognizers.OfType<IElementDefinition>())
							item.Parent = this;
						break;
				}

				_gestureWeakEventManager.HandleEvent(sender, args, nameof(GestureRecognizersCollectionChanged));
			};
		}

		Region ISpatialElement.Region { get; set; }

		/// <summary>Gets the list of recognizers that belong to the element.</summary>
		public IList<IGestureRecognizer> GestureRecognizers
		{
			get { return _gestureRecognizers; }
		}

		internal virtual void ValidateGesture(IGestureRecognizer gesture) { }

		class GestureRecognizerCollection : ObservableCollection<IGestureRecognizer>
		{
			protected override void ClearItems()
			{
				List<IGestureRecognizer> removed = new List<IGestureRecognizer>(this);
				base.ClearItems();
				base.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, removed));
			}
		}
	}
}
