using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Xamarin.Essentials
{
    public static class ExperimentalFeatures
    {
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("ExperimentalFeatures.ShareFileRequest is obsolete as of version 1.3.0 and no longer required to use the feature.")]
        public const string ShareFileRequest = "ShareFileRequest_Experimental";

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("ExperimentalFeatures.OpenFileRequest is obsolete as of version 1.3.0 and no longer required to use the feature.")]
        public const string OpenFileRequest = "OpenFileRequest_Experimental";

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("ExperimentalFeatures.EmailAttachments is obsolete as of version 1.3.0 and no longer required to use the feature.")]
        public const string EmailAttachments = "EmailAttachments_Experimental";

        static List<string> enabledFeatures;

        public static void Enable(params string[] featureNames)
        {
            if (enabledFeatures == null)
                enabledFeatures = new List<string>();

            foreach (var featureName in featureNames)
            {
                if (!enabledFeatures.Contains(featureName))
                    enabledFeatures.Add(featureName);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        internal static void VerifyEnabled(
            string featureName,
            [CallerMemberName] string memberName = "")
        {
            if (enabledFeatures == null || !enabledFeatures.Contains(featureName))
            {
                var call = string.IsNullOrEmpty(memberName) ? string.Empty : $"('{memberName}'), which is ";

                var errorMessage = $"The class, property, or method you are attempting to use {call}an experimental feature;"
                                    + " to use it, you must opt-in by calling "
                                    + $"ExperimentalFeatures.Enable(\"{featureName}\") before using this feature.";

                throw new InvalidOperationException(errorMessage);
            }
        }
    }
}
