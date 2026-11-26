// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
#nullable enable
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Microsoft.Maui.Controls.Xaml.UnitTests.ScopedExtensions;

// A hand-written static method sharing the name of a lowered accessor is not an accessor, even when its
// receiver is more specific than the one of the extension property
public static class ImpostorExtensions
{
	extension(View view)
	{
		public string Impostor
		{
			set => ThreeWayRecorder.Record(view, "extension(View):" + value);
		}
	}

	public static void set_Impostor(Label label, string value) => ThreeWayRecorder.Record(label, "handwritten(Label):" + value);
}

// NestedAmbiguous is ambiguous inside this container for a Label (IView and BindableObject are unrelated)...
public static class NestedAmbiguityExtensions
{
	extension(IView view)
	{
		public string NestedAmbiguous
		{
			set => ThreeWayRecorder.Record(view, "IView:" + value);
		}
	}

	extension(BindableObject bindable)
	{
		public string NestedAmbiguous
		{
			set => ThreeWayRecorder.Record(bindable, "BindableObject:" + value);
		}
	}
}

// ...and this less specific candidate from another container must not hide that ambiguity
public static class NestedAmbiguityFallbackExtensions
{
	extension(object target)
	{
		public string NestedAmbiguous
		{
			set => ThreeWayRecorder.Record(target, "object:" + value);
		}
	}
}

public interface IScopedExplicitTag
{
	string ExplicitTag { get; }
	string WritableExplicitTag { get; set; }
}

// Instance members win over extension properties of the same name, even when they cannot be assigned. An
// explicitly implemented interface property is not an instance member of the class and hides nothing.
public class ScopedPrecedenceView : ContentView, IScopedExplicitTag
{
	public string PlainClr { get; set; } = string.Empty;

	public string ReadOnlyClr { get; private set; } = string.Empty;

	public string Collision { get; set; } = string.Empty;

	string IScopedExplicitTag.ExplicitTag => string.Empty;
	string IScopedExplicitTag.WritableExplicitTag { get; set; } = string.Empty;
}

public static class ScopedPrecedenceExtensions
{
	extension(View view)
	{
		public string PlainClr
		{
			set => ThreeWayRecorder.Record(view, "extension PlainClr:" + value);
		}

		public string ReadOnlyClr
		{
			set => ThreeWayRecorder.Record(view, "extension ReadOnlyClr:" + value);
		}

		public string ExplicitTag
		{
			set => ThreeWayRecorder.Record(view, "extension ExplicitTag:" + value);
		}

		public string WritableExplicitTag
		{
			set => ThreeWayRecorder.Record(view, "extension WritableExplicitTag:" + value);
		}

		public int Collision
		{
			set => ThreeWayRecorder.Record(view, "extension Collision:" + value);
		}
	}
}

public static class ObjectImpostorExtensions
{
	extension(View view)
	{
		public object ObjectImpostor
		{
			set => ThreeWayRecorder.Record(view, "extension object:" + value);
		}

		public dynamic DynamicImpostor
		{
			set => ThreeWayRecorder.Record(view, "extension dynamic:" + value);
		}
	}

	public static void set_ObjectImpostor(View view, string value) => ThreeWayRecorder.Record(view, "handwritten object:" + value);
	public static void set_DynamicImpostor(View view, string value) => ThreeWayRecorder.Record(view, "handwritten dynamic:" + value);
}

public class VariantExtensionView : ContentView, IEnumerable<string>
{
	public IEnumerator<string> GetEnumerator() => ((IEnumerable<string>)Array.Empty<string>()).GetEnumerator();
	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

public static class VariantExtensions
{
	extension(IEnumerable<object> target)
	{
		public string VariantTag
		{
			set => ThreeWayRecorder.Record(target, "variant:" + value);
		}
	}
}

public static class VariantFallbackExtensions
{
	extension(object target)
	{
		public string VariantTag
		{
			set => ThreeWayRecorder.Record(target, "object:" + value);
		}
	}
}

public class ConvertibleExtensionView : ContentView
{
	public static implicit operator ExtensionReceiver(ConvertibleExtensionView value) => new();
}

public class ExtensionReceiver;

public static class UserConversionExtensions
{
	extension(ExtensionReceiver target)
	{
		public string UserConversionTag
		{
			set => ThreeWayRecorder.Record(target, value);
		}
	}
}

public sealed class ExtensionValue(string value)
{
	public string Value { get; } = value;
}

public class ExtensionValueConverter : TypeConverter, IExtendedTypeConverter
{
	public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType) => sourceType == typeof(string);
	public override object ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value) => new ExtensionValue((string)value);

	public object ConvertFromInvariantString(string value, IServiceProvider serviceProvider)
	{
		var target = serviceProvider.GetService(typeof(IProvideValueTarget)) as IProvideValueTarget;
		if (target?.TargetObject is not Label
			|| target.TargetProperty is not PropertyInfo { PropertyType: var type }
			|| type != typeof(ExtensionValue))
			throw new InvalidOperationException("The converter did not receive the extension property.");
		return new ExtensionValue(value);
	}
}

public static class ConvertedExtensionProperties
{
	extension(Label label)
	{
		[TypeConverter(typeof(ExtensionValueConverter))]
		public ExtensionValue ConvertedTag
		{
			set => ThreeWayRecorder.Record(label, "converted:" + value.Value);
		}

		public ImageSource ResourceImage
		{
			set => ThreeWayRecorder.Record(label, ((FileImageSource)value).File);
		}
	}
}

[ContentProperty(nameof(Value))]
[AcceptEmptyServiceProvider]
public class ExtensionStringValueExtension : IMarkupExtension<string>
{
	public string Value { get; set; } = string.Empty;
	public string ProvideValue(IServiceProvider serviceProvider) => Value;
	object IMarkupExtension.ProvideValue(IServiceProvider serviceProvider) => ProvideValue(serviceProvider);
}

[AcceptEmptyServiceProvider]
public class ExtensionObjectValueExtension : IMarkupExtension
{
	public object ProvideValue(IServiceProvider serviceProvider) => "provided as object";
}

[RequireService([typeof(IProvideValueTarget)])]
public class ExtensionTargetPropertyExtension : IMarkupExtension<object>
{
	public object ProvideValue(IServiceProvider serviceProvider)
	{
		var target = (IProvideValueTarget)serviceProvider.GetService(typeof(IProvideValueTarget))!;
		return target.TargetProperty is PropertyInfo property ? $"{property.Name}:{property.PropertyType.Name}" : "missing target property";
	}
	object IMarkupExtension.ProvideValue(IServiceProvider serviceProvider) => ProvideValue(serviceProvider);
}
