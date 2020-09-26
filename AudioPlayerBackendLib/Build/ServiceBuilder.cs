﻿using System;
using AudioPlayerBackend.Audio;
using AudioPlayerBackend.Communication;
using AudioPlayerBackend.Communication.MQTT;
using AudioPlayerBackend.Player;
using StdOttStandard.CommandlineParser;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using AudioPlayerBackend.Communication.OwnTcp;
using AudioPlayerBackend.Data;
using StdOttStandard.Linq;

namespace AudioPlayerBackend.Build
{
    public class ServiceBuilder : INotifyPropertyChanged
    {
        private readonly IServiceBuilderHelper helper;
        private bool ifNon, reload;
        private bool? isAllShuffle, isSearchShuffle, play, isStreaming;
        private int serverPort;
        private int? clientPort;
        private string searchKey, serverAddress, dataReadFile, dataWriteFile;
        private float? volume;
        private string[] mediaSources;
        private CommunicatorProtocol communicatorProtocol;
        private IWaveProviderPlayer player;

        public bool BuildStandalone { get; private set; }

        public bool BuildServer { get; private set; }

        public bool BuildClient { get; private set; }

        public CommunicatorProtocol CommunicatorProtocol
        {
            get => communicatorProtocol;
            set
            {
                if (value == communicatorProtocol) return;

                communicatorProtocol = value;
                OnPropertyChanged(nameof(CommunicatorProtocol));
            }
        }

        public bool IfNon
        {
            get => ifNon;
            set
            {
                if (value == ifNon) return;

                ifNon = value;
                OnPropertyChanged(nameof(IfNon));
            }
        }

        public bool Reload
        {
            get => reload;
            set
            {
                if (value == reload) return;

                reload = value;
                OnPropertyChanged(nameof(Reload));
            }
        }

        public bool? IsAllShuffle
        {
            get => isAllShuffle;
            set
            {
                if (value == isAllShuffle) return;

                isAllShuffle = value;
                OnPropertyChanged(nameof(IsAllShuffle));
            }
        }

        public bool? IsSearchShuffle
        {
            get => isSearchShuffle;
            set
            {
                if (value == isSearchShuffle) return;

                isSearchShuffle = value;
                OnPropertyChanged(nameof(IsSearchShuffle));
            }
        }

        public string SearchKey
        {
            get => searchKey;
            set
            {
                if (value == searchKey) return;

                searchKey = value;
                OnPropertyChanged(nameof(SearchKey));
            }
        }

        public bool? Play
        {
            get => play;
            set
            {
                if (value == play) return;

                play = value;
                OnPropertyChanged(nameof(Play));
            }
        }

        public bool? IsStreaming
        {
            get => isStreaming;
            set
            {
                if (value == isStreaming) return;

                isStreaming = value;
                OnPropertyChanged(nameof(IsStreaming));
            }
        }

        public int ServerPort
        {
            get => serverPort;
            set
            {
                if (value == serverPort) return;

                serverPort = value;
                OnPropertyChanged(nameof(ServerPort));
            }
        }

        public int? ClientPort
        {
            get => clientPort;
            set
            {
                if (value == clientPort) return;

                clientPort = value;
                OnPropertyChanged(nameof(ClientPort));
            }
        }

        public string ServerAddress
        {
            get => serverAddress;
            set
            {
                if (value == serverAddress) return;

                serverAddress = value;
                OnPropertyChanged(nameof(ServerAddress));
            }
        }

        public string DataReadFile
        {
            get => dataReadFile;
            set
            {
                if (value == dataReadFile) return;

                dataReadFile = value;
                OnPropertyChanged(nameof(DataReadFile));
            }
        }

        public string DataWriteFile
        {
            get => dataWriteFile;
            set
            {
                if (value == dataWriteFile) return;

                dataWriteFile = value;
                OnPropertyChanged(nameof(DataWriteFile));
            }
        }

        public float? Volume
        {
            get => volume;
            set
            {
                if (value == volume) return;

                volume = value;
                OnPropertyChanged(nameof(Volume));
            }
        }

