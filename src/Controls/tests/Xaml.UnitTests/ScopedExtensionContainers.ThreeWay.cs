// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
#nullable enable
using System.Collections.Generic;
using Microsoft.Maui.Controls;

namespace Microsoft.Maui.Controls.Xaml.UnitTests.ScopedExtensions;

// Three containers in scope declare the same name. Two of the receivers are incomparable (IView and
// BindableObject), the third one (Label) is more specific than both, so it is the one that has to win
// whichever order the containers happen to be enumerated in. ThreeWayLast puts the Label candidate on the
// container that sorts last, ThreeWayFirst on the one that sorts first.
public static class ThreeWayRecorder
{
	static readonly Dictionary<object, string> _values = new();

	public static void Record(object target, string value) => _values[target] = value;

	public static string Get(object target) => _values.TryGetValue(target, out var value) ? value : string.Empty;
}

public static class ThreeWayAExtensions
{
	extension(IView view)
	{
		public string ThreeWayLast
		{
			set => ThreeWayRecorder.Record(view, "A(IView):" + value);
		}
	}

	extension(Label label)
	{
		public string ThreeWayFirst
		{
			set => ThreeWayRecorder.Record(label, "A(Label):" + value);
		}
	}
}

public static class ThreeWayBExtensions
{
	extension(BindableObject bindable)
	{
		public string ThreeWayLast
		{
			set => ThreeWayRecorder.Record(bindable, "B(BindableObject):" + value);
		}

		public string ThreeWayFirst
		{
			set => ThreeWayRecorder.Record(bindable, "B(BindableObject):" + value);
		}
	}
}

public static class ThreeWayCExtensions
{
	extension(Label label)
	{
		public string ThreeWayLast
		{
			set => ThreeWayRecorder.Record(label, "C(Label):" + value);
		}
	}

	extension(IView view)
	{
		public string ThreeWayFirst
		{
			set => ThreeWayRecorder.Record(view, "C(IView):" + value);
		}
	}
}
