using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using static System.Net.Mime.MediaTypeNames;
using Microsoft.Data.Sqlite;
using SQLitePCL;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using static FoodStore.ManagerAddLanches;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace FoodStore
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ManagerAddLanches : Page
    {
        private List<string> droppedItems = new List<string>();
        private DispatcherTimer timer;
        private ObservableCollection<Ingrediente> ingredients;

        public ManagerAddLanches()
        {
            this.InitializeComponent();
            blinktext_txtBlock.Visibility = Visibility.Collapsed;
            GetCategories();
            LoadIngredients();
        }

        private void backButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(ManagementWindow));
        }

        private async void LoadIngredients()
        {
            ingredients = await GetIngredientesAsync();
            itemIngredients_checkBoxList.ItemsSource = ingredients;

            foreach (var ingredient in ingredients)
            {
                ingredient.PropertyChanged += Ingrediente_PropertyChanged;
            }
        }

        private void Ingrediente_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Ingrediente.IsSelected))
            {
                UpdateTotalValue();
            }
        }

        private void UpdateTotalValue()
        {
            double totalValue = ingredients.Where(i => i.IsSelected).Sum(i => i.Valor);
            itemPrice_txtBox.Text = $"{totalValue:F2}";
        }

        private void itemIngredients_checkBoxList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (itemIngredients_checkBoxList.SelectedItem is Ingrediente selectedIngrediente)
            {
                itemPrice_txtBox.Text = $"{selectedIngrediente.Valor:F2}";
            }
        }

        public class Ingrediente : INotifyPropertyChanged
        {
            private bool isSelected;
            public string Nome { get; set; }
            public double Valor { get; set; }

            public bool IsSelected
            {
                get => isSelected;
                set
                {
                    if (isSelected != value)
                    {
                        isSelected = value;
                        OnPropertyChanged();
                    }
                }
            }

            public event PropertyChangedEventHandler PropertyChanged;
            protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public async Task<ObservableCollection<Ingrediente>> GetIngredientesAsync()
        {
            var ingredients = new ObservableCollection<Ingrediente>();

            SQLitePCL.Batteries.Init();
            StorageFolder localFolder = ApplicationData.Current.LocalFolder;
            IStorageItem dbFile = await localFolder.TryGetItemAsync("Database.db");

            if (dbFile != null && dbFile.IsOfType(StorageItemTypes.File))
            {
                StorageFile myDBFile = (StorageFile)dbFile;
                string dbPath = myDBFile.Path;

                using (SqliteConnection db = new SqliteConnection($"Filename={dbPath}"))
                {
                    db.Open();
                    string selectCommand = "SELECT Nome, Valor FROM Ingredientes";

                    using (SqliteCommand query = new SqliteCommand(selectCommand, db))
                    {
                        using (SqliteDataReader reader = query.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                ingredients.Add(new Ingrediente
                                {
                                    Nome = reader["Nome"].ToString(),
                                    Valor = reader.GetDouble(1)
                                });
                            }
                        }
                    }
                }
            }
            return ingredients;
        }
        public async void GetCategories()
        {
            Batteries.Init();

            StorageFolder localFolder = ApplicationData.Current.LocalFolder;
            IStorageItem dbFile = await localFolder.TryGetItemAsync("Database.db");

            if (dbFile != null && dbFile.IsOfType(StorageItemTypes.File))
            {
                StorageFile myDBFile = (StorageFile)dbFile;
                string dbPath = myDBFile.Path;

                using (SqliteConnection db = new SqliteConnection($"Filename={dbPath}"))
                {
                    db.Open();

                    string selectCommand = "SELECT categoria FROM categorias";

                    using (SqliteCommand query = new SqliteCommand(selectCommand, db))
                    {
                        using (SqliteDataReader reader = query.ExecuteReader())
                        {
                            ObservableCollection<string> categories = new ObservableCollection<string>();

                            while (reader.Read())
                            {
                                string category = reader.GetString(0);
                                categories.Add(category);
                            }

                            itemCategory_comboBox.ItemsSource = categories;
                        }
                    }
                }
            }
        }

        private void ShowTextBlockForDuration()
        {
            blinktext_txtBlock.Visibility = Visibility.Visible;

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(5);
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private void Timer_Tick(object sender, object e)
        {
            blinktext_txtBlock.Visibility = Visibility.Collapsed;
            timer.Stop();
        }

        private async void Grid_DragOver(object sender, DragEventArgs e)
        {
            if (e.DataView.Contains(StandardDataFormats.StorageItems))
            {
                e.AcceptedOperation = DataPackageOperation.Copy;
            }
        }

        private async void Grid_Drop(object sender, DragEventArgs e)
        {
            if (e.DataView.Contains(StandardDataFormats.StorageItems))
            {
                var items = await e.DataView.GetStorageItemsAsync();
                if (items.Count > 0)
                {
                    var storageFile = items[0] as StorageFile;
                    if (storageFile != null)
                    {
                        var filePath = storageFile.Path;
                        if (!droppedItems.Contains(filePath))
                        {
                            droppedItems.Add(filePath);

                            var localFolder = ApplicationData.Current.LocalFolder;
                            var resourceFolder = await localFolder.CreateFolderAsync("Recursos", CreationCollisionOption.OpenIfExists);

                            var filesInResourceFolder = await resourceFolder.GetFilesAsync();
                            int fileCount = filesInResourceFolder.Count + 1;
                            var newFileName = $"imagem{fileCount}{storageFile.FileType}";
                            var newFile = await storageFile.CopyAsync(resourceFolder, newFileName, NameCollisionOption.ReplaceExisting);

                            var bitmapImage = new BitmapImage(new Uri(newFile.Path));
                            droppedImage.Source = bitmapImage;
                        }
                        else
                        {
                            ShowTextBlockForDuration();
                        }
                    }
                }
            }
        }

        private async void addTodb_button_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(itemName_txtBox.Text))
            {
                itemName_txtBox.Background = new SolidColorBrush(Windows.UI.Colors.Red);
                return;
            }else
            {
                itemName_txtBox.Background = new SolidColorBrush(Windows.UI.Colors.Black);
            }

            if (itemCategory_comboBox.SelectedItem == null)
            {
                itemCategory_comboBox.Background = new SolidColorBrush(Windows.UI.Colors.Red);
            }
            else
            {
                itemCategory_comboBox.Background = new SolidColorBrush(Windows.UI.Colors.Black);
            }

            string ingredientsString = string.Join(", ", ingredients.Where(i => i.IsSelected).Select(i => i.Nome));
            if (string.IsNullOrEmpty(ingredientsString))
            {
                itemIngredients_checkBoxList.Background = new SolidColorBrush(Windows.UI.Colors.Red);
                return;
            }
            else
            {
                itemIngredients_checkBoxList.Background = new SolidColorBrush(Windows.UI.Colors.Black);
            }


            if (string.IsNullOrEmpty(itemDescription_txtBox.Text))
            {
                itemDescription_txtBox.Background = new SolidColorBrush(Windows.UI.Colors.Red);
                return;
            }
            else
            {
                itemDescription_txtBox.Background = new SolidColorBrush(Windows.UI.Colors.Black);
            }

            SQLitePCL.Batteries.Init();
            StorageFolder localFolder = ApplicationData.Current.LocalFolder;
            IStorageItem dbFile = await localFolder.TryGetItemAsync("Database.db");

            if (dbFile != null && dbFile.IsOfType(StorageItemTypes.File))
            {
                StorageFile myDBFile = (StorageFile)dbFile;
                string dbPath = myDBFile.Path;

                using (SqliteConnection db = new SqliteConnection($"Filename={dbPath}"))
                {
                    db.Open();

                    string insertCommand = @"INSERT INTO Lanches (Nome, Categoria, Ingredientes, Valor, Detalhes)
                                     VALUES (@Nome, @Categoria, @Ingredientes, @Valor, @Detalhes)";
                    using (SqliteCommand query = new SqliteCommand(insertCommand, db))
                    {
                        query.Parameters.AddWithValue("@Nome", itemName_txtBox.Text);
                        query.Parameters.AddWithValue("@Categoria", itemCategory_comboBox.SelectedItem.ToString());
                        query.Parameters.AddWithValue("@Ingredientes", ingredientsString);
                        query.Parameters.AddWithValue("@Valor", Convert.ToDecimal(itemPrice_txtBox.Text));
                        query.Parameters.AddWithValue("@Detalhes", itemDescription_txtBox.Text);

                        try
                        {
                            query.ExecuteNonQuery();
                            Console.WriteLine("Lanche adicionado com sucesso!");
                            Frame.Navigate(typeof(ManagementWindow));
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Erro ao adicionar lanche: {ex.Message}");
                        }
                    }
                }
            }
        }

        private void backButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Frame.Navigate(typeof(ManagementWindow));
        }
    }
}
