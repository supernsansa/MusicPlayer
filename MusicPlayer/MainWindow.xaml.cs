using System;
using System.Windows;
using System.Diagnostics;
using NAudio.Wave;
using System.Drawing;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using NAudio.Utils;
using System.ComponentModel;

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
        private int lastTime;
        //private double currentTime;
        private double songLength;
        private BackgroundWorker progressWorker;

        public MainWindow()
        {
            InitializeComponent();
            outputDevice = new WaveOutEvent();
            playing = false;
            lastTime = 0;
            songLength = 0;
            //currentTime = 0;
        }

        //Allows user to select a song to play from their files
        private void SelectFile(object sender, EventArgs e)
        {
            // Create OpenFileDialog 
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog
            {
                // Set filter for file extension and default file extension 
                DefaultExt = ".png",
                Filter = "MP3 Files (*.mp3)|*.mp3|FLAC Files (*.flac)|*.flac|M4A Files (*.m4a)|*.m4a"
            };

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
                //Artist(s)
                string artists = tfile.Tag.JoinedPerformers;
                //Album name
                string albumTitle = tfile.Tag.Album;

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

                //Set song title text
                if (songTitle != null)
                { SongName.Text = songTitle; }
                else
                { SongName.Text = filename; }

                //Set song artist text
                if (artists != null)
                { ArtistName.Text = artists; }
                else 
                { ArtistName.Text = "Unknown"; }

                //Set album text
                if (albumTitle != null)
                { AlbumName.Text = albumTitle; }
                else
                { AlbumName.Text = "Unknown"; }
                

                //If a song is already loaded, stop it and reset play/pause button
                if (audioFile != null)
                {
                    outputDevice.Stop();
                    PlayButton.Content = "Play";
                    playing = false;
                    songLength = new MediaFoundationReader(filename).TotalTime.TotalMilliseconds;
                    PrintDebug(songLength.ToString());
                    ProgressSlider.Maximum = (int) songLength;
                    ProgressSlider.Value = 0;
                    lastTime = 0;
                }

                audioFile = new AudioFileReader(filename);
                outputDevice.Init(audioFile);

                //Song length in seconds
                songLength = new MediaFoundationReader(filename).TotalTime.TotalMilliseconds;
                PrintDebug("Song Length: " + songLength.ToString());
                //Set slider max to song length
                ProgressSlider.Maximum = (int) songLength;
                ProgressSlider.Value = 0;
                PrintDebug("Slider current value: " + ProgressSlider.Value.ToString());
                PrintDebug("Slider max value: " + ProgressSlider.Maximum.ToString());
            }
        }

        //Plays and pauses the currently playing song
        private void PlayPause(object sender, EventArgs e)
        {
            //If no song is loaded, do nothing
            if(audioFile == null) { return; }

            //Otherwise, play or pause the music
            if(playing == false)
            {
                //Play
                outputDevice.Play();
                PlayButton.Content = "Pause";
                playing = true;
                //Create backgroundworker to handle moving the slider
                PrintDebug("Starting worker");
                progressWorker = new BackgroundWorker();
                progressWorker.WorkerReportsProgress = true;
                progressWorker.DoWork += CheckPlaytime;
                progressWorker.ProgressChanged += UpdateProgressSlider;
                progressWorker.RunWorkerCompleted += UpdatePlayButton;
                progressWorker.RunWorkerAsync();
            }
            else
            {
                //Pause
                outputDevice.Pause();
                PlayButton.Content = "Play";
                playing = false;
                lastTime = (int) outputDevice.GetPositionTimeSpan().TotalMilliseconds;
            }
        }

        //Check if a second has passed
        private void CheckPlaytime(object sender, DoWorkEventArgs e)
        {
            while(playing)
            {
                double currentTime = outputDevice.GetPositionTimeSpan().TotalMilliseconds;
                //Check if song has progressed by 100 or more milliseconds
                if (currentTime - lastTime >= 100)
                {
                    PrintDebug("time has passed");
                    lastTime = (int) currentTime;
                    PrintDebug("Current Time: " + lastTime.ToString());
                    (sender as BackgroundWorker).ReportProgress((int) currentTime);
                }
                //If song has ended, stop playing
                if (currentTime >= songLength)
                {
                    lastTime = (int)currentTime;
                    PrintDebug("Current Time: " + lastTime.ToString());
                    PrintDebug("Song ended");
                    playing = false;
                    (sender as BackgroundWorker).ReportProgress((int) songLength);
                }
            }
            PrintDebug("Stopping worker");
        }

        //Updates progress slider UI element when thread has progress
        private void UpdateProgressSlider(object sender, ProgressChangedEventArgs e)
        {
            PrintDebug("Updating slider");
            ProgressSlider.Value = lastTime;
            PrintDebug(ProgressSlider.Value.ToString());
        }

        //Update play/pause button when thread completes
        private void UpdatePlayButton(object sender, RunWorkerCompletedEventArgs e)
        {
            if(playing) { PlayButton.Content = "Pause"; }
            else { PlayButton.Content = "Play"; }
        }

        //Prints string into debug output with formatting
        private void PrintDebug(string text)
        {
            //Console.ForegroundColor = ConsoleColor.Yellow;
            System.Diagnostics.Debug.WriteLine("DEBUG --->>> " + text);
            //Console.ResetColor();
        }
    }
}
