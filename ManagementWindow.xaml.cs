using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace FoodStore
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ManagementWindow : Page
    {
        public ManagementWindow()
        {
            this.InitializeComponent();
            DefaultState();
        }

        public void DefaultState()
        {
            contentFrame.Visibility = Visibility.Visible;
            LanchesGrid.Visibility = Visibility.Collapsed;
            CategoryGrid.Visibility = Visibility.Collapsed;
            IngredientsGrid.Visibility = Visibility.Collapsed;
        }
        private void AddLanchesPageItem_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Frame.Navigate(typeof(ManagerAddLanches));
        }
        private void AddCategoryPageItem_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Frame.Navigate(typeof(ManagerAddCategory));
        }

        private void AddIngredientsPageItem_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Frame.Navigate(typeof(ManagerAddIngredients));
        }

        private void AddAccessPageItem_Tapped(object sender, TappedRoutedEventArgs e)
        {
            // ADICIONAR CODIGO DE ACESSO AO SISTEMA
        }

        // LANCHES =====================

        private void LanchesPageItem_Tapped(object sender, TappedRoutedEventArgs e)
        {
            LanchesGrid.Visibility = Visibility.Visible;
            contentFrame.Visibility = Visibility.Collapsed;
            CategoryGrid.Visibility = Visibility.Collapsed;
            IngredientsGrid.Visibility = Visibility.Collapsed;
            FillLanchesListBox();
        }

        public class Lanche
        {
            public int ID { get; set; }
            public string Nome { get; set; }
            public string Categoria { get; set; }
            public string Ingredientes { get; set; }
            public decimal Valor { get; set; }
            public string Detalhes { get; set; }
        }

        private async void FillLanchesListBox()
        {
            StorageFolder localFolder = ApplicationData.Current.LocalFolder;
            IStorageItem dbFile = await localFolder.TryGetItemAsync("Database.db");

            if (dbFile != null && dbFile.IsOfType(StorageItemTypes.File))
            {
                StorageFile myDBFile = (StorageFile)dbFile;
                string dbPath = myDBFile.Path;

                using (SqliteConnection db = new SqliteConnection($"Filename={dbPath}"))
                {
                    db.Open();
                    var items = new List<Lanche>();
                    string selectCommand = $@"
                            SELECT ID, Nome, Categoria, Ingredientes, Valor, Detalhes
                            FROM LANCHES";

                    using (SqliteCommand query = new SqliteCommand(selectCommand, db))
                    {
                        using (SqliteDataReader reader = query.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                items.Add(new Lanche
                                {
                                    ID = reader.GetInt32(0),
                                    Nome = reader.GetString(1),
                                    Categoria = reader.GetString(2),
                                    Ingredientes = reader.GetString(3),
                                    Valor = reader.GetDecimal(4),
                                    Detalhes = reader.GetString(5)
                                });
                            }
                            LanchesListBox.ItemsSource = items.Select(i => i.Nome).ToList();
                            LanchesListBox.Tag = items;
                        }
                    }
                }
            }
        }

        private void LanchesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {   
            var selectedName = LanchesListBox.SelectedItem.ToString();
            var items = (List<Lanche>)LanchesListBox.Tag;
            var selectedLanche = items.FirstOrDefault(item => item.Nome == selectedName);

            if (selectedLanche != null)
            {
                IDLanche_txtBox.Text = selectedLanche.ID.ToString();
                NameLanche_txtBox.Text = selectedLanche.Nome;
                CategoryLanche_txtBox.Text = selectedLanche.Categoria;
                IngredientsLanche_txtBox.Text = selectedLanche.Ingredientes;
                PriceLanche_txtBox.Text = selectedLanche.Valor.ToString();
                DetailsLanche_txtBox.Text = selectedLanche.Detalhes;
            }
        }

        private async void UpdateLanche_Btn_Click(object sender, RoutedEventArgs e)
        {
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

                    string updateCommand = @"UPDATE Lanches 
                                            SET Nome = @Nome, Categoria = @Categoria, 
                                                       Ingredientes = @Ingredientes, Valor = @Valor, 
                                                       Detalhes = @Detalhes 
                                                       WHERE Id = @Id";

                    using (SqliteCommand query = new SqliteCommand(updateCommand, db))
                    {
                        query.Parameters.AddWithValue("@Nome", NameLanche_txtBox.Text);
                        query.Parameters.AddWithValue("@Categoria", CategoryLanche_txtBox.Text);
                        query.Parameters.AddWithValue("@Ingredientes", IngredientsLanche_txtBox.Text);
                        query.Parameters.AddWithValue("@Valor", Convert.ToDecimal(PriceLanche_txtBox.Text));
                        query.Parameters.AddWithValue("@Detalhes", DetailsLanche_txtBox.Text);
                        query.Parameters.AddWithValue("@Id", IDLanche_txtBox.Text); // Certifique-se de ter o Id do item que quer atualizar

                        try
                        {
                            query.ExecuteNonQuery();
                            Console.WriteLine("Lanche atualizado com sucesso!");
                            Frame.Navigate(typeof(ManagementWindow));
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Erro ao atualizar lanche: {ex.Message}");
                        }
                    }
                }
            }

        }

        private async void DeleteLanche_Btn_Click(object sender, RoutedEventArgs e)
        {
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

                    string deleteCommand = @"DELETE FROM Lanches WHERE Id = @Id";

                    using (SqliteCommand query = new SqliteCommand(deleteCommand, db))
                    {
                        query.Parameters.AddWithValue("@Id", IDLanche_txtBox.Text); // Certifique-se de ter o Id do item que quer excluir

                        try
                        {
                            query.ExecuteNonQuery();
                            Console.WriteLine("Lanche excluído com sucesso!");
                            Frame.Navigate(typeof(ManagementWindow));
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Erro ao excluir lanche: {ex.Message}");
                        }
                    }
                }
            }

        }

        // CATEGORIA =====================

        private void CategoryPageItem_Tapped(object sender, TappedRoutedEventArgs e)
        {
            CategoryGrid.Visibility = Visibility.Visible;
            contentFrame.Visibility = Visibility.Collapsed;
            LanchesGrid.Visibility = Visibility.Collapsed;
            IngredientsGrid.Visibility = Visibility.Collapsed;
            FillCategoryListBox();
        }

        public class Category
        {
            public int id { get; set; }
            public string category{ get; set; }
        }

        private async void FillCategoryListBox()
        {
            StorageFolder localFolder = ApplicationData.Current.LocalFolder;
            IStorageItem dbFile = await localFolder.TryGetItemAsync("Database.db");

            if (dbFile != null && dbFile.IsOfType(StorageItemTypes.File))
            {
                StorageFile myDBFile = (StorageFile)dbFile;
                string dbPath = myDBFile.Path;

                using (SqliteConnection db = new SqliteConnection($"Filename={dbPath}"))
                {
                    db.Open();
                    var items = new List<Category>();
                    string selectCommand = $@"
                            SELECT ID, Categoria FROM Categorias";

                    using (SqliteCommand query = new SqliteCommand(selectCommand, db))
                    {
                        using (SqliteDataReader reader = query.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                items.Add(new Category
                                {
                                    id = reader.GetInt32(0),
                                    category = reader.GetString(1)
                                });
                            }
                            CategoryListBox.ItemsSource = items.Select(i => i.category).ToList();
                            CategoryListBox.Tag = items;
                        }
                    }
                }
            }
        }

        private void CategoryListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedName = CategoryListBox.SelectedItem.ToString();
            var items = (List<Category>)CategoryListBox.Tag;
            var selectedCategory = items.FirstOrDefault(item => item.category == selectedName);

            if (selectedCategory != null)
            {
                IDCategory_txtBox.Text = selectedCategory.id.ToString();
                NameCategory_txtBox.Text = selectedCategory.category;
            }
        }

        private async void UpdateCategory_Btn_Click(object sender, RoutedEventArgs e)
        {
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

                    string updateCommand = @"UPDATE Categorias
                                            SET Categoria = @Categoria
                                            WHERE Id = @Id";

                    using (SqliteCommand query = new SqliteCommand(updateCommand, db))
                    {
                        string a = NameCategory_txtBox.Text;
                        string b = IDCategory_txtBox.Text;

                        query.Parameters.AddWithValue("@Categoria", NameCategory_txtBox.Text);
                        query.Parameters.AddWithValue("@Id", IDCategory_txtBox.Text); // Certifique-se de ter o Id do item que quer atualizar

                        try
                        {
                            query.ExecuteNonQuery();
                            Console.WriteLine("Categoria atualizada com sucesso!");
                            Frame.Navigate(typeof(ManagementWindow));
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Erro ao atualizar Categoria: {ex.Message}");
                        }
                    }
                }
            }
        }

        private async void DeleteCategory_Btn_Click(object sender, RoutedEventArgs e)
        {
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

                    string deleteCommand = @"DELETE FROM Categorias WHERE Id = @Id";

                    using (SqliteCommand query = new SqliteCommand(deleteCommand, db))
                    {
                        query.Parameters.AddWithValue("@Id", IDCategory_txtBox.Text); // Certifique-se de ter o Id do item que quer excluir

                        try
                        {
                            query.ExecuteNonQuery();
                            Console.WriteLine("Lanche excluído com sucesso!");
                            Frame.Navigate(typeof(ManagementWindow));
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Erro ao excluir lanche: {ex.Message}");
                        }
                    }
                }
            }
        }

        // INGREDIENTES =====================

        private void IngredientsPageItem_Tapped(object sender, TappedRoutedEventArgs e)
        {
            IngredientsGrid.Visibility = Visibility.Visible;
            contentFrame.Visibility = Visibility.Collapsed;
            LanchesGrid.Visibility = Visibility.Collapsed;
            CategoryGrid.Visibility = Visibility.Collapsed;
            FillIngredientsListBox();
        }

        public class Ingredients
        {
            public int id { get; set; }
            public string name { get; set; }
            public string price { get; set; }
        }

        public async void FillIngredientsListBox()
        {
            StorageFolder localFolder = ApplicationData.Current.LocalFolder;
            IStorageItem dbFile = await localFolder.TryGetItemAsync("Database.db");

            if (dbFile != null && dbFile.IsOfType(StorageItemTypes.File))
            {
                StorageFile myDBFile = (StorageFile)dbFile;
                string dbPath = myDBFile.Path;

                using (SqliteConnection db = new SqliteConnection($"Filename={dbPath}"))
                {
                    db.Open();
                    var items = new List<Ingredients>();
                    string selectCommand = $@"
                            SELECT ID, Nome, Valor from ingredientes";

                    using (SqliteCommand query = new SqliteCommand(selectCommand, db))
                    {
                        using (SqliteDataReader reader = query.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                items.Add(new Ingredients
                                {
                                    id = reader.GetInt32(0),
                                    name = reader.GetString(1),
                                    price = reader.GetString(2)
                                });
                            }
                            IngredientsListBox.ItemsSource = items.Select(i => i.name).ToList();
                            IngredientsListBox.Tag = items;
                        }
                    }
                }
            }
        }

        private void IngredientsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedName = IngredientsListBox.SelectedItem.ToString();
            var items = (List<Ingredients>)IngredientsListBox.Tag;
            var selectedIngredient = items.FirstOrDefault(item => item.name == selectedName);

            if (selectedIngredient != null)
            {
                IDIngredients_txtBox.Text = selectedIngredient.id.ToString();
                NameIngredients_txtBox.Text = selectedIngredient.name.ToString();
                PriceIngredients_txtBox.Text = selectedIngredient.price;
            }
        }

        private async void UpdateIngredients_Btn_Click(object sender, RoutedEventArgs e)
        {
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

                    string updateCommand = @"UPDATE Ingredientes
                                            SET Nome = @Nome, Valor = @Valor
                                            WHERE id = @id";

                    using (SqliteCommand query = new SqliteCommand(updateCommand, db))
                    {
                        string a = NameIngredients_txtBox.Text;
                        string b = PriceIngredients_txtBox.Text;

                        query.Parameters.AddWithValue("@Nome", NameIngredients_txtBox.Text);
                        query.Parameters.AddWithValue("@Valor", PriceIngredients_txtBox.Text); // Certifique-se de ter o Id do item que quer atualizar
                        query.Parameters.AddWithValue("@id", IDIngredients_txtBox.Text);

                        try
                        {
                            query.ExecuteNonQuery();
                            Console.WriteLine("Categoria atualizada com sucesso!");
                            Frame.Navigate(typeof(ManagementWindow));
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Erro ao atualizar Categoria: {ex.Message}");
                        }
                    }
                }
            }
        }

        private async void DeleteIngredients_Btn_Click(object sender, RoutedEventArgs e)
        {
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

                    string deleteCommand = @"DELETE FROM Ingredientes WHERE Id = @Id";

                    using (SqliteCommand query = new SqliteCommand(deleteCommand, db))
                    {
                        query.Parameters.AddWithValue("@Id", IDIngredients_txtBox.Text); // Certifique-se de ter o Id do item que quer excluir

                        try
                        {
                            query.ExecuteNonQuery();
                            Console.WriteLine("Ingrediente excluído com sucesso!");
                            Frame.Navigate(typeof(ManagementWindow));
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Erro ao excluir lanche: {ex.Message}");
                        }
                    }
                }
            }
        }








        private void AcessPageItem_Tapped(object sender, TappedRoutedEventArgs e)
        {
            contentFrame.Visibility = Visibility.Collapsed;
            LanchesGrid.Visibility = Visibility.Visible;
            //FillAccessListBox();
        }
    }
}
