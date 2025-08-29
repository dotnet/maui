#nullable disable
using System;
using System.Collections;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;

namespace Microsoft.Maui.Controls
{
	/// <summary>
	/// An abstract base class for all bindings providing <see cref="BindingMode" /> selection, fallback/target null values, and formatting support.
	/// </summary>
	/// <remarks>
	/// This class underlies concrete binding implementations (e.g., <see cref="Binding"/>, <see cref="MultiBinding"/>) and supplies common features such as
	/// binding mode control, string formatting and thread-safe collection synchronization helpers.
	/// </remarks>
	public abstract partial class BindingBase
	{
		static readonly ConditionalWeakTable<IEnumerable, CollectionSynchronizationContext> SynchronizedCollections = new ConditionalWeakTable<IEnumerable, CollectionSynchronizationContext>();

		BindingMode _mode = BindingMode.Default;
		string _stringFormat;
		object _targetNullValue;
		object _fallbackValue;
		WeakReference<Element> _relativeSourceTargetOverride;

		internal BindingBase()
		{
		}

		/// <summary>Gets or sets the mode for this binding.</summary>
		public BindingMode Mode
		{
			get { return _mode; }
			set
			{
				if (value != BindingMode.Default
					&& value != BindingMode.OneWay
					&& value != BindingMode.OneWayToSource
					&& value != BindingMode.TwoWay
					&& value != BindingMode.OneTime)
					throw new ArgumentException("mode is not a valid BindingMode", "mode");

				ThrowIfApplied();

				_mode = value;
			}
		}

		/// <summary>Gets or sets the string format applied to the bound value.</summary>
		/// <value>A standard <see cref="string.Format(string,object)" /> composite format string. For single-value bindings use one placeholder (e.g., <c>{0:C2}</c>).</value>
		/// <remarks>
		/// Used to format or composite the resulting bound value for display. Implementations follow standard <see cref="string.Format(string,object)" /> semantics.
		/// </remarks>
		public string StringFormat
		{
			get { return _stringFormat; }
			set
			{
				ThrowIfApplied();
				_stringFormat = value;
			}
		}

		/// <summary>
		/// Gets or sets the value to use when the binding successfully resolves the source path and the resulting source value is
		/// <see langword="null" />.
		/// </summary>
		/// <value>The value that will replace a resolved <see langword="null" /> source value when updating the target.</value>
		/// <remarks>
		/// <para><c>TargetNullValue</c> acts like a null-coalescing value for the binding source result: if the binding path resolves and the value is
		/// <see langword="null" />, the target receives <c>TargetNullValue</c> instead. It is <em>not</em> used when the binding cannot resolve (e.g. missing
		/// property, conversion error) — in those cases <see cref="FallbackValue" /> is considered.</para>
		/// </remarks>
		public object TargetNullValue
		{
			get { return _targetNullValue; }
			set
			{
				ThrowIfApplied();
				_targetNullValue = value;
			}
		}

		/// <summary>Gets or sets the value used when the binding cannot produce a source value (e.g. path not found, conversion failure).</summary>
		/// <value>The fallback value used instead of the target property's default value when source resolution fails entirely.</value>
		/// <remarks>
		/// <para><c>FallbackValue</c> is applied when the binding engine fails to obtain a value (e.g., missing source, unresolved path, or type conversion failure within the binding engine itself).
		/// It is not used for errors that occur inside value converters; such errors may be handled by the converter or use different fallback mechanisms. If the source resolves to <see langword="null" />, <see cref="TargetNullValue" /> is applied if set.</para>
		/// <para>Together with <see cref="TargetNullValue" /> this allows differentiating between a legitimate <see langword="null" /> value and an unresolved binding.</para>
		/// </remarks>
		public object FallbackValue
		{
			get => _fallbackValue;
			set
			{
				ThrowIfApplied();
				_fallbackValue = value;
			}
		}

		internal bool AllowChaining { get; set; }

		internal object Context { get; set; }

		internal bool IsApplied { get; private set; }

