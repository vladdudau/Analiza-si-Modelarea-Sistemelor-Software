using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ProiectAnaliza.Models
{
    public class AppointmentDateValidation : ValidationAttribute
    {
        public override bool IsValid (object value)
        {
            DateTime date = (DateTime)value;
            DateTime today = DateTime.Now;
            if (date.Date.Date.CompareTo(today.Date) >= 0)
                return true;
            else
                return false;
        }
    }
}