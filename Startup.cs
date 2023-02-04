using System;
using Hangfire;
using ProiectAnaliza.Controllers;
using ProiectAnaliza.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(ProiectAnaliza.Startup))]
namespace ProiectAnaliza
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            GlobalConfiguration.Configuration
               .UseSqlServerStorage("DefaultConnection");
            ConfigureAuth(app);
            CreateAdminUserAndApplicationRoles();

            app.UseHangfireDashboard();
            BackgroundJob.Enqueue(() => Console.WriteLine("Fire-and-forget!"));
            AppointmentController obj = new AppointmentController();



            RecurringJob.AddOrUpdate(() => obj.SendEmailAppointmentNotification(), "0 11 * * *");






            app.UseHangfireServer();
        }
        private void CreateAdminUserAndApplicationRoles()
        {
            ApplicationDbContext context = new ApplicationDbContext();
            var roleManager = new RoleManager<IdentityRole>(new
            RoleStore<IdentityRole>(context));
            var UserManager = new UserManager<ApplicationUser>(new
            UserStore<ApplicationUser>(context));
            // Se adauga rolurile aplicatiei
            if (!roleManager.RoleExists("Admin"))
            {
                var role = new IdentityRole();
                role.Name = "Admin";
                roleManager.Create(role);
                var user = new ApplicationUser();
                user.UserName = "neverloseyourhope04@gmail.com";
                user.Email = "neverloseyourhope04@gmail.com";
                var adminCreated = UserManager.Create(user, "****");
                if (adminCreated.Succeeded)
                {
                    UserManager.AddToRole(user.Id, "Admin");
                }
            }
            if (!roleManager.RoleExists("Editor"))
            {
                var role = new IdentityRole();
                role.Name = "Editor";
                roleManager.Create(role);
            }
            if (!roleManager.RoleExists("User"))
            {
                var role = new IdentityRole();
                role.Name = "User";
                roleManager.Create(role);
            }
        }
    }
}
