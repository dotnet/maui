#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/Style.xml" path="Type[@FullName='Microsoft.Maui.Controls.Style']/Docs/*" />
	[ContentProperty(nameof(Setters))]
	public sealed class Style : IStyle
	{
		internal const string StyleClassPrefix = "Microsoft.Maui.Controls.StyleClass.";

		readonly BindableProperty _basedOnResourceProperty = BindableProperty.CreateAttached(
			"BasedOnResource", typeof(Style), typeof(Style), default(Style),
			propertyChanged: OnBasedOnResourceChanged);

		readonly ConditionalWeakTable<BindableObject, object> _targets = new();

		Style _basedOnStyle;

		string _baseResourceKey;

		IList<Behavior> _behaviors;

		IList<TriggerBase> _triggers;

		// Fields for lazy/trimmable styles
		readonly string _assemblyQualifiedTargetTypeName;
		readonly object _initializerLock = new();
		Type _targetType;

		/// <include file="../../docs/Microsoft.Maui.Controls/Style.xml" path="//Member[@MemberName='.ctor']/Docs/*" />
		public Style([System.ComponentModel.TypeConverter(typeof(TypeTypeConverter))][Parameter("TargetType")] Type targetType)
		{
			_targetType = targetType ?? throw new ArgumentNullException(nameof(targetType));
			Setters = new List<Setter>();
		}

		/// <summary>
		/// Creates a lazy style that defers initialization until first application.
		/// This constructor is intended for source generator use only.
		/// </summary>
		/// <param name="assemblyQualifiedTargetTypeName">The assembly-qualified type name of the target type.</param>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public Style(string assemblyQualifiedTargetTypeName)
		{
			_assemblyQualifiedTargetTypeName = assemblyQualifiedTargetTypeName ?? throw new ArgumentNullException(nameof(assemblyQualifiedTargetTypeName));
			Setters = new List<Setter>();
		}

		/// <summary>
		/// Sets the initializer action that populates the style's Setters, Behaviors, and Triggers.
		/// This property is intended for source generator use only.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public Action<Style, BindableObject> LazyInitialization { private get; set; }

		/// <include file="../../docs/Microsoft.Maui.Controls/Style.xml" path="//Member[@MemberName='ApplyToDerivedTypes']/Docs/*" />
		public bool ApplyToDerivedTypes { get; set; }

		/// <include file="../../docs/Microsoft.Maui.Controls/Style.xml" path="//Member[@MemberName='BasedOn']/Docs/*" />
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

		/// <include file="../../docs/Microsoft.Maui.Controls/Style.xml" path="//Member[@MemberName='BaseResourceKey']/Docs/*" />
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

		/// <include file="../../docs/Microsoft.Maui.Controls/Style.xml" path="//Member[@MemberName='Behaviors']/Docs/*" />
		public IList<Behavior> Behaviors => _behaviors ??= new AttachedCollection<Behavior>();

		/// <include file="../../docs/Microsoft.Maui.Controls/Style.xml" path="//Member[@MemberName='CanCascade']/Docs/*" />
		public bool CanCascade { get; set; }

		/// <include file="../../docs/Microsoft.Maui.Controls/Style.xml" path="//Member[@MemberName='Class']/Docs/*" />
		public string Class { get; set; }

		/// <include file="../../docs/Microsoft.Maui.Controls/Style.xml" path="//Member[@MemberName='Setters']/Docs/*" />
		public IList<Setter> Setters { get; }

		/// <include file="../../docs/Microsoft.Maui.Controls/Style.xml" path="//Member[@MemberName='Triggers']/Docs/*" />
		public IList<TriggerBase> Triggers => _triggers ??= new AttachedCollection<TriggerBase>();

		void IStyle.Apply(BindableObject bindable, SetterSpecificity specificity)
		{
			InitializeIfNeeded(bindable);

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

		/// <include file="../../docs/Microsoft.Maui.Controls/Style.xml" path="//Member[@MemberName='TargetType']/Docs/*" />
		public Type TargetType => _targetType ??= ResolveTargetType();

		/// <summary>
		/// Attempts to resolve the target type from the assembly-qualified name.
		/// Returns null if the type was trimmed away.
		/// </summary>
		[UnconditionalSuppressMessage("Trimming", "IL2057:Unrecognized value passed to the parameter 'typeName' of method 'System.Type.GetType(String, Boolean)'",
			Justification = "Lazy styles intentionally allow target types to be trimmed. When a type is trimmed, TargetType returns null and the style is skipped at runtime. This enables the trimmer to remove unused styles.")]
		private Type ResolveTargetType()
		{
			Debug.Assert(_assemblyQualifiedTargetTypeName is not null, "Either _targetType or _assemblyQualifiedTargetTypeName must be set");
			return Type.GetType(_assemblyQualifiedTargetTypeName, throwOnError: false);
		}

		/// <summary>
		/// Gets the full name of the target type (namespace-qualified, without assembly).
		/// </summary>
		internal ReadOnlySpan<char> TargetTypeFullName
		{
			get
			{
				// If we have the type already, use it
				if (_targetType is not null)
					return _targetType.FullName.AsSpan();

				// Extract FullName from AQN: "Namespace.TypeName, AssemblyName, ..."
				// FullName is everything before the first comma
				Debug.Assert(_assemblyQualifiedTargetTypeName is not null, "Either _targetType or _assemblyQualifiedTargetTypeName must be set");

#if NETSTANDARD2_0 || NETSTANDARD2_1
#pragma warning disable CA1307 // string.IndexOf(char, StringComparison) is not available in netstandard
				var commaIndex = _assemblyQualifiedTargetTypeName.IndexOf(',');
#pragma warning restore CA1307
#else
				var commaIndex = _assemblyQualifiedTargetTypeName.IndexOf(',', StringComparison.Ordinal);
#endif
				return commaIndex > 0
					? _assemblyQualifiedTargetTypeName.AsSpan(0, commaIndex)
					: _assemblyQualifiedTargetTypeName.AsSpan();
			}
		}

		/// <summary>
		/// Initializes the lazy style if it hasn't been initialized yet.
		/// This is primarily intended for testing scenarios where setters need to be inspected
		/// before the style is applied to any element.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		internal void InitializeIfNeeded(BindableObject target)
		{
			lock (_initializerLock)
			{
				if (LazyInitialization is null)
					return;

				LazyInitialization(this, target);
				LazyInitialization = null;
			}
		}

		void IStyle.UnApply(BindableObject bindable)
		{
			UnApplyCore(bindable, BasedOn ?? GetBasedOnResource(bindable));
			bindable.RemoveDynamicResource(_basedOnResourceProperty);
			lock (_targets)
			{
				_targets.Remove(bindable);
			}
		}

		internal bool CanBeAppliedTo(Type targetType)
		{
			// Use FullName comparison to avoid resolving the type (which may have been trimmed)
			if (TargetTypeFullName.SequenceEqual(targetType.FullName))
				return true;
			if (!ApplyToDerivedTypes)
				return false;
			do
			{
				targetType = targetType.BaseType;
				if (TargetTypeFullName.SequenceEqual(targetType.FullName))
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
				((IStyle)basedOn).Apply(bindable, specificity.AsBaseStyle());

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
		/// Validates that BasedOn.TargetType is compatible with this style's TargetType.
		/// Returns true if validation passes or cannot be performed (types trimmed).
		/// </summary>
		bool ValidateBasedOn(Style value)
		{
			if (value is null)
				return true;

			// If either type was trimmed, we can't validate - allow it and let runtime handle it
			var basedOnTargetType = value.TargetType;
			var thisTargetType = TargetType;

			if (basedOnTargetType is null || thisTargetType is null)
				return true;

			return basedOnTargetType.IsAssignableFrom(thisTargetType);
		}
	}
}