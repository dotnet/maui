// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
#nullable enable
using Microsoft.Maui.Controls;

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
}

// Instance members win over extension properties of the same name, even when they cannot be assigned. An
// explicitly implemented interface property is not an instance member of the class and hides nothing.
public class ScopedPrecedenceView : ContentView, IScopedExplicitTag
{
	public string PlainClr { get; set; } = string.Empty;

	public string ReadOnlyClr { get; private set; } = string.Empty;

	string IScopedExplicitTag.ExplicitTag => string.Empty;
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
	}
}
