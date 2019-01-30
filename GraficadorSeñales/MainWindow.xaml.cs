using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using NAudio.Wave;

namespace GraficadorSeñales
{
    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        double amplitudMaxima = 1;
        Señal señal;
   
        Señal señalResultado;

        public MainWindow()
        {
            InitializeComponent();

            
            
           
        }

        private void btnGraficar_Click(object sender, RoutedEventArgs e)
        {

            AudioFileReader reader = new AudioFileReader(txtRutaArchivo.Text);

            double tiempoInicial = 0;
            double tiempoFinal =
                reader.TotalTime.TotalSeconds;
            double frecuenciaMuestreo =
                reader.WaveFormat.SampleRate;

            txtFrecuenciaMuestreo.Text = frecuenciaMuestreo.ToString();
            txtTiempoInicial.Text = "0";
            txtTiempoFinal.Text = tiempoFinal.ToString();

            señal = new SeñalPersonalizada();

           
            señal.TiempoInicial = tiempoInicial;
            señal.TiempoFinal = tiempoFinal;
            señal.FrecuenciaMuestreo = frecuenciaMuestreo;

            //Construir nuestra señal a travez del archivo de audio
            var bufferLectura = new float[reader.WaveFormat.Channels];
            int muestrasLeidas = 1;

            double instanteActual = 0;
            double intervaloMuestra = 1.0 / frecuenciaMuestreo;

            do
            {
                muestrasLeidas = reader.Read(bufferLectura, 0, reader.WaveFormat.Channels);
                if (muestrasLeidas > 0)
                {
                    double max = bufferLectura.Take(muestrasLeidas).Max();
                    señal.Muestras.Add(new Muestra(instanteActual, max));
                }
                instanteActual += intervaloMuestra;
            } while (muestrasLeidas > 0);

            señal.actualizarAmplitudMaxima();
   
                amplitudMaxima = señal.AmplitudMaxima;
            
            plnGrafica.Points.Clear();
            

            lblAmplitudMaximaY.Text = señal.AmplitudMaxima.ToString("F");
            lblAmplitudMaximaNegativaY.Text = "-" + amplitudMaxima.ToString("F");
            if (señal != null)
            {
                //Recorrer una  coleccion o arreglo
                foreach (Muestra muestra in señal.Muestras)
                {

                    plnGrafica.Points.Add(new Point((muestra.X - tiempoInicial) * scrContenedor.Width, ((muestra.Y / amplitudMaxima) * ((scrContenedor.Height / 2.0) - 30) * -1)
                    + (scrContenedor.Height / 2)));
                }
               
            }
          

            plnEjeX.Points.Clear();
            //Punto del Principio
            plnEjeX.Points.Add(new Point(0, (scrContenedor.Height / 2)));
            //Punto del Fin
            plnEjeX.Points.Add(new Point(((tiempoFinal - tiempoInicial) * scrContenedor.Width), (scrContenedor.Height / 2)));

            //Punto del Principio
            plnEjeY.Points.Add(new Point(0 - tiempoInicial * scrContenedor.Width, scrContenedor.Height));
            //Punto del Fin
            plnEjeY.Points.Add(new Point(0-tiempoInicial*scrContenedor.Width,scrContenedor.Height*-1));

      



            

        }

        private void btnGraficarRampa_Click(object sender, RoutedEventArgs e)
        {
            //todas las señales ocupan estas 3
            double tiempoInicial =
                double.Parse(txtTiempoInicial.Text);
            double tiempoFinal =
                double.Parse(txtTiempoFinal.Text);
            double frecuenciaMuestreo =
                double.Parse(txtFrecuenciaMuestreo.Text);

            SeñalRampa señal =
                new SeñalRampa();

            double periodoMuestreo = 1 / frecuenciaMuestreo;

            plnGrafica.Points.Clear();

            for (double i = tiempoInicial; i <= tiempoFinal; i += periodoMuestreo)
            {
                double valorMuestra = señal.evaluar(i);

                señal.Muestras.Add(new Muestra(i, valorMuestra));
                //Recorrer una  coleccion o arreglo Aqui se agregan los puntos
                
            }
            //Recorrer una  coleccion o arreglo Aqui se agregan los puntos
            foreach (Muestra muestra in señal.Muestras)
            {
                plnGrafica.Points.Add(new Point(muestra.X * scrContenedor.Width, (muestra.Y * ((scrContenedor.Height / 2.0) - 30) * -1)
                + (scrContenedor.Height / 2)));
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Señal transformada = Señal.transformar(señal);

            transformada.actualizarAmplitudMaxima();

            plnGraficaResultado.Points.Clear();

            double res;

            lblAmplitudMaximaY_Resultado.Text = transformada.AmplitudMaxima.ToString("F");
            lblAmplitudMaximaNegativaY_Resultado.Text = "-" + transformada.AmplitudMaxima.ToString("F");
            


            if (transformada != null)
            {
                //Recorrer una  coleccion o arreglo
                foreach (Muestra muestra in transformada.Muestras)
                {

                    plnGraficaResultado.Points.Add(new Point((muestra.X - transformada.TiempoInicial) * scrContenedor_Resultado.Width, ((muestra.Y / transformada.AmplitudMaxima) * ((scrContenedor_Resultado.Height / 2.0) - 30) * -1)
                    + (scrContenedor_Resultado.Height / 2)));
                }

                double valorMaximo = 0;
                int indiceMaximo = 0;
                int indiceActual = 0;
                foreach (Muestra muestra in transformada.Muestras)
                {
                    if(muestra.Y > valorMaximo)
                    {
                        valorMaximo = muestra.Y;
                        indiceMaximo = indiceActual;
                    }
                    indiceActual++;
                    if(indiceActual > (double)transformada.Muestras.Count / 2.0)
                    {
                        break;
                    }
                }


                //calcular frecuencia fundamental
                double frecuenciaFundamental = (double)indiceMaximo * señal.FrecuenciaMuestreo / (double)transformada.Muestras.Count;
                lblFrecuenciaFundamental.Text = frecuenciaFundamental.ToString() + " Hz";
            }



            plnEjeXResultado.Points.Clear();
            //Punto del Principio
            plnEjeXResultado.Points.Add(new Point(0, (scrContenedor_Resultado.Height / 2)));
            //Punto del Fin
            plnEjeXResultado.Points.Add(new Point(((transformada.TiempoFinal - transformada.TiempoInicial) * scrContenedor_Resultado.Width), (scrContenedor_Resultado.Height / 2)));

            //Punto del Principio
            plnEjeYResultado.Points.Add(new Point(0 - transformada.TiempoInicial * scrContenedor_Resultado.Width, scrContenedor_Resultado.Height));
            //Punto del Fin
            plnEjeYResultado.Points.Add(new Point(0 - transformada.TiempoInicial * scrContenedor_Resultado.Width, scrContenedor_Resultado.Height * -1));


        }

        private void btnExaminar_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fileDiaog = new OpenFileDialog();

            fileDiaog.ShowDialog();

            if((bool)fileDiaog.ShowDialog())
            {
                txtRutaArchivo.Text = fileDiaog.FileName;
            }

        }
    }
}
