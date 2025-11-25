#nullable disable
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls
{
	// Don't add IElementConfiguration<Cell> because it kills performance on UWP structures that use Cells
	/// <summary>Provides base class and capabilities for all Microsoft.Maui.Controls cells. Cells are elements meant to be added to <see cref="Microsoft.Maui.Controls.ListView"/> or <see cref="Microsoft.Maui.Controls.TableView"/>.</summary>
#if WINDOWS || ANDROID || IOS || MACCATALYST
#pragma warning disable CS0618 // Type or member is obsolete
	[ElementHandler<Handlers.Compatibility.CellRenderer>]
#pragma warning restore CS0618 // Type or member is obsolete
#endif
	public abstract class Cell : Element, ICellController, IFlowDirectionController, IPropertyPropagationController, IVisualController, IWindowController, IVisualTreeElement
	{
		/// <summary>The default height of cells.</summary>
		public const int DefaultCellHeight = 40;
		/// <summary>Bindable property for <see cref="IsEnabled"/>.</summary>
		public static readonly BindableProperty IsEnabledProperty = BindableProperty.Create(nameof(IsEnabled), typeof(bool), typeof(Cell), true, propertyChanged: OnIsEnabledPropertyChanged);

		ObservableCollection<MenuItem> _contextActions;
		List<MenuItem> _currentContextActions;
		readonly Lazy<ElementConfiguration> _elementConfiguration;

		double _height = -1;

		bool _nextCallToForceUpdateSizeQueued;

		/// <summary>Initializes a new instance of the Cell class.</summary>
		/// <remarks>Cell class is abstract, this constructor is never invoked directly.</remarks>
		public Cell()
		{
			_elementConfiguration = new Lazy<ElementConfiguration>(() => new ElementConfiguration(this));
		}

		EffectiveFlowDirection _effectiveFlowDirection = default(EffectiveFlowDirection);
		EffectiveFlowDirection IFlowDirectionController.EffectiveFlowDirection
		{
			get { return _effectiveFlowDirection; }
			set
			{
				if (value == _effectiveFlowDirection)
					return;

				_effectiveFlowDirection = value;

				var ve = (Parent as VisualElement);
				ve?.InvalidateMeasureInternal(InvalidationTrigger.Undefined);
				OnPropertyChanged(VisualElement.FlowDirectionProperty.PropertyName);
			}
		}

		IVisual _effectiveVisual = Microsoft.Maui.Controls.VisualMarker.Default;
		IVisual IVisualController.EffectiveVisual
		{
			get { return _effectiveVisual; }
			set
			{
				if (value == _effectiveVisual)
					return;

				_effectiveVisual = value;
				OnPropertyChanged(VisualElement.VisualProperty.PropertyName);
			}
		}
		IVisual IVisualController.Visual => Microsoft.Maui.Controls.VisualMarker.MatchParent;

		bool IFlowDirectionController.ApplyEffectiveFlowDirectionToChildContainer => true;

		IFlowDirectionController FlowController => this;
		IPropertyPropagationController PropertyPropagationController => this;

		Window _window;
		Window IWindowController.Window
		{
			get => _window;
			set
			{
				if (value == _window)
					return;

				_window = value;
				OnPropertyChanged(VisualElement.WindowProperty.PropertyName);
			}
		}

		/// <summary>Gets a list of menu items to display when the user performs the device-specific context gesture on the Cell.</summary>
		/// <remarks>The context gesture on the iOS platform is a left swipe. For Android and Windows Phone operating systems, the context gesture is a press and hold.</remarks>
		public IList<MenuItem> ContextActions
		{
			get
			{
				if (_contextActions == null)
				{
					_contextActions = new ObservableCollection<MenuItem>();
					_contextActions.CollectionChanged += OnContextActionsChanged;
				}

				return _contextActions;
			}
		}

		/// <summary>Gets a value that indicates whether the cell has at least one menu item in its <see cref="Microsoft.Maui.Controls.Cell.ContextActions"/> list property.</summary>
		public bool HasContextActions
		{
			get { return _contextActions != null && _contextActions.Count > 0 && IsEnabled; }
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/Cell.xml" path="//Member[@MemberName='IsContextActionsLegacyModeEnabled']/Docs/*" />
		public bool IsContextActionsLegacyModeEnabled { get; set; } = false;

		/// <summary>Gets or sets the height of the Cell.</summary>
		public double Height
		{
			get { return _height; }
			set
			{
				if (_height == value)
					return;

				OnPropertyChanging(nameof(Height));
				OnPropertyChanging(nameof(RenderHeight));
				_height = value;
				OnPropertyChanged(nameof(Height));
				OnPropertyChanged(nameof(RenderHeight));
			}
		}

		/// <summary>Gets or sets the IsEnabled state of the Cell. This is a bindable property.</summary>
		public bool IsEnabled
		{
			get { return (bool)GetValue(IsEnabledProperty); }
			set { SetValue(IsEnabledProperty, value); }
		}

		/// <summary>Gets the height of the rendered cell on the device.</summary>
		public double RenderHeight
		{
			get
			{
#pragma warning disable CS0618 // Type or member is obsolete
				var table = RealParent as TableView;
				if (table != null)
					return table.HasUnevenRows && Height > 0 ? Height : table.RowHeight;

				var list = RealParent as ListView;
#pragma warning restore CS0618 // Type or member is obsolete
				if (list != null)
					return list.HasUnevenRows && Height > 0 ? Height : list.RowHeight;

				return DefaultCellHeight;
			}
		}

		double IFlowDirectionController.Width => (Parent as VisualElement)?.Width ?? 0;

		public event EventHandler Appearing;

		public event EventHandler Disappearing;

		[EditorBrowsable(EditorBrowsableState.Never)]
		public event EventHandler ForceUpdateSizeRequested;

		/// <summary>Immediately updates the cell's size.</summary>
		/// <remarks>Developers can call this method to update the cell's size, even if the cell is currently visible. Developers should note that this operation can be expensive.</remarks>
		public void ForceUpdateSize()
		{
			if (_nextCallToForceUpdateSizeQueued)
				return;

#pragma warning disable CS0618 // Type or member is obsolete
			if ((Parent as ListView)?.HasUnevenRows == true || (Parent as TableView)?.HasUnevenRows == true)
			{
				_nextCallToForceUpdateSizeQueued = true;
				OnForceUpdateSizeRequested();
			}
#pragma warning restore CS0618 // Type or member is obsolete
		}

		public event EventHandler Tapped;

		protected internal virtual void OnTapped()
			=> Tapped?.Invoke(this, EventArgs.Empty);

		protected virtual void OnAppearing()
			=> Appearing?.Invoke(this, EventArgs.Empty);

		protected override void OnBindingContextChanged()
		{
			base.OnBindingContextChanged();

			if (HasContextActions)
			{
				for (var i = 0; i < _contextActions.Count; i++)
					SetInheritedBindingContext(_contextActions[i], BindingContext);
			}
		}

		protected virtual void OnDisappearing()
			=> Disappearing?.Invoke(this, EventArgs.Empty);

		protected override void OnParentSet()
		{
			if (RealParent != null)
			{
				RealParent.PropertyChanged += OnParentPropertyChanged;
				RealParent.PropertyChanging += OnParentPropertyChanging;
			}

			base.OnParentSet();

			PropertyPropagationController.PropagatePropertyChanged(null);
		}

		protected override void OnPropertyChanging(string propertyName = null)
		{
			if (propertyName == "Parent")
			{
				if (RealParent != null)
				{
					RealParent.PropertyChanged -= OnParentPropertyChanged;
					RealParent.PropertyChanging -= OnParentPropertyChanging;
				}

				PropertyPropagationController.PropagatePropertyChanged(null);
			}

			base.OnPropertyChanging(propertyName);
		}

		/// <summary>Internal API for Microsoft.Maui.Controls platform use.</summary>
		/// <remarks>For internal use only. This API can be changed or removed without notice at any time.</remarks>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public void SendAppearing()
		{
			OnAppearing();

#pragma warning disable CS0618 // Type or member is obsolete
			var container = RealParent as ListView;
#pragma warning restore CS0618 // Type or member is obsolete
			container?.SendCellAppearing(this);
		}

		/// <summary>Internal API for Microsoft.Maui.Controls platform use.</summary>
		/// <remarks>For internal use only. This API can be changed or removed without notice at any time.</remarks>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public void SendDisappearing()
		{
			OnDisappearing();

#pragma warning disable CS0618 // Type or member is obsolete
			var container = RealParent as ListView;
#pragma warning restore CS0618 // Type or member is obsolete
			container?.SendCellDisappearing(this);
		}

		void IPropertyPropagationController.PropagatePropertyChanged(string propertyName)
		{
			PropertyPropagationExtensions.PropagatePropertyChanged(propertyName, this, ((IVisualTreeElement)this).GetVisualChildren());
		}

		void OnContextActionsChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			for (var i = 0; i < _contextActions.Count; i++)
			{
				SetInheritedBindingContext(_contextActions[i], BindingContext);
				_contextActions[i].Parent = this;
				_currentContextActions?.Remove(_contextActions[i]);
			}

			if (_currentContextActions != null)
			{
				foreach (MenuItem item in _currentContextActions)
				{
					item.Parent = null;
				}
			}

			_currentContextActions = new List<MenuItem>(_contextActions);

			OnPropertyChanged(nameof(HasContextActions));
		}

		async void OnForceUpdateSizeRequested()
		{
			// don't run more than once per 16 milliseconds
			await Task.Delay(TimeSpan.FromMilliseconds(16));
			ForceUpdateSizeRequested?.Invoke(this, null);
			Handler?.Invoke("ForceUpdateSizeRequested", null);

			_nextCallToForceUpdateSizeQueued = false;
		}

		static void OnIsEnabledPropertyChanged(BindableObject bindable, object oldvalue, object newvalue)
		{
			(bindable as Cell).OnPropertyChanged(nameof(HasContextActions));
		}

		void OnParentPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			// Technically we might be raising this even if it didn't change, but I'm taking the bet that
			// its uncommon enough that we don't want to take the penalty of N GetValue calls to verify.
			if (e.PropertyName == "RowHeight")
				OnPropertyChanged(nameof(RenderHeight));
			else if (e.PropertyName == VisualElement.FlowDirectionProperty.PropertyName ||
					 e.PropertyName == VisualElement.VisualProperty.PropertyName)
				PropertyPropagationController.PropagatePropertyChanged(e.PropertyName);
		}

		void OnParentPropertyChanging(object sender, PropertyChangingEventArgs e)
		{
			if (e.PropertyName == "RowHeight")
				OnPropertyChanging(nameof(RenderHeight));
		}

#if ANDROID
		// This is used by ListView to pass data to the GetCell call
		// Ideally we can pass these as arguments to ToHandler
		// But we'll do that in a different smaller more targeted PR
		internal global::Android.Views.View ConvertView { get; set; }
#elif IOS
		internal UIKit.UITableViewCell ReusableCell { get; set; }

		WeakReference<UIKit.UITableView> _tableView;

		internal UIKit.UITableView TableView
		{
			get => _tableView?.GetTargetOrDefault();
			set => _tableView = value is null ? null : new(value);
		}
#endif


		IReadOnlyList<Maui.IVisualTreeElement> IVisualTreeElement.GetVisualChildren()
		{
			var children = new List<Maui.IVisualTreeElement>(LogicalChildrenInternal);

			if (_contextActions != null)
				children.AddRange(_contextActions);

			return children;
		}

		#region Nested IElementConfiguration<Cell> Implementation
		// This creates a nested class to keep track of IElementConfiguration<Cell> because adding 
		// IElementConfiguration<Cell> to the Cell itself tanks performance on UWP ListViews
		// Issue has been logged with UWP
		public IPlatformElementConfiguration<T, Cell> On<T>() where T : IConfigPlatform
		{
			return GetElementConfiguration().On<T>();
		}

		IElementConfiguration<Cell> GetElementConfiguration()
		{
			return _elementConfiguration.Value;
		}

		class ElementConfiguration : IElementConfiguration<Cell>
		{
			readonly Lazy<PlatformConfigurationRegistry<Cell>> _platformConfigurationRegistry;
			public ElementConfiguration(Cell cell)
			{
				_platformConfigurationRegistry =
					new Lazy<PlatformConfigurationRegistry<Cell>>(() => new PlatformConfigurationRegistry<Cell>(cell));
			}

			/// <inheritdoc/>
			public IPlatformElementConfiguration<T, Cell> On<T>() where T : IConfigPlatform
			{
				return _platformConfigurationRegistry.Value.On<T>();
			}

			internal PlatformConfigurationRegistry<Cell> Registry => _platformConfigurationRegistry.Value;
		}
		#endregion

	}
}