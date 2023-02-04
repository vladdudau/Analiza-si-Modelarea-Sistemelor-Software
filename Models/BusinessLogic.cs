using Itenso.TimePeriod;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ProiectAnaliza.Models
{
    public class BusinessLogic
    {
       
        public static bool IsInWorkingHours(DateTime start, DateTime end)
        {
            ApplicationDbContext db = new ApplicationDbContext();
          
            if (start.DayOfWeek == DayOfWeek.Saturday || start.DayOfWeek == DayOfWeek.Sunday)
            {
                return false;
            }

            TimeRange workingHours = new TimeRange(TimeTrim.Hour(start, int.Parse(db.Administrations.Find(2).Value)), TimeTrim.Hour(start, int.Parse(db.Administrations.Find(3).Value)));
            return workingHours.HasInside(new TimeRange(start, end));
        }

        public static bool IsInWorkingHours(TimeBlock block)
        {
            ApplicationDbContext db = new ApplicationDbContext();
            
            if (block.Start.DayOfWeek == DayOfWeek.Saturday || block.Start.DayOfWeek == DayOfWeek.Sunday)
            {
                return false;
            }
            TimeRange workingHours = new TimeRange(TimeTrim.Hour(block.Start.Date, int.Parse(db.Administrations.Find(2).Value)), TimeTrim.Hour(block.Start.Date, int.Parse(db.Administrations.Find(3).Value)));
            return workingHours.HasInside(block);
        }

        public static string ValidateNoAppoinmentClash(Appointment appointment)
        {
            ApplicationDbContext db = new ApplicationDbContext();
            var appointments = from a in db.Appointments.Where(x => x.CoachId == appointment.CoachId)
                               select a;
            foreach (var item in appointments)
            {
                if (item.Time.ToShortTimeString() == appointment.Time.ToShortTimeString() && item.Date.ToShortDateString() == appointment.Date.ToShortDateString())
                {
                    string errorMessage = String.Format(
                        "{0} are deja o programare la {1} si {2}.",
                        item.Coach.Name,
                        item.Date.ToShortDateString(),
                        item.Time.ToShortTimeString());
                    return errorMessage;
                }
            }
            return String.Empty;

        }

        public static List<SelectListItem> AvailableAppointments(int CoachID, DateTime date)
        {
            ApplicationDbContext db = new ApplicationDbContext();
            int A, S, E;
            A = int.Parse(db.Administrations.Find(1).Value);
            S = int.Parse(db.Administrations.Find(2).Value);
            E = int.Parse(db.Administrations.Find(3).Value);
            TimeBlock timeBlock = new MyTimeBlockExtention
                (
                new DateTime(date.Year, date.Month, date.Day, S, 0, 0),
                new TimeSpan(0, A, 0)
                );
            List<SelectListItem> ItemsList = new List<SelectListItem>();
            while (timeBlock.Start.CompareTo(DateTime.Now) <= 0) // Programari pt trecut!!
            {
                timeBlock.Move(new TimeSpan(0, A, 0));
                if (!IsInWorkingHours(timeBlock))
                    break;
            }
            var appointments = from a in db.Appointments.Where(x => x.CoachId == CoachID)
                               select a;
            bool overlaps = false;
            while (IsInWorkingHours(timeBlock))
            {
                foreach (var appointment in appointments)
                {
                    TimeBlock BookedTimeBlock = new MyTimeBlockExtention
                (
                (appointment.Date.Date.Add(appointment.Time.TimeOfDay)),
                new TimeSpan(0, A, 0)
                );
                    if (BookedTimeBlock.OverlapsWith(timeBlock))
                    {
                        overlaps = true;
                    }
                }
                if (!overlaps)
                {
                    ItemsList.Add(new SelectListItem() { Text = timeBlock.ToString(), Value = timeBlock.Start.ToString("HH:mm") });

                }
                overlaps = false;
                timeBlock.Move(new TimeSpan(0, A, 0));
            }
            if (ItemsList.Count != 0)
            {
                return ItemsList;
            }
            ItemsList.Add(new SelectListItem() { Text = "Nicio programare disponibila", Value = "DONT" });
            return ItemsList;
        }
    }
}