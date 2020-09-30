﻿using AudioPlayerBackend.Player;

namespace AudioPlayerFrontend.Join
{
    class PlayerHelper : NotifyPropertyChangedHelper, IAudioStreamHelper
    {
        private static PlayerHelper instance;

        public static PlayerHelper Current
        {
            get
            {
                if (instance == null) instance = new PlayerHelper();

                return instance;
            }
        }

        private PlayerHelper() { }
    }
}
