using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Markup.Xaml;
using Demo.Avalonia.Models;

namespace Demo.Avalonia.Views
{
    public class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            Work.Tr = this.FindControl<TreeView>("TreeView1");
            Work.Dg = this.FindControl<DataGrid>("DataGrid1");
            Work.Tr.SelectionChanged += Work.C_SelectionChanged;
        }

    }
}