﻿using System.Linq;
using AudioPlayerBackend.Audio;

namespace AudioPlayerBackend.Data
{
    public class SourcePlaylistData : PlaylistData
    {
        public string[] Sources { get; set; }

        public SourcePlaylistData() { }

        public SourcePlaylistData(ISourcePlaylistBase playlist) : base(playlist)
        {
            Sources = playlist.FileMediaSources.ToArray();
        }
    }
}
