using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Xaml.Diagnostics;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/Element.xml" path="Type[@FullName='Microsoft.Maui.Controls.Element']/Docs" />
	public abstract partial class Element : BindableObject, IElement, INameScope, IElementController, IVisualTreeElement
	{
		internal static readonly ReadOnlyCollection<Element> EmptyChildren = new ReadOnlyCollection<Element>(new Element[0]);

		/// <include file="../../docs/Microsoft.Maui.Controls/Element.xml" path="//Member[@MemberName='AutomationIdProperty']/Docs" />
		public static readonly BindableProperty AutomationIdProperty = BindableProperty.Create(nameof(AutomationId), typeof(string), typeof(Element), null);

		/// <include file="../../docs/Microsoft.Maui.Controls/Element.xml" path="//Member[@MemberName='ClassIdProperty']/Docs" />
		public static readonly BindableProperty ClassIdProperty = BindableProperty.Create(nameof(ClassId), typeof(string), typeof(Element), null);

		internal static readonly BindablePropertyKey WindowPropertyKey = BindableProperty.CreateReadOnly(nameof(Window), typeof(IWindow), typeof(Element), null, propertyChanged: OnWindowChanged);


		internal static readonly BindableProperty WindowProperty = WindowPropertyKey.BindableProperty;

		IList<BindableObject> _bindableResources;

		List<Action<object, ResourcesChangedEventArgs>> _changeHandlers;

		Dictionary<BindableProperty, string> _dynamicResources;

		IEffectControlProvider _effectControlProvider;

		TrackableCollection<Effect> _effects;

		Guid? _id;

		Element _parentOverride;

		string _styleId;


		/// <include file="../../docs/Microsoft.Maui.Controls/Element.xml" path="//Member[@MemberName='AutomationId']/Docs" />
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

		/// <include file="../../docs/Microsoft.Maui.Controls/Element.xml" path="//Member[@MemberName='ClassId']/Docs" />
		public string ClassId
		{
			get => (string)GetValue(ClassIdProperty);
			set => SetValue(ClassIdProperty, value);
		}
		/// <include file="../../docs/Microsoft.Maui.Controls/Element.xml" path="//Member[@MemberName='Effects']/Docs" />
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

		/// <include file="../../docs/Microsoft.Maui.Controls/Element.xml" path="//Member[@MemberName='Id']/Docs" />
		public Guid Id
		{
			get
			{
				if (!_id.HasValue)
					_id = Guid.NewGuid();
				return _id.Value;
			}
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/Element.xml" path="//Member[@MemberName='StyleId']/Docs" />
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

		internal IWindow Window => (IWindow)GetValue(WindowProperty);
		internal void SetWindow(IWindow window) => SetValue(WindowPropertyKey, window);

		protected private virtual void OnWindowChanged(IWindow oldValue, IWindow newValue) { }
		static void OnWindowChanged(BindableObject bindable, object oldValue, object newValue)
		{
			(bindable as Element)?.OnWindowChanged((IWindow)oldValue, (IWindow)newValue);
		}

		internal virtual IReadOnlyList<Element> LogicalChildrenInternal => EmptyChildren;

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

		IReadOnlyList<Element> IElementController.LogicalChildren => LogicalChildrenInternal;

		/// <include file="../../docs/Microsoft.Maui.Controls/Element.xml" path="//Member[@MemberName='LogicalChildren']/Docs" />
		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("Do not use! This is to be removed! Just used by Hot Reload! To be replaced with IVisualTreeElement!")]
		public ReadOnlyCollection<Element> LogicalChildren =>
			new ReadOnlyCollection<Element>(new TemporaryWrapper(LogicalChildrenInternal));

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
				{
					OnPropertyChanging(nameof(Parent));

					if (value != null)
						OnParentChangingCore(Parent, value);
					else
						OnParentChangingCore(Parent, RealParent);
				}

				_parentOverride = value;

				if (emitChange)
				{
					OnPropertyChanged(nameof(Parent));
					OnParentChangedCore();
				}
			}
		}

		// you're not my real dad
		/// <include file="../../docs/Microsoft.Maui.Controls/Element.xml" path="//Member[@MemberName='RealParent']/Docs" />
		[EditorBrowsable(EditorBrowsableState.Never)]
		public Element RealParent { get; private set; }

		Dictionary<BindableProperty, string> DynamicResources => _dynamicResources ?? (_dynamicResources = new Dictionary<BindableProperty, string>());

		void IElement.AddResourcesChangedListener(Action<object, ResourcesChangedEventArgs> onchanged)
		{
			_changeHandlers = _changeHandlers ?? new List<Action<object, ResourcesChangedEventArgs>>(2);
			_changeHandlers.Add(onchanged);
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/Element.xml" path="//Member[@MemberName='Parent']/Docs" />
		public Element Parent
		{
			get { return _parentOverride ?? RealParent; }
			set
			{
				if (RealParent == value)
					return;

				OnPropertyChanging();

				if (_parentOverride == null)
					OnParentChangingCore(Parent, value);

				if (RealParent != null)
				{
					((IElement)RealParent).RemoveResourcesChangedListener(OnParentResourcesChanged);

					if (value != null && (RealParent is Layout || RealParent is IControlTemplated))
						Application.Current?.FindMauiContext()?.CreateLogger<Element>()?.LogWarning($"{this} is already a child of {RealParent}. Remove {this} from {RealParent} before adding to {value}.");
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

				if (_parentOverride == null)
					OnParentChangedCore();

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

		/// <include file="../../docs/Microsoft.Maui.Controls/Element.xml" path="//Member[@MemberName='EffectControlProvider']/Docs" />
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
		/// <include file="../../docs/Microsoft.Maui.Controls/Element.xml" path="//Member[@MemberName='SetValueFromRenderer'][0]/Docs" />
		[EditorBrowsable(EditorBrowsableState.Never)]
		public void SetValueFromRenderer(BindableProperty property, object value)
		{
			SetValueCore(property, value);
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/Element.xml" path="//Member[@MemberName='SetValueFromRenderer'][1]/Docs" />
		[EditorBrowsable(EditorBrowsableState.Never)]
		public void SetValueFromRenderer(BindablePropertyKey property, object value)
		{
			SetValueCore(property, value);
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/Element.xml" path="//Member[@MemberName='EffectIsAttached']/Docs" />
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

		/// <include file="../../docs/Microsoft.Maui.Controls/Element.xml" path="//Member[@MemberName='FindByName']/Docs" />
		public object FindByName(string name)
		{
			var namescope = GetNameScope();
			if (namescope == null)
				throw new InvalidOperationException("this element is not in a namescope");
			return namescope.FindByName(name);
		}

		void INameScope.RegisterName(string name, object scopedElement)
		{
			var namescope = GetNameScope() ?? throw new InvalidOperationException("this element is not in a namescope");
			namescope.RegisterName(name, scopedElement);
		}

		void INameScope.UnregisterName(string name)
		{
			var namescope = GetNameScope() ?? throw new InvalidOperationException("this element is not in a namescope");
			namescope.UnregisterName(name);
		}

		public event EventHandler<ElementEventArgs> ChildAdded;

		public event EventHandler<ElementEventArgs> ChildRemoved;

		public event EventHandler<ElementEventArgs> DescendantAdded;

		public event EventHandler<ElementEventArgs> DescendantRemoved;

		/// <include file="../../docs/Microsoft.Maui.Controls/Element.xml" path="//Member[@MemberName='RemoveDynamicResource']/Docs" />
		public new void RemoveDynamicResource(BindableProperty property)
		{
			base.RemoveDynamicResource(property);
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/Element.xml" path="//Member[@MemberName='SetDynamicResource']/Docs" />
		public new void SetDynamicResource(BindableProperty property, string key)
		{
			base.SetDynamicResource(property, key);
		}

		IReadOnlyList<Maui.IVisualTreeElement> IVisualTreeElement.GetVisualChildren() => LogicalChildrenInternal;

		IVisualTreeElement IVisualTreeElement.GetVisualParent() => this.Parent;

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

			Handler?.UpdateValue(propertyName);

			foreach (var logicalChildren in ChildrenNotDrawnByThisElement)
			{
				if (logicalChildren is IPropertyPropagationController controller)
					PropertyPropagationExtensions.PropagatePropertyChanged(propertyName, this, new[] { logicalChildren });
			}

			if (_effects?.Count > 0)
			{
				var args = new PropertyChangedEventArgs(propertyName);
				foreach (Effect effect in _effects)
				{
					effect?.SendOnElementPropertyChanged(args);
				}
			}
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/Element.xml" path="//Member[@MemberName='Descendants']/Docs" />
		[EditorBrowsable(EditorBrowsableState.Never)]
		public IEnumerable<Element> Descendants()
		{
			var queue = new Queue<Element>(16);
			queue.Enqueue(this);

			while (queue.Count > 0)
			{
				IReadOnlyList<Element> children = queue.Dequeue().LogicalChildrenInternal;
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
				IReadOnlyList<Element> children = queue.Dequeue().LogicalChildrenInternal;
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
			if (effect is RoutingEffect re && re.Inner != null)
				effectToRegister = re.Inner;

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

		public event EventHandler<ParentChangingEventArgs> ParentChanging;
		public event EventHandler ParentChanged;

		protected virtual void OnParentChanging(ParentChangingEventArgs args) { }

		protected virtual void OnParentChanged() { }

		private protected virtual void OnParentChangedCore()
		{
			ParentChanged?.Invoke(this, EventArgs.Empty);
			OnParentChanged();
		}

		private protected virtual void OnParentChangingCore(Element oldParent, Element newParent)
		{
			if (oldParent == newParent)
				return;

			var args = new ParentChangingEventArgs(oldParent, newParent);
			ParentChanging?.Invoke(this, args);
			OnParentChanging(args);
		}

		class TemporaryWrapper : IList<Element>
		{
			IReadOnlyList<Element> _inner;

			public TemporaryWrapper(IReadOnlyList<Element> inner)
			{
				_inner = inner;
			}

			Element IList<Element>.this[int index] { get => _inner[index]; set => throw new NotSupportedException(); }

			int ICollection<Element>.Count => _inner.Count;

			bool ICollection<Element>.IsReadOnly => true;

			void ICollection<Element>.Add(Element item) => throw new NotSupportedException();

			void ICollection<Element>.Clear() => throw new NotSupportedException();

			bool ICollection<Element>.Contains(Element item) => _inner.IndexOf(item) != -1;

			void ICollection<Element>.CopyTo(Element[] array, int arrayIndex) => throw new NotSupportedException();

			IEnumerator<Element> IEnumerable<Element>.GetEnumerator() => _inner.GetEnumerator();

			System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => _inner.GetEnumerator();

			int IList<Element>.IndexOf(Element item) => _inner.IndexOf(item);

			void IList<Element>.Insert(int index, Element item) => throw new NotSupportedException();

			bool ICollection<Element>.Remove(Element item) => throw new NotSupportedException();

			void IList<Element>.RemoveAt(int index) => throw new NotSupportedException();
		}
	}
}
