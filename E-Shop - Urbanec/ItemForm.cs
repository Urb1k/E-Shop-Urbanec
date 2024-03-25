using System;
using System.Data.SQLite;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace E_Shop___Urbanec
{
    public partial class ItemForm : Form
    {
        private Product product;
        private SQLiteConnection connection;
        private bool isEditMode;
        private bool changedPicture = false;

        //inicializace ItemFormu
        public ItemForm(SQLiteConnection connection, Product product = null)
        {
            InitializeComponent();
            
            this.connection = connection;

            //nastavení velikosti okna
            this.Size = new Size(625, 400);

            //nastavení zda je to editing nebo adding button
            if (product != null)
            {
                isEditMode = true;
                this.product = product;

                txtName.Text = product.Name;
                textQuantity.Text = product.Quantity.ToString();
                txtPrice.Text = product.Price.ToString();
                pictureBox.Image = product.Image != null ? ByteArrayToImage(product.Image) : null;
            }
            else
            {
                isEditMode = false;
                this.product = new Product();
            }
        }

        private Image ByteArrayToImage(byte[] byteArray)
        {
            using (MemoryStream ms = new MemoryStream(byteArray))
            {
                return Image.FromStream(ms);
            }
        }
        //kontrola hodnot
        private void btnSave_Click_1(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text) || string.IsNullOrWhiteSpace(textQuantity.Text) || string.IsNullOrWhiteSpace(txtPrice.Text))
            {
                MessageBox.Show("Please fill all the fields.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            int quantity;
            if (!int.TryParse(textQuantity.Text, out quantity) || quantity <= 0)
            {
                MessageBox.Show("Please enter a valid quantity.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            decimal price;
            if (!decimal.TryParse(txtPrice.Text, out price) || price <= 0)
            {
                MessageBox.Show("Please enter a valid price.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            byte[] imageBytes = null;
            bool hasCustomImage = changedPicture;
            //ukládání a logika obrázků
            if (changedPicture)
            {
                if (pictureBox.Image != null)
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        pictureBox.Image.Save(ms, pictureBox.Image.RawFormat);
                        imageBytes = ms.ToArray();
                    }
                }
            }
            else
            {
                byte[] defaultImageBytes = File.ReadAllBytes(Properties.Resources.thereIsNoPicture);
                if (ByteArraysEqual(product.Image, defaultImageBytes))
                {
                    hasCustomImage = false; 
                }
                else
                {
                    if (isEditMode)
                    {
                        hasCustomImage = true;
                    }
                    
                    imageBytes = product.Image;
                }
            }
            //vložení do SQLite
            string query;
            if (isEditMode)
            {
                query = "UPDATE Products SET Name = @name, Quantity = @quantity, Image = @image, Price = @price, HasCustomImage = @hasCustomImage WHERE ID = @id";
            }
            else
            {
                query = "INSERT INTO Products (Name, Quantity, Image, Price, HasCustomImage) VALUES (@name, @quantity, @image, @price, @hasCustomImage)";
            }

            SQLiteCommand command = new SQLiteCommand(query, connection);
            command.Parameters.AddWithValue("@name", txtName.Text);
            command.Parameters.AddWithValue("@quantity", quantity);
            command.Parameters.AddWithValue("@image", imageBytes != null ? (object)imageBytes : DBNull.Value);
            command.Parameters.AddWithValue("@price", price);
            command.Parameters.AddWithValue("@hasCustomImage", hasCustomImage ? 1 : 0);
            if (isEditMode)
            {
                command.Parameters.AddWithValue("@id", product.ID);
            }

            command.ExecuteNonQuery();

            if (!isEditMode)
            {
                MessageBox.Show("Product added successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("Product updated successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            this.DialogResult = DialogResult.OK;
        }

        //hledání obrázku v průzkumníku
        private void btnBrowse_Click_1(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.gif;*.bmp";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                pictureBox.Image = new Bitmap(openFileDialog.FileName);
                changedPicture = true;
            }
        }

        //kontrola obrázku zda jsou stejní
        private bool ByteArraysEqual(byte[] array1, byte[] array2)
        {
            if (array1 == null || array2 == null)
            {
                return false;
            }

            if (array1.Length != array2.Length)
            {
                return false;
            }

            for (int i = 0; i < array1.Length; i++)
            {
                if (array1[i] != array2[i])
                {
                    return false;
                }
            }

            return true;
        }
    }
}
