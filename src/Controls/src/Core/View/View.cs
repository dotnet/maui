#nullable disable
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.HotReload;

namespace Microsoft.Maui.Controls
{
	/// <summary>A visual element that is used to place layouts and controls on the screen.</summary>
	/// <remarks>
	/// This is the base class for <see cref="Layout"/> and most of the controls.
	/// Because <see cref="View" /> ultimately inherits from <see cref="BindableObject" />, application developers can use the Model-View-ViewModel architecture, as well as XAML, to develop portable user interfaces.
	/// </remarks>
	public partial class View : VisualElement, IViewController, IGestureController, IGestureRecognizers, IView, IPropertyMapperView, IHotReloadableView, IControlsView, IViewWithWindow
	{
		protected internal IGestureController GestureController => this;

		/// <summary>Bindable property for <see cref="VerticalOptions"/>.</summary>
		public static readonly BindableProperty VerticalOptionsProperty =
			BindableProperty.Create(nameof(VerticalOptions), typeof(LayoutOptions), typeof(View), LayoutOptions.Fill,
									propertyChanged: (bindable, oldvalue, newvalue) =>
									((View)bindable).InvalidateMeasureInternal(InvalidationTrigger.VerticalOptionsChanged));

		/// <summary>Bindable property for <see cref="HorizontalOptions"/>.</summary>
		public static readonly BindableProperty HorizontalOptionsProperty =
			BindableProperty.Create(nameof(HorizontalOptions), typeof(LayoutOptions), typeof(View), LayoutOptions.Fill,
									propertyChanged: (bindable, oldvalue, newvalue) =>
									((View)bindable).InvalidateMeasureInternal(InvalidationTrigger.HorizontalOptionsChanged));

		/// <summary>Bindable property for <see cref="Margin"/>.</summary>
		public static readonly BindableProperty MarginProperty =
			BindableProperty.Create(nameof(Margin), typeof(Thickness), typeof(View), default(Thickness),
									propertyChanged: MarginPropertyChanged);

		internal static readonly BindableProperty MarginLeftProperty =
			BindableProperty.Create("MarginLeft", typeof(double), typeof(View), default(double),
									propertyChanged: OnMarginLeftPropertyChanged);

		static void OnMarginLeftPropertyChanged(BindableObject bindable, object oldValue, object newValue)
		{
			var margin = (Thickness)bindable.GetValue(MarginProperty);
			margin.Left = (double)newValue;
			bindable.SetValue(MarginProperty, margin);
		}

		internal static readonly BindableProperty MarginTopProperty =
			BindableProperty.Create("MarginTop", typeof(double), typeof(View), default(double),
									propertyChanged: OnMarginTopPropertyChanged);

		static void OnMarginTopPropertyChanged(BindableObject bindable, object oldValue, object newValue)
		{
			var margin = (Thickness)bindable.GetValue(MarginProperty);
			margin.Top = (double)newValue;
			bindable.SetValue(MarginProperty, margin);
		}

		internal static readonly BindableProperty MarginRightProperty =
			BindableProperty.Create("MarginRight", typeof(double), typeof(View), default(double),
									propertyChanged: OnMarginRightPropertyChanged);

		static void OnMarginRightPropertyChanged(BindableObject bindable, object oldValue, object newValue)
		{
			var margin = (Thickness)bindable.GetValue(MarginProperty);
			margin.Right = (double)newValue;
			bindable.SetValue(MarginProperty, margin);
		}

		internal static readonly BindableProperty MarginBottomProperty =
			BindableProperty.Create("MarginBottom", typeof(double), typeof(View), default(double),
									propertyChanged: OnMarginBottomPropertyChanged);


		static void OnMarginBottomPropertyChanged(BindableObject bindable, object oldValue, object newValue)
		{
			var margin = (Thickness)bindable.GetValue(MarginProperty);
			margin.Bottom = (double)newValue;
			bindable.SetValue(MarginProperty, margin);
		}

		readonly ObservableCollection<IGestureRecognizer> _gestureRecognizers = new ObservableCollection<IGestureRecognizer>();

		PointerGestureRecognizer _recognizerForPointerOverState;

