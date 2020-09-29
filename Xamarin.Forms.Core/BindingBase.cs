using System;
using System.Collections;
using System.Runtime.CompilerServices;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms
{
	public abstract class BindingBase
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

		public string StringFormat
		{
			get { return _stringFormat; }
			set
			{
				ThrowIfApplied();
				_stringFormat = value;
			}
		}

		public object TargetNullValue
		{
			get { return _targetNullValue; }
			set
			{
				ThrowIfApplied();
				_targetNullValue = value;
			}
		}

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

		public static void DisableCollectionSynchronization(IEnumerable collection)
		{
			if (collection == null)
				throw new ArgumentNullException(nameof(collection));

			SynchronizedCollections.Remove(collection);
		}

		public static void EnableCollectionSynchronization(IEnumerable collection, object context, CollectionSynchronizationCallback callback)
		{
			if (collection == null)
				throw new ArgumentNullException(nameof(collection));
			if (callback == null)
				throw new ArgumentNullException(nameof(callback));

			SynchronizedCollections.Add(collection, new CollectionSynchronizationContext(context, callback));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected void ThrowIfApplied()
		{
			if (IsApplied)
				throw new InvalidOperationException("Cannot change a binding while it's applied");
		}

		internal virtual void Apply(bool fromTarget) => IsApplied = true;

		internal virtual void Apply(object context, BindableObject bindObj, BindableProperty targetProperty, bool fromBindingContextChanged = false) => IsApplied = true;

		internal abstract BindingBase Clone();

		internal virtual object GetSourceValue(object value, Type targetPropertyType)
		{
			if (value == null && TargetNullValue != null)
				return TargetNullValue;

			if (StringFormat != null && TryFormat(StringFormat, value, out var formatted))
				return formatted;

			return value;
		}

		internal bool TryFormat(string format, object arg0, out string value)
		{
			try
			{
				value = string.Format(format, arg0);
				return true;
			}
			catch (FormatException)
			{
				value = null;
				Log.Warning("Binding", "FormatException");
				return false;
			}
		}

		internal bool TryFormat(string format, object[] args, out string value)
		{
			try
			{
				value = string.Format(format, args);
				return true;
			}
			catch (FormatException)
			{
				value = null;
				Log.Warning("Binding", "FormatException");
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