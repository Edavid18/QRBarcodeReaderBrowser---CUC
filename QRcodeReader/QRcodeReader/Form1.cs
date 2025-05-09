using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AForge;
using AForge.Video;
using ZXing;
using ZXing.QrCode;
using System.Text.RegularExpressions;
using AForge.Video.DirectShow;
using System.Linq.Expressions;
using Microsoft.Web.WebView2.Core;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using ZXing.Windows.Compatibility;


namespace QRcodeReader
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        //Agregamos referencias
        private FilterInfoCollection Dispositivos;
        private VideoCaptureDevice Camara;
        public static bool ValidateUrl(string url) // METODO CON EXPRESION REGULAR QUE VALIDA LAS URL
        {
            if (url == null || url == "") return false;
            Regex oRegExp = new Regex(@"(http|ftp|https)://([\w-]+\.)+(/[\w- ./?%&=]*)?", RegexOptions.IgnoreCase);

            return oRegExp.Match(url).Success;
        }
        private void Camara_NewFrame(object sender, NewFrameEventArgs
       eventArgs)
        {
            pictureBox1.Image = (Bitmap)eventArgs.Frame.Clone();
        }



        private void Form1_Load(object sender, EventArgs e)
        {
            int i = 0; // Contadora
                       // Listamos dispositivos
            Dispositivos = new
           FilterInfoCollection(FilterCategory.VideoInputDevice);
            //Cargamos dispositivos al combobox
            foreach (FilterInfo Device in Dispositivos)
            {
                //Contadora de camaras
                i = i + 1;
                comboBox1.Items.Add(Device.Name);
            }
            comboBox1.SelectedIndex = -1;
            if (i == 0) // Si no hay camara deshabilitar el boton
            {
                button1.Enabled = false;

            }
            else // Si hay camara el boton se habilita
            {
                button1.Enabled = true;
            }
            // Iniciar control de video
            Camara = new VideoCaptureDevice();
        }

        private void button1_Click(object sender, EventArgs e)
        {

            if (comboBox1.SelectedIndex == -1)
            {
                MessageBox.Show("No hay ningún dispositivo seleccionado.");
                return;
            }
            //Establecemos el dispositivo seleccionado como fuente de video
            Camara = new
            VideoCaptureDevice(Dispositivos[comboBox1.SelectedIndex].MonikerString);
            Camara.NewFrame += new
            NewFrameEventHandler(Camara_NewFrame);
            //Iniciar recepcion de video
            Camara.Start();
            // Habilitamos y empezamos el timer
            timer1.Enabled = true;
            timer1.Start();

        }


        private void timer1_Tick(object sender, EventArgs e)
        {
            if (pictureBox1.Image != null)
            {
                Bitmap image = (Bitmap)pictureBox1.Image.Clone();
                image.RotateFlip(RotateFlipType.Rotate180FlipX);

                BarcodeReader Reader = new BarcodeReader();
                Result result = Reader.Decode(image);
                image.Dispose(); // Dispose of the cloned image

                try
                {
                    if (result != null)
                    {
                        string decoded = result.ToString();

                        if (!string.IsNullOrEmpty(decoded))
                        {
                            timer1.Stop(); // Stop the timer

                            // ✅ Stop and fully release the camera
                            if (Camara != null)
                            {
                                if (Camara.IsRunning)
                                {
                                    Camara.SignalToStop();
                                    Camara.WaitForStop();
                                }

                                Camara.NewFrame -= Camara_NewFrame; // 🛑 Detach event handler
                                Camara = null;
                            }

                            // ✅ Dispose of the PictureBox image
                            if (pictureBox1.Image != null)
                            {
                                pictureBox1.Image.Dispose();
                                pictureBox1.Image = null;
                            }

                            if (!ValidateUrl(decoded))
                            {
                                MessageBox.Show("El código QR no corresponde a una URL, pero contiene: " + decoded);
                            }
                            else
                            {
                                webView21.Source = new Uri(decoded);
                                MessageBox.Show("Navegando a " + decoded);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error al procesar el código QR: " + ex.Message);
                }
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                if (timer1.Enabled)
                {
                    timer1.Stop();
                    timer1.Dispose();
                }

                // ✅ Fully shut down the camera
                if (Camara != null)
                {
                    if (Camara.IsRunning)
                    {
                        Camara.SignalToStop();
                        Camara.WaitForStop();
                    }

                    Camara.NewFrame -= Camara_NewFrame; // Detach event handler
                    Camara = null;
                }


                if (pictureBox1.Image != null)
                {
                    pictureBox1.Image.Dispose();
                    pictureBox1.Image = null;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cerrar: " + ex.Message);
            }

            Application.Exit();
        }


        private void label1_Click(object sender, EventArgs e)
        {

        }
    }
}