#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls
{
	/// <summary>
	/// Groups property setters that can be shared between multiple visual elements.
	/// </summary>
	[ContentProperty(nameof(Setters))]
	public sealed class Style : IStyle
	{
		internal const string StyleClassPrefix = "Microsoft.Maui.Controls.StyleClass.";

		readonly BindableProperty _basedOnResourceProperty = BindableProperty.CreateAttached("BasedOnResource", typeof(Style), typeof(Style), default(Style),
			propertyChanged: OnBasedOnResourceChanged);

		readonly ConditionalWeakTable<BindableObject, object> _targets = new();

		Style _basedOnStyle;

		string _baseResourceKey;

		IList<Behavior> _behaviors;

		IList<TriggerBase> _triggers;

		// Fields for lazy/trimmable styles
		readonly object _initializerLock = new();
		List<(BindableObject Target, SetterSpecificity Specificity)> _targetsAppliedDuringInitialization;
		bool _isInitialized;
		bool _settersReplacedBeforeInitialization;

		/// <summary>
		/// Initializes a new instance of the <see cref="Style"/> class.
		/// </summary>
		/// <param name="targetType">The type to which the style applies.</param>
		public Style([System.ComponentModel.TypeConverter(typeof(TypeTypeConverter))][Parameter("TargetType")] Type targetType)
		{
			TargetType = targetType ?? throw new ArgumentNullException(nameof(targetType));
			Setters = new List<Setter>();
		}

		/// <summary>
		/// Sets the initializer that populates the style's Setters, Behaviors, and Triggers.
		/// This property is intended for source generator use only.
		/// </summary>
		/// <remarks>
		/// The initializer runs every time the style is applied and receives the style and the target
		/// element. It returns whether the target is an instance of the style's target type; the style is
		/// not applied to targets for which it returns <see langword="false"/>. The style argument is
		/// non-null until the first compatible target is seen; the initializer then populates the
		/// collections. Afterwards it is <see langword="null"/> and the initializer only checks the target.
		/// </remarks>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public Func<Style, BindableObject, bool> LazyInitialization { internal get; set; }

		/// <summary>
		/// Gets or sets whether the style can be applied to types derived from <see cref="TargetType"/>.
		/// </summary>
		public bool ApplyToDerivedTypes { get; set; }

		/// <summary>
		/// Gets or sets the style from which this style inherits.
		/// </summary>
		public Style BasedOn
		{
			get { return _basedOnStyle; }
			set
			{
				if (_basedOnStyle == value)
					return;
				if (!ValidateBasedOn(value))
					throw new ArgumentException("BasedOn.TargetType is not compatible with TargetType");
				Style oldValue = _basedOnStyle;
				_basedOnStyle = value;
				BasedOnChanged(oldValue, value);
				if (value != null)
					BaseResourceKey = null;
			}
		}

		/// <summary>
		/// Gets or sets the resource key for the base style.
		/// </summary>
		public string BaseResourceKey
		{
			get { return _baseResourceKey; }
			set
			{
				if (_baseResourceKey == value)
					return;
				_baseResourceKey = value;
				//update all DynamicResources
				foreach (var target in (IEnumerable<KeyValuePair<BindableObject, object>>)(object)_targets)
				{
					target.Key.RemoveDynamicResource(_basedOnResourceProperty);
					if (value != null)
						target.Key.SetDynamicResource(_basedOnResourceProperty, value);
				}
				if (value != null)
					BasedOn = null;
			}
		}

		/// <summary>
		/// Gets the collection of <see cref="Behavior"/> objects attached to this style.
		/// </summary>
		/// <remarks>
		/// When the <c>MauiXamlTrimmableStyles</c> MSBuild property is <c>true</c> (the default when
		/// <c>PublishAot</c> is <c>true</c>), styles declared in XAML and compiled by the XAML source generator
		/// are initialized lazily: the setters, behaviors and triggers declared in XAML are added the first
		/// time the style is applied to an instance of <see cref="TargetType"/> (or a derived type). Until
		/// then, this collection only contains items added from code. Such a style is never applied to
		/// elements that are not instances of <see cref="TargetType"/>; a warning is logged instead.
		/// </remarks>
		public IList<Behavior> Behaviors => _behaviors ??= new AttachedCollection<Behavior>();

		/// <summary>
		/// Gets or sets whether this style can cascade to nested elements.
		/// </summary>
		public bool CanCascade { get; set; }

		/// <summary>
		/// Gets or sets the class name for the style.
		/// </summary>
		public string Class { get; set; }

		/// <summary>
		/// Gets the collection of <see cref="Setter"/> objects that define the property values for this style.
		/// </summary>
		/// <remarks>
		/// When the <c>MauiXamlTrimmableStyles</c> MSBuild property is <c>true</c> (the default when
		/// <c>PublishAot</c> is <c>true</c>), styles declared in XAML and compiled by the XAML source generator
		/// are initialized lazily: the setters, behaviors and triggers declared in XAML are added the first
		/// time the style is applied to an instance of <see cref="TargetType"/> (or a derived type). Until
		/// then, this collection only contains items added from code. Such a style is never applied to
		/// elements that are not instances of <see cref="TargetType"/>; a warning is logged instead.
		/// </remarks>
		public IList<Setter> Setters { get; }

		/// <summary>
		/// Gets the collection of <see cref="TriggerBase"/> objects attached to this style.
		/// </summary>
		/// <remarks>
		/// When the <c>MauiXamlTrimmableStyles</c> MSBuild property is <c>true</c> (the default when
		/// <c>PublishAot</c> is <c>true</c>), styles declared in XAML and compiled by the XAML source generator
		/// are initialized lazily: the setters, behaviors and triggers declared in XAML are added the first
		/// time the style is applied to an instance of <see cref="TargetType"/> (or a derived type). Until
		/// then, this collection only contains items added from code. Such a style is never applied to
		/// elements that are not instances of <see cref="TargetType"/>; a warning is logged instead.
		/// </remarks>
		public IList<TriggerBase> Triggers => _triggers ??= new AttachedCollection<TriggerBase>();

		/// <summary>
		/// Gets whether this style only applies to instances of its target type, as checked by the
		/// source-generated <see cref="LazyInitialization"/>.
		/// </summary>
		internal bool ChecksTargetType => LazyInitialization is not null;

		/// <summary>
		/// Populates a lazily initialized style for <paramref name="target"/> without applying it.
		/// </summary>
		internal void EnsureInitializedFor(BindableObject target)
		{
			var lazyInitialization = LazyInitialization;
			if (lazyInitialization is not null && !Volatile.Read(ref _isInitialized))
				TryInitialize(lazyInitialization, target, default, out _);
		}

		void IStyle.Apply(BindableObject bindable, SetterSpecificity specificity)
		{
			if (!CanApplyTo(bindable, specificity))
				return;

			lock (_targets)
			{
#if NETSTANDARD2_0
				_targets.Remove(bindable);
				_targets.Add(bindable, specificity);
#else
				_targets.AddOrUpdate(bindable, specificity);
#endif
			}

			if (BaseResourceKey != null)
				bindable.SetDynamicResource(_basedOnResourceProperty, BaseResourceKey);
			ApplyCore(bindable, BasedOn ?? GetBasedOnResource(bindable), specificity);
		}

		bool CanApplyTo(BindableObject bindable, SetterSpecificity specificity)
		{
			var lazyInitialization = LazyInitialization;
			if (lazyInitialization is null)
				return true;

			bool isCompatible;
			if (Volatile.Read(ref _isInitialized))
				isCompatible = lazyInitialization(null, bindable);
			else if (!TryInitialize(lazyInitialization, bindable, specificity, out isCompatible))
				return false;

			if (!isCompatible)
				LogTargetTypeMismatch(bindable);

			return isCompatible;
		}

		void LogTargetTypeMismatch(BindableObject bindable)
		{
			var message = $"Style TargetType {TargetType.FullName} is not compatible with element target type {bindable.GetType()}. The style is not applied.";
			var logger = bindable is Element element
				? element.CreateLogger<Style>()
				: Application.Current?.FindMauiContext()?.CreateLogger<Style>();

			// Elements are often styled before the app has a MauiContext; don't drop the warning then.
			if (logger is null)
				Trace.WriteLine(message);
			else if (logger.IsEnabled(LogLevel.Warning))
				logger.Log(LogLevel.Warning, message);
		}

		// Runs the initializer for the first compatible target. Returns false when the style is applied again
		// from inside its own initializer; that target is applied once initialization completes.
		bool TryInitialize(Func<Style, BindableObject, bool> lazyInitialization, BindableObject bindable, SetterSpecificity specificity, out bool isCompatible)
		{
			List<(BindableObject Target, SetterSpecificity Specificity)> deferredTargets;
			lock (_initializerLock)
			{
				if (_isInitialized)
				{
					isCompatible = lazyInitialization(null, bindable);
					return true;
				}

				if (_targetsAppliedDuringInitialization is not null)
				{
					_targetsAppliedDuringInitialization.Add((bindable, specificity));
					isCompatible = false;
					return false;
				}

				_targetsAppliedDuringInitialization = new();
				var existingSetterCount = Setters.Count;
				try
				{
					isCompatible = lazyInitialization(this, bindable);
				}
				finally
				{
					deferredTargets = _targetsAppliedDuringInitialization;
					_targetsAppliedDuringInitialization = null;
				}

				if (isCompatible)
				{
					// Setters replaced by Hot Reload before initialization already reflect the edited XAML.
					if (_settersReplacedBeforeInitialization)
						RemoveSettersFrom(existingSetterCount);
					else
						MoveInitializedSettersBeforeExisting(existingSetterCount);
					Volatile.Write(ref _isInitialized, true);
				}
			}

			foreach (var (target, targetSpecificity) in deferredTargets)
				((IStyle)this).Apply(target, targetSpecificity);

			return true;
		}

		void RemoveSettersFrom(int index)
		{
			for (var i = Setters.Count - 1; i >= index; i--)
				Setters.RemoveAt(i);
		}

		void MoveInitializedSettersBeforeExisting(int existingSetterCount)
		{
			// XAML setters precede setters added to the style in code, matching eager initialization.
			var initializedSetterCount = Setters.Count - existingSetterCount;
			for (var index = 0; index < initializedSetterCount; index++)
			{
				var setter = Setters[existingSetterCount + index];
				Setters.Insert(index, setter);
				Setters.RemoveAt(existingSetterCount + index + 1);
			}
		}

		/// <summary>
		/// Gets the type to which this style applies.
		/// </summary>
		public Type TargetType { get; }

		void IStyle.UnApply(BindableObject bindable)
		{
			lock (_targets)
			{
				// Nothing was applied to targets this style rejected.
				if (!_targets.TryGetValue(bindable, out _))
					return;
			}

			UnApplyCore(bindable, BasedOn ?? GetBasedOnResource(bindable));
			bindable.RemoveDynamicResource(_basedOnResourceProperty);
			lock (_targets)
			{
				_targets.Remove(bindable);
			}
		}

		internal bool CanBeAppliedTo(Type targetType)
		{
			if (TargetType == targetType)
				return true;
			if (!ApplyToDerivedTypes)
				return false;
			do
			{
				targetType = targetType.BaseType;
				if (TargetType == targetType)
					return true;
			} while (targetType != typeof(Element));
			return false;
		}

		void BasedOnChanged(Style oldValue, Style newValue)
		{
			foreach (var target in (IEnumerable<KeyValuePair<BindableObject, object>>)(object)_targets)
			{
				UnApplyCore(target.Key, oldValue);
				ApplyCore(target.Key, newValue, (SetterSpecificity)target.Value);
			}
		}

		Style GetBasedOnResource(BindableObject bindable) => (Style)bindable.GetValue(_basedOnResourceProperty);

		static void OnBasedOnResourceChanged(BindableObject bindable, object oldValue, object newValue)
		{
			Style style = (bindable as IStyleElement).Style;
			if (style == null)
				return;
			if (!style._targets.TryGetValue(bindable, out var objectspecificity))
				return;

			style.UnApplyCore(bindable, (Style)oldValue);
			style.ApplyCore(bindable, (Style)newValue, (SetterSpecificity)objectspecificity);
		}

		ConditionalWeakTable<BindableObject, object> specificities = new();

		void ApplyCore(BindableObject bindable, Style basedOn, SetterSpecificity specificity)
		{
			if (basedOn != null)
			{
				((IStyle)basedOn).UnApply(bindable);
				((IStyle)basedOn).Apply(bindable, specificity.AsBaseStyle());
			}

#if NETSTANDARD2_0
			specificities.Remove(bindable);
			specificities.Add(bindable, specificity);
#else
			specificities.AddOrUpdate(bindable, specificity);
#endif

			foreach (Setter setter in Setters)
				setter.Apply(bindable, specificity);

			((AttachedCollection<Behavior>)Behaviors).AttachTo(bindable);
			((AttachedCollection<TriggerBase>)Triggers).AttachTo(bindable);
		}

		void UnApplyCore(BindableObject bindable, Style basedOn)
		{
			((AttachedCollection<TriggerBase>)Triggers).DetachFrom(bindable);
			((AttachedCollection<Behavior>)Behaviors).DetachFrom(bindable);

			if (!specificities.TryGetValue(bindable, out var specificity))
				return;

			foreach (Setter setter in Setters)
				setter.UnApply(bindable, (SetterSpecificity)specificity);

			if (basedOn != null)
				((IStyle)basedOn).UnApply(bindable);
		}

		/// <summary>
		/// Replaces the setters of this style and reapplies them to every element the style is
		/// currently applied to (including elements reached through a derived style). Used by
		/// XAML Hot Reload so existing consumers observe edited setter values without being
		/// re-resolved. Triggers and behaviors are left untouched.
		/// </summary>
		internal void ReplaceSetters(IEnumerable<Setter> setters)
		{
			var newSetters = new List<Setter>(setters);
			var targets = new List<(BindableObject Target, SetterSpecificity Specificity)>();
			lock (_targets)
			{
				foreach (var target in (IEnumerable<KeyValuePair<BindableObject, object>>)(object)_targets)
				{
					if (specificities.TryGetValue(target.Key, out var specificity))
						targets.Add((target.Key, (SetterSpecificity)specificity));
				}
			}

			foreach (var (target, specificity) in targets)
				foreach (Setter setter in Setters)
					setter.UnApply(target, specificity);

			Setters.Clear();
			foreach (var setter in newSetters)
				Setters.Add(setter);

			if (LazyInitialization is not null)
			{
				lock (_initializerLock)
				{
					if (!_isInitialized)
						_settersReplacedBeforeInitialization = true;
				}
			}

			foreach (var (target, specificity) in targets)
			{
				var mergedStyle = target switch
				{
					StyleableElement element => element._mergedStyle,
					Span span => span._mergedStyle,
					ImageSource imageSource => imageSource._mergedStyle,
					Shadow shadow => shadow._mergedStyle,
					_ => null
				};
				if (mergedStyle != null && mergedStyle.ContainsStyle(this))
					mergedStyle.Reapply();
				else
					foreach (Setter setter in Setters)
						setter.Apply(target, specificity);
			}
		}

		internal bool ContainsStyle(Style style, BindableObject target)
			=> ReferenceEquals(this, style) || (BasedOn ?? GetBasedOnResource(target))?.ContainsStyle(style, target) == true;

		bool ValidateBasedOn(Style value)
			=> value is null || value.TargetType.IsAssignableFrom(TargetType);
	}
}