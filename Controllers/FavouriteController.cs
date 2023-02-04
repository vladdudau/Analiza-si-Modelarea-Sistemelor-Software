using ProiectAnaliza.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ProiectAnaliza.Controllers
{
    [Authorize(Roles ="User")]
    public class FavouriteController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        // GET: Favourite
        [Authorize(Roles = "User")]

        public ActionResult Index()
        {
            var coaches = new List<Coach>();
            HttpCookie FavouriteCookie = Request.Cookies["Favourite"];

            if(FavouriteCookie != null && !string.IsNullOrEmpty(FavouriteCookie.Value))
            {
                string[] splitString = FavouriteCookie.Value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string item in splitString)
                {
                    Boolean ok = false;
                    int coachId = int.Parse(item);
                    var coach = db.Coaches.Find(coachId);
                    CoachController.CalculateCoachFinalRating(coach);
                    foreach (var antrenor in coaches)
                        if (coachId == antrenor.Id)
                            ok = true;
                    if (ok == false)
                        coaches.Add(coach);
                }
            }
            ViewBag.Coaches = coaches;
            ViewBag.UserId = User.Identity.GetUserId();
            return View();
        }

        [Authorize(Roles="User")]
        public ActionResult New(int id)
        {
            if (!User.IsInRole("Admin") && !User.IsInRole("Editor") && !User.IsInRole("User"))
                return Redirect("/Account/Register");
            else
            {
                HttpCookie FavouriteCookie = new HttpCookie("Favourite");
                if (Request.Cookies["Favourite"] != null)
                    FavouriteCookie = Request.Cookies["Favourite"];

                if (string.IsNullOrEmpty(FavouriteCookie.Value))
                    FavouriteCookie.Value = id.ToString();
                else
                    FavouriteCookie.Value += "," + id.ToString();
                FavouriteCookie.Expires = DateTime.Now.AddHours(24);

                Response.Cookies.Add(FavouriteCookie);

                return Redirect("/Favourite/Index");

            }
        }

        public ActionResult Empty()
        {
            if (Request.Cookies["Favourite"] != null)
            {
                HttpCookie FavouriteCookie = Request.Cookies["Favourite"];
                FavouriteCookie.Value = "";
                FavouriteCookie.Expires = DateTime.Now;
                Response.Cookies.Add(FavouriteCookie);
            }
            return Redirect("/Favourite/Index");
        }

    }
}