        public string[] MediaSources
        {
            get => mediaSources;
            set
            {
                if (value.BothNullOrSequenceEqual(mediaSources)) return;

                mediaSources = value;
                OnPropertyChanged(nameof(MediaSources));
            }
        }

        public IWaveProviderPlayer Player
        {
            get => player;
            set
            {
                if (value == player) return;

                player = value;
                OnPropertyChanged(nameof(Player));
            }
        }

        public ServiceBuilder(IServiceBuilderHelper helper = null)
        {
            this.helper = helper;

            WithStandalone();
        }

        public ServiceBuilder WithArgs(IEnumerable<string> args)
        {
            Option clientOpt = new Option("c", "client", "Starts the app as client with the following server address and port", false, 3, 2);
            Option serverOpt = new Option("s", "server", "Starts the app as server with the following port", false, 2, 2);
            Option sourcesOpt = new Option("m", "media-sources", "Files and directories to play", false, -1, 0);
            Option ifNonOpt = new Option("i", "if-non", "If given the Media sources are only used if there are non", false, 0, 0);
            Option reloadOpt = new Option("r", "reload", "Forces to reload", false, 0, 0);
            Option allShuffleOpt = Option.GetLongOnly("all-shuffle", "Shuffles all songs.", false, 0, 0);
            Option searchShuffleOpt = Option.GetLongOnly("search-shuffle", "Shuffles all songs.", false, 0, 0);
            Option searchKeyOpt = Option.GetLongOnly("search-key", "Shuffles all songs.", false, 1, 0);
            Option playOpt = new Option("p", "play", "Starts playback on startup", false, 0, 0);
            Option serviceVolOpt = new Option("v", "volume", "The volume of service (value between 0 and 1)", false, 1, 1);
            Option streamingOpt = Option.GetLongOnly("stream", "If given the audio is streamed to the client", false, 0, 0);
            Option dataFileReadOpt = Option.GetLongOnly("data-file-read", "Filepath to where to read data from.", false, 1, 1);
            Option dataFileWriteOpt = Option.GetLongOnly("data-file-write", "Filepath to where to write data from.", false, 1, 1);

            Options options = new Options(sourcesOpt, ifNonOpt, reloadOpt, clientOpt, serverOpt, playOpt,
                allShuffleOpt, searchShuffleOpt, searchKeyOpt, serviceVolOpt, streamingOpt, dataFileReadOpt, dataFileWriteOpt);
            OptionParseResult result = options.Parse(args);

            OptionParsed parsed;

            if (result.TryGetFirstValidOptionParseds(serverOpt, out parsed))
            {
                WithCommunicatorProtocol((CommunicatorProtocol)Enum
                        .Parse(typeof(CommunicatorProtocol), parsed.Values[0], true))
                    .WithServer(int.Parse(parsed.Values[1]));
            }

            if (result.TryGetFirstValidOptionParseds(clientOpt, out parsed))
            {
                WithCommunicatorProtocol((CommunicatorProtocol)Enum
                    .Parse(typeof(CommunicatorProtocol), parsed.Values[0], true));

                if (parsed.Values.Count > 2) WithClient(parsed.Values[1], int.Parse(parsed.Values[2]));
                else WithClient(parsed.Values[1]);
            }

            if (result.HasValidOptionParseds(sourcesOpt))
            {
                WithMediaSources(result.GetValidOptionParseds(sourcesOpt).SelectMany(p => p.Values).ToArray());
            }

            if (result.TryGetFirstValidOptionParseds(allShuffleOpt, out parsed)) WithIsAllShuffle();
            if (result.TryGetFirstValidOptionParseds(searchShuffleOpt, out parsed)) WithIsSearchShuffle();
            if (result.TryGetFirstValidOptionParseds(searchKeyOpt, out parsed)) WithSearchKey(parsed.Values.FirstOrDefault());
            if (result.HasValidOptionParseds(ifNonOpt)) WithSetMediaIfNon();
            if (result.HasValidOptionParseds(reloadOpt)) WithReload();
            if (result.HasValidOptionParseds(playOpt)) WithPlay();

            if (result.TryGetFirstValidOptionParseds(serviceVolOpt, out parsed)) WithVolume(float.Parse(parsed.Values[0]));

            if (result.HasValidOptionParseds(streamingOpt)) WithIsStreaming();

            if (result.TryGetFirstValidOptionParseds(dataFileReadOpt, out parsed)) DataReadFile = parsed.Values[0];
            if (result.TryGetFirstValidOptionParseds(dataFileWriteOpt, out parsed)) DataWriteFile = parsed.Values[0];

            return this;
        }

