using System.Collections.Generic;
using System.ComponentModel;

namespace Microsoft.Maui.Controls.Internals;

/// <summary>
/// Provides cached <see cref="PropertyChangedEventArgs"/> and <see cref="PropertyChangingEventArgs"/> instances
/// for commonly-used property names to reduce allocations in hot paths.
/// </summary>
/// <remarks>
/// For known property names, a single shared instance is returned from an internal cache.
/// For unknown property names, a new instance is created and returned (and is not cached).
/// Null or empty property names (which indicate "all properties changed") always return a new uncached instance.
/// </remarks>
static class PropertyChangesEventArgsFactory
{
	static readonly Dictionary<string, PropertyChangedEventArgs> propertyChangedCache = new()
	{
		[nameof(IView.AnchorX)] = new(nameof(IView.AnchorX)),
		[nameof(IView.AnchorY)] = new(nameof(IView.AnchorY)),
		[nameof(IView.AutomationId)] = new(nameof(IView.AutomationId)),
		[nameof(IView.Background)] = new(nameof(IView.Background)),
		[nameof(IView.Clip)] = new(nameof(IView.Clip)),
		[nameof(IView.DesiredSize)] = new(nameof(IView.DesiredSize)),
		[nameof(IView.FlowDirection)] = new(nameof(IView.FlowDirection)),
		[nameof(IView.Frame)] = new(nameof(IView.Frame)),
		[nameof(IView.Handler)] = new(nameof(IView.Handler)),
		[nameof(IView.Height)] = new(nameof(IView.Height)),
		[nameof(IView.HorizontalLayoutAlignment)] = new(nameof(IView.HorizontalLayoutAlignment)),
		[nameof(IView.InputTransparent)] = new(nameof(IView.InputTransparent)),
		[nameof(IView.IsEnabled)] = new(nameof(IView.IsEnabled)),
		[nameof(IView.IsFocused)] = new(nameof(IView.IsFocused)),
		[nameof(IView.Margin)] = new(nameof(IView.Margin)),
		[nameof(IView.MaximumHeight)] = new(nameof(IView.MaximumHeight)),
		[nameof(IView.MaximumWidth)] = new(nameof(IView.MaximumWidth)),
		[nameof(IView.MinimumHeight)] = new(nameof(IView.MinimumHeight)),
		[nameof(IView.MinimumWidth)] = new(nameof(IView.MinimumWidth)),
		[nameof(IView.Opacity)] = new(nameof(IView.Opacity)),
		[nameof(IView.Parent)] = new(nameof(IView.Parent)),
		[nameof(IView.Rotation)] = new(nameof(IView.Rotation)),
		[nameof(IView.RotationX)] = new(nameof(IView.RotationX)),
		[nameof(IView.RotationY)] = new(nameof(IView.RotationY)),
		[nameof(IView.Scale)] = new(nameof(IView.Scale)),
		[nameof(IView.ScaleX)] = new(nameof(IView.ScaleX)),
		[nameof(IView.ScaleY)] = new(nameof(IView.ScaleY)),
		[nameof(IView.Semantics)] = new(nameof(IView.Semantics)),
		[nameof(IView.Shadow)] = new(nameof(IView.Shadow)),
		[nameof(IView.TranslationX)] = new(nameof(IView.TranslationX)),
		[nameof(IView.TranslationY)] = new(nameof(IView.TranslationY)),
		[nameof(IView.VerticalLayoutAlignment)] = new(nameof(IView.VerticalLayoutAlignment)),
		[nameof(IView.Visibility)] = new(nameof(IView.Visibility)),
		[nameof(IView.Width)] = new(nameof(IView.Width)),
		[nameof(IView.ZIndex)] = new(nameof(IView.ZIndex)),
		["HeightRequest"] = new("HeightRequest"),
		["WidthRequest"] = new("WidthRequest"),
	};

