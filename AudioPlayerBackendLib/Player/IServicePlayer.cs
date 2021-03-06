﻿using System;
using AudioPlayerBackend.Audio;

namespace AudioPlayerBackend.Player
{
    public interface IServicePlayer : IDisposable
    {
        IAudioService Service { get; }

        IPlayer Player { get; }
    }
}
