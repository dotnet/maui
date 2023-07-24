#nullable disable
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls
{
	/// <summary>Provides the base class for all <see cref="Controls"/> hierarchal elements.
	/// This class contains all the methods and properties required to represent an element in the <see cref="Controls"/>  hierarchy.
	/// </summary>
	/// <remarks>
	///		<para>Important categories of visual elements are noted in the following table:</para>
	///		<list type = "table" >
	///			<listheader>
	///				<term > Class </term >
	///				<description > Description </description >
	///			</listheader >
	///			<item>
	///				<term><see cref="VisualElement" /></term>
	///				<description>An <see cref="Element" /> that occupies an area on the screen, has a visual appearance, and can obtain touch input.</description>
	///			</item>
	///			<item>
	///				<term><see cref = "Cell"/></term>
	///				<description> Cells are elements meant to be added to <see cref="ListView"/> or <see cref="TableView"/>.</description>
	///			</item>
	///			<item>
	///				<term><see cref = "Page"/>
	///				</term>
	///				<description> A <see cref= "VisualElement"/> that occupies most or all of the screen and contains a single child.</description>
	///			</item>
	///			<item>
	///			<term><see cref = "Layout"/></term>
	///			<description><see cref= "Layout"/> have a single child of type <see cref="View" />, while subclasses of <see cref = "Layout"/> have a collection of multiple children views, including other layouts.</description>
	///			</item>
	///			<item>
	///				<term> Controls and specialized <see cref="View" /></term>
	///				<description>The lower part of the diagram shows the <see cref="Controls"/> classes for universally-available controls, such as <see cref = "Button"/> and <see cref="TableView"/>.</description>
	///			</item>
	///		</list>
	///</remarks>
	public abstract partial class Element : BindableObject, IElementDefinition, INameScope, IElementController, IVisualTreeElement, Maui.IElement, IEffectControlProvider, IToolTipElement, IContextFlyoutElement, IControlsElement
	{
		internal static readonly ReadOnlyCollection<Element> EmptyChildren = new ReadOnlyCollection<Element>(Array.Empty<Element>());

		/// <summary>Bindable property for <see cref="AutomationId"/>.</summary>
		public static readonly BindableProperty AutomationIdProperty = BindableProperty.Create(nameof(AutomationId), typeof(string), typeof(Element), null);

		/// <summary>Bindable property for <see cref="ClassId"/>.</summary>
		public static readonly BindableProperty ClassIdProperty = BindableProperty.Create(nameof(ClassId), typeof(string), typeof(Element), null);

		IList<BindableObject> _bindableResources;

		List<Action<object, ResourcesChangedEventArgs>> _changeHandlers;

		Dictionary<BindableProperty, (string, SetterSpecificity)> _dynamicResources;

		IEffectControlProvider _effectControlProvider;

		TrackableCollection<Effect> _effects;

		Guid? _id;

		Element _parentOverride;

		string _styleId;

		IReadOnlyList<Element> _logicalChildrenReadonly;

		IList<Element> _internalChildren;

		/// <summary>Gets or sets a value that allows the automation framework to find and interact with this element.</summary>
		/// <value>A value that the automation framework can use to find and interact with this element.</value>
		/// <remarks>This value may only be set once on an element.</remarks>
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

		/// <summary>Gets or sets a value used to identify a collection of semantically similar elements.</summary>
		/// <value>A string that represents the collection the element belongs to.</value>
		/// <remarks>Use the class id property to collect together elements into semantically similar groups for identification in ui testing and in theme engines.</remarks>
		public string ClassId
		{
			get => (string)GetValue(ClassIdProperty);
			set => SetValue(ClassIdProperty, value);
		}

		/// <summary>Gets or sets the styles and properties that will be applied to the element during runtime.</summary>
		/// <value>A collection containing the different <see cref="Effect"/> to be applied to the element.</value>
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

		/// <summary>Gets a value that can be used to uniquely identify an element throughout the run of your application.</summary>
		/// <value>A <see cref="Guid"/> uniquely identifying the element.</value>
		/// <remarks>This value is generated at runtime and is not stable across different runs.</remarks>
		/// <seealso cref="StyleId"/>
		public Guid Id
		{
			get
			{
				if (!_id.HasValue)
					_id = Guid.NewGuid();
				return _id.Value;
			}
		}

		/// <summary>Gets or sets a user defined value to uniquely identify the element.</summary>
		/// <value>A string uniquely identifying the element.</value>
		/// <remarks>Use the <see cref="StyleId"/> property to identify individual elements in your application for identification in UI testing and in theme engines.</remarks>
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

		// Leaving this internal for now.
		// If users want to add/remove from this they can use
		// AddLogicalChildren and RemoveLogicalChildren on the respective control
		// if available.
		//
		// Ultimately I don't think we'll need these to be virtual but some controls (layout)
		// are going to take a more focused effort so I'd rather just do that in a 
		// separate PR. I don't think there's ever a scenario where a subclass needs
		// to replace the backing store.
		// If everyone just uses AddLogicalChildren and RemoveLogicalChildren
		// and then overrides OnChildAdded/OnChildRemoved
		// that should be sufficient
		internal IReadOnlyList<Element> LogicalChildrenInternal
		{
			get
			{
				SetupChildren();
				return _logicalChildrenReadonly;
			}
		}

		private protected virtual IList<Element> LogicalChildrenInternalBackingStore
		{
			get
			{
				_internalChildren ??= new List<Element>();
				return _internalChildren;
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("Do not use! This is to be removed! Just used by Hot Reload! To be replaced with IVisualTreeElement!")]
		public ReadOnlyCollection<Element> LogicalChildren =>
			new ReadOnlyCollection<Element>(new TemporaryWrapper(LogicalChildrenInternal));

		/// <inheritdoc/>
		IReadOnlyList<Element> IElementController.LogicalChildren => LogicalChildrenInternal;

		void SetupChildren()
		{
			_logicalChildrenReadonly ??= new ReadOnlyCollection<Element>(LogicalChildrenInternalBackingStore);
		}

		/// <summary>
		/// Inserts an <see cref="Element"/> to the logical children at the specified index.
		/// </summary>
		/// <param name="index">The zero-based index at which <see cref="Element"/> should be inserted.</param>
		/// <param name="element">The <see cref="Element"/> to insert into the logical children.</param>
		public void InsertLogicalChild(int index, Element element)
		{
			if (element is null)
			{
				return;
			}

			SetupChildren();

			LogicalChildrenInternalBackingStore.Insert(index, element);
			OnChildAdded(element);
		}

		/// <summary>
		/// Adds an <see cref="Element"/> to the logical children.
		/// </summary>
		/// <param name="element">The <see cref="Element"/> to add to the logical children.</param>
		public void AddLogicalChild(Element element)
		{
			if (element is null)
			{
				return;
			}

			SetupChildren();

			LogicalChildrenInternalBackingStore.Add(element);
			OnChildAdded(element);
		}

		/// <summary>
		/// Removes the first occurrence of a specific <see cref="Element"/> from the logical children.
		/// </summary>
		/// <param name="element">The <see cref="Element"/> to remove.</param>
		/// <returns>
		///	true if item was successfully removed from the logical children;
		/// otherwise, false. This method also returns false if <see cref="Element"/> is not found.
		///	</returns>
		public bool RemoveLogicalChild(Element element)
		{
			if (element is null)
			{
				return false;
			}

			if (LogicalChildrenInternalBackingStore is null)
				return false;

			var index = LogicalChildrenInternalBackingStore.IndexOf(element);
			if (index < 0)
				return false;

			RemoveLogicalChild(element, index);

			return true;
		}

		/// <summary>
		///  Removes all child <see cref="Element"/>s.
		/// </summary>
		public void ClearLogicalChildren()
		{
			if (LogicalChildrenInternalBackingStore is null)
				return;

			if (LogicalChildrenInternal == EmptyChildren)
				return;

			// Reverse for-loop, so children can be removed while iterating
			for (int i = LogicalChildrenInternalBackingStore.Count - 1; i >= 0; i--)
			{
				RemoveLogicalChild(LogicalChildrenInternalBackingStore[i], i);
			}
		}

		internal bool RemoveLogicalChild(Element element, int index)
		{
			LogicalChildrenInternalBackingStore.Remove(element);
			OnChildRemoved(element, index);

			return true;
		}

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

		/// <summary>For internal use by the <see cref="Controls"/> platform.</summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public Element RealParent { get; private set; }

		Dictionary<BindableProperty, (string, SetterSpecificity)> DynamicResources => _dynamicResources ?? (_dynamicResources = new Dictionary<BindableProperty, (string, SetterSpecificity)>());

		/// <inheritdoc/>
		void IElementDefinition.AddResourcesChangedListener(Action<object, ResourcesChangedEventArgs> onchanged)
		{
			_changeHandlers = _changeHandlers ?? new List<Action<object, ResourcesChangedEventArgs>>(2);
			_changeHandlers.Add(onchanged);
		}

		/// <summary>Gets or sets the parent <see cref="Element"/> of this element.</summary>
		/// <value>The <see cref="Element"/> which should be the parent of this element.</value>
		/// <remarks>Most application authors will not need to set the parent element by hand.</remarks>
		public Element Parent
		{
			get { return _parentOverride ?? RealParent; }
			set => SetParent(value);
		}

		void SetParent(Element value)
		{
			if (RealParent == value)
				return;

			OnPropertyChanging(nameof(Parent));

			if (_parentOverride == null)
				OnParentChangingCore(Parent, value);

			if (RealParent != null)
			{
				((IElementDefinition)RealParent).RemoveResourcesChangedListener(OnParentResourcesChanged);

				if (value != null && (RealParent is Layout || RealParent is IControlTemplated))
					Application.Current?.FindMauiContext()?.CreateLogger<Element>()?.LogWarning($"{this} is already a child of {RealParent}. Remove {this} from {RealParent} before adding to {value}.");
			}

			RealParent = value;
			if (RealParent != null)
			{
				OnParentResourcesChanged(RealParent.GetMergedResources());
				((IElementDefinition)RealParent).AddResourcesChangedListener(OnParentResourcesChanged);
			}

			object context = value?.BindingContext;
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

			OnPropertyChanged(nameof(Parent));
		}

		internal bool IsTemplateRoot { get; set; }

		/// <inheritdoc/>
		void IElementDefinition.RemoveResourcesChangedListener(Action<object, ResourcesChangedEventArgs> onchanged)
		{
			if (_changeHandlers == null)
				return;
			_changeHandlers.Remove(onchanged);
		}

		/// <summary>For internal use by the <see cref="Controls"/> platform.</summary>
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

		/// <summary>For internal use by the <see cref="Controls"/> platform.</summary>
		void IElementController.SetValueFromRenderer(BindableProperty property, object value) => SetValueFromRenderer(property, value);

		/// <summary>For internal use by the <see cref="Controls"/> platform.</summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public void SetValueFromRenderer(BindableProperty property, object value)
		{
			SetValue(property, value, specificity: SetterSpecificity.FromHandler);
		}

		/// <summary>For internal use by the <see cref="Controls"/> platform.</summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public void SetValueFromRenderer(BindablePropertyKey property, object value)
		{
			SetValue(property, value, specificity: SetterSpecificity.FromHandler);
		}

		/// <summary>For internal use by the <see cref="Controls"/> platform.</summary>
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

		/// <summary>Returns the element that has the specified name.</summary>
		/// <param name="name">The name of the element to be found.</param>
		/// <returns>The element that has the specified name.</returns>
		/// <exception cref="InvalidOperationException">Thrown if the element's namescope couldn't be found.</exception>
		public object FindByName(string name)
		{
			var namescope = GetNameScope();
			if (namescope == null)
				throw new InvalidOperationException("this element is not in a namescope");
			return namescope.FindByName(name);
		}

		/// <inheritdoc/>
		void INameScope.RegisterName(string name, object scopedElement)
		{
			var namescope = GetNameScope() ?? throw new InvalidOperationException("this element is not in a namescope");
			namescope.RegisterName(name, scopedElement);
		}

		/// <inheritdoc/>
		void INameScope.UnregisterName(string name)
		{
			var namescope = GetNameScope() ?? throw new InvalidOperationException("this element is not in a namescope");
			namescope.UnregisterName(name);
		}

		/// <summary>Raised whenever a child element is added to the element.</summary>
		public event EventHandler<ElementEventArgs> ChildAdded;

		/// <summary>Raised whenever a child element is removed from the element.</summary>
		public event EventHandler<ElementEventArgs> ChildRemoved;

		/// <summary>Raised whenever a child element is added to the element's subtree.</summary>
		public event EventHandler<ElementEventArgs> DescendantAdded;

		/// <summary>Raised whenever a child element is removed from the elements subtree.</summary>
		public event EventHandler<ElementEventArgs> DescendantRemoved;

		/// <summary>Removes a previously set dynamic resource.</summary>
		/// <param name="property">The <see cref="BindableProperty"/> from which to remove the DynamicResource.</param>
		public new void RemoveDynamicResource(BindableProperty property)
		{
			base.RemoveDynamicResource(property);
		}

		/// <summary>Sets the <see cref="BindableProperty"/> property of this element to be updated via the DynamicResource with the provided key.</summary>
		/// <param name="property">The property to be updated.</param>
		/// <param name="key">The key for the requested resource.</param>
		public new void SetDynamicResource(BindableProperty property, string key)
		{
			base.SetDynamicResource(property, key);
		}

		/// <inheritdoc/>
		IReadOnlyList<Maui.IVisualTreeElement> IVisualTreeElement.GetVisualChildren()
			=> LogicalChildrenInternal;

		/// <inheritdoc/>
		IVisualTreeElement IVisualTreeElement.GetVisualParent() => this.Parent;

		/// <summary>Invoked whenever the binding context of the element changes. Implement this method to add class handling for this event.</summary>
		/// <remarks>Implementors must call the base method.</remarks>
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

		/// <summary>Raises the <see cref="ChildAdded"/> event. Implement this method to add class handling for this event.</summary>
		/// <remarks>This method has no default implementation. You should still call the base implementation in case an intermediate class has implemented this method.</remarks>
		/// <param name="child">The element that's been added as a child.</param>
		protected virtual void OnChildAdded(Element child)
		{
			child.SetParent(this);

			child.ApplyBindings(skipBindingContext: false, fromBindingContextChanged: true);

			ChildAdded?.Invoke(this, new ElementEventArgs(child));

			VisualDiagnostics.OnChildAdded(this, child);

			OnDescendantAdded(child);
			foreach (Element element in child.Descendants())
				OnDescendantAdded(element);
		}

		/// <summary> Raises the <see cref="ChildRemoved"/> event. Implement this method to add class handling for this event </summary>
		/// <remarks>
		/// This method has no default implementation. You should still call the base implementation in case an intermediate class has implemented this method.
		/// If not debugging, the logical tree index will not have any effect.
		/// </remarks>
		/// <param name="child">The child element that's been removed.</param>
		/// <param name="oldLogicalIndex">The child's element index in the logical tree.</param>
		protected virtual void OnChildRemoved(Element child, int oldLogicalIndex)
		{
			child.SetParent(null);

			ChildRemoved?.Invoke(this, new ElementEventArgs(child));

			VisualDiagnostics.OnChildRemoved(this, child, oldLogicalIndex);

			OnDescendantRemoved(child);
			foreach (Element element in child.Descendants())
				OnDescendantRemoved(element);
		}

		/// <summary>Raises the <see cref="ParentSet"/> event. Implement this method in order to add behavior when the element is added to a parent.</summary>
		/// <remarks>Implementors must call the base method.</remarks>
		protected virtual void OnParentSet()
		{
			ParentSet?.Invoke(this, EventArgs.Empty);
			ApplyStyleSheets();
			(this as IPropertyPropagationController)?.PropagatePropertyChanged(null);
		}

		/// <summary>Method that is called when a bound property is changed.</summary>
		/// <param name="propertyName">The name of the bound property that changed.</param>
		protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			base.OnPropertyChanged(propertyName);

			Handler?.UpdateValue(propertyName);

			if (_effects?.Count > 0)
			{
				var args = new PropertyChangedEventArgs(propertyName);
				foreach (Effect effect in _effects)
				{
					effect?.SendOnElementPropertyChanged(args);
				}
			}
		}

		internal IEnumerable<Element> Descendants() =>
			Descendants<Element>();

		/// <inheritdoc/>
		IEnumerable<Element> IElementController.Descendants() =>
			Descendants<Element>();

		internal IEnumerable<TElement> Descendants<TElement>()
			where TElement : Element
		{
			var queue = new Queue<Element>(16);
			queue.Enqueue(this);

			while (queue.Count > 0)
			{
				IReadOnlyList<Element> children = queue.Dequeue().LogicalChildrenInternal;
				for (var i = 0; i < children.Count; i++)
				{
					Element child = children[i];
					if (child is not TElement childT)
						continue;

					yield return childT;
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
				List<(BindableProperty, SetterSpecificity)> changedResources = null;
				foreach (KeyValuePair<BindableProperty, (string, SetterSpecificity)> dynR in DynamicResources)
				{
					// when the DynamicResource bound to a BindableProperty is
					// changing then the BindableProperty needs to be refreshed;
					// The .Value.Item1 is the name of DynamicResouce to which the BindableProperty is bound.
					// The .Key is the name of the DynamicResource whose value is changing.
					if (dynR.Value.Item1 != value.Key)
						continue;
					changedResources = changedResources ?? new List<(BindableProperty, SetterSpecificity)>();
					changedResources.Add((dynR.Key, dynR.Value.Item2));
				}
				if (changedResources == null)
					continue;
				foreach ((BindableProperty, SetterSpecificity) changedResource in changedResources)
					OnResourceChanged(changedResource.Item1, value.Value, changedResource.Item2);

				var bindableObject = value.Value as BindableObject;
				if (bindableObject != null && (bindableObject as Element)?.Parent == null)
				{
					if (!_bindableResources.Contains(bindableObject))
						_bindableResources.Add(bindableObject);
					SetInheritedBindingContext(bindableObject, BindingContext);
				}
			}
		}

		internal override void OnSetDynamicResource(BindableProperty property, string key, SetterSpecificity specificity)
		{
			base.OnSetDynamicResource(property, key, specificity);
			DynamicResources[property] = (key, specificity);
			if (this.TryGetResource(key, out var value))
				OnResourceChanged(property, value, specificity);
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

		void OnResourceChanged(BindableProperty property, object value, SetterSpecificity specificity)
			=> SetValueCore(property, value, SetValueFlags.ClearOneWayBindings | SetValueFlags.ClearTwoWayBindings, SetValuePrivateFlags.Default, specificity);

		/// <summary>Raised whenever the element's starts to change.</summary>
		public event EventHandler<ParentChangingEventArgs> ParentChanging;

		/// <summary>Raised whenever the element's parent has changed.</summary>
		public event EventHandler ParentChanged;

		/// <summary>When overridden in a derived class, should raise the <see cref="ParentChanging"/> event.</summary>
		/// <remarks>It is the implementor's responsibility to raise the <see cref="ParentChanging"/> event.</remarks>
		/// <param name="args">Provides data for the <see cref="ParentChanging"/> event.</param>
		protected virtual void OnParentChanging(ParentChangingEventArgs args) { }

		/// <summary>When overridden in a derived class, should raise the <see cref="ParentChanged"/> event.</summary>
		/// <remarks>It is the implementor's responsibility to raise the <see cref="ParentChanged"/> event.</remarks>
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

		IElementHandler _handler;
		EffectsFactory _effectsFactory;

		/// <inheritdoc/>
		Maui.IElement Maui.IElement.Parent => Parent;

		EffectsFactory EffectsFactory => _effectsFactory ??= Handler.MauiContext.Services.GetRequiredService<EffectsFactory>();

		/// <summary>Gets or sets the associated handler for this element.</summary>
		/// <value>An implementation of <see cref="IElementHandler"/>.</value>
		/// <remarks>Maps the element to platform-specific controls and implementations.</remarks>
		/// <seealso href="https://learn.microsoft.com/dotnet/maui/user-interface/handlers/">Conceptual documentation on handlers</seealso>
		public IElementHandler Handler
		{
			get => _handler;
			set => SetHandler(value);
		}

		/// <summary>Raised whenever the element's handler starts to change.</summary>
		public event EventHandler<HandlerChangingEventArgs> HandlerChanging;

		/// <summary>Raised whenever the element's handler has changed.</summary>
		public event EventHandler HandlerChanged;

		/// <summary>When overridden in a derived class, should raise the <see cref="HandlerChanging"/> event.</summary>
		/// <remarks>It is the implementor's responsibility to raise the <see cref="HandlerChanging"/> event.</remarks>
		/// <param name="args">Provides data for the <see cref="HandlerChanging"/> event.</param>
		protected virtual void OnHandlerChanging(HandlerChangingEventArgs args) { }

		/// <summary>When overridden in a derived class, should raise the <see cref="HandlerChanged"/> event.</summary>
		/// <remarks>It is the implementor's responsibility to raise the <see cref="HandlerChanged"/> event.</remarks>
		protected virtual void OnHandlerChanged() { }

		private protected virtual void OnHandlerChangedCore()
		{
			EffectControlProvider = (Handler != null) ? this : null;
			HandlerChanged?.Invoke(this, EventArgs.Empty);

			OnHandlerChanged();
		}

		private protected virtual void OnHandlerChangingCore(HandlerChangingEventArgs args)
		{
			HandlerChanging?.Invoke(this, args);
			OnHandlerChanging(args);
		}

		IElementHandler _previousHandler;
		void SetHandler(IElementHandler newHandler)
		{
			if (newHandler == _handler)
				return;

			try
			{
				// If a handler is getting changed before the end of this method
				// Something is wired up incorrectly
				if (_previousHandler != null)
					throw new InvalidOperationException("Handler is already being set elsewhere");

				_previousHandler = _handler;

				OnHandlerChangingCore(new HandlerChangingEventArgs(_previousHandler, newHandler));

				_handler = newHandler;

				// Only call disconnect if the previous handler is still connected to this virtual view.
				// If a handler is being reused for a different VirtualView then the virtual
				// view would have already rolled 
				if (_previousHandler?.VirtualView == this)
					_previousHandler?.DisconnectHandler();

				if (_handler?.VirtualView != this)
					_handler?.SetVirtualView(this);

				OnHandlerChangedCore();
			}
			finally
			{
				_previousHandler = null;
			}
		}

		/// <inheritdoc/>
		void IEffectControlProvider.RegisterEffect(Effect effect)
		{
			if (effect is RoutingEffect re && re.Inner != null)
			{
				re.Element = this;
				re.Inner.Element = this;
				return;
			}

			var platformEffect = EffectsFactory.CreateEffect(effect);

			if (platformEffect != null)
			{
				platformEffect.Element = this;
				effect.PlatformEffect = platformEffect;
			}
			else
			{
				effect.Element = this;
			}
		}

		/// <inheritdoc/>
		ToolTip IToolTipElement.ToolTip => ToolTipProperties.GetToolTip(this);

		/// <inheritdoc/>
		IFlyout IContextFlyoutElement.ContextFlyout => FlyoutBase.GetContextFlyout(this);

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
