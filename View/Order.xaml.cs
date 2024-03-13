using OOO_Modnica.Classes;
using System;
using System.Collections.Generic;
using System.IO;
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

using Word = Microsoft.Office.Interop.Word;

namespace OOO_Modnica.View
{
    /// <summary>
    /// Логика взаимодействия для Window1.xaml
    /// </summary>
    public partial class Order : Window
    {

        List<Classes.ProductInOrder> order;	//Сформированный заказ из окна Каталог

        /// Конструктор с параметром
        /// <param name="order">сформированный заказ</param>
        public Order(List<Classes.ProductInOrder> order)
        {
            InitializeComponent();
            this.DataContext = this;		//Элементы интерфейса связать с данными
            this.order = order;			//Получить переданный параметр

            //Заполнить список пунктами выдачи из БД
            List<Model.Point> points = new List<Model.Point>();
            points = Classes.Helper.DBHall.Point.ToList();
            cbPoint.ItemsSource = points;
            cbPoint.DisplayMemberPath = "PointAdress";
            cbPoint.SelectedValuePath = "PointID";
            cbPoint.SelectedIndex = 0;
            ShowOrder();
        }

        /// Показать заказ
        private void ShowOrder()
        {
            listBoxProductsInOrder.ItemsSource = null;		//Сбросить
            listBoxProductsInOrder.ItemsSource = order;		//Отобразить заказ
            InfoOrder();				//Информация и расчеты по заказу
        }

        /// Расчеты по всем товарам заказа
        private void InfoOrder()
        {
            decimal summaTotal = 0;
            decimal summaWithDiscount = 0;
            //Перебор всех товаров в заказе
            foreach (var item in order)
            {
                //Сумма заказа с учетом цена товара без скидки и их количества в заказе
                summaTotal += item.ProductExtendedInOrder.Product.ProductCost *
                                            item.countProductInOrder;
                //Сумма заказа с учетом цена товара со скидкой и их количества в заказе
                summaWithDiscount += item.ProductExtendedInOrder.ProductCostWithDiscount *
                                                          item.countProductInOrder;
            }
            tbSumma.Text = "Сумма заказа без скидки: " + summaTotal;
            tbSummaWithDiscount.Text = "Сумма заказа со скидкой: " + summaWithDiscount;
            //Величина скидка как разность между суммой без скидки и суммой со скидкой
            tbSummaDiscount.Text = "Скидка составила: " + (summaTotal - summaWithDiscount);
        }

        /// Удалить товар из заказа кнопкой «Удалить»
        private void butDel_Click(object sender, RoutedEventArgs e)
        {
            //Получить информацию о товаре, напротив которого была нажата кнопка
            //Использовать настроенной свойство DataContext в конструкторе окна
            Classes.ProductInOrder productInOrder = (sender as Button).DataContext
                                                                              as Classes.ProductInOrder;
            //Удаление найденного товара из заказа
            order.Remove(productInOrder);
            ShowOrder();			//Обновить список товаров
        }

        /// Обновление количества товара из поле ввода количества напротив товара
        private void tbCount_TextChanged(object sender, TextChangedEventArgs e)
        {
            //Получить информацию о товаре, напротив которого было введено количество
            //Использовать настроенной свойство DataContext в конструкторе окна
            Classes.ProductInOrder productInOrder = (sender as TextBox).DataContext
                                                                                           as Classes.ProductInOrder;
            //Получение количества из поля ввода
            string temp = (sender as TextBox).Text;
            if (String.IsNullOrEmpty(temp))		//Количество не может быть пустым
            {
                MessageBox.Show("Количество не может быть пустым");
                (sender as TextBox).Text = "1";	//Количеству присвоить хотя бы 1
            }
            int newCount;		//Новое количество товара, введенное пользователем
            if (int.TryParse(temp, out newCount))	//Контроль над вводом
            {
                if (newCount == 0)			//Удалить товар при 0 в количестве
                {
                    order.Remove(productInOrder);	//Удалить товар из заказа
                }
                else
                {
                    //Внести изменение в количество найденного товара в заказе
                    productInOrder.countProductInOrder = newCount;
                }
                ShowOrder();				//Обновить список
            }
            else
            {
                MessageBox.Show("Неверный формат для количества");
                (sender as TextBox).Text = "1";		//Занести хотя бы 1
            }

        }

