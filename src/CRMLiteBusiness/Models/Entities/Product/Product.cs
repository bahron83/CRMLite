using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CRMLiteBusiness
{
    public enum ProductType { Goods, Service }

    public class Product
    {
        public int ProductID { get; set; }
        public string Name { get; set; }
        public string Sku { get; set; }  
        public double Price { get; set; }
        public int CategoryID { get; set; }
        public ProductType? ProductType { get; set; }
    }
}