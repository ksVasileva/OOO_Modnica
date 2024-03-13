using OOO_Modnica.Classes;
using OOO_Modnica.Model;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace OOO_Modnica.View
{
    /// <summary>
    /// Логика взаимодействия для Catalog.xaml
    /// </summary>
    public partial class Catalog : Window
    {

        List<Classes.ProductInOrder> listProductInOrder = new List<Classes.ProductInOrder>();
        User user;

        public Catalog()
        {
            InitializeComponent();
        }

        public Catalog(User user)
        {
            this.user = user;
            InitializeComponent();
            userName.Text = user.UserFullName;
            if(user.UserRole == 2)
            {
                miAddInOrder.Header = "Редактировать";
            }
        }

        private void buttonNavigate_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
            
        }

        public void Window_loaded(object sender, RoutedEventArgs e)
        {
           


        }

        private void butAddProduct_Click(object sender, RoutedEventArgs e)
        {
            //Вызов конструктора без параметров
            View.EditCatalog editCatalog = new View.EditCatalog();
            this.Hide();
            editCatalog.ShowDialog();
            this.ShowDialog();
        }

        /// Удалить товар
        private void butDeleteProduct_Click(object sender, RoutedEventArgs e)
        {
            Classes.ProductExtended productSelected = null;
            if (listBoxProducts.SelectedItem == null)	//Проверка, что есть выбранный товар в списке
            {
                MessageBox.Show("Выберите удаляемый товар");
                return;
            }
            //Выбранный товар в каталоге
            productSelected = listBoxProducts.SelectedItem as Classes.ProductExtended;
            if (MessageBox.Show("Вы действительно хотите удалить этот товар?",
                        "Подтверждение удаления", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                //Выбранный товар
                Model.Product product = productSelected.Product;
                //Поиск его среди заказанных товаров для правильного удаления
                Model.OrderProduct orderProduct = Helper.DBHall.OrderProduct.
                                                  FirstOrDefault(pr => pr.ProductArticle == product.ProductArticle);
                if (orderProduct == null)				//Товар еще не заказывали
                {
                    Helper.DBHall.Product.Remove(product); 	//Можно удалять
                    try
                    {
                        Helper.DBHall.SaveChanges(); 		//Фиксация изменений в БД
                        MessageBox.Show("Товар успешно удален");
                        ShowProducts();
                    }
                    catch
                    {
                        MessageBox.Show("При удалении возникли проблемы");
                        return;
                    }
                }
                else 					//Товар присутствует в заказах - удалять нельзя
                {
                    MessageBox.Show("Удалить нельзя, т.к. товар есть в заказах");
                    return;
                }
            }
        }

        /// Для редактирования товара - двойной клик по товару
        private void listBoxProducts_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (user.UserRole == 2) 		//Только для роли администратора 
            {
                //Выбранный товар в каталоге
                Classes.ProductExtended productSelected =
                                                     listBoxProducts.SelectedItem as Classes.ProductExtended;
                //Вызов конструктора с параметром - выбранный товар для редактирования
                View.EditCatalog editCatalog = new View.EditCatalog(productSelected);
                this.Hide();
                editCatalog.ShowDialog();
                this.ShowDialog();
            }
        }

        /// При активизации окна - переход на это окно - обновить каталог при отображении
        private void Window_Activated(object sender, EventArgs e)
        {
            ShowProducts();
        }
    


    private void butWorkOrder_Click(object sender, RoutedEventArgs e)
        {
            WorkOrder workOrder = new WorkOrder(user);
            this.Hide();
            workOrder.ShowDialog();
            this.ShowDialog();
        }


        private void ListBoxProduct_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void butNavigator_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }


        private void miAddInOrder_Click(object sender, RoutedEventArgs e)
        {
           
            butViewOrder.Visibility = Visibility.Visible;   //Показать кнопку «Оформление заказа»
                                                            //Выбранный товар в списке всех товаров
            Classes.ProductExtended productSelect =
                                                            listBoxProducts.SelectedItem as Classes.ProductExtended;
            //Артикул выбранного товара
            string art = productSelect.Product.ProductArticle;
            //Поиск товара с этим артиклем в заказе
            Classes.ProductInOrder productFind =
                        listProductInOrder.Find(pr => pr.ProductExtendedInOrder.Product.ProductArticle == art);
            if (productFind != null)            //Нашел - такой товар уже есть в заказе
            {
                productFind.countProductInOrder++;  //Увеличиваем его количество в заказе
            }
            else                  //такого товара еще не было – создаем новый товар в заказе
            {
                Classes.ProductInOrder productNew = new Classes.ProductInOrder();
                productNew.countProductInOrder = 1;
                productNew.ProductExtendedInOrder = productSelect;
                listProductInOrder.Add(productNew);
            }

        }



        /// Перейти в окно просмотра заказа
        private void butViewOrder_Click(object sender, RoutedEventArgs e)
        {
            // Окну передаем созданный заказ из товаров
            Order order = new Order(listProductInOrder);
            this.Hide();
            order.ShowDialog();
            this.ShowDialog();
        }


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //Формирование списка из категорий из БД
            List<Category> categories = Helper.DBHall.Category.ToList();
            //Добавление в него нового элемента, которого нет в БД
            Category allCat = new Category();
            allCat.CategoryID = 0;
            allCat.CategoryName = "Все категории";
            categories.Insert(0, allCat);
            cbFilterCategory.ItemsSource = categories;	//Связь списка с ЭУ
            //Настройка необходимых свойства ЭУ для правильного отображения и получения
            cbFilterCategory.DisplayMemberPath = "CategoryName";
            cbFilterCategory.SelectedValuePath = "CategoryId";
            //Первый отображается из списка
            cbSort.SelectedIndex = 0;
            cbFilterDiscount.SelectedIndex = 0;
            cbFilterCategory.SelectedIndex = 0;
            ShowProducts();
            listBoxProducts.Visibility = Visibility.Hidden;
            butAddProduct.Visibility = Visibility.Hidden;
            butDeleteProduct.Visibility = Visibility.Hidden;
            butWorkOrder.Visibility = Visibility.Hidden;

            if (user != null)
            {
                switch (user.UserRole)
                {
                    case 3:
                        listBoxProducts.Visibility = Visibility.Visible;
                        break;
                    case 1:
                        butWorkOrder.Visibility = Visibility.Visible;
                        break;
                    case 2:
                        butWorkOrder.Visibility = Visibility.Visible;
                        butAddProduct.Visibility = Visibility.Visible;
                        butDeleteProduct.Visibility = Visibility.Visible;
                        listBoxProducts.Visibility = Visibility.Visible;
                        break;
                }
            }
            else
            {
                listBoxProducts.Visibility = Visibility.Visible;
            }
        }

        /// ОТобразить товары с учетом фильтрации
        private void ShowProducts()
        {
            List<Model.Product> products = new List<Model.Product>();
            products = Helper.DBHall.Product.ToList();
            int totalCount = products.Count;		//Число товаров в БД
            //Сортировка
            switch (cbSort.SelectedIndex)
            {
                case 0:
                    products = products.OrderBy(pr => pr.ProductCost).ToList();
                    break;
                case 1:
                    products = products.OrderByDescending(pr => pr.ProductCost).ToList();
                    break;
            }
            double max = 100, min = 0;
            //Фильтрация по скидке
            switch (cbFilterDiscount.SelectedIndex)
            {
                case 0:
                    min = 0; max = 100;
                    break;
                case 1:
                    min = 0; max = 9.99;
                    break;
                case 2:
                    min = 10; max = 14.99;
                    break;
                case 3:
                    min = 15; max = 100;
                    break;
            }
            products = products.Where(pr => pr.ProductDiscountMax <= max &&
                                                                                  pr.ProductDiscountMax >= min).ToList();
            //Фильтрация по категории в случае, если не все категории
            if (cbFilterCategory.SelectedIndex > 0)
            {
                products = products.Where(pr => pr.ProductCategory ==
                                                                                    cbFilterCategory.SelectedIndex).ToList();
            }
            //Поиск по названию
            string search = tbSearch.Text;	//Введенная строка поиска
            if (search.Length > 0)		//Если она не пустая
            {
                products = products.Where(pr => pr.ProductName.Contains(search)).ToList();
            }
            //Количество товаров после фильтрации
            int filterCount = products.Count;
            tbCount.Text = filterCount + " Из " + totalCount;
            //Перенести данные списка из БД в список с расширенными свойствами
            List<Classes.ProductExtended> productExtendeds = new List<Classes.ProductExtended>();
            foreach (Model.Product product in products)
            {
                Classes.ProductExtended productExtended = new Classes.ProductExtended();
                productExtended.Product = product;
                productExtendeds.Add(productExtended);
            }
            //Отобразить отфильтрованный список в интерфейсе
            listBoxProducts.ItemsSource = productExtendeds;

        }

        //Выбор направления сортировки
        private void cbSort_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ShowProducts();
        }

        //Выбор направления фильтрации по скидке
        private void cbFilterDiscount_SelectionChanged(object sender,
                                                             SelectionChangedEventArgs e)
        {
            ShowProducts();
        }
        //Выбор направления фильтрации по категории
        private void cbFilterCategory_SelectionChanged(object sender,
                                 SelectionChangedEventArgs e)
        {
            ShowProducts();
        }

        //Ввод строки для быстрого поиска
        private void tbSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            ShowProducts();
        }
        


    }
}
