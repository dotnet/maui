using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Xaml.Diagnostics;

namespace Xamarin.Forms
{
	public abstract partial class Element : BindableObject, IElement, INameScope, IElementController
	{
		public static readonly BindableProperty MenuProperty = BindableProperty.CreateAttached(nameof(Menu), typeof(Menu), typeof(Element), null);

		public static Menu GetMenu(BindableObject bindable) => (Menu)bindable.GetValue(MenuProperty);
		public static void SetMenu(BindableObject bindable, Menu menu) => bindable.SetValue(MenuProperty, menu);

		internal static readonly ReadOnlyCollection<Element> EmptyChildren = new ReadOnlyCollection<Element>(new Element[0]);

		public static readonly BindableProperty AutomationIdProperty = BindableProperty.Create(nameof(AutomationId), typeof(string), typeof(Element), null);

		public static readonly BindableProperty ClassIdProperty = BindableProperty.Create(nameof(ClassId), typeof(string), typeof(Element), null);

		IList<BindableObject> _bindableResources;

		List<Action<object, ResourcesChangedEventArgs>> _changeHandlers;

		Dictionary<BindableProperty, string> _dynamicResources;

		IEffectControlProvider _effectControlProvider;

		TrackableCollection<Effect> _effects;

		Guid? _id;

		Element _parentOverride;

		string _styleId;


		public string AutomationId
		{
			get { return (string)GetValue(AutomationIdProperty); }
			set
			{
				if (AutomationId != null)
					throw new InvalidOperationException($"{nameof(AutomationId)} may only be set one time.");

				SetValue(AutomationIdProperty, value);
			}
		}

		public string ClassId
		{
			get => (string)GetValue(ClassIdProperty);
			set => SetValue(ClassIdProperty, value);
		}

		public IList<Effect> Effects
		{
			get
			{
				if (_effects == null)
				{
					_effects = new TrackableCollection<Effect>();
					_effects.CollectionChanged += EffectsOnCollectionChanged;
					_effects.Clearing += EffectsOnClearing;
				}
				return _effects;
			}
		}

		public Guid Id
		{
			get
			{
				if (!_id.HasValue)
					_id = Guid.NewGuid();
				return _id.Value;
			}
		}

