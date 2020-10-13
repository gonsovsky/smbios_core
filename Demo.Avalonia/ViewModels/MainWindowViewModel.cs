using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;
using Demo.Avalonia.Models;
using SmBios.Data;
using SmBios.Extractor;
using SmBios.Reader;

namespace Demo.Avalonia.ViewModels
{

    public class MainWindowViewModel : ViewModelBase
    {

        public ObservableCollection<Conference> League  => Work.League;


        public  ObservableCollection<Prop> Props => Work.Props;


        public MainWindowViewModel()
        {
            Work.League = new ObservableCollection<Conference>(Work.FillLeague());
        }
    }
}