        public ServiceBuilder WithService(IAudioService service)
        {
            return WithMediaSources(service.SourcePlaylist.FileMediaSources)
                .WithIsAllShuffle(service.SourcePlaylist.IsAllShuffle)
                .WithIsSearchShuffle(service.SourcePlaylist.IsSearchShuffle)
                .WithSearchKey(service.SourcePlaylist.SearchKey)
                //.WithPlay(service.PlayState == PlaybackState.Playing)
                .WithReload(false)
                .WithSetMediaIfNon(false)
                .WithVolume(service.Volume);
        }

        public ServiceBuilder WithStandalone()
        {
            BuildStandalone = true;
            BuildServer = false;
            BuildClient = false;

            return this;
        }

        public ServiceBuilder WithServer(int port)
        {
            BuildStandalone = false;
            BuildServer = true;
            BuildClient = false;

            return WithServerPort(port);
        }

        public ServiceBuilder WithCommunicatorProtocol(CommunicatorProtocol communicatorProtocol)
        {
            CommunicatorProtocol = communicatorProtocol;
            return this;
        }

        public ServiceBuilder WithMqtt()
        {
            CommunicatorProtocol = CommunicatorProtocol.MQTT;
            return this;
        }

        public ServiceBuilder WithOwnTcp()
        {
            CommunicatorProtocol = CommunicatorProtocol.OwnTCP;
            return this;
        }

        public ServiceBuilder WithServerPort(int port)
        {
            ServerPort = port;

            return this;
        }

        public ServiceBuilder WithClient(string serverAddress, int? port = null)
        {
            BuildStandalone = false;
            BuildServer = false;
            BuildClient = true;

            return WithServerAddress(serverAddress).WithClientPort(port);
        }

        public ServiceBuilder WithCommunicator(ICommunicator communicator)
        {
            if (communicator is IClientCommunicator)
            {
                IClientCommunicator clientCommunicator = (IClientCommunicator)communicator;
                return WithClient(clientCommunicator.ServerAddress, clientCommunicator.Port);
            }
            else if (communicator is IServerCommunicator)
            {
                IServerCommunicator serverCommunicator = (IServerCommunicator)communicator;
                return WithServer(serverCommunicator.Port);
            }

            return WithStandalone();
        }

        public ServiceBuilder WithServerAddress(string serverAddress)
        {
            ServerAddress = serverAddress;

            return this;
        }

        public ServiceBuilder WithClientPort(int? port)
        {
            ClientPort = port;

            return this;
        }

        public ServiceBuilder WithMediaSources(string[] mediaSources)
        {
            MediaSources = mediaSources;

            return this;
        }

        public ServiceBuilder WithSetMediaIfNon(bool value = true)
        {
            IfNon = value;

            return this;
        }

        public ServiceBuilder WithIsAllShuffle(bool? value = true)
        {
            IsAllShuffle = value;

            return this;
        }

        public ServiceBuilder WithIsSearchShuffle(bool? value = true)
        {
            IsSearchShuffle = value;

            return this;
        }

        public ServiceBuilder WithSearchKey(string value)
        {
            SearchKey = value;

            return this;
        }

        public ServiceBuilder WithReload(bool value = true)
        {
            Reload = value;

            return this;
        }

        public ServiceBuilder WithPlay(bool? value = true)
        {
            Play = value;

            return this;
        }

        public ServiceBuilder WithVolume(float? volume)
        {
            Volume = volume;

            return this;
        }

        public ServiceBuilder WithIsStreaming(bool? value = true)
        {
            IsStreaming = value;

            return this;
        }

