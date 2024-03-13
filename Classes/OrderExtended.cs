using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OOO_Modnica.Classes
{
    /// Класс, построенный на основании класса Order модели БД,
    /// но с дополнительными расчётными свойствами
    public class OrderExtended
    {
        //Свойства класса
        public Model.Order Order { get; set; }		//Связь с моделью

        public double TotalSumma { get; set; }	//Сумма всего заказа
        public double TotalDiscount { get; set; }   //Суммарная скидка


        public double TotalDiscountPercent  //Свойство класса – суммарная скидка в %
        {
            get
            {
                return TotalDiscount * 100 / TotalSumma;
            }
        }

        /// Метод расчета суммы заказа по номеру заказа
        public decimal CalcTotalSummma(List<Model.OrderProduct> productsInOrder)
        {
            decimal total = 0;
            //Перебор всех заказанных товаров
            foreach (var item in productsInOrder)
            {
                if (item.OrderID == Order.OrderID)	//Выделение только товаров текущего заказа
                {
                    total += item.Product.ProductCost * item.Count;
                }
            }
            return total;
        }

        /// Метод расчета суммарной скидки заказа по номеру заказа
        public decimal CalcTotalDiscount(List<Model.OrderProduct> productsInOrder)
        {
            decimal total = 0;
            foreach (var item in productsInOrder)
            {
                if (item.OrderID == Order.OrderID)
                {
                    //Стоимость товара с учетом скидки
                    decimal discountAmount = item.Product.ProductCost *
                                                               (decimal)(item.Product.ProductDiscountCurrent / 100.0);
                    total += discountAmount * item.Count;
                }
            }
            return total;
        }
    }
}
