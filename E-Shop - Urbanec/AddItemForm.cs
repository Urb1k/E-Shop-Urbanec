using System;
using System.Data.SQLite;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace E_Shop___Urbanec
{
    public partial class AddItemForm : Form
    {
        private SQLiteConnection connection;

        public AddItemForm(SQLiteConnection connection)
        {
            InitializeComponent();
            this.connection = connection;
        }

        private void btnSave_Click_1(object sender, EventArgs e)
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(txtName.Text) || string.IsNullOrWhiteSpace(textQuantity.Text))
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

            // Save the image to a byte array
            byte[] imageBytes = null;
            if (pictureBox.Image != null)
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    pictureBox.Image.Save(ms, pictureBox.Image.RawFormat);
                    imageBytes = ms.ToArray();
                }
            }

            // Insert new product into database
            string insertQuery = "INSERT INTO Products (Name, Quantity, Image, HasCustomImage) VALUES (@name, @quantity, @image, @hasCustomImage);";
            SQLiteCommand command = new SQLiteCommand(insertQuery, connection);
            command.Parameters.AddWithValue("@name", txtName.Text);
            command.Parameters.AddWithValue("@quantity", quantity);
            command.Parameters.AddWithValue("@image", imageBytes != null ? (object)imageBytes : DBNull.Value);
            command.Parameters.AddWithValue("@hasCustomImage", imageBytes != null ? 1 : 0);
            command.ExecuteNonQuery();

            MessageBox.Show("Product added successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            this.DialogResult = DialogResult.OK;
        }

        private void btnBrowse_Click_1(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.gif;*.bmp";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                pictureBox.Image = new Bitmap(openFileDialog.FileName);
            }
        }
    }
}
