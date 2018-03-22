using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;

namespace Microsoft.Caboodle
{
    public static partial class DataTransfer
    {
        public static Task RequestAsync(ShareTextRequest request)
        {
            var dataTransferManager = DataTransferManager.GetForCurrentView();

            dataTransferManager.DataRequested += ShareTextHandler;

            DataTransferManager.ShowShareUI();

            void ShareTextHandler(DataTransferManager sender, DataRequestedEventArgs e)
            {
                dataTransferManager.DataRequested -= ShareTextHandler;

                var newRequest = e.Request;

                newRequest.Data.Properties.Title = request.Title ?? AppInfo.Name;

                if (!string.IsNullOrWhiteSpace(request.Text))
                {
                    newRequest.Data.SetText(request.Text);
                }

                if (!string.IsNullOrWhiteSpace(request.Uri))
                {
                    newRequest.Data.SetWebLink(new Uri(request.Uri));
                }
            }

            return Task.CompletedTask;
        }
    }
}
