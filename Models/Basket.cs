using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ProiectAnaliza.Models
{
    public class Basket:BaseEntity
    {
        public virtual ICollection<BasketItem> BasketItems { get; set; }

        public Basket()
        {
            this.BasketItems = new List<BasketItem>();
        }
    }
}