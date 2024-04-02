using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using Microsoft.Maui;


namespace MauiApp3
{
    public class MainPageViewModel : INotifyPropertyChanged
    {
        private bool _isPersonVisible = false;

        public bool IsPersonVisible
        {
            get
            {
                return _isPersonVisible;
            }

            set
            {
                if (_isPersonVisible != value)
                {
                    _isPersonVisible = value;
                    OnPropertyChanged("IsPersonVisible");
                }
            }
        }

        public ICommand PopulatePersonCommand => new Command(PopulatePerson);
        public event PropertyChangedEventHandler PropertyChanged;

        ObservableCollection<Person> _people = new ObservableCollection<Person>();

        public ObservableCollection<Person> People 
        { 
            get 
                { return _people; }
            set
            {
                if (_people != value)
                {
                    _people = value;
                    OnPropertyChanged("People");
                }
            }
        }

        public MainPageViewModel()
        {

        }


       
        private void PopulatePerson(object parameter)
        {
            IsPersonVisible = true;

            try
            {
                for (int j = 0; j < 10; j++)
                {
                    People.Add(new Person { Name = "Person " + j });
                }
            }
            catch (Exception ex) 
            { 
                Console.WriteLine(ex.ToString());
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChangedEventHandler changed = PropertyChanged;
            if (changed == null)
            {
                return;
            }

            changed.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


    }
}
