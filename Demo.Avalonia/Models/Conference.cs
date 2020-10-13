using System.Collections.ObjectModel;

namespace Demo.Avalonia.Models
{
    public class Conference
    {
        public string ConferenceName { get; set; }

        public ObservableCollection<Team> Teams { get; }

        public Conference()
        {
            Teams = new ObservableCollection<Team>();
        }
        
    }
}