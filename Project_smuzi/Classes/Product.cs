﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
#pragma warning disable CS0067
namespace Project_smuzi.Classes
{
    public class Product : Element, INotifyPropertyChanged
    {
        public List<Element> Elements { get; set; }
        public List<Product> Products { get; set; }
        public Product(string ident) : base(ident)
        {
            Elements = new List<Element>();
            Products = new List<Product>();
        }
    }
}
