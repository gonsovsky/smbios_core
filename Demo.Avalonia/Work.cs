using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text;
using Avalonia.Controls;
using Demo.Avalonia.Models;
using SmBios.Data;
using SmBios.Extractor;
using SmBios.Reader;

namespace Demo.Avalonia
{
    public static class Work
    {
        public static BiosData Bios;

        public static ObservableCollection<Prop> Props { get; set; } = new ObservableCollection<Prop>()
        {
            
        };

        public static ObservableCollection<Conference> League { get; set; }
        = new ObservableCollection<Conference>();

        public static TreeView Tr;

        public static DataGrid Dg;

        public static void C_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0)
                return; ;
            var q = e.AddedItems[0];
            if (q is Team)
            {
                Props.Clear();
                var tt = (Team) q;
                var ttt = tt.Table.GetType();
                PropertyInfo[] properties = ttt.GetProperties();
                foreach (PropertyInfo property in properties)
                {
                    string displayName = property.Name;
                    try
                    {
                        var attribute = property.GetCustomAttributes(typeof(DisplayNameAttribute), true)
                            .Cast<DisplayNameAttribute>().Single();
                        displayName = attribute.DisplayName;
                    }
                    catch (Exception exception)
                    {

                    }

                    try
                    {
                        var attribute = property.GetCustomAttributes(typeof(BrowsableAttribute), true)
                            .Cast<BrowsableAttribute>().Single();
                        if (!attribute.Browsable)
                            continue;
                    }
                    catch (Exception exception)
                    {

                    }

                    string value = property.GetValue(tt.Table).ToString();

                    var z = new Prop() {Property = displayName, Value = value};
                    Props.Add(z);
                }

                Dg.Items = null;
                Dg.Items = Props;
                
            }
        }

        public static IEnumerable<Conference> FillLeague()
        {
            using (var stream = SmBiosExtractor.OpenRead())
            {
                using (var reader = new SmBiosReader(stream, stream.Version))
                {
                    Bios = reader.ReadBios();
                }
            }

            PropertyInfo[] properties = typeof(BiosData).GetProperties();
            foreach (PropertyInfo property in properties)
            {

                var attribute = property.GetCustomAttributes(typeof(DisplayNameAttribute), true)
                    .Cast<DisplayNameAttribute>().Single();
                string displayName = attribute.DisplayName;

                var p = new Conference();
                p.ConferenceName = displayName;

                IList list = (IList)property.GetValue(Bios);

                foreach (Table z in list)
                {
                    var team = new Team();
                    team.Table = z;
                    team.TeamName = z.Name;
                    p.Teams.Add(team);
                }

                yield return p;
            }
        }
    }
}
