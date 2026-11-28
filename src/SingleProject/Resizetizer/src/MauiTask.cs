using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Microsoft.Maui.Resizetizer
{
	/// <summary>
	/// Base class for synchronous Resizetizer tasks that can run in multithreaded MSBuild.
	/// Concrete tasks must also be marked with <see cref="MSBuildMultiThreadableTaskAttribute"/>.
	/// </summary>
	public abstract class MauiTask : Task, IMultiThreadableTask
	{
		/// <summary>
		/// The per-project environment provided by MSBuild. Use it to resolve relative paths and
		/// read environment variables instead of the process-wide working directory and environment.
		/// </summary>
		public TaskEnvironment TaskEnvironment { get; set; } = TaskEnvironment.Fallback;
	}
}
