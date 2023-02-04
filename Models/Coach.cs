using ProiectAnaliza.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProiectAnaliza.Models
{
    public class Coach
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage ="Numele este obligatoriu")]
        [StringLength(100,ErrorMessage = "Numele nu poate avea mai mult de 100 de caractere")]
        public string Name { get; set; }
        [Required]
        public string Content { get; set; }
        public int FinalRating { get; set; }
        public int Age { get; set; }
        public string UserId { get; set; }
        public string FileName { get; set; }
        public virtual ApplicationUser User { get; set; }
        public bool Approved { get; set; }
        [NotMapped]
        public HttpPostedFileBase Image { get; set; }
        public int CategoryId { get; set; }
        public virtual Category Category { get; set; }
        public virtual ICollection<Review> Reviews { get; set; }

        [Display(Name = "Programari noi disabled")]
        public Boolean DisableNewAppointments  { get; set; }

        public virtual List<Appointment> Appointments { get; set; }

    }
}