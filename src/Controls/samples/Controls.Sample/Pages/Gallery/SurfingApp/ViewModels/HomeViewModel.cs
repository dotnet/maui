using System.Collections.ObjectModel;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Gallery.SurfingApp
{
    public class HomeViewModel : BindableObject
    {
        ObservableCollection<User> _users;
        ObservableCollection<Post> _posts;

        public HomeViewModel()
        {
            LoadData();
        }

        public ObservableCollection<User> Users
        {
            get { return _users; }
            set
            {
                _users = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<Post> Posts
        {
            get { return _posts; }
            set
            {
                _posts = value;
                OnPropertyChanged();
            }
        }

        void LoadData()
        {
            Users = new ObservableCollection<User>(UserService.Instance.GetUsers());
            Posts = new ObservableCollection<Post>(PostService.Instance.GetPosts());
        }
    }
}