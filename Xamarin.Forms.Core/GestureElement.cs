using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms
{
	public class GestureElement : Element, ISpatialElement, IGestureRecognizers
	{
		readonly GestureRecognizerCollection _gestureRecognizers = new GestureRecognizerCollection();
		internal event NotifyCollectionChangedEventHandler GestureRecognizersCollectionChanged;

		public GestureElement()
		{
			_gestureRecognizers.CollectionChanged += (sender, args) =>
			{
				void AddItems()
				{
					foreach (IElement item in args.NewItems.OfType<IElement>())
					{
						ValidateGesture(item as IGestureRecognizer);
						item.Parent = this;
					}
				}

				void RemoveItems()
				{
					foreach (IElement item in args.OldItems.OfType<IElement>())
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
						foreach (IElement item in _gestureRecognizers.OfType<IElement>())
							item.Parent = this;
						break;
				}

				GestureRecognizersCollectionChanged?.Invoke(sender, args);
			};
		}

		Region ISpatialElement.Region { get; set; }

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