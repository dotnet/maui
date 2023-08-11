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
	/// <include file="../../../docs/Microsoft.Maui.Controls/Cell.xml" path="Type[@FullName='Microsoft.Maui.Controls.Cell']/Docs/*" />
	public abstract class Cell : Element, ICellController, IFlowDirectionController, IPropertyPropagationController, IVisualController, IWindowController, IVisualTreeElement
	{
		/// <include file="../../../docs/Microsoft.Maui.Controls/Cell.xml" path="//Member[@MemberName='DefaultCellHeight']/Docs/*" />
		public const int DefaultCellHeight = 40;
		/// <summary>Bindable property for <see cref="IsEnabled"/>.</summary>
		public static readonly BindableProperty IsEnabledProperty = BindableProperty.Create("IsEnabled", typeof(bool), typeof(Cell), true, propertyChanged: OnIsEnabledPropertyChanged);

		ObservableCollection<MenuItem> _contextActions;
		List<MenuItem> _currentContextActions;
		readonly Lazy<ElementConfiguration> _elementConfiguration;

		double _height = -1;

		bool _nextCallToForceUpdateSizeQueued;

		/// <include file="../../../docs/Microsoft.Maui.Controls/Cell.xml" path="//Member[@MemberName='.ctor']/Docs/*" />
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

		/// <include file="../../../docs/Microsoft.Maui.Controls/Cell.xml" path="//Member[@MemberName='ContextActions']/Docs/*" />
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

		/// <include file="../../../docs/Microsoft.Maui.Controls/Cell.xml" path="//Member[@MemberName='HasContextActions']/Docs/*" />
		public bool HasContextActions
		{
			get { return _contextActions != null && _contextActions.Count > 0 && IsEnabled; }
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/Cell.xml" path="//Member[@MemberName='IsContextActionsLegacyModeEnabled']/Docs/*" />
		public bool IsContextActionsLegacyModeEnabled { get; set; } = false;

		/// <include file="../../../docs/Microsoft.Maui.Controls/Cell.xml" path="//Member[@MemberName='Height']/Docs/*" />
		public double Height
		{
			get { return _height; }
			set
			{
				if (_height == value)
					return;

				OnPropertyChanging("Height");
				OnPropertyChanging("RenderHeight");
				_height = value;
				OnPropertyChanged("Height");
				OnPropertyChanged("RenderHeight");
			}
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/Cell.xml" path="//Member[@MemberName='IsEnabled']/Docs/*" />
		public bool IsEnabled
		{
			get { return (bool)GetValue(IsEnabledProperty); }
			set { SetValue(IsEnabledProperty, value); }
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/Cell.xml" path="//Member[@MemberName='RenderHeight']/Docs/*" />
		public double RenderHeight
		{
			get
			{
				var table = RealParent as TableView;
				if (table != null)
					return table.HasUnevenRows && Height > 0 ? Height : table.RowHeight;

				var list = RealParent as ListView;
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

		/// <include file="../../../docs/Microsoft.Maui.Controls/Cell.xml" path="//Member[@MemberName='ForceUpdateSize']/Docs/*" />
		public void ForceUpdateSize()
		{
			if (_nextCallToForceUpdateSizeQueued)
				return;

			if ((Parent as ListView)?.HasUnevenRows == true || (Parent as TableView)?.HasUnevenRows == true)
			{
				_nextCallToForceUpdateSizeQueued = true;
				OnForceUpdateSizeRequested();
			}
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

		/// <include file="../../../docs/Microsoft.Maui.Controls/Cell.xml" path="//Member[@MemberName='SendAppearing']/Docs/*" />
		[EditorBrowsable(EditorBrowsableState.Never)]
		public void SendAppearing()
		{
			OnAppearing();

			var container = RealParent as ListView;
			container?.SendCellAppearing(this);
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/Cell.xml" path="//Member[@MemberName='SendDisappearing']/Docs/*" />
		[EditorBrowsable(EditorBrowsableState.Never)]
		public void SendDisappearing()
		{
			OnDisappearing();

			var container = RealParent as ListView;
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

			OnPropertyChanged("HasContextActions");
		}

		async void OnForceUpdateSizeRequested()
		{
			// don't run more than once per 16 milliseconds
			await Task.Delay(TimeSpan.FromMilliseconds(16));
			ForceUpdateSizeRequested?.Invoke(this, null);

			_nextCallToForceUpdateSizeQueued = false;
		}

		static void OnIsEnabledPropertyChanged(BindableObject bindable, object oldvalue, object newvalue)
		{
			(bindable as Cell).OnPropertyChanged("HasContextActions");
		}

		void OnParentPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			// Technically we might be raising this even if it didn't change, but I'm taking the bet that
			// its uncommon enough that we don't want to take the penalty of N GetValue calls to verify.
			if (e.PropertyName == "RowHeight")
				OnPropertyChanged("RenderHeight");
			else if (e.PropertyName == VisualElement.FlowDirectionProperty.PropertyName ||
					 e.PropertyName == VisualElement.VisualProperty.PropertyName)
				PropertyPropagationController.PropagatePropertyChanged(e.PropertyName);
		}

		void OnParentPropertyChanging(object sender, PropertyChangingEventArgs e)
		{
			if (e.PropertyName == "RowHeight")
				OnPropertyChanging("RenderHeight");
		}

#if ANDROID
		// This is used by ListView to pass data to the GetCell call
		// Ideally we can pass these as arguments to ToHandler
		// But we'll do that in a different smaller more targeted PR
		internal Android.Views.View ConvertView { get; set; }
#elif IOS
		internal UIKit.UITableViewCell ReusableCell { get; set; }
		internal UIKit.UITableView TableView { get; set; }

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