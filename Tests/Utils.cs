using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Essentials;

namespace Tests
{
    public static class TestUtils
    {
        public static void EnableExperimentalFeatures()
        {
            ExperimentalFeatures.Enable(
                ExperimentalFeatures.EmailAttachments,
                ExperimentalFeatures.ShareFileRequest,
                ExperimentalFeatures.OpenFileRequest);
        }
    }
}
