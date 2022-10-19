using System;
using System.Collections.Generic;
using System.Text;

namespace MusicPlayer
{
    public class Song
    {
        private string SongName { get; set; }
        private string Artist { get; set; }
        private string AlbumName { get; set; }
        private string AlbumArtist { get; set; }
        private int SongNumber { get; set; }
        private string Genre { get; set; }
        private bool Liked { get; set; }
        private string FilePath { get; set; }
        private int Bitrate { get; set; }
        private string FileExtension { get; set; }

    }
}
