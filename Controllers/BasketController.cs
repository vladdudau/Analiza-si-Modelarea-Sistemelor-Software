using ProiectAnaliza.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ProiectAnaliza.Controllers
{
    [Authorize(Roles ="User,Editor")]
    public class BasketController : Controller
    {
        // GET: Basket

        IBasketService basketService;
        IOrderService orderService;

        public BasketController(IBasketService BasketService, IOrderService OrderService)
        {
            basketService = BasketService;
            orderService = OrderService;
        }


        public ActionResult Index()
        {
            var model = basketService.GetBasketItems(this.HttpContext);
            return View(model);
        }

        public ActionResult AddToBasket(string Id)
        {
            basketService.AddToBasket(this.HttpContext, Id);
            return RedirectToAction("Index");
        }

        public ActionResult RemoveFromBasket(string Id)
        {
            basketService.RemoveFromBasket(this.HttpContext, Id);
            return RedirectToAction("Index");
        }

        public ActionResult RemoveFromBasketProduct(string Id)
        {
            basketService.RemoveFromBasketProduct(this.HttpContext, Id);
            return RedirectToAction("Index");
        }

        public PartialViewResult BasketSummary()
        {
            var basketSummary = basketService.GetBasketSummary(this.HttpContext);

            return PartialView(basketSummary);
        }

        public ActionResult Checkout()
        {
            Order order = new Order();
            order.UserId = User.Identity.GetUserId();
            return View(order);
        }
        
        [HttpPost]
        public ActionResult Checkout(Order order)
        {
            var basketItems = basketService.GetBasketItems(this.HttpContext);
            order.OrderStatus = "Comanda creata";
            TempData["message"] = "Ai trimis o cerere catre admin pentru a plasa o comanda!";
            order.UserId = User.Identity.GetUserId();
            orderService.CreateOrder(order, basketItems);
            basketService.ClearBasket(this.HttpContext);
            return RedirectToAction("ThankYou", new { OrderId = order.Id });
        }

        public ActionResult ThankYou(string OrderId)
        {
            ViewBag.OrderId = OrderId;
            return View();
        }

        
    }
}