	static readonly Dictionary<string, PropertyChangingEventArgs> propertyChangingCache = new()
	{
		[nameof(IView.AnchorX)] = new(nameof(IView.AnchorX)),
		[nameof(IView.AnchorY)] = new(nameof(IView.AnchorY)),
		[nameof(IView.AutomationId)] = new(nameof(IView.AutomationId)),
		[nameof(IView.Background)] = new(nameof(IView.Background)),
		[nameof(IView.Clip)] = new(nameof(IView.Clip)),
		[nameof(IView.DesiredSize)] = new(nameof(IView.DesiredSize)),
		[nameof(IView.FlowDirection)] = new(nameof(IView.FlowDirection)),
		[nameof(IView.Frame)] = new(nameof(IView.Frame)),
		[nameof(IView.Handler)] = new(nameof(IView.Handler)),
		[nameof(IView.Height)] = new(nameof(IView.Height)),
		[nameof(IView.HorizontalLayoutAlignment)] = new(nameof(IView.HorizontalLayoutAlignment)),
		[nameof(IView.InputTransparent)] = new(nameof(IView.InputTransparent)),
		[nameof(IView.IsEnabled)] = new(nameof(IView.IsEnabled)),
		[nameof(IView.IsFocused)] = new(nameof(IView.IsFocused)),
		[nameof(IView.Margin)] = new(nameof(IView.Margin)),
		[nameof(IView.MaximumHeight)] = new(nameof(IView.MaximumHeight)),
		[nameof(IView.MaximumWidth)] = new(nameof(IView.MaximumWidth)),
		[nameof(IView.MinimumHeight)] = new(nameof(IView.MinimumHeight)),
		[nameof(IView.MinimumWidth)] = new(nameof(IView.MinimumWidth)),
		[nameof(IView.Opacity)] = new(nameof(IView.Opacity)),
		[nameof(IView.Parent)] = new(nameof(IView.Parent)),
		[nameof(IView.Rotation)] = new(nameof(IView.Rotation)),
		[nameof(IView.RotationX)] = new(nameof(IView.RotationX)),
		[nameof(IView.RotationY)] = new(nameof(IView.RotationY)),
		[nameof(IView.Scale)] = new(nameof(IView.Scale)),
		[nameof(IView.ScaleX)] = new(nameof(IView.ScaleX)),
		[nameof(IView.ScaleY)] = new(nameof(IView.ScaleY)),
		[nameof(IView.Semantics)] = new(nameof(IView.Semantics)),
		[nameof(IView.Shadow)] = new(nameof(IView.Shadow)),
		[nameof(IView.TranslationX)] = new(nameof(IView.TranslationX)),
		[nameof(IView.TranslationY)] = new(nameof(IView.TranslationY)),
		[nameof(IView.VerticalLayoutAlignment)] = new(nameof(IView.VerticalLayoutAlignment)),
		[nameof(IView.Visibility)] = new(nameof(IView.Visibility)),
		[nameof(IView.Width)] = new(nameof(IView.Width)),
		[nameof(IView.ZIndex)] = new(nameof(IView.ZIndex)),
		["HeightRequest"] = new("HeightRequest"),
		["WidthRequest"] = new("WidthRequest"),
	};

	/// <summary>
	/// Returns a cached <see cref="PropertyChangedEventArgs"/> instance for the specified property name when available;
	/// otherwise creates and returns a new instance.
	/// </summary>
	/// <param name="name">The property name associated with the change notification. Null or empty indicates all properties changed.</param>
	/// <returns>
	/// A cached <see cref="PropertyChangedEventArgs"/> for commonly-used property names; otherwise a new instance.
	/// Null or empty names always return a new uncached instance.
	/// </returns>
	internal static PropertyChangedEventArgs GetOrCreatePropertyChangedEventArgs(string name)
	{
		if (string.IsNullOrEmpty(name))
		{
			return new(name);
		}

		if (propertyChangedCache.TryGetValue(name, out var args))
		{
			return args;
		}

		return new(name);
	}

	/// <summary>
	/// Returns a cached <see cref="PropertyChangingEventArgs"/> instance for the specified property name when available;
	/// otherwise creates and returns a new instance.
	/// </summary>
	/// <param name="name">The property name associated with the changing notification. Null or empty indicates all properties changing.</param>
	/// <returns>
	/// A cached <see cref="PropertyChangingEventArgs"/> for commonly-used property names; otherwise a new instance.
	/// Null or empty names always return a new uncached instance.
	/// </returns>
	internal static PropertyChangingEventArgs GetOrCreatePropertyChangingEventArgs(string name)
	{
		if (string.IsNullOrEmpty(name))
		{
			return new(name);
		}

		if (propertyChangingCache.TryGetValue(name, out var args))
		{
			return args;
		}

		return new(name);
	}
}