        private void buttonNavigate_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void butCreateOrder_Click(object sender, RoutedEventArgs e)
        {
            if (order.Count <= 0)			//Пустой заказ
            {
                MessageBox.Show("Корзина пуста!");
                return;
            }
            //Формируем объект для таблицы Order БД
            Model.Order orderDB = new Model.Order();
            //Заполняем все обязательные поля кроме авто инкрементируемых
            orderDB.OrderCheckDate = DateTime.Now;			//Дата заказа
            orderDB.OrderDeliveryDate = DateTime.Now.AddDays(6);	//Дата доставки
            orderDB.OrderPoint = (int)cbPoint.SelectedValue;
            orderDB.OrderComment = Comment.Text;
            List<Model.User> managers = Helper.DBHall.User.Where(u => u.UserRole == 1).ToList();
            orderDB.OrderManager = managers[new Random().Next(0, managers.Count)].UserID;
            Model.Client client = new Model.Client();
            client.ClientName = tbFIO.Text;
            client.ClientPhone = "+" + new Random().Next(1000000, 10000000).ToString();
            Helper.DBHall.Client.Add(client);
            Helper.DBHall.SaveChanges();
            orderDB.OrderClient = Helper.DBHall.Client.Where(u => u.ClientPhone == client.ClientPhone).FirstOrDefault().ClientId;
            orderDB.OrderStatus = 2;
            //Добавляем заказ в виртуальную таблицу
            Helper.DBHall.Order.Add(orderDB);
            
            try
            {
                Helper.DBHall.SaveChanges();
                //Все товары в этом заказе сохраняем в таблице OrderProduct
                foreach (var product in order)
                {
                    //Создаем объект для сохранения
                    Model.OrderProduct orderProduct = new Model.OrderProduct();
                    //Берем Id только что созданного заказа, чтобы записать его для каждого товара
                    orderProduct.OrderID = orderDB.OrderID;
                    //Артикль
                    orderProduct.ProductArticle = product.ProductExtendedInOrder.Product.ProductArticle;
                    //Количество
                    orderProduct.Count = product.countProductInOrder;
                    //Добавляем заполненный товар заказа сначала в виртуальную таблицу
                    Helper.DBHall.OrderProduct.Add(orderProduct);
                }
                Helper.DBHall.SaveChanges();			//Обновляем БД
                if (MessageBox.Show("Ваш заказ оформлен\nЧерез 6 календарных дней, Ваш заказ " +
                                                 "прибудет в пункт выдачи\n" +
                                                "Поступит СМС на номер: " + client.ClientPhone + "\nВы хотите распечатать чек?", "Успех!", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    Word.Application wordApp = new Word.Application();      //Создание сервера Word
                    Word.Document wordDoc = wordApp.Documents.Add();    //Создание документа word

                    //Первый параграф – заголовок документа
                    Word.Paragraph wordParagraphTitle = wordDoc.Paragraphs.Add();
                    Word.Range wordRangeTitel = wordParagraphTitle.Range;
                    wordRangeTitel.Text = "        Талон заказа №" + orderDB.OrderID;
                    wordRangeTitel.Font.Bold = 1;
                    wordRangeTitel.Font.Size = 20;
                    //Word.InlineShape wordShape =
                    //         wordDoc.InlineShapes.AddPicture(Environment.CurrentDirectory + "/logo.png",
                    //        Type.Missing, Type.Missing, wordRangeTitel);
                    //wordShape.Width = 50;
                    //wordShape.Height = 50;
                    wordRangeTitel.ParagraphFormat.Alignment =
                                 Word.WdParagraphAlignment.wdAlignParagraphCenter;
                    wordRangeTitel.InsertParagraphAfter();

                    //Описание заказа
                    Word.Paragraph wordParagraphDesc = wordDoc.Paragraphs.Add();
                    Word.Range wordRangeDesc = wordParagraphDesc.Range;
                    wordRangeDesc.Text = "Дата заказа: " + orderDB.OrderCheckDate.ToLongDateString() +
                                           Environment.NewLine;
                    wordRangeDesc.Text += "ФИО менеджера по продажам, оформившего заказ: " +
                        Helper.DBHall.User.Where(u => u.UserID == orderDB.OrderManager).FirstOrDefault().UserFullName + Environment.NewLine;
                    wordRangeDesc.Font.Bold = 0;
                    wordRangeDesc.Font.Size = 12;
                    wordRangeDesc.ParagraphFormat.Alignment =
                                           Word.WdParagraphAlignment.wdAlignParagraphLeft;
                    wordRangeDesc.InsertParagraphAfter();

                    //Таблица
                    Word.Paragraph wordParagraphTable = wordDoc.Paragraphs.Add();
                    Word.Range wordRangeTable = wordParagraphTable.Range;
                    Word.Table wordTable = wordDoc.Tables.Add(wordRangeTable, order.Count + 1, 4);
                    wordTable.Borders.OutsideLineStyle = Word.WdLineStyle.wdLineStyleDouble;
                    wordTable.Borders.InsideLineStyle = Word.WdLineStyle.wdLineStyleSingle;
                    //Шапка таблицы
                    Word.Range wordRangeCell;
                    wordRangeCell = wordTable.Cell(1, 1).Range;
                    wordRangeCell.Text = "Товар";
                    wordRangeCell = wordTable.Cell(1, 2).Range;
                    wordRangeCell.Text = "Цена";
                    wordRangeCell = wordTable.Cell(1, 3).Range;
                    wordRangeCell.Text = "Количество";
                    wordRangeCell = wordTable.Cell(1, 4).Range;
                    wordRangeCell.Text = "Стоимость";
                    wordTable.Rows[1].Range.Bold = 1;
                    wordTable.Rows[1].Range.Font.Size = 14;
                    wordTable.Rows[1].Range.ParagraphFormat.Alignment =
                                           Word.WdParagraphAlignment.wdAlignParagraphCenter;
                    //Содержание таблицы
                    int indexTab = 2;                   //Номер строки после заголовка
                    double totalSumma = 0;
                    double totalSale = 0;
                    double totalSummaWithSale = 0;
                    foreach (var item in order)
                    {
                        totalSumma += (double)item.ProductExtendedInOrder.Product.ProductCost * item.countProductInOrder;
                        totalSale += (double)item.ProductExtendedInOrder.Product.ProductCost - (double)item.ProductExtendedInOrder.ProductCostWithDiscount;
                        totalSummaWithSale += (double)item.ProductExtendedInOrder.ProductCostWithDiscount * item.countProductInOrder;
                        wordRangeCell = wordTable.Cell(indexTab, 1).Range;
                        wordRangeCell.Text = item.ProductExtendedInOrder.Product.ProductName;
                        wordRangeCell = wordTable.Cell(indexTab, 2).Range;
                        wordRangeCell.Text = item.ProductExtendedInOrder.ProductCostWithDiscount.ToString();
                        wordRangeCell = wordTable.Cell(indexTab, 3).Range;
                        wordRangeCell.Text = item.countProductInOrder.ToString();
                        wordRangeCell = wordTable.Cell(indexTab, 4).Range;
                        wordRangeCell.Text = (item.ProductExtendedInOrder.ProductCostWithDiscount * item.countProductInOrder).ToString();
                        indexTab++;
                    }
                    wordRangeTable.InsertParagraphAfter();

                    //Итоговые расчеты
                    Word.Paragraph wordParagraphResult = wordDoc.Paragraphs.Add();
                    Word.Range wordRangeResult = wordParagraphResult.Range;
                    wordRangeResult.Text = "Сумма заказа без скидки: " + totalSumma + Environment.NewLine;
                    wordRangeResult.Text += "Величина скидки: " + totalSale + Environment.NewLine;
                    wordRangeResult.Text += "Сумма заказа со скидкой: " + totalSummaWithSale + Environment.NewLine;
                    wordRangeResult.Text += "Пункт выдачи: " + cbPoint.Text + Environment.NewLine;
                    wordRangeResult.Text += "Дата получения: " + orderDB.OrderDeliveryDate.ToLongDateString() +
                                                          Environment.NewLine;
                    wordRangeResult.Text += "Ваш номер телефона: " + orderDB.Client.ClientPhone +
                                                          Environment.NewLine;
                    wordRangeResult.ParagraphFormat.Alignment =
                                                         Word.WdParagraphAlignment.wdAlignParagraphLeft;
                    wordRangeResult.InsertParagraphAfter();

                    //Сохранить в формате pdf
                    string fullNameFile = Environment.CurrentDirectory + "/Coupon/" + orderDB.OrderID + ".pdf";
                    wordDoc.Saved = true;
                    if (File.Exists(fullNameFile))
                    {
                        File.Delete(fullNameFile);
                    }
                    wordDoc.SaveAs(fullNameFile, Word.WdExportFormat.wdExportFormatPDF);
                    //wordDoc.SaveAs2(fullNameFile, Word.WdExportFormat.wdExportFormatPDF);
                    wordDoc.Close(true, null, null);
                    //wordDoc = null;
                    MessageBox.Show("Талон создан успешно");
                    wordApp.Quit();                 //Выйти из Word
                                                    ////Уничтожить COM-объекты
                                                    //System.Runtime.InteropServices.Marshal.FinalReleaseComObject(wordApp);
                                                    ////Заставляет сборщик мусора провести сборку мусора
                                                    //GC.Collect();
                                                    //Вызвать свою подпрограмму убивания процессов
                    releaseObject(wordDoc);             //Уничтожить документ
                    releaseObject(wordApp);             //Удалить из Диспетчера задач
                    this.Close();

                }
                order.Clear();
            }
            catch
            {
                MessageBox.Show("При добавлении данных возникла ошибка");
                return;
            }
        }

        public void releaseObject(object obj)
        {
            try
            {
                System.Runtime.InteropServices.Marshal.ReleaseComObject(obj);
                obj = null;
            }
            catch (Exception ex)
            {
                obj = null;
                MessageBox.Show("Не могу освободить объект " + ex.ToString());
            }
            finally
            {
                GC.Collect();
            }
        }



        private void cbPoint_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            
        }
    }
}