        public ServiceBuilder WithPlayer(IWaveProviderPlayer player)
        {
            Player = player;

            return this;
        }

        public ICommunicator CreateCommunicator()
        {
            switch (CommunicatorProtocol)
            {
                case CommunicatorProtocol.MQTT:
                    if (BuildServer) return CreateMqttServerCommunicator(ServerPort);
                    if (BuildClient) return CreateMqttClientCommunicator(ServerAddress, ClientPort);
                    break;

                case CommunicatorProtocol.OwnTCP:
                    if (BuildServer) return CreateOwnTcpServerCommunicator(ServerPort);
                    if (BuildClient) return CreateOwnTcpClientCommunicator(ServerAddress, ClientPort ?? 1884);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }


            return null;
        }

        public IServicePlayer CreateServicePlayer(IAudioService service)
        {
            if (BuildClient) return CreateAudioStreamPlayer(Player, service);
            return CreateAudioServicePlayer(Player, service);
        }

        public ReadWriteAudioServiceData CompleteService(IAudioService service)
        {
            bool setMediaSources = mediaSources != null && (!ifNon || !service.SourcePlaylist.FileMediaSources.ToNotNull().Any());
            if (setMediaSources && !mediaSources.BothNullOrSequenceEqual(service.SourcePlaylist.FileMediaSources))
            {
                service.SourcePlaylist.FileMediaSources = mediaSources;
            }
            else if (reload) service.SourcePlaylist.Reload();

            if (IsAllShuffle.HasValue) service.SourcePlaylist.IsAllShuffle = IsAllShuffle.Value;
            if (IsSearchShuffle.HasValue) service.SourcePlaylist.IsSearchShuffle = IsSearchShuffle.Value;
            if (SearchKey != null) service.SourcePlaylist.SearchKey = SearchKey;
            if (play.HasValue) service.PlayState = play.Value ? PlaybackState.Playing : PlaybackState.Paused;
            if (volume.HasValue) service.Volume = volume.Value;

            return new ReadWriteAudioServiceData(DataReadFile, DataWriteFile, service, helper);
        }

        protected virtual MqttClientCommunicator CreateMqttClientCommunicator(string serverAddress, int? port)
        {
            return new MqttClientCommunicator(serverAddress, port, helper);
        }

        protected virtual MqttServerCommunicator CreateMqttServerCommunicator(int port)
        {
            return new MqttServerCommunicator(port, helper);
        }

        protected virtual OwnTcpClientCommunicator CreateOwnTcpClientCommunicator(string serverAddress, int port)
        {
            return new OwnTcpClientCommunicator(serverAddress, port, helper);
        }

        protected virtual OwnTcpServerCommunicator CreateOwnTcpServerCommunicator(int port)
        {
            return new OwnTcpServerCommunicator(port, helper);
        }

        protected virtual AudioStreamPlayer CreateAudioStreamPlayer(IWaveProviderPlayer player, IAudioService service)
        {
            return helper.CreateAudioStreamPlayer(player, service);
        }

        protected virtual AudioServicePlayer CreateAudioServicePlayer(IWaveProviderPlayer player, IAudioService service)
        {
            return helper.CreateAudioServicePlayer(player, service);
        }

        public ServiceBuilder Clone()
        {
            return new ServiceBuilder(helper)
            {
                BuildClient = BuildClient,
                BuildServer = BuildServer,
                BuildStandalone = BuildStandalone,
                ClientPort = ClientPort,
                IfNon = ifNon,
                IsAllShuffle = IsAllShuffle,
                IsSearchShuffle = IsSearchShuffle,
                IsStreaming = IsStreaming,
                MediaSources = MediaSources?.ToArray(),
                Play = Play,
                Player = Player,
                Reload = Reload,
                SearchKey = SearchKey,
                ServerAddress = ServerAddress,
                ServerPort = ServerPort,
                Volume = Volume
            };
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string name)
        {
            if (PropertyChanged == null) return;

            if (helper?.InvokeDispatcher != null) helper.InvokeDispatcher(Raise);
            else Raise();

            void Raise() => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