		/// <summary>
		/// Initializes a new instance of the <see cref="View"/> class
		/// </summary>
		/// <remarks>It is unlikely that an application developer would want to create an instance of <see cref="View"/> on their own.</remarks>
		protected internal View()
		{
			_gestureManager = new GestureManager(this);
			_gestureRecognizers.CollectionChanged += (sender, args) =>
			{
				void AddItems(IList newItems)
				{
					foreach (IElementDefinition item in newItems)
					{
						var gestureRecognizer = item as IGestureRecognizer;
						if (ValidateGesture(gestureRecognizer))
						{
							item.Parent = this;
							GestureController.CompositeGestureRecognizers.Add(gestureRecognizer);
						}
					}
				}

				void RemoveItems(IList oldItems)
				{
					foreach (IElementDefinition item in oldItems)
					{
						item.Parent = null;
						GestureController.CompositeGestureRecognizers.Remove(item as IGestureRecognizer);
					}
				}

				switch (args.Action)
				{
					case NotifyCollectionChangedAction.Add:
						AddItems(args.NewItems);
						break;
					case NotifyCollectionChangedAction.Remove:
						RemoveItems(args.OldItems);
						break;
					case NotifyCollectionChangedAction.Replace:
						AddItems(args.NewItems);
						RemoveItems(args.OldItems);
						break;
					case NotifyCollectionChangedAction.Reset:

						foreach (IGestureRecognizer gestureRecognizer in _gestureRecognizers)
						{
							if (gestureRecognizer is IElementDefinition item)
							{
								item.Parent = this;
							}
						}

						HashSet<IGestureRecognizer> compositeGestureRecognizers = new(GestureController.CompositeGestureRecognizers);

						foreach (IGestureRecognizer gestureRecognizer in compositeGestureRecognizers)
						{
							if (gestureRecognizer is IElementDefinition item)
							{
								if (item == _recognizerForPointerOverState)
									continue;

								if (_gestureRecognizers.Contains(gestureRecognizer))
								{
									item.Parent = this;
								}
								else
								{
									item.Parent = null;
									GestureController.CompositeGestureRecognizers.Remove(gestureRecognizer);
								}
							}
						}

						break;
				}
			};
		}

		/// <summary>The collection of gesture recognizers associated with this view.</summary>
		/// <remarks>
		/// Adding items to this collection will associate gesture events with this element.
		/// It is not recommended to add gesture recognizers for gestures that elements already natively support.
		/// <para>
		/// For example, adding a <see cref="TapGestureRecognizer"/> to a <see cref="Button"/> may lead to unexpected results.
		/// </para>
		/// </remarks>
		public IList<IGestureRecognizer> GestureRecognizers
		{
			get { return _gestureRecognizers; }
		}

		ObservableCollection<IGestureRecognizer> _compositeGestureRecognizers;
		IList<IGestureRecognizer> IGestureController.CompositeGestureRecognizers
		{
			get
			{
				if (_compositeGestureRecognizers == null)
				{
					_compositeGestureRecognizers = new ObservableCollection<IGestureRecognizer>();
					CheckPointerOver();
				}

				return _compositeGestureRecognizers;
			}
		}

		protected internal override void ChangeVisualState()
		{
			CheckPointerOver();

			if (_recognizerForPointerOverState == null && IsPointerOver)
				SetPointerOver(false, false);

			base.ChangeVisualState();
		}

		void CheckPointerOver() =>
			PointerGestureRecognizer
				.SetupForPointerOverVSM(this, (result) => SetPointerOver(result), ref _recognizerForPointerOverState);

		/// <summary>
		/// Gets the child elements that are visually beneath the specified <paramref name="point" />.
		/// </summary>
		/// <param name="point">The point under which to search for child elements.</param>
		/// <returns>All child elements visually beneath <paramref name="point"/>.</returns>
		public virtual IList<GestureElement> GetChildElements(Point point)
		{
			return null;
		}

		/// <summary>
		/// Gets or sets the <see cref="LayoutOptions" /> that define how the element gets arranged in a layout cycle. This is a bindable property.
		/// </summary>
		/// <remarks>
		/// Assigning <see cref="HorizontalOptions"/> modifies how the element is arranged when there is excess space available along the X axis from the parent layout.
		/// If multiple elements inside a layout are set to expand, the extra space is distributed proportionally.
		/// </remarks>
		public LayoutOptions HorizontalOptions
		{
			get { return (LayoutOptions)GetValue(HorizontalOptionsProperty); }
			set { SetValue(HorizontalOptionsProperty, value); }
		}

