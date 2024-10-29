using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
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
    public sealed partial class AuthWindow : Page
    {
        public AuthWindow()
        {
            this.InitializeComponent();
        }

        private async void btn_Login_Click(object sender, RoutedEventArgs e)
        {
            string input_username = txtBox_Username.Text;
            string input_password = pwdBox_Password.Password;
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

                    string selectCommand = @"
                        SELECT b.id AS 'privilegio_id', b.nome AS 'privilegio_nome', a.login, a.senha 
                        FROM auth a
                        JOIN authprivilegios b
                        ON b.id = a.authprivilegio_id";

                    using (SqliteCommand query = new SqliteCommand(selectCommand, db))
                    {
                        using (SqliteDataReader reader = query.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string _privilegioId = reader.GetString(0);
                                string _privilegioNome = reader.GetString(1);
                                string _username = reader.GetString(2);
                                string _password = reader.GetString(3);

                                if (_username == input_username)
                                {
                                    if (_password == input_password)
                                    {
                                        Console.WriteLine("Usuário LOGADO!");

                                        switch (_privilegioId)
                                        {
                                            case "1":
                                                Console.WriteLine("Admin");
                                                Frame.Navigate(typeof(ManagementWindow));
                                                break;
                                            case "2":
                                                Console.WriteLine("usuário");
                                                Frame.Navigate(typeof(Cardapio));
                                                break;
                                            case "3":
                                                Console.WriteLine("Moderador");
                                                break;
                                        }
                                    }
                                }
                                else
                                {
                                    Console.WriteLine("Informações de Login não condizem!");
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
