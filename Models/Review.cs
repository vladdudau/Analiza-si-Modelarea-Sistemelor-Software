using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ProiectAnaliza.Models
{
    public class Review
    {
        [Key]
        public int ReviewId { get; set; }

        [Required(ErrorMessage = "Campul nu poate fi necompletat")]
        public string Content { get; set; }
        
        public DateTime Date { get; set; }

        public int CoachId { get; set; }

        public int Rating { get; set; }

        public string UserId { get; set; }

        public virtual ApplicationUser User { get; set; }

        public virtual Coach Coach { get; set; }
    }
}