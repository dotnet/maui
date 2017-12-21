using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms
{
	public class View : VisualElement, IViewController
	{
		public static readonly BindableProperty VerticalOptionsProperty =
			BindableProperty.Create(nameof(VerticalOptions), typeof(LayoutOptions), typeof(View), LayoutOptions.Fill,
									propertyChanged: (bindable, oldvalue, newvalue) =>
									((View)bindable).InvalidateMeasureInternal(InvalidationTrigger.VerticalOptionsChanged));

		public static readonly BindableProperty HorizontalOptionsProperty =
			BindableProperty.Create(nameof(HorizontalOptions), typeof(LayoutOptions), typeof(View), LayoutOptions.Fill,
									propertyChanged: (bindable, oldvalue, newvalue) =>
									((View)bindable).InvalidateMeasureInternal(InvalidationTrigger.HorizontalOptionsChanged));

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

		protected internal View()
		{
			_gestureRecognizers.CollectionChanged += (sender, args) =>
			{
				switch (args.Action)
				{
					case NotifyCollectionChangedAction.Add:
						foreach (IElement item in args.NewItems.OfType<IElement>())
						{
							ValidateGesture(item as IGestureRecognizer);
							item.Parent = this;
						}
						break;
					case NotifyCollectionChangedAction.Remove:
						foreach (IElement item in args.OldItems.OfType<IElement>())
							item.Parent = null;
						break;
					case NotifyCollectionChangedAction.Replace:
						foreach (IElement item in args.NewItems.OfType<IElement>())
						{
							ValidateGesture(item as IGestureRecognizer);
							item.Parent = this;
						}
						foreach (IElement item in args.OldItems.OfType<IElement>())
							item.Parent = null;
						break;
					case NotifyCollectionChangedAction.Reset:
						foreach (IElement item in _gestureRecognizers.OfType<IElement>())
							item.Parent = this;
						break;
				}
			};
		}

		public IList<IGestureRecognizer> GestureRecognizers
		{
			get { return _gestureRecognizers; }
		}

		public LayoutOptions HorizontalOptions
		{
			get { return (LayoutOptions)GetValue(HorizontalOptionsProperty); }
			set { SetValue(HorizontalOptionsProperty, value); }
		}

		public Thickness Margin
		{
			get { return (Thickness)GetValue(MarginProperty); }
			set { SetValue(MarginProperty, value); }
		}

		public LayoutOptions VerticalOptions
		{
			get { return (LayoutOptions)GetValue(VerticalOptionsProperty); }
			set { SetValue(VerticalOptionsProperty, value); }
		}

		protected override void OnBindingContextChanged()
		{
			var gotBindingContext = false;
			object bc = null;

			for (var i = 0; i < GestureRecognizers.Count; i++)
			{
				var bo = GestureRecognizers[i] as BindableObject;
				if (bo == null)
					continue;

				if (!gotBindingContext)
				{
					bc = BindingContext;
					gotBindingContext = true;
				}

				SetInheritedBindingContext(bo, bc);
			}

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