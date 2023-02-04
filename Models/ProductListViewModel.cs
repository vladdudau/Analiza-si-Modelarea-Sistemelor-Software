using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ProiectAnaliza.Models
{
    public class ProductListViewModel
    {
        public IEnumerable<Product> Products { get; set; }
        public IEnumerable<ProductCategory> ProductCategories;
    }
}