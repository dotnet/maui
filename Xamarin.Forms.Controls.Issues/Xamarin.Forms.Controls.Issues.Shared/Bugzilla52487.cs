using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;
using System.Linq;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Collections;
using System.Collections.Concurrent;
using System.Threading;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(
		IssueTracker.Bugzilla,
		502487,
		"ListView with Recycle + HasUnevenRows generates lots (and lots!) of content view",
		// https://bugzilla.xamarin.com/show_bug.cgi?id=52487
		PlatformAffected.iOS
	)]
	public class Bugzilla52487 : TestContentPage
	{
#if __IOS__
		const int MaxAskDelta = 0;
		const int MaxViewDelta = 5;
		const int MaxAttachDelta = 5;
#elif __ANDROID__
		const int MaxAskDelta = 0;
		const int MaxViewDelta = 1;
		const int MaxAttachDelta = 1;
#else
		const int MaxAskDelta = 0;
		const int MaxViewDelta = int.MaxValue;
		const int MaxAttachDelta = int.MaxValue;
#endif

		const int CountFontSize = 12;
		const int CellFontSize = 12;
		const int MinScrollDelta = 2;
		const int ItemsCount = 1000;
		const int GroupCount = 100;
		const int DefaultItemHeight = 300 / 4;
		const int MinimumItemHeight = 40;

		// dis-enable item type when % item id is zero
		const int DisableModulous = 11;

		// generate alternate item type when % item id is zero
		const int ItemTypeModulous = 7;

		// render half height when % item id is zero (RecycleElement or RecycleElementAndDataTemplate)
		const int HalfHeightModulous = 5;

		// select alternate cell type when % item id is zero (RecycleElement)
		const int DataTemplateModulous = 3;

		static Tuple<int, int, int> Mix = new Tuple<int, int, int>(255, 255, 100);
		static Tuple<int, int, int> AltMix = new Tuple<int, int, int>(100, 100, 255);
		static IEnumerable<Color> ColorGenerator(Tuple<int, int, int> mix)
		{
			double colorDelta = 0;
			while (true)
			{
				colorDelta += 2 * Math.PI / 100;
				var r = (Math.Sin(colorDelta) + 1) / 2 * 255;
				var g = (Math.Sin(colorDelta * 2) + 1) / 2 * 255;
				var b = (Math.Sin(colorDelta * 3) + 1) / 2 * 255;

				if (mix != null)
				{
					r = (r + mix.Item1) / 2;
					g = (g + mix.Item2) / 2;
					b = (b + mix.Item3) / 2;
				}

				yield return Color.FromRgb((int)r, (int)g, (int)b);
			}
		}

		[Preserve(AllMembers = true)]
		class LazyReadOnlyList<V> : IReadOnlyList<V>
			where V : class
		{
			int _count;
			object _context;
			List<WeakReference<V>> _items;
			Action<int> _onAsk;
			Func<LazyReadOnlyList<V>, int, object, V> _activate;

			internal LazyReadOnlyList(
				int count,
				object context,
				Action<int> onAsk,
				Func<LazyReadOnlyList<V>, int, object, V> activate)
			{
				_count = count;
				_context = context;
				_onAsk = onAsk;
				_activate = activate;
				_items = new List<WeakReference<V>>(
					Enumerable.Range(0, count)
					.Select(o => new WeakReference<V>(null))
				);
			}

			protected object Context
			{
				get { return _context; }
				set { _context = value; }
			}
			protected IEnumerable<WeakReference<V>> WeakItems =>
				_items;

			public V this[int index]
			{
				get
				{
					_onAsk(index);

					var weakItem = _items[index];

					V item;
					if (!weakItem.TryGetTarget(out item))
					{
						_items[index] =
							new WeakReference<V>(
								item = _activate(this, index, _context));
					}

					return item;
				}
			}

			public int Count
				=> _count;

			public IEnumerator<V> GetEnumerator()
			{
				for (var i = 0; i < Count; i++)
					yield return this[i];
			}

			IEnumerator IEnumerable.GetEnumerator()
				=> GetEnumerator();
		}

		[Preserve(AllMembers = true)]
		public abstract partial class ListViewSpy<T> : ListViewSpy
		{
			[Preserve(AllMembers = true)]
			abstract class Selector : DataTemplateSelector
			{
				[Preserve(AllMembers = true)]
				internal class SelectByData : Selector
				{
					public SelectByData() : base(
						typeof(ItemViewCell.Selected.ByDataNormal),
						typeof(ItemViewCell.Selected.ByDataAlternate)
					)
					{ }

					protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
					{
						// RecycleElement previously placed no restraint on the type of view 
						// the resulting DataTemplate could return. So the resulting DataTemplate
						// could randomly pick a view type to render items even between appearances
						// on screen. 

						// After this fix, the DataTempate will be required to return the same
						// type of view although the type need not be a function of only the item type.
						// So the type of view a DataTemplates chooses to return can be a function of
						// the item _data_ and not only an items type.

						__counter.OnSelectTemplate++;

						// item could be either Item.Full type or Item.Half type...
						if (!(item is Item.Normal) && !(item is Item.Alternate))
							throw new ArgumentException();

						// ... but selector chooses DataTemplate strictly via item _data_.
						return ((Item)item).Id % DataTemplateModulous == 0 ?
							_dataTemplateAlt : _dataTemplate;
					}
				}

				[Preserve(AllMembers = true)]
				internal class SelectByType : Selector
				{
					public SelectByType() : base(
						typeof(ItemViewCell.Selected.ByTypeNormal),
						typeof(ItemViewCell.Selected.ByTypeAlternate)
					)
					{ }

					protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
					{
						// RecycleElementAndDataTemplate requires that 
						// DataTempalte be a function of the item _type_

						__counter.OnSelectTemplate++;

						if (item is Item.Normal)
							return _dataTemplate;

						if (item is Item.Alternate)
							return _dataTemplateAlt;

						throw new ArgumentException();
					}
				}

				DataTemplate _dataTemplate;
				DataTemplate _dataTemplateAlt;

				public Selector(Type nomral, Type alternate)
				{
					// RecycleElementAndDataTemplate requires 
					// that the DataTemplate use the .ctor that takes a type
					_dataTemplate = new DataTemplate(nomral);
					_dataTemplateAlt = new DataTemplate(alternate);
				}
			}

			[Preserve(AllMembers = true)]
			abstract class ItemViewCell : ViewCell
			{
				[Preserve(AllMembers = true)]
				internal abstract class Selected : ItemViewCell
				{
					[Preserve(AllMembers = true)]
					internal class ByTypeNormal : ByType
					{
						static Color NextColor() { Colors.MoveNext(); return Colors.Current; }
						static readonly IEnumerator<Color> Colors = ColorGenerator(Mix).GetEnumerator();

						public ByTypeNormal() : base(NextColor()) { }
					}
					[Preserve(AllMembers = true)]
					internal class ByTypeAlternate : ByType
					{
						static Color NextColor() { Colors.MoveNext(); return Colors.Current; }
						static readonly IEnumerator<Color> Colors = ColorGenerator(AltMix).GetEnumerator();

						public ByTypeAlternate() : base(NextColor()) { }

						protected override bool IsAlternate => true;
					}
					[Preserve(AllMembers = true)]
					internal abstract class ByType : Selected
					{
						internal ByType(Color color)
							: base(color) { }

						protected override void OnBindingContextChanged()
						{
							base.OnBindingContextChanged();

							if (BindingContext == null || !(BindingContext is ItemViewCell))
								return;

							// check that template is a function of the item type
							var itemType = BindingContext.GetType();
							var itemIsNormalType = itemType == typeof(Item.Normal);
							var expectedTemplateType = itemIsNormalType ? typeof(ByTypeNormal) : typeof(ByTypeAlternate);

							var templateType = GetType();
							if (templateType != expectedTemplateType)
								throw new ArgumentException(
									$"BindingContext.GetType() = {itemType.Name}, " +
									$"TemplateType {templateType.Name}!={expectedTemplateType.Name}");
						}
					}

					[Preserve(AllMembers = true)]
					internal class ByDataNormal : ByData
					{
						static Color NextColor() { Colors.MoveNext(); return Colors.Current; }
						static readonly IEnumerator<Color> Colors = ColorGenerator(Mix).GetEnumerator();

						public ByDataNormal() : base(NextColor()) { }
					}
					[Preserve(AllMembers = true)]
					internal class ByDataAlternate : ByData
					{
						static Color NextColor() { Colors.MoveNext(); return Colors.Current; }
						static readonly IEnumerator<Color> Colors = ColorGenerator(AltMix).GetEnumerator();

						public ByDataAlternate() : base(NextColor()) { }

						protected override bool IsAlternate => true;
					}
					[Preserve(AllMembers = true)]
					internal abstract class ByData : Selected
					{
						internal ByData(Color color)
							: base(color) { }

						protected override void OnBindingContextChanged()
						{
							base.OnBindingContextChanged();

							if (BindingContext == null)
								return;

							// check that template is a function of the item data
							var isRemainderZero = BindingContext.Id % DataTemplateModulous == 0;
							var expectedItemType = isRemainderZero ? typeof(Item.Alternate) : typeof(Item.Normal);
							var expectedTemplateType = isRemainderZero ? typeof(ByDataAlternate) : typeof(ByDataNormal);

							var templateType = GetType();
							if (templateType != expectedTemplateType)
								throw new ArgumentException(
									$"Item.Id = {BindingContext?.Id}, " +
									$"TemplateType {templateType.Name}!={expectedTemplateType.Name}");
						}
					}

					internal Selected(Color color)
						: base(color) { }
				}

				[Preserve(AllMembers = true)]
				internal class Constant : ItemViewCell
				{
					static Color NextColor() { Colors.MoveNext(); return Colors.Current; }
					static readonly IEnumerator<Color> Colors = ColorGenerator(Mix).GetEnumerator();

					public Constant() : base(NextColor()) { }
				}

				readonly int _id;
				readonly Label _label;

				ItemViewCell(Color color)
				{
					_id = __counter.CellAlloc++;

					View = _label = new Label
					{
						BackgroundColor = color,
						VerticalTextAlignment = TextAlignment.Center,
						HorizontalTextAlignment = TextAlignment.Center,
						FontSize = CellFontSize
					};

					_label.SetBinding(HeightRequestProperty, nameof(Item.Value));
				}

				Item BindingContext
					=> (Item)base.BindingContext;
				int ItemId
					=> BindingContext.Id;
				int? ItemGroupId
					=> BindingContext.GroupId;
				bool IsAlternateItem
					=> BindingContext is Item.Alternate;

				protected virtual bool IsAlternate => false;

				protected override void OnBindingContextChanged()
				{
					base.OnBindingContextChanged();

					__counter.CellBind++;

					if (BindingContext == null)
						return;

					// double check that item generator returned correct type of item
					var isRemainderZero = BindingContext.Id % ItemTypeModulous == 0;
					var expectedItemType = isRemainderZero ? typeof(Item.Alternate) : typeof(Item.Normal);
					var itemType = BindingContext.GetType();
					if (itemType != expectedItemType)
						throw new ArgumentException(
							$"Item.Id = {BindingContext?.Id}, ItemType {GetType().Name}!={expectedItemType.Name}");

				}

				protected override void OnAppearing()
				{
					IsEnabled = ItemId % DisableModulous == 0;

					_label.Text = ToString();
					_label.FontAttributes = IsEnabled ? FontAttributes.Italic : FontAttributes.None;

					__counter.AttachCell(_id);
				}

				public new int Id
					=> _id;

				// cell type is (1) constant or a function of the the Item (2) data or (3) type
				public override string ToString()
					=> $"{ItemId}" +
						(ItemGroupId == null ? "" : "/" + ItemGroupId) +
							$"{(IsAlternateItem ? "*" : "")} ->" +
								$" {_id}{(IsAlternate ? "*" : "")}";

				~ItemViewCell()
				{
					int id;
					__counter.CellFree++;
					__counter.DetachCell(_id);
					// update would be off UI thread
				}
			}

			[Preserve(AllMembers = true)]
			abstract class Item : INotifyPropertyChanged
			{
				[Preserve(AllMembers = true)]
				internal class Normal : Item
				{
					internal Normal(int id, int? groupId, int height)
						: base(id, groupId, height) { }
				};

				[Preserve(AllMembers = true)]
				internal class Alternate : Item
				{
					internal Alternate(int id, int? groupId, int height)
						: base(id, groupId, height) { }
				};

				internal static Item Create(LazyItemList list, int index, int height)
				{

					var id = list.ItemIdOffset + index;

					if (id % ItemTypeModulous == 0)
						return new Alternate(id, list.Id, height);

					return new Normal(id, list.Id, height);
				}

				int _allocId;

				int _id;
				int _index;
				int? _groupId;
				int _height;

				private Item(int id, int? groupId, int height)
				{
					_allocId = Interlocked.Increment(ref __counter.ItemAlloc);

					if (id % HalfHeightModulous == 0)
						height = height / 2;

					_groupId = groupId;
					_height = height;
					_id = id;
				}

				public int Id
					=> _id;
				public int? GroupId
					=> _groupId;
				public int Value
				{
					get { return _height; }
					set
					{
						_height = value;
						OnPropertyChanged();
					}
				}

				public event PropertyChangedEventHandler PropertyChanged;
				protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
					=> PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

				public override string ToString()
					=> _groupId == null ?
						$"{Id}, value={Value}, _alloc={_allocId}" :
						$"{Id}, group={GroupId}, value={Value}, _alloc={_allocId}";

				~Item()
				{
					Interlocked.Increment(ref __counter.ItemFree);
				}
			}

			interface IItemList : IEnumerable, IDisposable
			{
				void UpdateHeights(double multipule);
				int Count { get; }
				void Dispose();
				Item this[int index] { get; }
			}

			[Preserve(AllMembers = true)]
			class LazyItemList : LazyReadOnlyList<Item>, IItemList
			{
				int _itemIdOffset;
				int? _id;
				int _count;

				internal LazyItemList(int count)
					: this(null /* grouping disabled */, 0, count) { }
				internal LazyItemList(int? id, int itemIdOffset, int count)
					: base(count, DefaultItemHeight,
						onAsk: o => __counter.ViewModelAsk.Add(o),
						activate: (self, subIndex, height) =>
							Item.Create(
								list: (LazyItemList)self,
								index: subIndex,
								height: (int)height
							)
					)
				{
					_id = id;
					_itemIdOffset = itemIdOffset;
					_count = count;
				}

				protected int Context
				{
					get { return (int)base.Context; }
					set { base.Context = value; }
				}

				public void UpdateHeights(double multipule)
				{
					if (multipule < 1 && Context < MinimumItemHeight)
						return;

					Context = (int)(Context * multipule);

					foreach (var weakItem in WeakItems)
					{
						Item item;
						if (!(weakItem.TryGetTarget(out item)))
							continue;

						item.Value = Context;
					}
				}
				public int ItemIdOffset
					=> _itemIdOffset;
				public int? Id
					=> _id;

				public void Dispose()
				{
					foreach (var weakItem in WeakItems)
						weakItem.SetTarget(null);
				}

				public override string ToString()
					=> $"{_id}";
			}

			[Preserve(AllMembers = true)]
			class LazyGroupedItemList : LazyReadOnlyList<LazyItemList>, IItemList
			{
				int _itemsPerGroup;

				internal LazyGroupedItemList(int numberOfGroups, int count)
					: base(numberOfGroups,
						  context: null,
						  onAsk: o => __counter.ViewModelAsk.Add(o),
						  activate: (self, groupId, context) =>
							new LazyItemList(
								id: groupId,
								itemIdOffset: groupId * (count / numberOfGroups),
								count: count / numberOfGroups
							)
						)
				{ _itemsPerGroup = count / numberOfGroups; }

				Item IItemList.this[int index]
				{
					get
					{
						var group = index / _itemsPerGroup;
						index = index % _itemsPerGroup;
						return this[group][index];
					}
				}

				public void UpdateHeights(double multipule)
				{
					foreach (var weakItem in WeakItems)
					{
						LazyItemList group;
						if (!(weakItem.TryGetTarget(out group)))
							continue;

						group.UpdateHeights(multipule);
					}
				}

				public void Dispose()
				{
					foreach (var weakItem in WeakItems)
					{
						LazyItemList group;
						if (!(weakItem.TryGetTarget(out group)))
							continue;

						group.Dispose();
					}
				}
			}

			[Preserve(AllMembers = true)]
			class SnapShot
			{
				Counter _counter;

				internal int Views;
				internal int Attached;
				internal int Binds;
				internal int Items;
				internal int Asks;

				public SnapShot(Counter counter)
				{
					_counter = counter;

					Views = _counter.Views;
					Attached = _counter.AttachedCells;
					Binds = _counter.CellBind;
					Items = _counter.Items;
					Asks = _counter.OnSelectTemplate;
				}

				void Subtract()
				{
					Views = _counter.Views - Views;
					Attached = _counter.AttachedCells - Attached;
					Binds = _counter.CellBind - Binds;
					Items = _counter.Items - Items;
					Asks = _counter.OnSelectTemplate - Asks;
				}

				public void Update()
					=> Subtract();
			}

			[Preserve(AllMembers = true)]
			class Counter
			{
				internal int CellAlloc;
				internal int CellFree;
				internal int CellBind;

				internal int ItemAlloc;
				internal int ItemFree;
				internal int OnSelectTemplate;
				internal HashSet<int> ViewModelAsk
					= new HashSet<int>();

				HashSet<int> CellAttached
					= new HashSet<int>();

				internal int AttachedCells
				{
					get
					{
						lock (this)
							return CellAttached.Count;
					}
				}
				internal void AttachCell(int id)
				{
					lock (this)
						CellAttached.Add(id);
				}
				internal void DetachCell(int id)
				{
					lock (this)
						CellAttached.Remove(id);
				}

				internal int Views => CellAlloc - CellFree;
				internal int Items => ItemAlloc - ItemFree;
			}

			[Preserve(AllMembers = true)]
			class CounterView : StackLayout
			{
				static Label CreateLabel()
					=> new Label() { FontSize = CountFontSize };

				internal void Update()
				{
					ViewCountLabel.Text
						= $"View={__counter.Views}";
					AttachedCountLabel.Text
						= $"Atch={__counter.AttachedCells}";
					BindCountLabel.Text
						= $"Bind={__counter.CellBind}";
					ItemCountLabel.Text
						= $"Item={__counter.Items}";
					AskLabel.Text
						= $"Ask={__counter.OnSelectTemplate}";
				}

				Label AttachedCountLabel = CreateLabel();
				Label ViewCountLabel = CreateLabel();
				Label BindCountLabel = CreateLabel();
				Label ItemCountLabel = CreateLabel();
				Label AskLabel = CreateLabel();

				internal CounterView()
				{
					Children.Add(ViewCountLabel);
					Children.Add(AttachedCountLabel);
					Children.Add(BindCountLabel);
					Children.Add(ItemCountLabel);
					Children.Add(AskLabel);
					Update();
				}
			}

			// Cell is activated via DataTemplate using default ctor which
			// makes it difficult to pass the counter to the cell. So we make
			// it static to give cell access and create a different generic
			// instantiation for each type of ListView to get different counters
			static Counter __counter = new Counter();

			ListView _listView;
			int _appeared;
			int _disappeared;
			IItemList _itemsList;

			public ListViewSpy()
			{
				__listViewSpyAlloc++;

				var name = GetType().Name;

				var hasUnevenRows = name.Contains("UnevenRows");

				var isGrouped = name.Contains("Grouped");

				_itemsList = isGrouped ? (IItemList)
					new LazyGroupedItemList(GroupCount, ItemsCount) :
					new LazyItemList(ItemsCount);

				var strategy =
					name.Contains("RecycleElementAndDataTemplate") ? ListViewCachingStrategy.RecycleElementAndDataTemplate :
					name.Contains("RecycleElement") ? ListViewCachingStrategy.RecycleElement :
					ListViewCachingStrategy.RetainElement;

				var dataTemplate =
					strategy == ListViewCachingStrategy.RecycleElement ? new Selector.SelectByData() :
					strategy == ListViewCachingStrategy.RecycleElementAndDataTemplate ? new Selector.SelectByType() :
					new DataTemplate(typeof(ItemViewCell.Constant));

				_listView = new ListView(strategy)
				{
					HasUnevenRows = hasUnevenRows,
					// see https://github.com/xamarin/Xamarin.Forms/pull/994/files
					//RowHeight = 50,
					ItemsSource = _itemsList,
					ItemTemplate = dataTemplate,

					IsGroupingEnabled = isGrouped,
					GroupDisplayBinding = null,
					GroupShortNameBinding = null,
					GroupHeaderTemplate = null
				};
				Children.Add(_listView);

				_listView.AutomationId = $"__ListView";

				var counter = new CounterView();

				_listView.ItemAppearing += (o, e) =>
				{
					_appeared = (e.Item as Item)?.Id ?? -1;
					counter.Update();
				};

				_listView.ItemDisappearing += (o, e) =>
				{
					_disappeared = (e.Item as Item)?.Id ?? -1;
					counter.Update();
				};

				Children.Add(counter);
			}

			void Scroll(int target)
			{
				var snapShot = new SnapShot(__counter);
				_listView.ScrollTo(_itemsList[target], ScrollToPosition.MakeVisible, animated: true);
				snapShot.Update();

				// TEST
				if (!_listView.IsGroupingEnabled &&
					_listView.CachingStrategy == ListViewCachingStrategy.RecycleElementAndDataTemplate)
				{
					if (snapShot.Attached > MaxAttachDelta)
						throw new Exception($"Attached Delta: {snapShot.Attached}");
					if (snapShot.Views > MaxViewDelta)
						throw new Exception($"Views Delta: {snapShot.Views}");
					if (snapShot.Asks > MaxAskDelta)
						throw new Exception($"Asks Delta: {snapShot.Asks}");
				}
			}

			internal override void Down()
			{
				var target = Math.Max(_appeared, _disappeared);
				target += Math.Abs(_appeared - _disappeared) + MinScrollDelta;
				if (target >= _itemsList.Count)
					target = _itemsList.Count - 1;

				Scroll(target);
			}
			internal override void Up()
			{
				var target = Math.Min(_appeared, _disappeared);
				target -= Math.Abs(_appeared - _disappeared) + MinScrollDelta;
				if (target < 0)
					target = 0;

				Scroll(target);
			}
			internal override void UpdateHeights(double multipule)
				=> _itemsList.UpdateHeights(multipule);

			internal override void Dispose()
				=> _itemsList.Dispose();

			~ListViewSpy()
			{
				__listViewSpyFree++;
			}
		}

		[Preserve(AllMembers = true)]
		public abstract partial class ListViewSpy : StackLayout
		{
			[Preserve(AllMembers = true)]
			internal sealed class Retain :
				ListViewSpy<Retain>
			{ }

			[Preserve(AllMembers = true)]
			internal sealed class UnevenRowsRecycleElement :
				ListViewSpy<UnevenRowsRecycleElement>
			{ }

			[Preserve(AllMembers = true)]
			internal sealed class UnevenRowsRecycleElementAndDataTemplate :
				ListViewSpy<UnevenRowsRecycleElementAndDataTemplate>
			{ }

			[Preserve(AllMembers = true)]
			internal sealed class EvenRowsRecycleElement :
				ListViewSpy<EvenRowsRecycleElement>
			{ }

			[Preserve(AllMembers = true)]
			internal sealed class EvenRowsRecycleElementAndDataTemplate :
				ListViewSpy<EvenRowsRecycleElementAndDataTemplate>
			{ }

			[Preserve(AllMembers = true)]
			internal sealed class GroupedRetain :
				ListViewSpy<GroupedRetain>
			{ }

			[Preserve(AllMembers = true)]
			internal sealed class GroupedUnevenRowsRecycleElement :
				ListViewSpy<GroupedUnevenRowsRecycleElement>
			{ }

			[Preserve(AllMembers = true)]
			internal sealed class GroupedUnevenRowsRecycleElementAndDataTemplate :
				ListViewSpy<GroupedUnevenRowsRecycleElementAndDataTemplate>
			{ }

			[Preserve(AllMembers = true)]
			internal sealed class GroupedEvenRowsRecycleElement :
				ListViewSpy<GroupedEvenRowsRecycleElement>
			{ }

			[Preserve(AllMembers = true)]
			internal sealed class GroupedEvenRowsRecycleElementAndDataTemplate :
				ListViewSpy<GroupedEvenRowsRecycleElementAndDataTemplate>
			{ }

			internal abstract void Down();
			internal abstract void Up();
			internal abstract void UpdateHeights(double difference);
			internal abstract void Dispose();
		}

		static int __listViewSpyAlloc;
		static int __listViewSpyFree;
		ListViewSpy[] __listViews;

		IEnumerable<ListViewSpy> ListViews()
			=> __listViews ?? Enumerable.Empty<ListViewSpy>();

		void Update()
			=> Title = $"ListViews={__listViewSpyAlloc - __listViewSpyFree}";

		Grid RecycleListViews(bool group = false)
		{
			// reclaim
			foreach (var o in ListViews())
				o.Dispose();

			GC.Collect();
			GC.WaitForPendingFinalizers();

			__listViews = group ?
				new ListViewSpy[]
				{
					new ListViewSpy.GroupedRetain(),
					new ListViewSpy.GroupedEvenRowsRecycleElement(),
					new ListViewSpy.GroupedEvenRowsRecycleElementAndDataTemplate(),
					new ListViewSpy.GroupedUnevenRowsRecycleElement(),
					new ListViewSpy.GroupedUnevenRowsRecycleElementAndDataTemplate(),
				} :
				new ListViewSpy[]
				{
					new ListViewSpy.Retain(),
					new ListViewSpy.EvenRowsRecycleElement(),
					new ListViewSpy.EvenRowsRecycleElementAndDataTemplate(),
					new ListViewSpy.UnevenRowsRecycleElement(),
					new ListViewSpy.UnevenRowsRecycleElementAndDataTemplate(),
				};

			var grid = new Grid();
			foreach (var o in __listViews)
				grid.Children.AddHorizontal(o);

			Update();

			return grid;
		}

		private class ButtonGird : Grid
		{
			Bugzilla52487 _test;

			internal ButtonGird(Bugzilla52487 test)
			{
				_test = test;
			}

			private void ForEachListView(Action<ListViewSpy> onClick) 
				=> _test.ListViews().ForEach(o => onClick(o));

			public void Add(View view)
				=> Children.AddHorizontal(view);

			public Switch AddSwitch(Action<bool> onToggle)
			{
				var toggle = new Switch();
				toggle.Toggled += (o, s) => onToggle(s.Value);
				Add(toggle);
				return toggle;
			}

			public Button AddButton(string text, Action onClick)
			{
				var button = new Button() { Text = text };
				button.Clicked += (o, s) => onClick();
				Add(button);
				return button;
			}

			public Button AddButton(string text, Action<ListViewSpy> onClick)
				=> AddButton(text, () => ForEachListView(onClick));

			public Entry AddEntry()
			{
				var entry = new Entry();
				Add(entry);
				return entry;
			}
		}

		protected override void Init()
		{
			var listViewGrid = new ContentView();
			listViewGrid.Content = RecycleListViews(false);

			var buttonsGrid = new ButtonGird(this);
			var more = buttonsGrid.AddButton("More", x => x.UpdateHeights(2));
			var less = buttonsGrid.AddButton($"Less", x => x.UpdateHeights(.5));
			var up = buttonsGrid.AddButton("Up", x => x.Up());
			var down = buttonsGrid.AddButton("Down", x => x.Down());
			var group = buttonsGrid.AddSwitch(
				isGrouped => listViewGrid.Content = RecycleListViews(isGrouped));

			Content = new StackLayout
			{
				Children = {
					listViewGrid,
					buttonsGrid,
				}
			};

			Update();
		}
		public static class Id
		{
			public static string Down = nameof(Down);
		}
#if UITEST
		[Test]
		public void Bugzilla52487Test()
		{
			try
			{
				RunningApp.WaitForElement(Id.Down);
				RunningApp.Screenshot("Down");

				RunningApp.Tap(Id.Down);
			}

			finally
			{
				RunningApp.Screenshot("Finally");
			}
		}
#endif
	}
}