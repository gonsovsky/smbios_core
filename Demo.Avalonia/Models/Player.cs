using System.Collections.ObjectModel;

namespace Demo.Avalonia.Models
{
    public class Player : Person
    {
        public ObservableCollection<string> Positions { get; }

        public Player()
        {
            Positions = new ObservableCollection<string>();
        }
    }
}