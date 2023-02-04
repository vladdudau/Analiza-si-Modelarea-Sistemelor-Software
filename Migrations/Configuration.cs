namespace ProiectAnaliza.Migrations
{
    using ProiectAnaliza.Models;
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<ProiectAnaliza.Models.ApplicationDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
            AutomaticMigrationDataLossAllowed = true;
            ContextKey = "ProiectAnaliza.Models.ApplicationDbContext";
        }

        protected override void Seed(ProiectAnaliza.Models.ApplicationDbContext context)
        {
            var admin = new List<Administration>
            {
                 new Administration { Name = "Durata programarii in minute [Default:90]",   Value = "90"},
                 new Administration { Name = "Programul de lucru incepe la ora: [Default:8]",   Value = "8"},
                 new Administration { Name = "Programul de lucru se termina la ora [Default:17]",   Value = "17"},
            };

            admin.ForEach(s => context.Administrations.AddOrUpdate(p => p.Name, s));
            context.SaveChanges();
        }
    }
}
