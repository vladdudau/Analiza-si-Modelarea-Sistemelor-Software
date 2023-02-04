using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ProiectAnaliza.Models
{
    public class Category
    {
        [Key]
        public int CategoryId { get; set; }
        [Required(ErrorMessage = "Descrierea produsului este obligatorie")]
        public string CategoryName { get; set; }

        public virtual ICollection<Coach> Coaches { get; set; }
        
    }
}