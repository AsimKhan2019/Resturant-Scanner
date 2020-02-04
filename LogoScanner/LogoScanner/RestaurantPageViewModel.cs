using System;
using System.ComponentModel;
using System.Collections.ObjectModel;
using Xamarin.Forms;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Threading.Tasks;
using LogoScanner.Helpers;
using Newtonsoft.Json.Linq;

namespace LogoScanner
{
    public class RestaurantPageViewModel : INotifyPropertyChanged
    {
        const int RefreshDuration = 2;
        int itemNumber = 1;
        bool isRefreshing;

        public bool IsRefreshing
        {
            get { return isRefreshing; }
            set
            {
                isRefreshing = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<AvailableTime> Items { get; private set; }

        public ICommand RefreshCommand => new Command(async () => await RefreshItemsAsync());

        public RestaurantPageViewModel()
        {
            Items = new ObservableCollection<AvailableTime>();
            AddItems();
        }

        void AddItems()
        {
            for (int i = 0; i < 50; i++)
            {
                Items.Add(new AvailableTime
                {
                    Date = "",
                    Time = "",
                    RestaurantAreas = "",
                    Available = "AVAILABLE NOW",
                    Colour = "Red"
                });
            }
        }

        async Task RefreshItemsAsync()
        {
            IsRefreshing = true;
            await Task.Delay(TimeSpan.FromSeconds(RefreshDuration));
            AddItems();
            IsRefreshing = false;
        }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
