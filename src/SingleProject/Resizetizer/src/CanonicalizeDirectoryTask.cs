using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Microsoft.Maui.Resizetizer
{
	/// <summary>
	/// Resolves an existing directory before a later filesystem snapshot so both operations stay anchored
	/// to the same physical root even when the lexical path is subsequently retargeted.
	/// </summary>
	public class CanonicalizeDirectoryTask : Task
	{
		[Required]
		public string Directory { get; set; }

		[Output]
		public ITaskItem CanonicalDirectory { get; set; }

		internal bool AllowLinkResolution { get; set; } = true;

		public override bool Execute()
		{
			CanonicalDirectory = null;

			var canonical = new PathCanonicalizer(AllowLinkResolution).CanonicalizeExistingDirectory(Directory);
			if (canonical is null)
			{
				Log.LogMessage(MessageImportance.Low, $"The directory '{Directory}' could not be resolved.");
				return true;
			}

			CanonicalDirectory = new TaskItem(canonical);
			return true;
		}
	}
}
