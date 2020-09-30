﻿using System.Threading.Tasks;

namespace AudioPlayerBackend.Audio
{
    public interface ISourcePlaylist : ISourcePlaylistBase, IPlaylist
    {
        Task Update();

        Task Reload();
    }
}
