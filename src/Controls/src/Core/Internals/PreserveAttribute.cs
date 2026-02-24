#nullable disable
using System;
using System.ComponentModel;

namespace Microsoft.Maui.Controls.Internals
{
	/// <summary>For internal use by platform renderers.</summary>
	[AttributeUsage(AttributeTargets.All)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public sealed class PreserveAttribute : Attribute
	{
		/// <summary>For internal use by platform renderers.</summary>
		public bool AllMembers;
		/// <summary>For internal use by platform renderers.</summary>
		public bool Conditional;

		/// <summary>Creates a PreserveAttribute with the specified options.</summary>
		public PreserveAttribute(bool allMembers, bool conditional)
		{
			AllMembers = allMembers;
			Conditional = conditional;
		}

		/// <summary>Creates a default PreserveAttribute.</summary>
		public PreserveAttribute()
		{
		}
	}
}