		/// <summary>
		/// Gets or set the margin for the view.
		/// </summary>
		public Thickness Margin
		{
			get { return (Thickness)GetValue(MarginProperty); }
			set { SetValue(MarginProperty, value); }
		}

		/// <summary>
		/// Gets or sets the <see cref="LayoutOptions" /> that define how the element gets arrange in a layout cycle. This is a bindable property.
		/// </summary>
		/// <remarks>
		/// Assigning <see cref="VerticalOptions"/> modifies how the element is arrange when there is excess space available along the Y axis from the parent layout.
		/// If multiple elements inside a layout are set to expand, the extra space is distributed proportionally.
		/// </remarks>
		public LayoutOptions VerticalOptions
		{
			get { return (LayoutOptions)GetValue(VerticalOptionsProperty); }
			set { SetValue(VerticalOptionsProperty, value); }
		}

		/// <summary>
		/// Invoked whenever the binding context of the <see cref="View" /> changes. 
		/// </summary>
		/// <remarks>This method can be overridden to add class handling for this event. Overrides must call the base method.</remarks>
		protected override void OnBindingContextChanged()
		{
			this.PropagateBindingContext(GestureRecognizers);
			base.OnBindingContextChanged();
		}

		static void MarginPropertyChanged(BindableObject bindable, object oldValue, object newValue)
		{
			((View)bindable).InvalidateMeasureInternal(InvalidationTrigger.MarginChanged);
		}

		bool ValidateGesture(IGestureRecognizer gesture)
		{
			if (gesture == null)
				return false;
			if (gesture is PinchGestureRecognizer && _gestureRecognizers.GetGesturesFor<PinchGestureRecognizer>().Count() > 1)
				throw new InvalidOperationException($"Only one {nameof(PinchGestureRecognizer)} per view is allowed");
			return true;
		}

#nullable enable
		/// <inheritdoc/>
		Thickness IView.Margin => Margin;
		partial void HandlerChangedPartial();
		GestureManager _gestureManager;
		IWindow? IViewWithWindow.Window => Window;

		private protected override void OnHandlerChangedCore()
		{
			base.OnHandlerChangedCore();

			HandlerChangedPartial();
		}

		/// <summary>
		/// Represents the view's internal <see cref="PropertyMapper"/>.
		/// </summary>
		/// <remarks>Contains the unique property overrides that are used by <see cref="IPropertyMapperView.GetPropertyMapperOverrides"/>.</remarks>
		protected PropertyMapper propertyMapper;

		internal protected PropertyMapper<T> GetRendererOverrides<T>() where T : IView =>
			(PropertyMapper<T>)(propertyMapper as PropertyMapper<T> ?? (propertyMapper = new PropertyMapper<T>()));

		PropertyMapper IPropertyMapperView.GetPropertyMapperOverrides() => propertyMapper;

		/// <inheritdoc/>
		Primitives.LayoutAlignment IView.HorizontalLayoutAlignment => HorizontalOptions.ToCore();

		/// <inheritdoc/>
		Primitives.LayoutAlignment IView.VerticalLayoutAlignment => VerticalOptions.ToCore();

		#region HotReload

		IView IReplaceableView.ReplacedView =>
			MauiHotReloadHelper.GetReplacedView(this) ?? this;

		IReloadHandler IHotReloadableView.ReloadHandler { get; set; }

		void IHotReloadableView.TransferState(IView newView)
		{
			//TODO: LEt you hot reload the the ViewModel
			if (newView is View v)
				v.BindingContext = BindingContext;
		}

		void IHotReloadableView.Reload()
		{
			Dispatcher.Dispatch(() =>
			{
				this.CheckHandlers();
				//Handler = null;
				var reloadHandler = ((IHotReloadableView)this).ReloadHandler;
				reloadHandler?.Reload();
				//TODO: if reload handler is null, Do a manual reload?
			});
		}

		#endregion

#nullable disable
	}
}