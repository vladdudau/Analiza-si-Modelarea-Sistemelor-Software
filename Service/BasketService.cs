﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ProiectAnaliza.Models
{
    public class BasketService : IBasketService
    {
        IRepository<Product> productContext;
        IRepository<Basket> basketContext;

        public const string BasketSessionName = "DudFiT";

        public BasketService(IRepository<Product> productContext, IRepository<Basket> basketContext)
        {
            this.basketContext = basketContext;
            this.productContext = productContext;
        }

        private Basket GetBasket(HttpContextBase httpContext, bool createIfNull)
        {
            HttpCookie cookie = httpContext.Request.Cookies.Get(BasketSessionName);

            Basket basket = new Basket();

            if(cookie != null)
            {
                string basketId = cookie.Value;
                if(!string.IsNullOrEmpty(basketId))
                {
                    basket = basketContext.Find(basketId);
                }
                else
                {
                    if(createIfNull)
                    {
                        basket = CreateNewBasket(httpContext);
                    }
                }
            }
            else
            {
                if (createIfNull)
                {
                    basket = CreateNewBasket(httpContext);
                }
            }
            return basket;
        }

        private Basket CreateNewBasket(HttpContextBase httpContext)
        {
            Basket basket = new Basket();
            basketContext.Insert(basket);
            basketContext.Commit();

            HttpCookie cookie = new HttpCookie(BasketSessionName);
            cookie.Value = basket.Id;
            cookie.Expires = DateTime.Now.AddDays(1);

            httpContext.Response.Cookies.Add(cookie);

            return basket;
        }


        public void AddToBasket(HttpContextBase httpContext, string productId)
        {
            Basket basket = GetBasket(httpContext, true);
            BasketItem item = basket.BasketItems.FirstOrDefault(i => i.ProductId == productId);

            if (item == null)
            {
                item = new BasketItem()
                {
                    BasketId = basket.Id,
                    ProductId = productId,
                    Quantity = 1
                };
                basket.BasketItems.Add(item);
            }
            else
            {
                item.Quantity = item.Quantity + 1;

            }

            basketContext.Commit();
        }


        public void RemoveFromBasketProduct(HttpContextBase httpContext, string productId)
        {
            Basket basket = GetBasket(httpContext, true);
            BasketItem item = basket.BasketItems.FirstOrDefault(i => i.ProductId == productId);

            if (item.Quantity == 1)
            {
                basket.BasketItems.Remove(item);
                
            }
            else
                item.Quantity = item.Quantity - 1;
            basketContext.Commit();
        }

        public void RemoveFromBasket(HttpContextBase httpContext, string itemId)
        {
            Basket basket = GetBasket(httpContext, true);
            BasketItem item = basket.BasketItems.FirstOrDefault(i => i.Id == itemId);

            if(item !=null)
            {
                basket.BasketItems.Remove(item);
                basketContext.Commit();
            }
        }

        public List<BasketItemView> GetBasketItems(HttpContextBase httpContext)
        {
            Basket basket = GetBasket(httpContext, false);

            if(basket!=null)
            {
                var results = (from b in basket.BasketItems
                              join p in productContext.Collection() on b.ProductId equals p.Id
                              select new BasketItemView() { Id = b.Id, Quantity = b.Quantity, ProductName = p.Name, Image = p.Image, Price = p.Price, ProductId =p.Id }
                              ).ToList();
                return results;
            }
            else
            {
                return new List<BasketItemView>();
            }
        }

        public BasketSummaryView GetBasketSummary(HttpContextBase httpContext)
        {
            Basket basket = GetBasket(httpContext, false);
            BasketSummaryView model = new BasketSummaryView(0, 0);
            if(basket!=null)
            {
                int? basketCount = (from item in basket.BasketItems
                                    select item.Quantity).Sum();
                decimal? basketTotal = (from item in basket.BasketItems
                                        join p in productContext.Collection() on item.ProductId equals p.Id
                                        select item.Quantity * p.Price).Sum();
                model.BasketCount = basketCount ?? 0;
                model.BasketTotal = basketTotal ?? 0;

                return model;
            }
            else
            {
                return model;
            }
        }


        public void ClearBasket(HttpContextBase httpContext)
        {
            Basket basket = GetBasket(httpContext, false);
            basket.BasketItems.Clear();
            basketContext.Commit();
        }

    }
}