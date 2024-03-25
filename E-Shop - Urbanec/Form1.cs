using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace E_Shop___Urbanec
{
    public partial class Form1 : Form
    {
        private SQLiteConnection connection;
        private SQLiteDataAdapter dataAdapter;
        private DataTable dataTable;
        private List<Product> products;

        //inicializace Form1
        public Form1()
        {
            InitializeComponent();

            string dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "eshop.db");
            connection = new SQLiteConnection($"Data Source={dbPath};Version=3;");
            connection.Open();

            //nastavení databáze
            string createTableQuery = @"CREATE TABLE IF NOT EXISTS Products (
                                            ID INTEGER PRIMARY KEY AUTOINCREMENT,
                                            Name TEXT NOT NULL,
                                            Quantity INTEGER NOT NULL,
                                            Image BLOB,
                                            Price REAL,
                                            HasCustomImage INTEGER DEFAULT 0
                                        );";
            SQLiteCommand command = new SQLiteCommand(createTableQuery, connection);
            command.ExecuteNonQuery();
            //nahrání věcí z databáze do objektů
            products = LoadProductsFromDatabase();
            dataGridViewProducts.DataSource = products;

            //nastavení dataGridView
            if (dataGridViewProducts.Columns.Count > 0)
            {
                dataGridViewProducts.Columns["ID"].Visible = false;
                dataGridViewProducts.Columns["Image"].Width = 100;
                dataGridViewProducts.Columns["Name"].HeaderText = "Product Name";
                dataGridViewProducts.Columns["Quantity"].HeaderText = "Available Quantity";
                dataGridViewProducts.Columns["Image"].Visible = false;
                dataGridViewProducts.Columns["HasCustomImage"].HeaderText = "Has Image";
                dataGridViewProducts.Columns["Price"].HeaderText = "Price";
            }
        }
        //uložení z databáze
        private List<Product> LoadProductsFromDatabase()
        {
            dataAdapter = new SQLiteDataAdapter("SELECT * FROM Products", connection);
            dataTable = new DataTable();
            dataAdapter.Fill(dataTable);

            List<Product> products = new List<Product>();

            foreach (DataRow row in dataTable.Rows)
            {
                Product product = new Product
                {
                    ID = Convert.ToInt32(row["ID"]),
                    Name = row["Name"].ToString(),
                    Quantity = Convert.ToInt32(row["Quantity"]),
                    Image = row["Image"] == DBNull.Value ? File.ReadAllBytes(Properties.Resources.thereIsNoPicture) : (byte[])row["Image"],
                    Price = Convert.ToDecimal(row["Price"]),
                    HasCustomImage = Convert.ToInt32(row["HasCustomImage"]) == 1 ? true : false
                };
                products.Add(product);
            }

            return products;
        }
        //otevření ItemFormu a nahrání do objektů
        private void btnAddItem_Click_1(object sender, EventArgs e)
        {
            ItemForm addItemForm = new ItemForm(connection);
            if (addItemForm.ShowDialog() == DialogResult.OK)
            {
                products = LoadProductsFromDatabase();
                dataGridViewProducts.DataSource = products;
            }
        }
        //otevření ItemFormu a nahrání do Objektů
        private void editButton_Click(object sender, EventArgs e)
        {
            if (dataGridViewProducts.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a product to edit.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            int selectedIndex = dataGridViewProducts.SelectedRows[0].Index;
            Product selectedProduct = products[selectedIndex];

            ItemForm editForm = new ItemForm(connection, selectedProduct);
            if (editForm.ShowDialog() == DialogResult.OK)
            {
                products = LoadProductsFromDatabase();
                dataGridViewProducts.DataSource = products;
            }
        }
        //vymazání dat z SQLite a vymazání z objektů
        private void deleteButton_Click(object sender, EventArgs e)
        {
            if (dataGridViewProducts.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a product to delete.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            int selectedIndex = dataGridViewProducts.SelectedRows[0].Index;
            Product selectedProduct = products[selectedIndex];

            string deleteQuery = "DELETE FROM Products WHERE ID = @id";
            SQLiteCommand deleteCommand = new SQLiteCommand(deleteQuery, connection);
            deleteCommand.Parameters.AddWithValue("@id", selectedProduct.ID);
            deleteCommand.ExecuteNonQuery();

            products.RemoveAt(selectedIndex);

            dataGridViewProducts.DataSource = null;
            dataGridViewProducts.DataSource = products;
            dataGridViewProducts.Columns["ID"].Visible = false;
            dataGridViewProducts.Columns["Image"].Visible = false;
        }

    }
}