		[Obsolete("ParentView is obsolete as of version 2.1.0. Please use Parent instead.")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public VisualElement ParentView
		{
			get
			{
				Element parent = Parent;
				while (parent != null)
				{
					var parentView = parent as VisualElement;
					if (parentView != null)
						return parentView;
					parent = parent.RealParent;
				}
				return null;
			}
		}

		public string StyleId
		{
			get { return _styleId; }
			set
			{
				if (_styleId == value)
					return;

				OnPropertyChanging();
				_styleId = value;
				OnPropertyChanged();
			}
		}

		internal virtual ReadOnlyCollection<Element> LogicalChildrenInternal => EmptyChildren;
		internal IEnumerable<Element> AllChildren
		{
			get
			{
				foreach (var child in LogicalChildrenInternal)
					yield return child;

				foreach (var child in ChildrenNotDrawnByThisElement)
					yield return child;
			}
		}

		internal virtual IEnumerable<Element> ChildrenNotDrawnByThisElement => EmptyChildren;


		[EditorBrowsable(EditorBrowsableState.Never)]
		public ReadOnlyCollection<Element> LogicalChildren => LogicalChildrenInternal;

		internal bool Owned { get; set; }

		internal Element ParentOverride
		{
			get { return _parentOverride; }
			set
			{
				if (_parentOverride == value)
					return;

				bool emitChange = Parent != value;

				if (emitChange)
					OnPropertyChanging(nameof(Parent));

				_parentOverride = value;

				if (emitChange)
					OnPropertyChanged(nameof(Parent));
			}
		}

		// you're not my real dad
		[EditorBrowsable(EditorBrowsableState.Never)]
		public Element RealParent { get; private set; }

		Dictionary<BindableProperty, string> DynamicResources => _dynamicResources ?? (_dynamicResources = new Dictionary<BindableProperty, string>());

		void IElement.AddResourcesChangedListener(Action<object, ResourcesChangedEventArgs> onchanged)
		{
			_changeHandlers = _changeHandlers ?? new List<Action<object, ResourcesChangedEventArgs>>(2);
			_changeHandlers.Add(onchanged);
		}

		public Element Parent
		{
			get { return _parentOverride ?? RealParent; }
			set
			{
				if (RealParent == value)
					return;

				OnPropertyChanging();

				if (RealParent != null)
				{
					((IElement)RealParent).RemoveResourcesChangedListener(OnParentResourcesChanged);

					if (value != null && (RealParent is Layout || RealParent is IControlTemplated))
						Log.Warning("Element", $"{this} is already a child of {RealParent}. Remove {this} from {RealParent} before adding to {value}.");
				}

				RealParent = value;
				if (RealParent != null)
				{
					OnParentResourcesChanged(RealParent.GetMergedResources());
					((IElement)RealParent).AddResourcesChangedListener(OnParentResourcesChanged);
				}

				object context = value != null ? value.BindingContext : null;
				if (value != null)
				{
					value.SetChildInheritedBindingContext(this, context);
				}
				else
				{
					SetInheritedBindingContext(this, null);
				}

				OnParentSet();

				OnPropertyChanged();
			}
		}

		internal bool IsTemplateRoot { get; set; }

		void IElement.RemoveResourcesChangedListener(Action<object, ResourcesChangedEventArgs> onchanged)
		{
			if (_changeHandlers == null)
				return;
			_changeHandlers.Remove(onchanged);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public IEffectControlProvider EffectControlProvider
		{
			get { return _effectControlProvider; }
			set
			{
				if (_effectControlProvider == value)
					return;
				if (_effectControlProvider != null && _effects != null)
				{
					foreach (Effect effect in _effects)
						effect?.SendDetached();
				}
				_effectControlProvider = value;
				if (_effectControlProvider != null && _effects != null)
				{
					foreach (Effect effect in _effects)
					{
						if (effect != null)
							AttachEffect(effect);
					}
				}
			}
		}

		void IElementController.SetValueFromRenderer(BindableProperty property, object value) => SetValueFromRenderer(property, value);
		[EditorBrowsable(EditorBrowsableState.Never)]
		public void SetValueFromRenderer(BindableProperty property, object value)
		{
			SetValueCore(property, value);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void SetValueFromRenderer(BindablePropertyKey property, object value)
		{
			SetValueCore(property, value);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool EffectIsAttached(string name)
		{
			foreach (var effect in Effects)
			{
				if (effect.ResolveId == name)
					return true;
			}
			return false;
		}

		public object FindByName(string name)
		{
			var namescope = GetNameScope();
			if (namescope == null)
				throw new InvalidOperationException("this element is not in a namescope");
			return namescope.FindByName(name);
		}

		void INameScope.RegisterName(string name, object scopedElement)
		{
			var namescope = GetNameScope();
			if (namescope == null)
				throw new InvalidOperationException("this element is not in a namescope");
			namescope.RegisterName(name, scopedElement);
		}

		public event EventHandler<ElementEventArgs> ChildAdded;

		public event EventHandler<ElementEventArgs> ChildRemoved;

		public event EventHandler<ElementEventArgs> DescendantAdded;

		public event EventHandler<ElementEventArgs> DescendantRemoved;

		public new void RemoveDynamicResource(BindableProperty property)
		{
			base.RemoveDynamicResource(property);
		}

		public new void SetDynamicResource(BindableProperty property, string key)
		{
			base.SetDynamicResource(property, key);
		}

		protected override void OnBindingContextChanged()
		{
			this.PropagateBindingContext(LogicalChildrenInternal, (child, bc) =>
			{
				SetChildInheritedBindingContext((Element)child, bc);
			});

			if (_bindableResources != null)
				foreach (BindableObject item in _bindableResources)
				{
					SetInheritedBindingContext(item, BindingContext);
				}

			base.OnBindingContextChanged();
		}

		protected virtual void OnChildAdded(Element child)
		{
			child.Parent = this;

			child.ApplyBindings(skipBindingContext: false, fromBindingContextChanged: true);

			ChildAdded?.Invoke(this, new ElementEventArgs(child));

			VisualDiagnostics.OnChildAdded(this, child);

			OnDescendantAdded(child);
			foreach (Element element in child.Descendants())
				OnDescendantAdded(element);
		}

		[Obsolete("OnChildRemoved(Element) is obsolete as of version 4.8.0. Please use OnChildRemoved(Element, int) instead.")]
		protected virtual void OnChildRemoved(Element child) => OnChildRemoved(child, -1);

		protected virtual void OnChildRemoved(Element child, int oldLogicalIndex)
		{
			child.Parent = null;

			ChildRemoved?.Invoke(child, new ElementEventArgs(child));

			VisualDiagnostics.OnChildRemoved(this, child, oldLogicalIndex);

			OnDescendantRemoved(child);
			foreach (Element element in child.Descendants())
				OnDescendantRemoved(element);
		}

		protected virtual void OnParentSet()
		{
			ParentSet?.Invoke(this, EventArgs.Empty);
			ApplyStyleSheets();
			(this as IPropertyPropagationController)?.PropagatePropertyChanged(null);
		}

		protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			base.OnPropertyChanged(propertyName);
			foreach (var logicalChildren in ChildrenNotDrawnByThisElement)
			{
				if (logicalChildren is IPropertyPropagationController controller)
					PropertyPropagationExtensions.PropagatePropertyChanged(propertyName, this, new[] { logicalChildren });
			}

			if (_effects == null || _effects.Count == 0)
				return;

			var args = new PropertyChangedEventArgs(propertyName);
			foreach (Effect effect in _effects)
			{
				effect?.SendOnElementPropertyChanged(args);
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public IEnumerable<Element> Descendants()
		{
			var queue = new Queue<Element>(16);
			queue.Enqueue(this);

			while (queue.Count > 0)
			{
				ReadOnlyCollection<Element> children = queue.Dequeue().LogicalChildrenInternal;
				for (var i = 0; i < children.Count; i++)
				{
					Element child = children[i];
					yield return child;
					queue.Enqueue(child);
				}
			}
		}

		internal virtual void OnParentResourcesChanged(object sender, ResourcesChangedEventArgs e)
		{
			if (e == ResourcesChangedEventArgs.StyleSheets)
				ApplyStyleSheets();
			else
				OnParentResourcesChanged(e.Values);
		}

		internal virtual void OnParentResourcesChanged(IEnumerable<KeyValuePair<string, object>> values)
		{
			OnResourcesChanged(values);
		}

		internal override void OnRemoveDynamicResource(BindableProperty property)
		{
			DynamicResources.Remove(property);

			if (DynamicResources.Count == 0)
				_dynamicResources = null;
			base.OnRemoveDynamicResource(property);
		}

		internal virtual void OnResourcesChanged(object sender, ResourcesChangedEventArgs e)
		{
			if (e == ResourcesChangedEventArgs.StyleSheets)
				ApplyStyleSheets();
			else
				OnResourcesChanged(e.Values);
		}

		internal void OnResourcesChanged(IEnumerable<KeyValuePair<string, object>> values)
		{
			if (values == null)
				return;
			if (_changeHandlers != null)
				foreach (Action<object, ResourcesChangedEventArgs> handler in _changeHandlers)
					handler(this, new ResourcesChangedEventArgs(values));
			if (_dynamicResources == null)
				return;
			if (_bindableResources == null)
				_bindableResources = new List<BindableObject>();
			foreach (KeyValuePair<string, object> value in values)
			{
				List<BindableProperty> changedResources = null;
				foreach (KeyValuePair<BindableProperty, string> dynR in DynamicResources)
				{
					// when the DynamicResource bound to a BindableProperty is
					// changing then the BindableProperty needs to be refreshed;
					// The .Value is the name of DynamicResouce to which the BindableProperty is bound.
					// The .Key is the name of the DynamicResource whose value is changing.
					if (dynR.Value != value.Key)
						continue;
					changedResources = changedResources ?? new List<BindableProperty>();
					changedResources.Add(dynR.Key);
				}
				if (changedResources == null)
					continue;
				foreach (BindableProperty changedResource in changedResources)
					OnResourceChanged(changedResource, value.Value);

				var bindableObject = value.Value as BindableObject;
				if (bindableObject != null && (bindableObject as Element)?.Parent == null)
				{
					if (!_bindableResources.Contains(bindableObject))
						_bindableResources.Add(bindableObject);
					SetInheritedBindingContext(bindableObject, BindingContext);
				}
			}
		}

		internal override void OnSetDynamicResource(BindableProperty property, string key)
		{
			base.OnSetDynamicResource(property, key);
			DynamicResources[property] = key;
			if (this.TryGetResource(key, out var value))
				OnResourceChanged(property, value);
		}

		internal event EventHandler ParentSet;

		internal virtual void SetChildInheritedBindingContext(Element child, object context)
		{
			SetInheritedBindingContext(child, context);
		}

		internal IEnumerable<Element> VisibleDescendants()
		{
			var queue = new Queue<Element>(16);
			queue.Enqueue(this);

			while (queue.Count > 0)
			{
				ReadOnlyCollection<Element> children = queue.Dequeue().LogicalChildrenInternal;
				for (var i = 0; i < children.Count; i++)
				{
					var child = children[i] as VisualElement;
					if (child == null || !child.IsVisible)
						continue;
					yield return child;
					queue.Enqueue(child);
				}
			}
		}

		void AttachEffect(Effect effect)
		{
			if (_effectControlProvider == null)
				return;
			if (effect.IsAttached)
				throw new InvalidOperationException("Cannot attach Effect to multiple sources");

			Effect effectToRegister = effect;
			if (effect is RoutingEffect)
				effectToRegister = ((RoutingEffect)effect).Inner;
			_effectControlProvider.RegisterEffect(effectToRegister);
			effectToRegister.Element = this;
			effect.SendAttached();
		}

		void EffectsOnClearing(object sender, EventArgs eventArgs)
		{
			foreach (Effect effect in _effects)
			{
				effect?.ClearEffect();
			}
		}

		void EffectsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			switch (e.Action)
			{
				case NotifyCollectionChangedAction.Add:
					foreach (Effect effect in e.NewItems)
					{
						AttachEffect(effect);
					}
					break;
				case NotifyCollectionChangedAction.Move:
					break;
				case NotifyCollectionChangedAction.Remove:
					foreach (Effect effect in e.OldItems)
					{
						effect.ClearEffect();
					}
					break;
				case NotifyCollectionChangedAction.Replace:
					foreach (Effect effect in e.NewItems)
					{
						AttachEffect(effect);
					}
					foreach (Effect effect in e.OldItems)
					{
						effect.ClearEffect();
					}
					break;
				case NotifyCollectionChangedAction.Reset:
					if (e.NewItems != null)
					{
						foreach (Effect effect in e.NewItems)
						{
							AttachEffect(effect);
						}
					}
					if (e.OldItems != null)
					{
						foreach (Effect effect in e.OldItems)
						{
							effect.ClearEffect();
						}
					}
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		internal INameScope GetNameScope()
		{
			var element = this;
			do
			{
				var ns = NameScope.GetNameScope(element);
				if (ns != null)
					return ns;
			} while ((element = element.RealParent) != null);
			return null;
		}

		void OnDescendantAdded(Element child)
		{
			DescendantAdded?.Invoke(this, new ElementEventArgs(child));
			RealParent?.OnDescendantAdded(child);
		}

		void OnDescendantRemoved(Element child)
		{
			DescendantRemoved?.Invoke(this, new ElementEventArgs(child));
			RealParent?.OnDescendantRemoved(child);
		}

		void OnResourceChanged(BindableProperty property, object value)
		{
			SetValueCore(property, value, SetValueFlags.ClearOneWayBindings | SetValueFlags.ClearTwoWayBindings);
		}

		#region Obsolete IPlatform Stuff

#pragma warning disable CS0618 // Type or member is obsolete
		private IPlatform _platform;
#pragma warning restore CS0618 // Type or member is obsolete

		// Platform isn't needed anymore, but the Previewer will still try to set it via reflection
		// and throw an NRE if it's not available
		// So even if this property eventually gets removed, we still need to keep something settable on
		// Page called Platform

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("IPlatform is obsolete as of 3.5.0. Do not use this property.")]
		public IPlatform Platform
		{
			get => _platform;
			set
			{
				if (_platform == value)
					return;
				_platform = value;
				PlatformSet?.Invoke(this, EventArgs.Empty);
				foreach (Element descendant in Descendants())
				{
					descendant._platform = _platform;
					descendant.PlatformSet?.Invoke(this, EventArgs.Empty);
				}
			}
		}

		[Obsolete("PlatformSet is obsolete as of 3.5.0. Do not use this event.")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public event EventHandler PlatformSet;

		#endregion
	}
}