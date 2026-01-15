using ObjCRuntime;

namespace Microsoft.Maui.Platform
{
	/// <summary>
	/// Result codes for MauiCALayerAutosizeToSuperLayerBehavior
	/// </summary>
	[Native]
	enum MauiCALayerAutosizeToSuperLayerResult : long
	{
		Success = 0,
		MissingSuperlayer = 1
	}
}