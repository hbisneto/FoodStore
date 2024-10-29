using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
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

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace FoodStore
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ManagerAddCategory : Page
    {
        public ManagerAddCategory()
        {
            this.InitializeComponent();
        }

        private void backButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(ManagementWindow));
        }

        private async void addTodb_button_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(itemName_txtBox.Text))
            {
                itemName_txtBox.Background = new SolidColorBrush(Windows.UI.Colors.Red);
                return;
            }
            else
            {
                itemName_txtBox.Background = new SolidColorBrush(Windows.UI.Colors.Black);
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

                    string insertCommand = @"INSERT INTO Categorias (Categoria) 
                                            VALUES (@Categoria)";
                    using (SqliteCommand query = new SqliteCommand(insertCommand, db))
                    {
                        query.Parameters.AddWithValue("@Categoria", itemName_txtBox.Text);

                        try
                        {
                            query.ExecuteNonQuery();
                            Console.WriteLine("Categoria adicionada com sucesso!");
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
    }
}
