using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace App2
{
    public partial class Page1 : ContentPage
    {
        public Page1()
        {
            InitializeComponent();

            ObservableCollection<CheckListDetails> objlist = new ObservableCollection<CheckListDetails>();
            objlist.Add(new CheckListDetails() { CallOut = "AAAAA", Action = "Action", ChecklistStatus = CheckListStatus.Completed });
            objlist.Add(new CheckListDetails() { CallOut = "AAAAA", Action = "Action", ChecklistStatus = CheckListStatus.Completed });
            objlist.Add(new CheckListDetails() { CallOut = "AAAAA", Action = "Action", ChecklistStatus = CheckListStatus.Completed });
            objlist.Add(new CheckListDetails() { CallOut = "AAAAA", Action = "Action", ChecklistStatus = CheckListStatus.Completed });
            objlist.Add(new CheckListDetails() { CallOut = "AAAAA", Action = "ActionAction", ChecklistStatus = CheckListStatus.Completed });
            objlist.Add(new CheckListDetails() { CallOut = "AAAAA", Action = "Action", ChecklistStatus = CheckListStatus.Completed });
            objlist.Add(new CheckListDetails() { CallOut = "AAAAA", Action = "ActionAction", ChecklistStatus = CheckListStatus.Completed });
            objlist.Add(new CheckListDetails() { CallOut = "AAAAA", Action = "Action", ChecklistStatus = CheckListStatus.Default });
            objlist.Add(new CheckListDetails() { CallOut = "AAAAA", Action = "Action", ChecklistStatus = CheckListStatus.Default });
            objlist.Add(new CheckListDetails() { CallOut = "AAAAA", Action = "Action", ChecklistStatus = CheckListStatus.Default });
            objlist.Add(new CheckListDetails() { CallOut = "AAAAA", Action = "Action", ChecklistStatus = CheckListStatus.OverRide });
            objlist.Add(new CheckListDetails() { CallOut = "AAAAA", Action = "Action", ChecklistStatus = CheckListStatus.OverRide });
            objlist.Add(new CheckListDetails() { CallOut = "AAAAA", Action = "Action", ChecklistStatus = CheckListStatus.OverRide });
            objlist.Add(new CheckListDetails() { CallOut = "AAAAA", Action = "Action", ChecklistStatus = CheckListStatus.OverRide });
            objlist.Add(new CheckListDetails() { CallOut = "AAAAA", Action = "Action", ChecklistStatus = CheckListStatus.Default });
            objlist.Add(new CheckListDetails() { CallOut = "AAAAA", Action = "Action", ChecklistStatus = CheckListStatus.Default });
            objlist.Add(new CheckListDetails() { CallOut = "AAAAA", Action = "Action", ChecklistStatus = CheckListStatus.Default });
            objlist.Add(new CheckListDetails() { CallOut = "AAAAA", Action = "Action", ChecklistStatus = CheckListStatus.Default });
            objlist.Add(new CheckListDetails() { CallOut = "AAAAA", Action = "Action", ChecklistStatus = CheckListStatus.Default });
            objlist.Add(new CheckListDetails() { CallOut = "AAAAA", Action = "Action", ChecklistStatus = CheckListStatus.Default });
            objlist.Add(new CheckListDetails() { CallOut = "AAAAA", Action = "Action", ChecklistStatus = CheckListStatus.Default });
            objlist.Add(new CheckListDetails() { CallOut = "AAAAA", Action = "Action", ChecklistStatus = CheckListStatus.Default });
            objlist.Add(new CheckListDetails() { CallOut = "AAAAA", Action = "ActionAction", ChecklistStatus = CheckListStatus.Default });
            objlist.Add(new CheckListDetails() { CallOut = "AAAAA", Action = "Action", ChecklistStatus = CheckListStatus.Default });
            objlist.Add(new CheckListDetails() { CallOut = "AAAAA", Action = "Action", ChecklistStatus = CheckListStatus.Default });
            objlist.Add(new CheckListDetails() { CallOut = "AAAAA", Action = "Action", ChecklistStatus = CheckListStatus.Default });
            objlist.Add(new CheckListDetails() { CallOut = "AAAAA", Action = "Action", ChecklistStatus = CheckListStatus.Default });
            objlist.Add(new CheckListDetails() { CallOut = "AAAAA", Action = "Action", ChecklistStatus = CheckListStatus.Default });
            objlist.Add(new CheckListDetails() { CallOut = "AAAAA", Action = "Action", ChecklistStatus = CheckListStatus.Default });
            objlist.Add(new CheckListDetails() { CallOut = "AAAAA", Action = "Action", ChecklistStatus = CheckListStatus.Default });
            objlist.Add(new CheckListDetails() { CallOut = "AAAAA", Action = "Action", ChecklistStatus = CheckListStatus.Default });
            objlist.Add(new CheckListDetails() { CallOut = "AAAAA", Action = "ActionAction", ChecklistStatus = CheckListStatus.Default });
            objlist.Add(new CheckListDetails() { CallOut = "AAAAA", Action = "", ChecklistStatus = CheckListStatus.Default });
            objlist.Add(new CheckListDetails() { CallOut = "", Action = "", ChecklistStatus = CheckListStatus.Default });
            objlist.Add(new CheckListDetails() { CallOut = "", Action = "", ChecklistStatus = CheckListStatus.Default });
            objlist.Add(new CheckListDetails() { CallOut = "", Action = "", ChecklistStatus = CheckListStatus.Completed });
            objlist.Add(new CheckListDetails() { CallOut = "", Action = "", ChecklistStatus = CheckListStatus.Default });
            objlist.Add(new CheckListDetails() { CallOut = "", Action = "", ChecklistStatus = CheckListStatus.Default });
            objlist.Add(new CheckListDetails() { CallOut = "", Action = "", ChecklistStatus = CheckListStatus.Default });
            objlist.Add(new CheckListDetails() { CallOut = "", Action = "", ChecklistStatus = CheckListStatus.Default });
            objlist.Add(new CheckListDetails() { CallOut = "", Action = "", ChecklistStatus = CheckListStatus.Default });
            objlist.Add(new CheckListDetails() { CallOut = "", Action = "", ChecklistStatus = CheckListStatus.Default });
            objlist.Add(new CheckListDetails() { CallOut = "", Action = "", ChecklistStatus = CheckListStatus.Default });
            objlist.Add(new CheckListDetails() { CallOut = "", Action = "", ChecklistStatus = CheckListStatus.Default });
            objlist.Add(new CheckListDetails() { CallOut = "", Action = "", ChecklistStatus = CheckListStatus.Default });
            objlist.Add(new CheckListDetails() { CallOut = "", Action = "", ChecklistStatus = CheckListStatus.Default });
            objlist.Add(new CheckListDetails() { CallOut = "", Action = "", ChecklistStatus = CheckListStatus.Default });
            objlist.Add(new CheckListDetails() { CallOut = "", Action = "", ChecklistStatus = CheckListStatus.Default });
            objlist.Add(new CheckListDetails() { CallOut = "", Action = "", ChecklistStatus = CheckListStatus.Default });
            objlist.Add(new CheckListDetails() { CallOut = "AAAAA", Action = "Action", ChecklistStatus = CheckListStatus.OverRide });
            objlist.Add(new CheckListDetails() { CallOut = "AAAAA", Action = "Action", ChecklistStatus = CheckListStatus.Default });
            objlist.Add(new CheckListDetails() { CallOut = "AAAAA", Action = "Action", ChecklistStatus = CheckListStatus.Default });
            objlist.Add(new CheckListDetails() { CallOut = "AAAAA", Action = "Action", ChecklistStatus = CheckListStatus.Default });
            objlist.Add(new CheckListDetails() { CallOut = "AAAAA", Action = "Action", ChecklistStatus = CheckListStatus.Default });
            objlist.Add(new CheckListDetails() { CallOut = "AAAAA", Action = "Action", ChecklistStatus = CheckListStatus.Default });
            objlist.Add(new CheckListDetails() { CallOut = "AAAAA", Action = "Action", ChecklistStatus = CheckListStatus.Default });
            objlist.Add(new CheckListDetails() { CallOut = "AAAAA", Action = "Action", ChecklistStatus = CheckListStatus.Default });
            objlist.Add(new CheckListDetails() { CallOut = "AAAAA", Action = "Action", ChecklistStatus = CheckListStatus.Default });
            objlist.Add(new CheckListDetails() { CallOut = "AAAAA", Action = "ActionAction", ChecklistStatus = CheckListStatus.Default });
            objlist.Add(new CheckListDetails() { CallOut = "AAAAA", Action = "Action", ChecklistStatus = CheckListStatus.Default });
            objlist.Add(new CheckListDetails() { CallOut = "AAAAA", Action = "Action", ChecklistStatus = CheckListStatus.Default });
            objlist.Add(new CheckListDetails() { CallOut = "AAAAA", Action = "Action", ChecklistStatus = CheckListStatus.Default });
            objlist.Add(new CheckListDetails() { CallOut = "AAAAA", Action = "Action", ChecklistStatus = CheckListStatus.Default });
            objlist.Add(new CheckListDetails() { CallOut = "AAAAA", Action = "Action", ChecklistStatus = CheckListStatus.Default });
            objlist.Add(new CheckListDetails() { CallOut = "AAAAA", Action = "Action", ChecklistStatus = CheckListStatus.Default });
            objlist.Add(new CheckListDetails() { CallOut = "AAAAA", Action = "Action", ChecklistStatus = CheckListStatus.Default });
            objlist.Add(new CheckListDetails() { CallOut = "AAAAA", Action = "Action", ChecklistStatus = CheckListStatus.Default });
            objlist.Add(new CheckListDetails() { CallOut = "AAAAA", Action = "ActionAction", ChecklistStatus = CheckListStatus.Default });
            objlist.Add(new CheckListDetails() { CallOut = "AAAAA", Action = "", ChecklistStatus = CheckListStatus.Default });
            objlist.Add(new CheckListDetails() { CallOut = "AAAAA", Action = "Action", ChecklistStatus = CheckListStatus.OverRide });
            objlist.Add(new CheckListDetails() { CallOut = "AAAAA", Action = "Action", ChecklistStatus = CheckListStatus.Default });
            objlist.Add(new CheckListDetails() { CallOut = "AAAAA", Action = "Action", ChecklistStatus = CheckListStatus.Default });
            objlist.Add(new CheckListDetails() { CallOut = "AAAAA", Action = "Action", ChecklistStatus = CheckListStatus.Default });
            objlist.Add(new CheckListDetails() { CallOut = "AAAAA", Action = "Action", ChecklistStatus = CheckListStatus.Default });
            objlist.Add(new CheckListDetails() { CallOut = "AAAAA", Action = "Action", ChecklistStatus = CheckListStatus.Default });
            objlist.Add(new CheckListDetails() { CallOut = "AAAAA", Action = "Action", ChecklistStatus = CheckListStatus.Default });
            objlist.Add(new CheckListDetails() { CallOut = "AAAAA", Action = "Action", ChecklistStatus = CheckListStatus.Default });
            objlist.Add(new CheckListDetails() { CallOut = "AAAAA", Action = "Action", ChecklistStatus = CheckListStatus.Default });
            objlist.Add(new CheckListDetails() { CallOut = "AAAAA", Action = "ActionAction", ChecklistStatus = CheckListStatus.Default });
            objlist.Add(new CheckListDetails() { CallOut = "AAAAA", Action = "Action", ChecklistStatus = CheckListStatus.Default });
            objlist.Add(new CheckListDetails() { CallOut = "AAAAA", Action = "Action", ChecklistStatus = CheckListStatus.Default });
            objlist.Add(new CheckListDetails() { CallOut = "AAAAA", Action = "Action", ChecklistStatus = CheckListStatus.Default });
            objlist.Add(new CheckListDetails() { CallOut = "AAAAA", Action = "Action", ChecklistStatus = CheckListStatus.Default });
            objlist.Add(new CheckListDetails() { CallOut = "AAAAA", Action = "Action", ChecklistStatus = CheckListStatus.Default });
            objlist.Add(new CheckListDetails() { CallOut = "AAAAA", Action = "Action", ChecklistStatus = CheckListStatus.Default });
            objlist.Add(new CheckListDetails() { CallOut = "AAAAA", Action = "Action", ChecklistStatus = CheckListStatus.Default });
            objlist.Add(new CheckListDetails() { CallOut = "AAAAA", Action = "Action", ChecklistStatus = CheckListStatus.Default });
            objlist.Add(new CheckListDetails() { CallOut = "AAAAA", Action = "ActionAction", ChecklistStatus = CheckListStatus.Default });
            objlist.Add(new CheckListDetails() { CallOut = "AAAAA", Action = "", ChecklistStatus = CheckListStatus.Default });


            itemListView.ItemsSource = objlist;
        }
    }

    public class CheckListDetails
    {
	    string _callout;
	    string _action;
	    string _currentStatusD;

	    CheckListStatus _checklistStatus = CheckListStatus.Default;

        public CheckListDetails()
        {
            _currentStatusD = string.Empty;
        }

        public CheckListStatus ChecklistStatus
        {
            get { return _checklistStatus; }
            set
            {
                if (value == CheckListStatus.Default)
                {

                }

                SetProperty(ref _checklistStatus, value);
            }
        }

        public string CurrentStatusD
        {
            get { return _currentStatusD; }
            set { SetProperty(ref _currentStatusD, value); }
        }

        public string Action
        {
            get { return _action; }
            set { SetProperty(ref _action, value); }
        }

        public string CallOut
        {
            get { return _callout; }
            set { SetProperty(ref _callout, value); }
        }


        public event PropertyChangedEventHandler PropertyChanged;

        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(storage, value))
            {
                return false;
            }

            storage = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;

            if (handler != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }

    public enum CheckListStatus
    {
        Default,
        Completed,
        Pending,
        OverRide
    }
}
