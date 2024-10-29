using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Automation;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using static FoodStore.ManagementWindow;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace FoodStore
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Cardapio : Page
    {
        public string itemID { get; set; }
        private ObservableCollection<Ingrediente> ingredients;
        public Cardapio()
        {
            this.InitializeComponent();
            LoadLanches();
            LoadIngredients();
        }

        public class Lanche
        {
            public int ID { get; set; }
            public string Nome { get; set; }
            public string Categoria { get; set; }
            public string Ingredientes { get; set; }
        }

        private async void LoadLanches()
        {
            SQLitePCL.Batteries.Init();

            StorageFolder localFolder = ApplicationData.Current.LocalFolder;
            IStorageItem dbFile = await localFolder.TryGetItemAsync("Database.db");
            string dbPath = dbFile.Path;

            List<Lanche> lanches = GetLanchesFromDatabase(dbPath);

            AddLanchesToNavigationView("Clássicos", "Clássicos", lanches);
            AddLanchesToNavigationView("Saudáveis", "Saudáveis", lanches);
            AddLanchesToNavigationView("Vegetarianos", "Vegetarianos", lanches);
        }

        private List<Lanche> GetLanchesFromDatabase(string dbpath)
        {
            List<Lanche> lanches = new List<Lanche>();

            using (SqliteConnection db = new SqliteConnection($"Filename={dbpath}"))
            {
                db.Open();
                SqliteCommand selectCommand = new SqliteCommand("SELECT * FROM Lanches", db);
                SqliteDataReader query = selectCommand.ExecuteReader();

                while (query.Read())
                {
                    lanches.Add(new Lanche
                    {
                        ID = query.GetInt32(0),
                        Nome = query.GetString(1),
                        Categoria = query.GetString(2),
                        Ingredientes = query.GetString(3)
                    });
                }
            }

            return lanches;
        }

        private void AddLanchesToNavigationView(string header, string categoria, List<Lanche> lanches)
        {
            NavigationViewItemHeader categoryHeader = new NavigationViewItemHeader { Content = header };
            navView.MenuItems.Add(categoryHeader);

            foreach (var lanche in lanches)
            {
                if (lanche.Categoria == categoria)
                {
                    StackPanel stackPanel = new StackPanel();
                    stackPanel.Orientation = Orientation.Vertical;

                    TextBlock mainText = new TextBlock();
                    mainText.Text = lanche.Nome;
                    mainText.FontSize = 16;

                    TextBlock descriptionText = new TextBlock();
                    descriptionText.Text = "Descrição do item";
                    descriptionText.FontSize = 12;
                    descriptionText.Opacity = 0.7;

                    stackPanel.Children.Add(mainText);
                    stackPanel.Children.Add(descriptionText);

                    NavigationViewItem newItem = new NavigationViewItem();
                    newItem.Content = stackPanel;
                    newItem.Icon = new SymbolIcon(Symbol.MapPin);

                    newItem.Tag = lanche.ID.ToString();
                    newItem.Tapped += navView_Tapped;
                    navView.MenuItems.Add(newItem);
                }
            }
        }

        private void NavView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            if (args.IsSettingsInvoked)
            {
                Frame.Navigate(typeof(AuthWindow));
            }
        }

        private async void navView_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (sender is NavigationViewItem navItem)
            {
                string item = navItem.Tag as string;
                splitView.IsPaneOpen = true;
                splitViewTitle.Text = item;
                itemID = item;
                
                StorageFolder localFolder = ApplicationData.Current.LocalFolder;
                IStorageItem dbFile = await localFolder.TryGetItemAsync("Database.db");

                if (dbFile != null && dbFile.IsOfType(StorageItemTypes.File))
                {
                    StorageFile myDBFile = (StorageFile)dbFile;
                    string dbPath = myDBFile.Path;

                    using (SqliteConnection db = new SqliteConnection($"Filename={dbPath}"))
                    {
                        db.Open();
                        string selectCommand = $@"
                            SELECT ID, Nome, Categoria, Ingredientes, Valor, Detalhes
                            FROM LANCHES
                            WHERE ID = {item}";

                        using (SqliteCommand query = new SqliteCommand(selectCommand, db))
                        {
                            using (SqliteDataReader reader = query.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    string _id = reader.GetString(0);
                                    string _nome = reader.GetString(1);
                                    string _categoria = reader.GetString(2);
                                    string _ingredientes = reader.GetString(3);
                                    string _valor = reader.GetString(4);
                                    string _detalhes = reader.GetString(5);

                                    splitViewTitle.Text = _nome;
                                    CategoriaNome_txtBlock.Text = _categoria;
                                    Detalhes_txtBlock.Text = _detalhes;
                                    NomeLanche_txtBlock.Text = _nome;
                                    Ingredients_txtBlock.Text = _ingredientes;
                                    Valor_txtBlock.Text = $"{_valor:F2}";
                                }
                            }
                        }
                    }
                }
            }
        }

        private async void AddToCart_btn_Click(object sender, RoutedEventArgs e)
        {
            SQLitePCL.Batteries.Init();
            StorageFolder localFolder = ApplicationData.Current.LocalFolder;
            IStorageItem dbFile = await localFolder.TryGetItemAsync("Database.db");

            string ingredientsString = string.Join(", ", ingredients.Where(i => i.IsSelected).Select(i => i.Nome));

            if (dbFile != null && dbFile.IsOfType(StorageItemTypes.File))
            {
                StorageFile myDBFile = (StorageFile)dbFile;
                string dbPath = myDBFile.Path;

                using (SqliteConnection db = new SqliteConnection($"Filename={dbPath}"))
                {
                    db.Open();

                    string insertCommand = @"INSERT INTO Pedido (NomeItem, IngredientesAdicionais, ValorItem)
                                 VALUES (@NomeItem, @IngredientesAdicionais, @ValorItem)";

                    using (SqliteCommand query = new SqliteCommand(insertCommand, db))
                    {
                        query.Parameters.AddWithValue("@NomeItem", NomeLanche_txtBlock.Text); 
                        query.Parameters.AddWithValue("@IngredientesAdicionais", ingredientsString);
                        query.Parameters.AddWithValue("@ValorItem", Valor_txtBlock.Text);
                        
                        try
                        {
                            query.ExecuteNonQuery();
                            Console.WriteLine("Item adicionado ao pedido com sucesso!");
                            Frame.Navigate(typeof(Cardapio));
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Erro ao adicionar item ao pedido: {ex.Message}");
                        }
                    }
                }
            }
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
            double fixedValue = Convert.ToDouble(Valor_txtBlock.Text);
            double totalValue = ingredients.Where(i => i.IsSelected).Sum(i => i.Valor) + fixedValue;
            Valor_txtBlock.Text = $"{totalValue:F2}";

        }

        private void itemIngredients_checkBoxList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (itemIngredients_checkBoxList.SelectedItem is Ingrediente selectedIngrediente)
            {
                Valor_txtBlock.Text = $"{selectedIngrediente.Valor:F2}";
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
    }
}
