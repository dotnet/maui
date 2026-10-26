using System.Collections.Generic;
using Android.Content;
using Android.Views;

namespace Microsoft.Maui.ExternalBackend
{
	/// <summary>
	/// Android variant of the external platform's native view type.
	/// </summary>
	/// <remarks>
	/// On a platform target framework, <see cref="Handlers.ViewHandler{TVirtualView, TPlatformView}"/>
	/// constrains <c>TPlatformView</c> to the platform's own base view type — here
	/// <c>Android.Views.View</c>. A real external backend's views satisfy that too. What makes it
	/// an <em>external</em> backend is that its concrete types are its own, and therefore are not the
	/// types the aliased handler interfaces are pinned to (<c>AppCompatTextView</c> for
	/// <c>ILabelHandler</c> on Android). That mismatch is what produces <c>CS0738</c>.
	/// </remarks>
	public class FakeNativeView : View
	{
		public FakeNativeView(Context context)
			: base(context)
		{
		}

		public List<FakeNativeView> FakeChildren { get; } = new List<FakeNativeView>();

		public double FakeOpacity { get; set; } = 1;
	}

	/// <summary>Native label type for the fake external platform.</summary>
	public class FakeNativeLabel : FakeNativeView
	{
		public FakeNativeLabel(Context context)
			: base(context)
		{
		}

		public string? FakeText { get; set; }
	}

	/// <summary>Native content-host type for the fake external platform.</summary>
	public class FakeNativeContentView : FakeNativeView
	{
		public FakeNativeContentView(Context context)
			: base(context)
		{
		}

		public FakeNativeView? FakeContent { get; set; }
	}

	/// <summary>Native layout container type for the fake external platform.</summary>
	public class FakeNativeLayoutView : FakeNativeView
	{
		public FakeNativeLayoutView(Context context)
			: base(context)
		{
		}
	}
}
