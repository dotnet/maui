namespace Microsoft.Maui.Controls
{
	/// <summary>Enumeration specifying whether <see cref="M:Microsoft.Maui.Controls.DependencyService.Get`{T}(Microsoft.Maui.Controls.DependencyFetchTarget)"/> should return a reference to a global or new instance.</summary>
	public enum DependencyFetchTarget
	{
		/// <summary>Return a global instance.</summary>
		GlobalInstance,
		/// <summary>Return a new instance.</summary>
		NewInstance
	}
}