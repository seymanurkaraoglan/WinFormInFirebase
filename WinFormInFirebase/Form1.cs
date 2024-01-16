using FireSharp.Interfaces;
using FireSharp.Config;
using FireSharp.Response;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Drawing.Imaging;

namespace WinFormInFirebase
{
    public partial class Form1 : Form
    {
        DataTable dt = new DataTable();

        IFirebaseConfig config = new FirebaseConfig
        {
            AuthSecret = "oOyTAmULkJzgbVVBD309p7HS9aNNavGWIVEaFqwm",
            BasePath = "https://winforminfirebase-default-rtdb.firebaseio.com/"
        };
        IFirebaseClient client;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            client = new FireSharp.FirebaseClient(config);
            if(client!= null ) {
                MessageBox.Show("Connection is successfull");
            }

            dt.Columns.Add("Id");
            dt.Columns.Add("Name");
            dt.Columns.Add("Address");
            dt.Columns.Add("Age");
            dt.Columns.Add("Image",typeof(Image));

            dataGridView1.DataSource = dt;
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            FirebaseResponse resp = await client.GetAsync("Counter/node");
            Counter_class get = resp.ResultAs<Counter_class>();

            MemoryStream ms = new MemoryStream();
            //seçtiğimiz image'i istenen formatta streame kaydediyor.
            imageBox.Image.Save(ms, ImageFormat.Jpeg);
            byte[] a = ms.GetBuffer();

            string output = Convert.ToBase64String(a);

            var data = new Data
            {
                Id = (Convert.ToInt32(get.cnt)+1).ToString(),
                Name = textBox2.Text,
                Address = textBox3.Text,
                Age = textBox4.Text,
                Img = output
            };
            SetResponse response = await client.SetAsync("Information/" + data.Id, data);
            Data result = response.ResultAs<Data>();

            MessageBox.Show("Data Inserted" + result.Id);
            var obj = new Counter_class
            {
                cnt = data.Id
            };
            SetResponse response1 = await client.SetAsync("Counter/node", obj);
        }

        private async void button2_Click(object sender, EventArgs e)
        {
            FirebaseResponse response = await client.GetAsync("Information/" + textBox1.Text);
            Data obj = response.ResultAs<Data>();
            textBox1.Text = obj.Id;
            textBox2.Text = obj.Name;
            textBox3.Text = obj.Address;
            textBox4.Text = obj.Age;

            MessageBox.Show("Data Retrieved successfully!");
        }

        private async void button3_Click(object sender, EventArgs e)
        {
            var data = new Data
            {
                Id = textBox1.Text,
                Name = textBox2.Text,
                Address = textBox3.Text,
                Age = textBox4.Text
            };

            FirebaseResponse response = await client.UpdateAsync("Information/" + textBox1.Text, data);
            Data result = response.ResultAs<Data>();

            MessageBox.Show("Data Updated at Id : " + result.Id);
        }

        private async void button4_Click(object sender, EventArgs e)
        {
            FirebaseResponse response = await client.DeleteAsync("Information/" + textBox1.Text);

            MessageBox.Show("Deleted record of Id : " + textBox1.Text);
        }

        private async void button5_Click(object sender, EventArgs e)
        {
            FirebaseResponse response = await client.DeleteAsync("Information");

            MessageBox.Show("All elements deleted / Information node has been deleted" + textBox1.Text);
        }

        private void button6_Click(object sender, EventArgs e)
        {
            export();
        }
        private async void export()
        {

            dt.Rows.Clear();
            int i = 0;
            FirebaseResponse resp1 = await client.GetAsync("Counter/node");
            Counter_class obj1 = resp1.ResultAs<Counter_class>();
            int cnt = Convert.ToInt32(obj1.cnt);
            while (true)
            {
                if(i == cnt)
                {
                    break;
                }
                i++;
                try
                {
                    FirebaseResponse resp2 = await client.GetAsync("Information/" + i);
                    Data obj2 = resp2.ResultAs<Data>();


                    DataRow row = dt.NewRow();
                    row["Id"] = obj2.Id;
                    row["Name"] = obj2.Name;
                    row["Address"] = obj2.Address;
                    row["Age"] = obj2.Age;

                   

                    byte[] b = Convert.FromBase64String(obj2.Img);
                    MemoryStream ms = new MemoryStream();
                    ms.Write(b, 0, Convert.ToInt32(b.Length));

                    Bitmap bm = new Bitmap(ms, false);
                    row["Image"] = bm;
                    dt.Rows.Add(row);
                }
                catch
                {

                }
                
            }
            MessageBox.Show("Done");
        }

        private void button7_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "Select image";
            ofd.Filter = "Image Files (*.jpg) | *.jpg";

            if(ofd.ShowDialog() == DialogResult.OK)
            {
                Image img = new Bitmap(ofd.FileName);
                imageBox.Image = img.GetThumbnailImage(350,200,null,new IntPtr());
            }

        }

        private async void button8_Click(object sender, EventArgs e)
        {
            MemoryStream ms = new MemoryStream();
            //seçtiğimiz image'i istenen formatta streame kaydediyor.
            imageBox.Image.Save(ms, ImageFormat.Jpeg);
            byte[] a = ms.GetBuffer();

            string output = Convert.ToBase64String(a);
            var data = new Image_Model
            {
                Img = output
            };

            SetResponse response = await client.SetAsync("Image/", data);
            Image_Model result = response.ResultAs<Image_Model>();
            imageBox.Image = null;
            MessageBox.Show("Image Inserted");
        }

        private async void button9_Click(object sender, EventArgs e)
        {
            //retrieve base64 -> byte[] -> ms -> bitemap
            FirebaseResponse response = await client.GetAsync("Image/");
            Image_Model image = response.ResultAs<Image_Model>();

            byte[] b = Convert.FromBase64String(image.Img);
            MemoryStream ms = new MemoryStream();
            ms.Write(b,0, Convert.ToInt32(b.Length));

            Bitmap bm = new Bitmap(ms, false);
            ms.Dispose();
            imageBox.Image = bm;

        }
    }
}
