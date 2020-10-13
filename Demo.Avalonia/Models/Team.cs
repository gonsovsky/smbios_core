using System.Collections.ObjectModel;
using SmBios.Data;

namespace Demo.Avalonia.Models
{
    public class Team
    {
        public string TeamName { get; set; }

        public Table Table { get; set; }

        public ObservableCollection<Person> Roster { get; }

        public Team()
        {
            Roster = new ObservableCollection<Person>();
        }
    }
}