#nullable disable
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Xaml.Diagnostics;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	/// <summary>
	/// Represents a table view control for displaying tabular data.
	/// </summary>
	[Obsolete("Please use CollectionView instead.")]
	[ContentProperty(nameof(Root))]
#if WINDOWS || IOS || MACCATALYST || TIZEN || ANDROID
#pragma warning disable CS0618 // Type or member is obsolete
	[ElementHandler(typeof(Handlers.Compatibility.TableViewRenderer))]
#pragma warning restore CS0618 // Type or member is obsolete
#endif
	public class TableView : View, ITableViewController, IElementConfiguration<TableView>, IVisualTreeElement
	{
		/// <summary>Bindable property for <see cref="RowHeight"/>.</summary>
		public static readonly BindableProperty RowHeightProperty = BindableProperty.Create(nameof(RowHeight), typeof(int), typeof(TableView), -1);

		/// <summary>Bindable property for <see cref="HasUnevenRows"/>.</summary>
		public static readonly BindableProperty HasUnevenRowsProperty = BindableProperty.Create(nameof(HasUnevenRows), typeof(bool), typeof(TableView), false);

		readonly Lazy<PlatformConfigurationRegistry<TableView>> _platformConfigurationRegistry;

		readonly TableSectionModel _tableModel;

		TableIntent _intent = TableIntent.Data;

		TableModel _model;

		/// <summary>
		/// Initializes a new instance of the <see cref="TableView"/> class.
		/// </summary>
		public TableView() : this(null)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="TableView"/> class with the specified root.
		/// </summary>
		public TableView(TableRoot root)
		{
#pragma warning disable CS0618 // Type or member is obsolete
			VerticalOptions = HorizontalOptions = LayoutOptions.FillAndExpand;
#pragma warning restore CS0618 // Type or member is obsolete
			Model = _tableModel = new TableSectionModel(this, root);
			_platformConfigurationRegistry = new Lazy<PlatformConfigurationRegistry<TableView>>(() => new PlatformConfigurationRegistry<TableView>(this));
		}

		/// <summary>
		/// Gets or sets a value indicating whether the table has rows with uneven heights.
		/// </summary>
		public bool HasUnevenRows
		{
			get { return (bool)GetValue(HasUnevenRowsProperty); }
			set { SetValue(HasUnevenRowsProperty, value); }
		}

		/// <summary>
		/// Gets or sets the intent of the table view.
		/// </summary>
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

		/// <summary>
		/// Gets or sets the root of the table view.
		/// </summary>
		public TableRoot Root
		{
			get { return _tableModel.Root; }
			set
			{
				if (_tableModel.Root != null)
				{
					_tableModel.Root.SectionCollectionChanged -= OnSectionCollectionChanged;
					_tableModel.Root.PropertyChanged -= OnTableModelRootPropertyChanged;
					VisualDiagnostics.OnChildRemoved(this, _tableModel.Root, 0);
				}
				_tableModel.Root = value ?? new TableRoot();
				VisualDiagnostics.OnChildAdded(this, _tableModel.Root);
				SetInheritedBindingContext(_tableModel.Root, BindingContext);

				Root.SelectMany(r => r).ForEach(cell => cell.Parent = this);
				_tableModel.Root.SectionCollectionChanged += OnSectionCollectionChanged;
				_tableModel.Root.PropertyChanged += OnTableModelRootPropertyChanged;
				OnModelChanged();
			}
		}

		/// <summary>
		/// Gets or sets the height of each row.
		/// </summary>
		public int RowHeight
		{
			get { return (int)GetValue(RowHeightProperty); }
			set { SetValue(RowHeightProperty, value); }
		}

		/// <summary>
		/// Gets or sets the table model.
		/// </summary>
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

		[Obsolete("Use MeasureOverride instead")]
		protected override SizeRequest OnMeasure(double widthConstraint, double heightConstraint)
		{
			var minimumSize = new Size(40, 40);
			var scaled = DeviceDisplay.MainDisplayInfo.GetScaledScreenSize();
			double width = Math.Min(scaled.Width, scaled.Height);
			var request = new Size(width, Math.Max(scaled.Width, scaled.Height));

			return new SizeRequest(request, minimumSize);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public event EventHandler ModelChanged;

		/// <inheritdoc/>
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
			childCollectionChangedEventArgs.Args.NewItems?.Cast<Cell>().ForEach(cell => cell.Parent = this);
			OnModelChanged();
		}

		void OnTableModelRootPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == TableSectionBase.TitleProperty.PropertyName)
				OnModelChanged();
		}

		IReadOnlyList<Maui.IVisualTreeElement> IVisualTreeElement.GetVisualChildren() => new List<Maui.IVisualTreeElement>() { this.Root };

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
					throw new ArgumentNullException(nameof(item));

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