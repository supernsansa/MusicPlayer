using System;
using System.Windows;
using System.Diagnostics;
using NAudio.Wave;
using System.Drawing;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MusicPlayer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private AudioFileReader audioFile;
        private WaveOutEvent outputDevice;
        private bool playing;

        public MainWindow()
        {
            InitializeComponent();
            outputDevice = new WaveOutEvent();
            playing = false;
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

            // Get the selected file and display song name in a TextBlock 
            if (result == true)
            {
                // Open file
                string filename = dlg.FileName;
                //Extract metadata
                //Song title
                TagLib.File tfile = TagLib.File.Create(@filename);
                string songTitle = tfile.Tag.Title;
                //Album Art
                bool picFound = false;

                //Try to find album art
                foreach (TagLib.IPicture pic in tfile.Tag.Pictures)
                {
                    //If found, display
                    if (pic != null)
                    {
                        try
                        {
                            MemoryStream ms = new MemoryStream(pic.Data.Data);
                            BitmapImage imageSource = new BitmapImage();
                            imageSource.BeginInit();
                            imageSource.StreamSource = ms;
                            imageSource.EndInit();
                            AlbumArt.Source = imageSource;
                            picFound = true;
                            break;
                        }
                        catch(NotSupportedException)
                        {
                            continue;
                        }
                    }
                }
                //Otherwise, display default image
                if(!picFound)
                {
                    AlbumArt.Source = new BitmapImage(new Uri("default.png",UriKind.Relative));
                }

                if (songTitle != null)
                {
                    FilenameTextBlock.Text = songTitle;
                }
                else
                {
                    FilenameTextBlock.Text = filename;
                }

                if(audioFile != null)
                {
                    outputDevice.Stop();
                    PlayButton.Content = "Play";
                    playing = false;
                }

                audioFile = new AudioFileReader(filename);
                outputDevice.Init(audioFile);
            }
        }

        //Plays and pauses the currently playing song
        private void PlayPause(object sender, EventArgs e)
        {
            //If no song is loaded, do nothing
            if(audioFile == null)
            {
                return;
            }

            //Otherwise, play or pause the music
            if(playing == false)
            {
                //Play
                outputDevice.Play();
                PlayButton.Content = "Pause";
                playing = true;
            }
            else
            {
                //Pause
                outputDevice.Pause();
                PlayButton.Content = "Play";
                playing = false;
            }
        }
    }
}
