using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NAudio.Wave;
using NAudio.FileFormats;
using NAudio.CoreAudioApi;
using NAudio;

namespace Audio
{
    public partial class Form1 : Form
    {
        /// <summary>
        /// Stream for record
        /// </summary>
        WaveIn waveIn;

        /// <summary>
        /// Class for writing in file
        /// </summary>
        WaveFileWriter writer;

        /// <summary>
        /// Name of file
        /// </summary>
        string outputFilename = "NoName.wav";

        public Form1()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Get data from input buffer
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void waveIn_DataAvailable(object sender, WaveInEventArgs e)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new EventHandler<WaveInEventArgs>(waveIn_DataAvailable), sender, e);
            }
            else
            {
                //Writing data in file from buffer
                writer.WriteData(e.Buffer, 0, e.BytesRecorded);
            }
        }


        //Stop recording
        private void waveIn_RecordingStopped(object sender, EventArgs e)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new EventHandler(waveIn_RecordingStopped), sender, e);
            }
            else
            {
                waveIn.Dispose();
                waveIn = null;
                writer.Close();
                writer = null;
            }
        }
        
        /// <summary>
        /// Start recording
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                var saveDialog = new SaveFileDialog {Filter = "WAVE| *.wav"};
                if (saveDialog.ShowDialog() != DialogResult.OK) return;
                outputFilename = saveDialog.FileName;

                waveIn = new WaveIn();
                //Number device of recording by default (laptop microphone = 0)
                waveIn.DeviceNumber = 0;
                //Attach to event the DataAvailable handler, arising  if  the write data is
                waveIn.DataAvailable += waveIn_DataAvailable;
                //Attach handler of stoping of a record 
                waveIn.RecordingStopped += new EventHandler<StoppedEventArgs>(waveIn_RecordingStopped);
                //Format of the file - get parametrs - a sampling frequency and number of channels(mono)
                waveIn.WaveFormat = new WaveFormat(8000, 1);
                //Initialize the object WaveFileWriter
                writer = new WaveFileWriter(outputFilename, waveIn.WaveFormat);
                //Start recording
                waveIn.StartRecording();
            }
            catch (Exception ex) //Bad practice X-/
            {
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// Break the recording
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {
            waveIn?.StopRecording();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            var openFileDialog = new OpenFileDialog {Filter = "Wave file (*.wav)|*.wav"};
            if (openFileDialog.ShowDialog() != DialogResult.OK) return;

            chart1.Series.Add("wave");
            chart1.Series["wave"].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.FastLine;
            chart1.Series["wave"].ChartArea = "ChartArea1";
            
            var wave = new WaveChannel32(new WaveFileReader(openFileDialog.FileName));
            byte[] buffer = new byte[16384];
            int read = 0;
            while (wave.Position < wave.Length)
            {
                read = wave.Read(buffer, 0, 16384);
                for (int i = 0; i<read/4; i++)
                {
                    chart1.Series["wave"].Points.Add(BitConverter.ToSingle(buffer, i*4));
                }
            }
            waveViewer1.WaveStream = new NAudio.Wave.WaveFileReader(openFileDialog.FileName);

        }
    }
}
