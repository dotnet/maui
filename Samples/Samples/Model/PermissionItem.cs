using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Samples.Model
{
    public class PermissionItem : INotifyPropertyChanged
    {
        public PermissionItem(string title, Permissions.BasePermission permission)
        {
            Title = title;
            Permission = permission;
            Status = PermissionStatus.Unknown;
        }

        public string Title { get; set; }

        public PermissionStatus Status { get; set; }

        public Permissions.BasePermission Permission { get; set; }

        public ICommand CheckStatusCommand =>
            new Command(async () =>
            {
                try
                {
                    Status = await Permission.CheckStatusAsync();
                    NotifyPropertyChanged(nameof(Status));
                }
                catch (Exception ex)
                {
                    MessagingCenter.Send<PermissionItem, Exception>(this, nameof(PermissionException), ex);
                }
            });

        public ICommand RequestCommand =>
            new Command(async () =>
            {
                try
                {
                    Status = await Permission.RequestAsync();
                    NotifyPropertyChanged(nameof(Status));
                }
                catch (Exception ex)
                {
                    MessagingCenter.Send<PermissionItem, Exception>(this, nameof(PermissionException), ex);
                }
            });

        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string name)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
