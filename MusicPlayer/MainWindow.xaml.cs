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
using System.Diagnostics;
using NAudio;
using NAudio.Wave;

namespace MusicPlayer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private AudioFileReader audioFile;
        private WaveOutEvent outputDevice;

        public MainWindow()
        {
            InitializeComponent();
            outputDevice = new WaveOutEvent();
        }

        //Allows user to select a song to play from their files
        private void SelectFile(object sender, EventArgs e)
        {
            Trace.WriteLine("Deezy McNuts");

            // Create OpenFileDialog 
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            // Set filter for file extension and default file extension 
            dlg.DefaultExt = ".png";
            dlg.Filter = "MP3 Files (*.mp3)|*.mp3|FLAC Files (*.flac)|*.flac|M4A Files (*.m4a)|*.m4a";

            // Display OpenFileDialog by calling ShowDialog method 
            bool? result = dlg.ShowDialog();

            // Get the selected file name and display in a TextBlock 
            if (result == true)
            {
                // Open file
                string filename = dlg.FileName;
                FilenameTextBlock.Text = filename;
                PlaySong(filename);
            }
        }

        //Stops the current song and plays a new one
        private void PlaySong(string filename)
        {
            if(audioFile != null)
            {
                outputDevice.Stop();
            }

            audioFile = new AudioFileReader(filename);
            outputDevice.Init(audioFile);
            outputDevice.Play();
        }
    }
}
