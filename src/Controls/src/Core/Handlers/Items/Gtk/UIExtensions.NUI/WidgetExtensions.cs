using System;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Platform.Gtk;
using Microsoft.Maui.Platform;

namespace Gtk.UIExtensions.NUI;

public static class WidgetExtensions
{
	public static float PositionX(this Widget it) => it?.Allocation.X ?? 0;

	public static float PositionY(this Widget it) => it?.Allocation.Y ?? 0;

	public static float SizeWidth(this Widget it) => it?.AllocatedWidth ?? 0;

	public static float SizeHeight(this Widget it) => it?.AllocatedHeight ?? 0;

	public static Size Size(this Widget it) => it is not { } ? Microsoft.Maui.Graphics.Size.Zero : new(it.Allocation.Width, it.Allocation.Height);

	public static void UpdateBounds(this Widget it, Rect bounds)
	{
		if (it is ViewHolder holder)
		{
			holder.UpdateBounds(bounds);

			return;
		}

		it.Arrange(bounds);
	}

	public static Rect GetBounds(this Widget it)
	{
		if (it is ViewHolder holder)
		{
			return holder.Bounds;
		}

		return it.Allocation.ToRect();
	}

	public static void UpdateSize(this Widget nativeView, Size size)
	{
		var widthRequest = Microsoft.Maui.WidgetExtensions.Request(size.Width);
		var doResize = false;

		if (widthRequest != -1 && widthRequest != nativeView.AllocatedWidth)
		{
			nativeView.WidthRequest = widthRequest;
			doResize = true;
		}

		var heightRequest = Microsoft.Maui.WidgetExtensions.Request(size.Height);

		if (heightRequest != -1 && heightRequest != nativeView.AllocatedHeight)
		{
			nativeView.HeightRequest = heightRequest;
			doResize = true;
		}

		if (doResize)
			nativeView.QueueResize();
	}

	[MissingMapper]
	public static void RaiseToTop(this Widget it) { }

	public static void Add(this Widget it, Widget child)
	{
		if (it is CollectionView cw)
		{
			cw.Add(child);

			return;
		}

		if (it is Container c)
		{
			c.Add(child);
		}
	}

	public static class DeviceInfo
	{
		public static double ScalingFactor = 1;
	}

	public static int ToScaledPixel(this double it) => (int)Math.Round(it * DeviceInfo.ScalingFactor);

	public static double ToScaledDP(this int pixel)
	{
		if (pixel == int.MaxValue)
			return double.PositiveInfinity;

		return pixel / DeviceInfo.ScalingFactor;
	}

	public static double ToScaledDP(this double pixel)
	{
		return pixel / DeviceInfo.ScalingFactor;
	}

	public static Size ToPixel(this Size it) => it;

	public static void WidthSpecification(this Widget it, LayoutParamPolicies p)
	{
		if (p == LayoutParamPolicies.MatchParent)
		{
			it.Vexpand = true;
		}
	}

	public static void HeightSpecification(this Widget it, LayoutParamPolicies p)
	{
		if (p == LayoutParamPolicies.MatchParent)
		{
			it.Hexpand = true;
		}
	}

	public static void WidthResizePolicy(this Widget it, ResizePolicyType p)
	{
		if (p == ResizePolicyType.FillToParent)
		{
			it.Hexpand = true;
		}
	}

	public static void HeightResizePolicy(this Widget it, ResizePolicyType p)
	{
		if (p == ResizePolicyType.FillToParent)
		{
			it.Vexpand = true;
		}
	}
}

/// <summary>
/// Layout policies to decide the size of View when the View is laid out in its parent View.
/// </summary>
/// <example>
/// <code>
/// // matchParentView matches its size to its parent size.
/// matchParentView.WidthSpecification (LayoutParamPolicies.MatchParent);
/// matchParentView.HeightSpecification (LayoutParamPolicies.MatchParent);
///
/// // wrapContentView wraps its children with their desired size.
/// wrapContentView.WidthSpecification (LayoutParamPolicies.WrapContent);
/// wrapContentView.HeightSpecification (LayoutParamPolicies.WrapContent);
/// </code>
/// </example>
/// <since_tizen> 9 </since_tizen>
public enum LayoutParamPolicies
{
	/// <summary>
	/// Constant which indicates child size should match parent size.
	/// </summary>
	/// <since_tizen> 9 </since_tizen>
	MatchParent,

	/// <summary>
	/// Constant which indicates parent should take the smallest size possible to wrap its children with their desired size.
	/// </summary>
	WrapContent
}

public enum ResizePolicyType
{
	FillToParent
}