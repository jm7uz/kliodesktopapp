using KlioDesktopApp.Components.Basket;
using KlioDesktopApp.Components.Category;
using KlioDesktopApp.Components.Product;
using KlioDesktopApp.Entities;
using System.Net.Http;
using System.Reflection.Emit;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

//Printer
using System.Drawing.Printing;
namespace KlioDesktopApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<CategoryViewControl> categories;
        private List<ProductViewControl> products;
        private List<UserBasket> userBaskets;
        private int currentIndex;
        private const int MaxDisplayedCategories = 5;
        private const string CategoryApiUrl = "http://bb6d-91-196-77-120.ngrok-free.app/api/category/index?searchable=&page=1&perPage=200";
        private const string ProductApiUrl = "http://bb6d-91-196-77-120.ngrok-free.app/api/product/index?searchable=&category_id={0}&page=1&perPage=200";
        private int colored = 0;
        private bool _isSelectedCategoryBtn = false;

        public MainWindow()
        {
            InitializeComponent();
            LoadPrinters();
            InitializeCategories();
            userBaskets = new List<UserBasket>();
        }

        private async void InitializeCategories()
        {
            categories = new List<CategoryViewControl>();
            var categoryData = await FetchCategoriesFromApi();
            foreach (var category in categoryData)
            {
                var categoryControl = new CategoryViewControl();
                categoryControl.LbCategory.Content = category.Name;
                categoryControl.Tag = category.Id;
                categoryControl.btnCategory.Tag = category.Id;
                categoryControl.MouseLeftButtonUp += CategoryControl_MouseLeftButtonUp;
                categories.Add(categoryControl);
                categoryControl.btnCategory.Click += Button_Click_Category;
            }
            currentIndex = 0;
            UpdateCategoryDisplay();

            // Select the first category by default
            if (categories.Count > 0)
            {
                categories[0].cBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#EC7403"));
                FetchProducts((int)categories[0].Tag); 
            }
        }


        private void Button_Click_Category(object sender, RoutedEventArgs e)
        {
            if (sender is Button clickedButton && clickedButton.Tag is int categoryId)
            {
                if (categoryId > 0 && categoryId <= categories.Count)
                {
                    if (colored >= 0 && colored < categories.Count)
                    {
                        categories[colored].cBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FEFEFE"));
                    }

                    colored = categoryId - 1;
                    categories[colored].cBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#EC7403"));

                    FetchProducts(categoryId);
                }
            }
        }

        private async Task<List<Category>> FetchCategoriesFromApi()
        {
            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = await client.GetAsync(CategoryApiUrl);
                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    var apiResult = JsonSerializer.Deserialize<ApiResponse<Category>>(jsonResponse, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    return apiResult.Resoult.Data;
                }
                else
                {
                    MessageBox.Show("Failed to fetch categories from API.");
                    return new List<Category>();
                }
            }
        }

        private async void FetchProducts(int categoryId)
        {
            products = new List<ProductViewControl>();
            var productData = await FetchProductsFromApi(categoryId);
            foreach (var product in productData)
            {
                var productControl = new ProductViewControl();
                productControl.ProductName.Content = product.Name;
                productControl.ProductDesc.Content = $"Price: {product.Price}";
                productControl.Tag = product.Id;

                products.Add(productControl);

                productControl.btnProduct.Tag = product.Id;
                productControl.btnProduct.Click += Button_Click_Product;
            }
            UpdateProductDisplay();
        }

        private void Button_Click_Product(object sender, RoutedEventArgs e)
        {
            Button clickedButton = sender as Button;

            if (clickedButton != null && clickedButton.Tag is int productId)
            {
                var product = products.FirstOrDefault(p => (int)p.Tag == productId);

                if (product != null)
                {
                    var productName = product.ProductName.Content.ToString();
                    var productPrice = decimal.Parse(product.ProductDesc.Content.ToString().Replace("Price: ", ""));

                    var existingBasketItem = userBaskets.FirstOrDefault(b => b.ProductId == productId);
                    if (existingBasketItem != null)
                    {
                        existingBasketItem.Count++;
                    }
                    else
                    {
                        var userBasketItem = new UserBasket
                        {
                            ProductId = productId,
                            Name = productName,
                            Count = 1,
                            Rate = 5.0,
                            Desc = 0,
                            Price = productPrice
                        };
                        userBaskets.Add(userBasketItem);
                    }
                }
            }
            UpdateUserBasketDisplay();
        }

        private void UpdateUserBasketDisplay()
        {
            // Clear current basket display
            BasketContainer.Children.Clear();

            decimal total = 0;



            foreach (var basketItem in userBaskets)
            {
                var basketControl = new BasketViewControl
                {
                    ProductId = basketItem.ProductId,
                    BasketlbName = { Content = basketItem.Name },
                    BasketlbCount = { Content = basketItem.Count.ToString() },
                    BasketlbTotal = { Content = (basketItem.Price * basketItem.Count).ToString() }
                };

                total += basketItem.Price * basketItem.Count;

                basketControl.BasketbtnMinus.Tag = basketItem.ProductId;
                basketControl.BasketbtnMinus.Click += Button_Click_Basket_Minus;
                basketControl.BasketbtnPlus.Tag = basketItem.ProductId;
                basketControl.BasketbtnPlus.Click += Button_Click_Basket_Plus;
                basketControl.BasketbtnTrash.Tag = basketItem.ProductId;
                basketControl.BasketbtnTrash.Click += Button_Click_Basket_Trash;

                BasketContainer.Children.Add(basketControl);

                lbTotalBalance.Content = total.ToString() + "$";
            }
        }

        private void Button_Click_Basket_Minus(object sender, RoutedEventArgs e)
        {
            Button clickedButton = sender as Button;
            if (clickedButton != null && clickedButton.Tag is int productId)
            {
                var existingBasketItem = userBaskets.FirstOrDefault(b => b.ProductId == productId);
                if (existingBasketItem != null)
                {
                    if (existingBasketItem.Count > 1)
                    {
                        existingBasketItem.Count--;

                        UpdateUserBasketDisplay();
                    }
                    
                }
                
            }
        }

        private void Button_Click_Basket_Plus(object sender, RoutedEventArgs e)
        {
            Button clickedButton = sender as Button;
            if (clickedButton != null && clickedButton.Tag is int productId)
            {
                var existingBasketItem = userBaskets.FirstOrDefault(b => b.ProductId == productId);
                if (existingBasketItem != null)
                {
                    existingBasketItem.Count++;

                    UpdateUserBasketDisplay();
                }
                
            }
        }

        private void Button_Click_Basket_Trash(object sender, RoutedEventArgs e)
        {
            Button clickedButton = sender as Button;
            if (clickedButton != null && clickedButton.Tag is int productId)
            {
                var existingBasketItem = userBaskets.FirstOrDefault(b => b.ProductId == productId);

                if (existingBasketItem != null)
                {
                    userBaskets.Remove(existingBasketItem);

                    UpdateUserBasketDisplay();
                }
                
            }
        }

        private void BasketControl_OnMinusClicked(object sender, int productId)
        {
            var basketItem = userBaskets.FirstOrDefault(item => item.ProductId == productId);
            if (basketItem != null && basketItem.Count > 1)
            {
                basketItem.Count--;
                UpdateUserBasketDisplay();
            }
        }

        private void BasketControl_OnPlusClicked(object sender, int productId)
        {
            var basketItem = userBaskets.FirstOrDefault(item => item.ProductId == productId);
            if (basketItem != null)
            {
                basketItem.Count++;
                UpdateUserBasketDisplay();
            }
        }

        private void BasketControl_OnTrashClicked(object sender, int productId)
        {
            var basketItem = userBaskets.FirstOrDefault(item => item.ProductId == productId);
            if (basketItem != null)
            {
                userBaskets.Remove(basketItem);
                UpdateUserBasketDisplay();
            }
        }

        private async Task<List<Product>> FetchProductsFromApi(int categoryId)
        {
            using (HttpClient client = new HttpClient())
            {
                string url = string.Format(ProductApiUrl, categoryId);
                HttpResponseMessage response = await client.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    var apiResult = JsonSerializer.Deserialize<ApiResponse<Product>>(jsonResponse, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    return apiResult.Resoult.Data;
                }
                else
                {
                    MessageBox.Show("Failed to fetch products from API.");
                    return new List<Product>();
                }
            }
        }

        private void UpdateCategoryDisplay()
        {
            catles.Children.Clear();
            int categoriesToShow = Math.Min(MaxDisplayedCategories, categories.Count - currentIndex);
            for (int i = currentIndex; i < currentIndex + categoriesToShow; i++)
            {
                catles.Children.Add(categories[i]);
            }

            PreviewCategorie.Visibility = currentIndex > 0 ? Visibility.Visible : Visibility.Hidden;
            NextProduct.Visibility = currentIndex + categoriesToShow < categories.Count ? Visibility.Visible : Visibility.Hidden;
        }


        private void UpdateProductDisplay()
        {
            wpProducts.Children.Clear();
            foreach (var product in products)
            {
                wpProducts.Children.Add(product);
            }
        }

        private void CategoryControl_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (sender is CategoryViewControl categoryControl && categoryControl.Tag is int categoryId)
            {
                FetchProducts(categoryId);
            }
        }

        private void NextProduct_Click(object sender, RoutedEventArgs e)
        {
            if (currentIndex + MaxDisplayedCategories < categories.Count)
            {
                currentIndex++;
                UpdateCategoryDisplay();
            }
        }

        private void PreviewCategorie_Click(object sender, RoutedEventArgs e)
        {
            if (currentIndex > 0)
            {
                currentIndex--;
                UpdateCategoryDisplay();
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void btnMinimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void brDragable_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void LoadPrinters()
        {
            PrinterSettings.StringCollection printers = PrinterSettings.InstalledPrinters;

            if (printers.Count == 0)
            {
                MessageBox.Show("Unfortunately, you do not have a printer device connected.");
                return;
            }

            foreach (string printer in printers)
            {
                printerComboBox.Items.Add(printer);
            }

            if (printerComboBox.Items.Count > 0)
            {
                printerComboBox.SelectedIndex = 0;
            }
        }

        private void PrintButton_Click(object sender, RoutedEventArgs e)
        {
            if (printerComboBox.SelectedItem == null)
            {
                MessageBox.Show("Please select a printer.");
                return;
            }

            string selectedPrinterName = printerComboBox.SelectedItem.ToString();
            PrintInvoice(selectedPrinterName);
        }

        private void PrintInvoice(string printerName)
        {
            PrintDocument printDocument = new PrintDocument();
        printDocument.PrinterSettings.PrinterName = printerName;

        printDocument.PrintPage += (sender, e) =>
        {
            System.Drawing.Font titleFont = new System.Drawing.Font("Arial", 16, System.Drawing.FontStyle.Bold);
            System.Drawing.Font headerFont = new System.Drawing.Font("Arial", 12, System.Drawing.FontStyle.Bold);
            System.Drawing.Font bodyFont = new System.Drawing.Font("Arial", 12);
            float lineHeight = bodyFont.GetHeight(e.Graphics) + 4;
            float x = 100;
            float y = 100;

            e.Graphics.DrawString("Invoice", titleFont, System.Drawing.Brushes.Black, x, y);
            y += lineHeight * 2;

            e.Graphics.DrawString("ProductId", headerFont, System.Drawing.Brushes.Black, x, y);
            e.Graphics.DrawString("Name", headerFont, System.Drawing.Brushes.Black, x + 100, y);
            e.Graphics.DrawString("Count", headerFont, System.Drawing.Brushes.Black, x + 250, y);
            e.Graphics.DrawString("Rate", headerFont, System.Drawing.Brushes.Black, x + 350, y);
            e.Graphics.DrawString("Desc", headerFont, System.Drawing.Brushes.Black, x + 450, y);
            e.Graphics.DrawString("Price", headerFont, System.Drawing.Brushes.Black, x + 550, y);
            y += lineHeight;

            foreach (var item in userBaskets)
            {
                e.Graphics.DrawString(item.ProductId.ToString(), bodyFont, System.Drawing.Brushes.Black, x, y);
                e.Graphics.DrawString(item.Name, bodyFont, System.Drawing.Brushes.Black, x + 100, y);
                e.Graphics.DrawString(item.Count.ToString(), bodyFont, System.Drawing.Brushes.Black, x + 250, y);
                e.Graphics.DrawString(item.Rate.ToString(), bodyFont, System.Drawing.Brushes.Black, x + 350, y);
                e.Graphics.DrawString(item.Desc.ToString(), bodyFont, System.Drawing.Brushes.Black, x + 450, y);
                e.Graphics.DrawString(item.Price.ToString("C"), bodyFont, System.Drawing.Brushes.Black, x + 550, y);
                y += lineHeight;
            }

            y += lineHeight;
            decimal total = 0;
            foreach (var item in userBaskets)
            {
                total += item.Price * item.Count;
            }
            e.Graphics.DrawString($"Total: {total:C}", headerFont, System.Drawing.Brushes.Black, x, y);
        };

            try
            {
                printDocument.Print();
                MessageBox.Show("Data has been sent to the printer.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to print: {ex.Message}");
            }
        }
    }


    public class ApiResponse<T>
    {
        public bool Status { get; set; }
        public Result<T> Resoult { get; set; }
    }

    public class Result<T>
    {
        public List<T> Data { get; set; }
    }

    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Date { get; set; }
    }

    public class Product
    {
        public int Id { get; set; }
        public int CategoryId { get; set; }
        public string Name { get; set; }
        public string Price { get; set; }
        public string Count { get; set; }
        public string Date { get; set; }
    }
}