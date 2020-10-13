using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Demo.Avalonia.Models
{
    public class Prop
    {
        [DisplayName("Свойство")]
        public string Property { get; set; }

        [DisplayName("Значение")]
        public string Value { get; set; }
    }
}
