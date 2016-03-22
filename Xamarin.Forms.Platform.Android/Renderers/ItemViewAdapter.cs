using System.Collections.Generic;
using Android.Support.V7.Widget;
using Android.Views;

namespace Xamarin.Forms.Platform.Android
{
	internal class ItemViewAdapter : RecyclerView.Adapter
	{
		readonly IVisualElementRenderer _renderer;
		readonly Dictionary<int, object> _typeByTypeId;
		readonly Dictionary<object, int> _typeIdByType;
		int _nextItemTypeId;

		public ItemViewAdapter(IVisualElementRenderer carouselRenderer)
		{
			_renderer = carouselRenderer;
			_typeByTypeId = new Dictionary<int, object>();
			_typeIdByType = new Dictionary<object, int>();
			_nextItemTypeId = 0;
		}

		public override int ItemCount
		{
			get { return Element.Count; }
		}

		IItemViewController Controller
		{
			get { return Element; }
		}

		ItemsView Element
		{
			get { return (ItemsView)_renderer.Element; }
		}

		public override int GetItemViewType(int position)
		{
			// get item and type from ItemSource and ItemTemplate
			object item = Controller.GetItem(position);
			object type = Controller.GetItemType(item);

			// map type as DataTemplate to type as Id
			int id = default(int);
			if (!_typeIdByType.TryGetValue(type, out id))
			{
				id = _nextItemTypeId++;
				_typeByTypeId[id] = type;
				_typeIdByType[type] = id;
			}
			return id;
		}

		public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
		{
			var carouselHolder = (CarouselViewHolder)holder;

			object item = Controller.GetItem(position);
			Controller.BindView(carouselHolder.View, item);
		}

		public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
		{
			// create view from type
			object type = _typeByTypeId[viewType];
			View view = Controller.CreateView(type);

			// create renderer for view
			IVisualElementRenderer renderer = Platform.CreateRenderer(view);
			Platform.SetRenderer(view, renderer);

			// package renderer + view
			return new CarouselViewHolder(view, renderer);
		}

		class CarouselViewHolder : RecyclerView.ViewHolder
		{
			public CarouselViewHolder(View view, IVisualElementRenderer renderer) : base(renderer.ViewGroup)
			{
				VisualElementRenderer = renderer;
				View = view;
			}

			public View View { get; }

			public IVisualElementRenderer VisualElementRenderer { get; }
		}
	}
}