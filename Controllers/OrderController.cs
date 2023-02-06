using ProiectAnaliza.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ProiectAnaliza.Controllers
{
    public class OrderController : Controller
    {
        IOrderService orderService;

        public OrderController(IOrderService OrderService)
        {
            orderService = OrderService;
        }

        // GET: Order
        public ActionResult Index()
        {
            List<Order> orders = orderService.GetOrderList();

            return View(orders);
        }

        public ActionResult UpdateOrder(string Id)
        {
            ViewBag.StatusList = new List<String>
            {
                "Comanda creata",
                "Comanda confirmata",
                "Comanda livrata",
                "Comanda completa"
            };
            Order order = orderService.GetOrder(Id);
            return View(order);
        }

        [HttpPost]
        public ActionResult UpdateOrder(Order updatedOrder, string Id)
        {
            Order order = orderService.GetOrder(Id);
            order.OrderStatus = updatedOrder.OrderStatus;
            orderService.UpdateOrder(order);

            return RedirectToAction("Index");
        }

        public ActionResult Detalii()
        {
            string id = User.Identity.GetUserId();
            
            var Db = new ApplicationDbContext();
            Customer customer = Db.Customers.Find(id);
            
            var model = new EditCustomerViewModel(customer);
            return View(model);
        }
    }
}