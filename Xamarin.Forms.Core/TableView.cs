using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Platform;

namespace Xamarin.Forms
{
	[ContentProperty("Root")]
	[RenderWith(typeof(_TableViewRenderer))]
	public class TableView : View, ITableViewController, IElementConfiguration<TableView>
	{
		public static readonly BindableProperty RowHeightProperty = BindableProperty.Create("RowHeight", typeof(int), typeof(TableView), -1);

		public static readonly BindableProperty HasUnevenRowsProperty = BindableProperty.Create("HasUnevenRows", typeof(bool), typeof(TableView), false);

		readonly Lazy<PlatformConfigurationRegistry<TableView>> _platformConfigurationRegistry;

		readonly TableSectionModel _tableModel;

		TableIntent _intent = TableIntent.Data;

		TableModel _model;

		public TableView() : this(null)
		{
		}

		public TableView(TableRoot root)
		{
			VerticalOptions = HorizontalOptions = LayoutOptions.FillAndExpand;
			Model = _tableModel = new TableSectionModel(this, root);
			_platformConfigurationRegistry = new Lazy<PlatformConfigurationRegistry<TableView>>(() => new PlatformConfigurationRegistry<TableView>(this));
		}

		public bool HasUnevenRows
		{
			get { return (bool)GetValue(HasUnevenRowsProperty); }
			set { SetValue(HasUnevenRowsProperty, value); }
		}

		public TableIntent Intent
		{
			get { return _intent; }
			set
			{
				if (_intent == value)
					return;

				OnPropertyChanging();
				_intent = value;
				OnPropertyChanged();
			}
		}

		public TableRoot Root
		{
			get { return _tableModel.Root; }
			set
			{
				if (_tableModel.Root != null)
				{
					_tableModel.Root.SectionCollectionChanged -= OnSectionCollectionChanged;
					_tableModel.Root.PropertyChanged -= OnTableModelRootPropertyChanged;
				}
				_tableModel.Root = value ?? new TableRoot();
				SetInheritedBindingContext(_tableModel.Root, BindingContext);

				Root.SelectMany(r => r).ForEach(cell => cell.Parent = this);
				_tableModel.Root.SectionCollectionChanged += OnSectionCollectionChanged;
				_tableModel.Root.PropertyChanged += OnTableModelRootPropertyChanged;
				OnModelChanged();
			}
		}

		public int RowHeight
		{
			get { return (int)GetValue(RowHeightProperty); }
			set { SetValue(RowHeightProperty, value); }
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public TableModel Model
		{
			get { return _model; }
			set
			{
				_model = value;
				OnModelChanged();
			}
		}

		ITableModel ITableViewController.Model
		{
			get
			{
				return Model;
			}
		}

		protected override void OnBindingContextChanged()
		{
			base.OnBindingContextChanged();
			if (Root != null)
				SetInheritedBindingContext(Root, BindingContext);
		}

		protected virtual void OnModelChanged()
		{
			foreach (Cell cell in Root.SelectMany(r => r))
				cell.Parent = this;

			ModelChanged?.Invoke(this, EventArgs.Empty);
		}

		[Obsolete("OnSizeRequest is obsolete as of version 2.2.0. Please use OnMeasure instead.")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		protected override SizeRequest OnSizeRequest(double widthConstraint, double heightConstraint)
		{
			var minimumSize = new Size(40, 40);
			double width = Math.Min(Device.Info.ScaledScreenSize.Width, Device.Info.ScaledScreenSize.Height);
			var request = new Size(width, Math.Max(Device.Info.ScaledScreenSize.Width, Device.Info.ScaledScreenSize.Height));

			return new SizeRequest(request, minimumSize);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public event EventHandler ModelChanged;

		public IPlatformElementConfiguration<T, TableView> On<T>() where T : IConfigPlatform
		{
			return _platformConfigurationRegistry.Value.On<T>();
		}

		void CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			OnModelChanged();
		}

		void OnSectionCollectionChanged(object sender, ChildCollectionChangedEventArgs childCollectionChangedEventArgs)
		{
			if (childCollectionChangedEventArgs.Args.NewItems != null)
				childCollectionChangedEventArgs.Args.NewItems.Cast<Cell>().ForEach(cell => cell.Parent = this);
			OnModelChanged();
		}

		void OnTableModelRootPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == TableSectionBase.TitleProperty.PropertyName)
				OnModelChanged();
		}

		internal class TableSectionModel : TableModel
		{
			static readonly BindableProperty PathProperty = BindableProperty.Create("Path", typeof(Tuple<int, int>), typeof(Cell), null);

			readonly TableView _parent;
			TableRoot _root;

			public TableSectionModel(TableView tableParent, TableRoot tableRoot)
			{
				_parent = tableParent;
				Root = tableRoot ?? new TableRoot();
			}

			public TableRoot Root
			{
				get { return _root; }
				set
				{
					if (_root == value)
						return;

					RemoveEvents(_root);
					_root = value;
					ApplyEvents(_root);
				}
			}

			public override Cell GetCell(int section, int row)
			{
				var cell = (Cell)GetItem(section, row);
				SetPath(cell, new Tuple<int, int>(section, row));
				return cell;
			}

			public override object GetItem(int section, int row)
			{
				return _root[section][row];
			}

			public override int GetRowCount(int section)
			{
				return _root[section].Count;
			}

			public override int GetSectionCount()
			{
				return _root.Count;
			}

			public override string GetSectionTitle(int section)
			{
				return _root[section].Title;
			}

			public override Color GetSectionTextColor(int section)
			{
				return _root[section].TextColor;
			}

			protected override void OnRowSelected(object item)
			{
				base.OnRowSelected(item);

				((Cell)item).OnTapped();
			}

			internal static Tuple<int, int> GetPath(Cell item)
			{
				if (item == null)
					throw new ArgumentNullException("item");

				return (Tuple<int, int>)item.GetValue(PathProperty);
			}

			void ApplyEvents(TableRoot tableRoot)
			{
				tableRoot.CollectionChanged += _parent.CollectionChanged;
				tableRoot.SectionCollectionChanged += _parent.OnSectionCollectionChanged;
			}

			void RemoveEvents(TableRoot tableRoot)
			{
				if (tableRoot == null)
					return;

				tableRoot.CollectionChanged -= _parent.CollectionChanged;
				tableRoot.SectionCollectionChanged -= _parent.OnSectionCollectionChanged;
			}

			static void SetPath(Cell item, Tuple<int, int> index)
			{
				if (item == null)
					return;

				item.SetValue(PathProperty, index);
			}
		}
	}
}