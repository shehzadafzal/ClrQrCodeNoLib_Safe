using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        private const string ConnectionString = "Data Source=(local);Initial Catalog=db;User ID=sa;Password=pass";

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            int scale;
            Int32.TryParse(this.textBox2.Text, out scale);

            string query = String.Format(@"SELECT dbo.GetQrCode('{0}',{1}) AS QrCodeImage", this.textBox1.Text, scale);

            // Create a new SQL connection using the connection string
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                // Create a new SQL command using the query and connection
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    // Open the SQL connection
                    connection.Open();

                    // Execute the SQL command and retrieve the query result
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            // Get the image data from the query result
                            byte[] imageData = (byte[])reader["QrCodeImage"];


                            //File.WriteAllBytes(@"c:\del\test.bmp", imageData);



                            // Convert the image data to a Bitmap
                            Bitmap newBitmap;
                            using (MemoryStream memoryStream = new MemoryStream(imageData))
                            {
                                newBitmap = new Bitmap(memoryStream);
                            }

                            // Display the image in the PictureBox control
                            pictureBox1.Image = newBitmap;


                        }
                    }
                }
            }
        }
    }
}
