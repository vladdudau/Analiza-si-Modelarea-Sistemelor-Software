using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace ProiectAnaliza.Models
{
    public class Appointment : IComparable <Appointment>
    {
        [Key]
        public int AppointmentId { get; set; }

        [ForeignKey("Customer")]
        public string UserId { get; set; }

        [Required]
        public int CoachId { get; set; }

        [Required]
        [Display(Name ="Data programarii")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [AppointmentDateValidation(ErrorMessage = "Iti faci o programare pentru trecut?")]
        public DateTime Date { get; set; }
         DateTime get()
    {
        return DateTime.Now;
    }


[DataType(DataType.Time)]
        public DateTime Time { get; set; }

        public string TimeBlockHelper { get; set; }

        public virtual Coach Coach { get; set; }

        public virtual Customer Customer { get; set; }

        public int CompareTo(Appointment other)
        {
            return this.Date.Date.Add(this.Time.TimeOfDay).CompareTo(other.Date.Date.Add(other.Time.TimeOfDay));
        }

    }
}
