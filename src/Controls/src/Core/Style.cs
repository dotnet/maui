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

		readonly BindableProperty _basedOnResourceProperty;

		readonly ConditionalWeakTable<BindableObject, object> _targets = new();

		Style _basedOnStyle;

		string _baseResourceKey;

		IList<Behavior> _behaviors;

		IList<TriggerBase> _triggers;

		// Fields for lazy/trimmable styles
		readonly string _assemblyQualifiedTargetTypeName;
		Action<Style, BindableObject> _initializer;
		readonly object _initializerLock = new();
		Type _targetType;

		// TODO: Revisit this suppression - consider removing [RequiresUnreferencedCode] from TargetType as it causes cascading issues
		[UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code.",
			Justification = "The BindableProperty stores Style instances but the callback doesn't access TargetType directly. The analyzer warns because Style.TargetType has [RequiresUnreferencedCode].")]
		Style()
		{
			_basedOnResourceProperty = BindableProperty.CreateAttached("BasedOnResource", typeof(Style), typeof(Style), default(Style),
				propertyChanged: OnBasedOnResourceChanged);
			Setters = new List<Setter>();
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/Style.xml" path="//Member[@MemberName='.ctor']/Docs/*" />
		public Style([System.ComponentModel.TypeConverter(typeof(TypeTypeConverter))][Parameter("TargetType")] Type targetType)
			: this()
		{
			_targetType = targetType ?? throw new ArgumentNullException(nameof(targetType));
		}

		/// <summary>
		/// Creates a lazy style that defers initialization until first application.
		/// This constructor is intended for source generator use only.
		/// </summary>
		/// <param name="assemblyQualifiedTargetTypeName">The assembly-qualified type name of the target type.</param>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public Style(string assemblyQualifiedTargetTypeName)
			: this()
		{
			_assemblyQualifiedTargetTypeName = assemblyQualifiedTargetTypeName ?? throw new ArgumentNullException(nameof(assemblyQualifiedTargetTypeName));
		}

		/// <summary>
		/// Sets the initializer action that populates the style's Setters, Behaviors, and Triggers.
		/// This property is intended for source generator use only.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public Action<Style, BindableObject> Initializer
		{
			set => _initializer = value;
		}

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
			EnsureInitialized(bindable);

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
		public Type TargetType
		{
			[RequiresUnreferencedCode("TargetType may have been trimmed when using lazy styles. Use IStyleExtensions.TryGetTargetType for trim-safe access.")]
			get
			{
				if (_targetType is not null)
					return _targetType;

				// For lazy styles, resolve from AQN - type may have been trimmed
				Debug.Assert(_assemblyQualifiedTargetTypeName is not null, "Either _targetType or _assemblyQualifiedTargetTypeName must be set");
				_targetType = Type.GetType(_assemblyQualifiedTargetTypeName, throwOnError: false);
				Debug.Assert(_targetType is not null, "TargetType was trimmed - callers should use TryGetTargetType for safe access");
				return _targetType;
			}
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

		// TODO: Try to remove this property if not needed after full implementation
		/// <summary>
		/// Returns true if this is a lazy style that hasn't been initialized yet.
		/// </summary>
		internal bool IsLazyStyle => _assemblyQualifiedTargetTypeName is not null;

		/// <summary>
		/// Ensures the lazy style is initialized for the given target.
		/// </summary>
		private void EnsureInitialized(BindableObject target)
		{
			lock (_initializerLock)
			{
				if (_initializer is null)
					return;

				_initializer(this, target);
				_initializer = null;
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

		bool ValidateBasedOn(Style value)
		{
			if (value is null)
				return true;

			// If we can't get the target type (trimmed), validation fails.
			// A type that exists can't be based on a trimmed type - if the base type existed,
			// it wouldn't have been trimmed (all base types of preserved types are preserved).
			if (!((IStyle)value).TryGetTargetType(out var basedOnTargetType))
				return false;

			if (!((IStyle)this).TryGetTargetType(out var thisTargetType))
				return false;

			return basedOnTargetType.IsAssignableFrom(thisTargetType);
		}
	}
}