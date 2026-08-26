using System.Collections.Generic;

namespace Microsoft.Maui.ExternalBackend
{
	/// <summary>
	/// Stands in for the native view type of a platform .NET MAUI knows nothing about.
	/// </summary>
	/// <remarks>
	/// Deliberately does not derive from, or reference, any .NET MAUI type. This is the whole point of
	/// the test assembly: a backend's native types are its own.
	/// </remarks>
	public class FakeNativeView
	{
		public List<FakeNativeView> Children { get; } = new List<FakeNativeView>();

		public double Opacity { get; set; } = 1;
	}

	/// <summary>Native label type for the fake external platform.</summary>
	public class FakeNativeLabel : FakeNativeView
	{
		public string? Text { get; set; }
	}

	/// <summary>Native content-host type for the fake external platform.</summary>
	public class FakeNativeContentView : FakeNativeView
	{
		public FakeNativeView? Content { get; set; }
	}

	/// <summary>Native layout container type for the fake external platform.</summary>
	public class FakeNativeLayoutView : FakeNativeView
	{
	}

	/// <summary>Native window type for the fake external platform. Not even a view.</summary>
	public class FakeNativeWindow
	{
		public string? Title { get; set; }

		public FakeNativeView? Content { get; set; }
	}
}
