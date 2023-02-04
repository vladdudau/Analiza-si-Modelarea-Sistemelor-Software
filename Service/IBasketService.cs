using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace ProiectAnaliza.Models
{
    public interface IBasketService
    {
        void AddToBasket(HttpContextBase httpContext, string productId);
        List<BasketItemView> GetBasketItems(HttpContextBase httpContext);
        void RemoveFromBasket(HttpContextBase httpContext, string itemId);
        void RemoveFromBasketProduct(HttpContextBase httpContext, string productId);
        BasketSummaryView GetBasketSummary(HttpContextBase httpContext);
        void ClearBasket(HttpContextBase httpContext);
    }
}
