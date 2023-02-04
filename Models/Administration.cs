using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ProiectAnaliza.Models
{
    public class Administration
    {
        [Required]
        public int ID { get; set; }

        [Required]
        public String Name { get; set; }

        [Required]
        public string Value { get; set; }
    }
}