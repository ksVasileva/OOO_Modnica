using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows;
using OOO_Modnica.Model;

namespace OOO_Modnica.Classes
{
    public class ProductExtended
    {
        //Для связи с товарами из БД
        public Model.Product Product { get; set; }

        //*********Дополнительные свойства к классу Product

        //Правильное фото с учетом заглушки и пути к фото
        public string PhotoCorrect
        {
            get
            {
                string result = "";
                if (this.Product.ProductPhoto != null)	//Фото есть в БД
                {
                    result = Environment.CurrentDirectory + "/Images/" + this.Product.ProductPhoto;
                }
                else					//Фото нет - заглушка
                {
                    result = "/Resources/picture.png";
                }
                return result;
            }
        }

        /// Расчетное свойство цены товара со скидкой 
        private decimal productCostWithDiscount;		//Поле для set
        public decimal ProductCostWithDiscount
        {
            get
            {
                decimal discountAmount = (decimal)Product.ProductCost *
                                                        (decimal)(Product.ProductDiscountCurrent / 100.0);
                decimal priceWithDiscount = (decimal)Product.ProductCost - discountAmount;
                return priceWithDiscount;
            }
            set
            {
                productCostWithDiscount = value;
            }
        }

        /// Свойство видимости скидки и цены для случая наличия скидки у товара
        public Visibility ProductCostDiscountVisibility
        {
            get
            {
                Visibility result = Visibility.Collapsed;
                if (Product.ProductDiscountCurrent > 0)
                {
                    result = Visibility.Visible;
                }
                return result;
            }
        }

        /// Свойство для окраски товаров с повышенной скидкой
        public SolidColorBrush ColorFocused
        {
            get
            {
                SolidColorBrush result = new SolidColorBrush();
                result.Color = Color.FromArgb(255, 255, 255, 255);
                if (Product.ProductDiscountCurrent > 15)
                {
                    result.Color = Color.FromArgb(0xFF, 0xCC, 0x66, 0x00);
                }
                return result;
            }
        }
    }

    }
