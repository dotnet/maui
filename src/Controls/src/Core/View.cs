using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using Microsoft.Maui;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/View.xml" path="Type[@FullName='Microsoft.Maui.Controls.View']/Docs/*" />
	public partial class View : VisualElement, IViewController, IGestureController, IGestureRecognizers
	{
		protected internal IGestureController GestureController => this;

		/// <include file="../../docs/Microsoft.Maui.Controls/View.xml" path="//Member[@MemberName='VerticalOptionsProperty']/Docs/*" />
		public static readonly BindableProperty VerticalOptionsProperty =
			BindableProperty.Create(nameof(VerticalOptions), typeof(LayoutOptions), typeof(View), LayoutOptions.Fill,
									propertyChanged: (bindable, oldvalue, newvalue) =>
									((View)bindable).InvalidateMeasureInternal(InvalidationTrigger.VerticalOptionsChanged));

		/// <include file="../../docs/Microsoft.Maui.Controls/View.xml" path="//Member[@MemberName='HorizontalOptionsProperty']/Docs/*" />
		public static readonly BindableProperty HorizontalOptionsProperty =
			BindableProperty.Create(nameof(HorizontalOptions), typeof(LayoutOptions), typeof(View), LayoutOptions.Fill,
									propertyChanged: (bindable, oldvalue, newvalue) =>
									((View)bindable).InvalidateMeasureInternal(InvalidationTrigger.HorizontalOptionsChanged));

		/// <include file="../../docs/Microsoft.Maui.Controls/View.xml" path="//Member[@MemberName='MarginProperty']/Docs/*" />
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

		protected internal View()
		{
			_gestureRecognizers.CollectionChanged += (sender, args) =>
			{
				void AddItems(IEnumerable<IElementDefinition> elements)
				{
					foreach (IElementDefinition item in elements)
					{
						ValidateGesture(item as IGestureRecognizer);
						item.Parent = this;
						GestureController.CompositeGestureRecognizers.Add(item as IGestureRecognizer);
					}
				}

				void RemoveItems(IEnumerable<IElementDefinition> elements)
				{
					foreach (IElementDefinition item in elements)
					{
						item.Parent = null;
						GestureController.CompositeGestureRecognizers.Remove(item as IGestureRecognizer);
					}
				}

				switch (args.Action)
				{
					case NotifyCollectionChangedAction.Add:
						AddItems(args.NewItems.OfType<IElementDefinition>());
						break;
					case NotifyCollectionChangedAction.Remove:
						RemoveItems(args.OldItems.OfType<IElementDefinition>());
						break;
					case NotifyCollectionChangedAction.Replace:
						AddItems(args.NewItems.OfType<IElementDefinition>());
						RemoveItems(args.OldItems.OfType<IElementDefinition>());
						break;
					case NotifyCollectionChangedAction.Reset:

						List<IElementDefinition> remove = new List<IElementDefinition>();
						List<IElementDefinition> add = new List<IElementDefinition>();

						foreach (IElementDefinition item in _gestureRecognizers.OfType<IElementDefinition>())
						{
							if (!_gestureRecognizers.Contains((IGestureRecognizer)item))
								add.Add(item);
							item.Parent = this;
						}

						foreach (IElementDefinition item in GestureController.CompositeGestureRecognizers.OfType<IElementDefinition>())
						{
							if (item == _recognizerForPointerOverState)
								continue;

							if (_gestureRecognizers.Contains((IGestureRecognizer)item))
								item.Parent = this;
							else
								remove.Add(item);
						}

						AddItems(add);
						RemoveItems(remove);

						break;
				}
			};
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/View.xml" path="//Member[@MemberName='GestureRecognizers']/Docs/*" />
		public IList<IGestureRecognizer> GestureRecognizers
		{
			get { return _gestureRecognizers; }
		}

		ObservableCollection<IGestureRecognizer> _compositeGestureRecognizers;

		IList<IGestureRecognizer> IGestureController.CompositeGestureRecognizers
		{
			get
			{
				if (_compositeGestureRecognizers is not null)
					return _compositeGestureRecognizers;

				_recognizerForPointerOverState = new PointerGestureRecognizer();

				_recognizerForPointerOverState.PointerEntered += (s, e) =>
				{
					IsPointerOver = true;
				};

				_recognizerForPointerOverState.PointerExited += (s, e) =>
				{
					IsPointerOver = false;
				};

				_compositeGestureRecognizers = new ObservableCollection<IGestureRecognizer>() { _recognizerForPointerOverState };
				return _compositeGestureRecognizers;
			}
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/View.xml" path="//Member[@MemberName='GetChildElements']/Docs/*" />
		public virtual IList<GestureElement> GetChildElements(Point point)
		{
			return null;
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/View.xml" path="//Member[@MemberName='HorizontalOptions']/Docs/*" />
		public LayoutOptions HorizontalOptions
		{
			get { return (LayoutOptions)GetValue(HorizontalOptionsProperty); }
			set { SetValue(HorizontalOptionsProperty, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/View.xml" path="//Member[@MemberName='Margin']/Docs/*" />
		public Thickness Margin
		{
			get { return (Thickness)GetValue(MarginProperty); }
			set { SetValue(MarginProperty, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/View.xml" path="//Member[@MemberName='VerticalOptions']/Docs/*" />
		public LayoutOptions VerticalOptions
		{
			get { return (LayoutOptions)GetValue(VerticalOptionsProperty); }
			set { SetValue(VerticalOptionsProperty, value); }
		}

		protected override void OnBindingContextChanged()
		{
			this.PropagateBindingContext(GestureRecognizers);
			base.OnBindingContextChanged();
		}

		static void MarginPropertyChanged(BindableObject bindable, object oldValue, object newValue)
		{
			((View)bindable).InvalidateMeasureInternal(InvalidationTrigger.MarginChanged);
		}

		void ValidateGesture(IGestureRecognizer gesture)
		{
			if (gesture == null)
				return;
			if (gesture is PinchGestureRecognizer && _gestureRecognizers.GetGesturesFor<PinchGestureRecognizer>().Count() > 1)
				throw new InvalidOperationException($"Only one {nameof(PinchGestureRecognizer)} per view is allowed");
		}
	}
}