		internal Element RelativeSourceTargetOverride
		{
			get
			{
				Element element = null;
				_relativeSourceTargetOverride?.TryGetTarget(out element);
				return element;
			}
			set
			{
				if (value != null)
					_relativeSourceTargetOverride = new WeakReference<Element>(value);
				else
					_relativeSourceTargetOverride = null;
			}
		}

		/// <summary>Stops collection synchronization previously enabled for <paramref name="collection" />.</summary>
		/// <param name="collection">The collection on which to disable synchronization.</param>
		/// <remarks>See <see cref="EnableCollectionSynchronization(IEnumerable, object, CollectionSynchronizationCallback)" /> for details on thread-safe collection access.</remarks>
		public static void DisableCollectionSynchronization(IEnumerable collection)
		{
			if (collection == null)
				throw new ArgumentNullException(nameof(collection));

			SynchronizedCollections.Remove(collection);
		}

		/// <summary>Enables synchronized (thread-safe) access to <paramref name="collection" /> using the supplied callback.</summary>
		/// <param name="collection">The collection that will be read or updated from multiple threads.</param>
		/// <param name="context">A context object (optionally a lock object) passed to <paramref name="callback" />; may be <see langword="null" />.</param>
		/// <param name="callback">Delegate invoked by the framework to perform collection access under synchronization.</param>
		/// <remarks>
		/// The framework holds only a weak reference to the collection. The callback receives parameters indicating whether write access is required;
		/// implementers should perform appropriate locking (often on <paramref name="context" />) before invoking the supplied access delegate.
		/// </remarks>
		public static void EnableCollectionSynchronization(IEnumerable collection, object context, CollectionSynchronizationCallback callback)
		{
			if (collection == null)
				throw new ArgumentNullException(nameof(collection));
			if (callback == null)
				throw new ArgumentNullException(nameof(callback));

			SynchronizedCollections.Add(collection, new CollectionSynchronizationContext(context, callback));
		}

		/// <summary>Throws <see cref="InvalidOperationException" /> if the binding has already been applied.</summary>
		/// <remarks>Used by property setters to prevent mutation after the binding has been attached to a target.</remarks>
		/// <exception cref="InvalidOperationException">The binding has already been applied.</exception>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected void ThrowIfApplied()
		{
			if (IsApplied)
				throw new InvalidOperationException("Cannot change a binding while it's applied");
		}

		internal virtual void Apply(bool fromTarget)
				=> IsApplied = true;

		internal virtual void Apply(object context, BindableObject bindObj, BindableProperty targetProperty, bool fromBindingContextChanged, SetterSpecificity specificity)
			=> IsApplied = true;

		internal abstract BindingBase Clone();

		internal virtual object GetSourceValue(object value, Type targetPropertyType)
		{
			if (value == null && TargetNullValue != null)
				return TargetNullValue;

			if (StringFormat != null && TryFormat(StringFormat, value, out var formatted))
				return formatted;

			return value;
		}

		internal static bool TryFormat(string format, object arg0, out string value)
		{
			try
			{
				value = string.Format(format, arg0);
				return true;
			}
			catch (FormatException)
			{
				value = null;
				Application.Current?.FindMauiContext()?.CreateLogger<BindingBase>()?.LogWarning("FormatException");
				return false;
			}
		}

		internal static bool TryFormat(string format, object[] args, out string value)
		{
			try
			{
				value = string.Format(format, args);
				return true;
			}
			catch (FormatException)
			{
				value = null;
				Application.Current?.FindMauiContext()?.CreateLogger<BindingBase>()?.LogWarning("FormatException");
				return false;
			}
		}

		internal virtual object GetTargetValue(object value, Type sourcePropertyType) => value;

		internal static bool TryGetSynchronizedCollection(IEnumerable collection, out CollectionSynchronizationContext synchronizationContext)
		{
			if (collection == null)
				throw new ArgumentNullException(nameof(collection));

			return SynchronizedCollections.TryGetValue(collection, out synchronizationContext);
		}

		internal virtual void Unapply(bool fromBindingContextChanged = false) => IsApplied = false;
	}
}