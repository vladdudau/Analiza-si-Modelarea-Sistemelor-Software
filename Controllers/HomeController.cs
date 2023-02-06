using Hangfire;
using ProiectAnaliza.Models;
using PagedList;
using Postal;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;

namespace ProiectAnaliza.Controllers
{
    public class HomeController : Controller
    {
        IRepository<Product> context;
        IRepository<ProductCategory> productCategories;

        private ApplicationDbContext db = new ApplicationDbContext();
        public HomeController(IRepository<Product> productContext, IRepository<ProductCategory> productCategoryContext)
        {
            context = productContext;
            productCategories = productCategoryContext;
        }

       public ActionResult FirstPage()
        {
            return View();
        }

        public ActionResult Index(int? page,String Category = null )
        {
            var pageNumber = page ?? 1;
            List<Product> products = context.Collection().ToList();
            var filtru = Request.Params["filter"];
            var sort = Request.Params["sort"];
            if (sort == null)
                sort = "";
            if (filtru == null)
                filtru = "";
            if (Category == null)
                context.Collection().ToList();
            else
            {
                products = context.Collection().Where(p => p.Category == Category).ToList();
            }
            if (sort == "price-asc")
                products = products.OrderBy(c => c.Price).ToList();
            if (sort == "price-desc")
                products = products.OrderByDescending(c => c.Price).ToList();
            if (sort == "name-asc")
                products = products.OrderBy(c => c.Name).ToList();
            if (sort == "name-desc")
                products = products.OrderByDescending(c => c.Name).ToList();
            ViewBag.Sort = sort;
            System.Diagnostics.Debug.WriteLine(sort);
            var categorii = from category in db.ProductCategories
                             orderby category.Category
                             select category;
            ViewBag.Categories = categorii;
            List<Product> filtrare = new List<Product>().ToList();
           
            foreach (var filter in products)
                if (filter.Category == filtru)
                    filtrare.Add(filter);
            if (filtru == "All" || filtru == "")
                filtrare = products;
            
            return View(filtrare.ToPagedList(pageNumber,4));
        }

        public ActionResult Details(string Id)
        {
            Product product = context.Find(Id);
            if (product == null)
            {
                return HttpNotFound();
            }
            else
                return View(product);
        }

        public ActionResult About()
        {
            return View();
        }

        public ActionResult Search(string param, int? page)
        {
            var pageNumber = page ?? 1;
            string word1 = param.ToLower();

            var productsRelated = new List<Product>().ToList();

            var products = db.Products.ToList();
            foreach (var produs in products)
            {
                string nume = produs.Name.ToLower();
                string word = word1.ToLower();
                double dist_nume = Levenstein(nume, word);
                if (dist_nume <= 0.5)
                {
                    productsRelated.Add(produs);
                }
            }
            var categorii = from category in db.ProductCategories
                            orderby category.Category
                            select category;
            ViewBag.Categories = categorii;
            ViewBag.SearchWord = param;
            var filtru = Request.Params["filter"];
            if (filtru == null)
                filtru = "";
            List<Product> filtrare = new List<Product>().ToList();
            if (filtru == "All" || filtru == "")
                filtrare = db.Products.ToList();
            return View(productsRelated.ToPagedList(pageNumber, 8));
        }



        public double Levenstein(string cuv1, string cuv2)
        {
            int[,] matrice = new int[cuv1.Length + 1, cuv2.Length + 1];

            for (int i = 0; i <= cuv2.Length; i++)
                matrice[0, i] = i;

            for (int i = 0; i <= cuv1.Length; i++)
                matrice[i, 0] = i;

            for (int i = 1; i <= cuv1.Length; i++)
                for (int j = 1; j <= cuv2.Length; j++)
                {
                    if (cuv1[i - 1] == cuv2[j - 1])
                        matrice[i, j] = matrice[i - 1, j - 1];
                    else
                        matrice[i, j] = Math.Min(matrice[i - 1, j], Math.Min(matrice[i, j - 1], matrice[i - 1, j - 1])) + 1;
                }

            return (double)(matrice[cuv1.Length, cuv2.Length]) / (double)(Math.Max(cuv1.Length, cuv2.Length));
        }

    }
}