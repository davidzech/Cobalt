using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using CobaltCore.Ctcp;
using CobaltCore.Network;
using JetBrains.Annotations;

namespace Cobalt.Core.Irc
{
    /// <summary>
    /// Describes all possible states of an IrcSession object.
    /// </summary>
    public enum IrcConnectionState
    {
        /// <summary>
        /// The session is in the process of connecting. Either the server connection has not been established yet,
        /// or the user has not been registered.
        /// </summary>
        Connecting,

        /// <summary>
        /// The user has been registered with the IRC server and and has chosen a nickname. Commands can now be accepted.
        /// </summary>
        Connected,

        /// <summary>
        /// The session is not connected to any IRC server.
        /// </summary>
        Disconnected
    }

    /// <summary>
    /// Responsible for creating and maintaining a single IRC session, which consists of a connection to one IRC server. IRC activity is
    /// processed via this class and propagated to consuming objects through events. Methods are exposed to send commands to the IRC server.
    /// </summary>
    public partial class IrcConnection : IDisposable
    {
        private const int ReconnectWaitTime = 5000;

        private string _password;
        private bool _isInvisible;
        private IrcConnectionState _state;
        private List<IrcCodeHandler> _captures = new List<IrcCodeHandler>();
        private bool _isWaitingForActivity;
        private bool _findExternalAddress;
        private Timer _reconnectTimer;
       // private Task _socketLoopTask;
        private CancellationTokenSource _wtoken;
        private TcpClient _tcpClient;

        /// <summary>
        /// Gets the server to which the session is connected or will connect.
        /// </summary>
        public string Hostname { get; private set; }

        /// <summary>
        /// Gets the server port.
        /// </summary>
        public int Port { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the session uses an encrypted (SSL) connection.

        /// </summary>

        /// <summary/>
        public bool IsSecure { get; private set; }
        /// Gets a value that determines whether or not the connection should allow insecure TLS Connections
        /// 
        public bool AcceptInsecureCertificate => true;

        /// <summary>
        /// Gets the current nickname or the desired nickname if the session is not connected.
        /// </summary>
        public string Nickname { get; private set; }

        /// <summary>
        /// Gets the username reported to the server.
        /// </summary>
        public string Username { get; private set; }

        /// <summary>
        /// Gets the full name reported to the server.
        /// </summary>
        public string FullName { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether the session should automatically attempt to reconnect if it is disconnected.
        /// </summary>
        public bool AutoReconnect { get; set; }

        /// <summary>
        /// Force ignore AutoReconnect. Set this to true when manually disconnecting
        /// </summary>
        public bool ForceDisconnect { get; set; }

        /// <summary>
        /// Gets the name of the IRC network to which the client is connected. By default, this will simply be the server name but
        /// may be updated when the network name is determined.
        /// </summary>
        public string NetworkName { [UsedImplicitly] get; private set; }

        /// <summary>
        /// Gets the current set of user modes that apply to the session.
        /// </summary>
        public char[] UserModes { get; private set; }

        /// <summary>
        /// Gets the internal IP address of the computer running the session. This is the private IP address that is used behind
        /// a NAT firewall.
        /// </summary>
        public IPAddress InternalAddress { [UsedImplicitly] get; private set; }

        /// <summary>
        /// Gets the external IP address of the computer running the session. The IRC server is queried to retrieve the address or hostname.
        /// If a hostname is returned, the IP address is retrieved via DNS. If no external address can be found, the local IP address
        /// is provided.
        /// </summary>
        public IPAddress ExternalAddress { [UsedImplicitly] get; private set; }

        /// <summary>
        /// Gets or sets proxy information, identifying the SOCKS5 proxy server to use when connecting to a server.
        /// </summary>
        public ProxyInfo Proxy { get; set; }

        /// <summary>
        /// Gets the current state of the session.
        /// </summary>
        public IrcConnectionState State
        {
            get
            {
                return _state;
            }
            private set
            {
                if (_state != value)
                {
                    _state = value;
                    OnStateChanged();
                }
            }
        }

        /// <summary>
        /// Fires when the state of the session has changed.
        /// </summary>
        public event EventHandler<EventArgs> StateChanged;

        /// <summary>
        /// Fires when a connection error has occurred and the session must close.
        /// </summary>
        public event EventHandler<ErrorEventArgs> ConnectionError;

        /// <summary>
        /// Fires when any message has been received.
        /// </summary>
        public event EventHandler<IrcEventArgs> RawMessageReceived;

        /// <summary>
        /// Fires when any message has been sent.
        /// </summary>
        public event EventHandler<IrcEventArgs> RawMessageSent;

        /// <summary>
        /// Fires when another user has changed their nickname. Nick changes are only visible if the user is on a channel
        /// that the session is currently joined to.
        /// </summary>
        public event EventHandler<IrcNickEventArgs> NickChanged;

        /// <summary>
        /// Fires when the session nickname has changed. This may be a result of a nick change command or a forced nickname change.
        /// </summary>
        public event EventHandler<IrcNickEventArgs> SelfNickChanged;

        /// <summary>
        /// Fires when a private message has been received, either via a channel or directly from another user (a PM).
        /// </summary>
        public event EventHandler<IrcMessageEventArgs> PrivateMessaged;

        /// <summary>
        /// Fires when a notice message has been received, either via a channel or directly from another user.
        /// </summary>
        public event EventHandler<IrcMessageEventArgs> Noticed;

        /// <summary>
        /// Fires when another user has quit.
        /// </summary>
        public event EventHandler<IrcQuitEventArgs> UserQuit;

        /// <summary>
        /// Fires when another user has joined a channel.
        /// </summary>
        public event EventHandler<IrcJoinEventArgs> Joined;

        /// <summary>
        /// Fires when the session joins a channel.
        /// </summary>
        public event EventHandler<IrcJoinEventArgs> SelfJoined;

        /// <summary>
        /// Fires when another user has left a channel.
        /// </summary>
        public event EventHandler<IrcPartEventArgs> Parted;

        /// <summary>
        /// Fires when the session has left a channel.
        /// </summary>
        public event EventHandler<IrcPartEventArgs> SelfParted;

        /// <summary>
        /// Fires when the topic of a channel has been changed.
        /// </summary>
        public event EventHandler<IrcTopicEventArgs> TopicChanged;

        /// <summary>
        /// Fires when the session has been invited to a channel.
        /// </summary>
        public event EventHandler<IrcInviteEventArgs> Invited;

        /// <summary>
        /// Fires when a user has been kicked from a channel.
        /// </summary>
        public event EventHandler<IrcKickEventArgs> Kicked;

        /// <summary>
        /// Fires when the session has been kicked from a channel.
        /// </summary>
        public event EventHandler<IrcKickEventArgs> SelfKicked;

        /// <summary>
        /// Fires when a channel's modes have been changed.
        /// </summary>
        public event EventHandler<IrcChannelModeEventArgs> ChannelModeChanged;

        /// <summary>
        /// Fires when the session's user modes have been changed.
        /// </summary>
        public event EventHandler<IrcUserModeEventArgs> UserModeChanged;

        /// <summary>
        /// Fires when a miscellaneous numeric message was received from the server.
        /// </summary>
        public event EventHandler<IrcInfoEventArgs> InfoReceived;

        /// <summary>
        /// Fires when a CTCP command has been received from another user.
        /// </summary>
        public event EventHandler<CtcpEventArgs> CtcpCommandReceived;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public IrcConnection()
        {
            State = IrcConnectionState.Disconnected;
            UserModes = new char[0];

        }

        /// <summary>
        /// Connects to the IRC session and attempts to connect to a server. When the task completes, it means the Socket has been setup, but registration has not necessailry been finished
        /// </summary>
        /// <param name="hostname">The hostname or IP representation of a server.</param>
        /// <param name="port">The IRC port.</param>
        /// <param name="isSecure">True to use an encrypted (SSL) connection, false to use plain text.</param>
        /// <param name="nickname">The desired nickname.</param>
        /// <param name="userName">The username that will be shown to other users.</param>
        /// <param name="fullname">The full name that will be shown to other users.</param>
        /// <param name="autoReconnect">Indicates whether to automatically reconnect upon disconnection.</param>
        /// <param name="password">The optional password to supply while logging in.</param>
        /// <param name="invisible">Determines whether the +i flag will be set by default.</param>
        /// <param name="findExternalAddress">Determines whether to find the external IP address by querying the IRC server upon connect.</param>
        /// <param name="proxy">Socks Proxy Info</param>
        public async Task ConnectAsync(string hostname, int port, bool isSecure, string nickname,
            string userName, string fullname, bool autoReconnect, string password = null, bool invisible = false, bool findExternalAddress = true,
            ProxyInfo proxy = null)
        {
            if (string.IsNullOrEmpty(nickname))
            {
                throw new ArgumentNullException(nameof(nickname));
            }
            _password = password;
            _isInvisible = invisible;
            _findExternalAddress = findExternalAddress;
            Nickname = nickname;
            Hostname = hostname;
            Port = port;
            IsSecure = isSecure;
            Username = userName;
            FullName = fullname;
            NetworkName = Hostname; // just for now
            UserModes = new char[0];
            AutoReconnect = autoReconnect;
            Proxy = proxy;
            ForceDisconnect = false;

            await OpenSocketAsync().ConfigureAwait(false);
        }


        /// <summary>
        /// Disposes the session, closing any open connection.
        /// </summary>
        public void Dispose()
        {
            _tcpClient?.Close();
            _stream?.Dispose();
        }

        /// <summary>
        /// Determine whether the specified target refers to this session by comparing the nickname to the session's current nickname.
        /// </summary>
        /// <param name="target">The target to evaluate.</param>
        /// <returns>True if the target refers to this session, false otherwise.</returns>
        public bool IsSelf(IrcTarget target)
        {
            return target != null && !target.IsChannel &&
                string.Compare(target.Name, Nickname, StringComparison.OrdinalIgnoreCase) == 0;
        }

        /// <summary>
        /// Determines whether the specified nickname matches the session's current nickname.
        /// </summary>
        /// <param name="nick">The nickname to evaluate.</param>
        /// <returns>True if the nickname matches the session's current nickname, false otherwise.</returns>
        public bool IsSelf(string nick)
        {
            return string.Compare(Nickname, nick, StringComparison.OrdinalIgnoreCase) == 0;
        }

        /// <summary>
        /// Send a message to the server.
        /// </summary>
        /// <param name="message">The message to send.</param>
        public async Task SendAsync(IrcMessage message)
        {
            if (State != IrcConnectionState.Disconnected)
            {
                await PostMessageAsync(message);
            }
        }

        /// <summary>
        /// Send a message to the server.
        /// </summary>
        /// <param name="command">The name of the command.</param>
        /// <param name="parameters">The optional command parameters.</param>
        public async Task SendAsync(string command, params string[] parameters)
        {
            if (State != IrcConnectionState.Disconnected)
            {
                await PostMessageAsync(new IrcMessage(command, parameters));
            }
        }

        /// <summary>
        /// Send a message to the server.
        /// </summary>
        /// <param name="command">The name of the command.</param>
        /// <param name="target">The target of the command.</param>
        /// <param name="parameters">The optional command parameters.</param>
        public async Task SendAsync(string command, IrcTarget target, params string[] parameters)
        {
            await SendAsync(command, (new[] { target.ToString() }).Union(parameters).ToArray());
        }

        /// <summary>
        /// Sends a raw message to the server, no formatting
        /// </summary>
        /// <param name="command"></param>
        /// <param name="parameters"></param>
        public async Task SendRawAsync(string command, params string[] parameters)
        {
            if (State != IrcConnectionState.Disconnected)
            {
                await PostMessageAsync(new IrcMessage(true, null, command, parameters));
            }
        }

        /// <summary>
        /// Send a CTCP message to another client.
        /// </summary>
        /// <param name="target">The user to which the CTCP command will be delivered.</param>
        /// <param name="command">The CTCP command to send.</param>
        /// <param name="isResponse">Indicates whether the CTCP message is a response to a command that was received. This parameter
        /// is important for preventing an infinite back-and-forth loop between two clients.</param>
        public async Task SendCtcpAsync(IrcTarget target, CtcpCommand command, bool isResponse)
        {
            await SendAsync(isResponse ? "NOTICE" : "PRIVMSG", target, command.ToString());
        }

        /// <summary>
        /// Send the raw text to the server.
        /// </summary>
        /// <param name="rawText">The raw text to send. This should be in the format of a standard IRC message, per RFC 2812.</param>
        public async Task QuoteAsync(string rawText)
        {
            await SendAsync(new IrcMessage(rawText));
        }

        /// <summary>
        /// Change the nickname.
        /// </summary>
        /// <param name="newNickname">The new nickname.</param>
        public async Task NickAsync(string newNickname)
        {
            if (State == IrcConnectionState.Connecting || State == IrcConnectionState.Connected)
            {
                await SendAsync("NICK", newNickname);
            }
            else
            {
                Nickname = newNickname;
            }
        }

        /// <summary>
        /// Send a private message to a user or channel.
        /// </summary>
        /// <param name="target">The user or channel that the message will be delivered to.</param>
        /// <param name="text">The message text.</param>
        public async Task PrivateMessageAsync(IrcTarget target, string text)
        {
            await SendAsync("PRIVMSG", target, text);
        }

        /// <summary>
        /// Send a notice to a user or channel.
        /// </summary>
        /// <param name="target">The user or channel that the notice will be delivered to.</param>
        /// <param name="text">The notice text.</param>
        public async Task NoticeAsync(IrcTarget target, string text)
        {
            await SendAsync("NOTICE", target, text);
        }

        /// <summary>
        /// Quit from the server and close the connection.
        /// </summary>
        /// <param name="text">The optional quit text.</param>
        public async Task QuitAsync(string text)
        {
            //this.AutoReconnect = false;
            ForceDisconnect = true;
            if (State != IrcConnectionState.Disconnected)
            {
                await SendAsync("QUIT", text);
                Close();
            }
        }

        /// <summary>
        /// Join a channel.
        /// </summary>
        /// <param name="channel">The name of the channel to join.</param>
        public async Task JoinAsync(string channel)
        {
            await SendAsync("JOIN", channel);    
        }

        /// <summary>
        /// Join a channel.
        /// </summary>
        /// <param name="channel">The name of the channel to join.</param>
        /// <param name="key">The key required to join the channel.</param>
        public async Task JoinAsync(string channel, string key)
        {
            await SendAsync("JOIN", channel, key);
        }

        /// <summary>
        /// Part (leave) a channel.
        /// </summary>
        /// <param name="channel">The channel to leave.</param>
        /// <param name="message"></param>
        public async Task PartAsync(string channel, string message = null)
        {
            if (message != null)
                await SendAsync("PART", channel, message);
            else
                await SendAsync("PART", channel);

        }

        /// <summary>
        /// Change the topic on a channel. The session must have the appropriate permissions on the channel.
        /// </summary>
        /// <param name="channel">The channel on which to set a new topic.</param>
        /// <param name="topic">The topic text.</param>
        public async Task TopicAsync(string channel, string topic)
        {
            await SendAsync("TOPIC", channel, topic);
        }

        /// <summary>
        /// Request the existing topic for a channel.
        /// </summary>
        /// <param name="channel">The channel on which the topic should be retrieved.</param>
        public async Task TopicAsync(string channel)
        {
            await SendAsync("TOPIC", channel);
        }

        /// <summary>
        /// Invite another user to a channel. The session must have the appropriate permissions on the channel.
        /// </summary>
        /// <param name="channel">The channel to which the user will be invited.</param>
        /// <param name="nickname">The nickname of the user to invite.</param>
        public async Task InviteAsync(string channel, string nickname)
        {
            await SendAsync("INVITE", nickname, channel);
        }

        /// <summary>
        /// Kick a user from a channel. The session must have ops in the channel.
        /// </summary>
        /// <param name="channel">The channel from which to kick the user.</param>
        /// <param name="nickname">The nickname of the user to kick.</param>
        public async Task Kick(string channel, string nickname)
        {
            await SendAsync("KICK", channel, nickname);
        }

        /// <summary>
        /// Kick a user from a channel. The session must have ops in the channel.
        /// </summary>
        /// <param name="channel">The channel from which to kick the user.</param>
        /// <param name="nickname">The nickname of the user to kick.</param>
        /// <param name="text">The kick text, typically describing the reason for kicking a user.</param>
        public async Task KickAsync(string channel, string nickname, string text)
        {
            await SendAsync("KICK", channel, nickname, text);
        }

        /// <summary>
        /// Request the server MOTD (message of the day).
        /// </summary>
        public async Task MotdAsync()
        {
            await SendAsync("MOTD");
        }

        /// <summary>
        /// Request a server MOTD (message of the day).
        /// </summary>
        /// <param name="server">The name of the server from which to request the MOTD.</param>
        public async Task MotdAsync(string server)
        {
            await SendAsync("MOTD", server);
        }

        /// <summary>
        /// Execute the WHO command, retrieving basic information on users.
        /// </summary>
        /// <param name="mask">The wildcard to search for, matching nickname, hostname, server, and full name.</param>
        public async Task WhoAsync(string mask)
        {
            await SendAsync("WHO", mask);
        }

        /// <summary>
        /// Retrieve information about a user.
        /// </summary>
        /// <param name="mask">The nickname of the user to retrieve information about. Wildcards may or may not be supported.</param>
        public async Task WhoisAsync(string mask)
        {
            await SendAsync("WHOIS", mask);
        }

        /// <summary>
        /// Retrieve information about a user.
        /// </summary>
        /// <param name="target">
        ///     The sever to which the request should be routed (or the nickname of the user to route the request to his server).
        ///     The nickname of the user to retrieve information about. Wildcards may or may not be supported.
        /// </param>
        /// <param name="mask">The mask to whois</param>
        public async Task WhoisAsync(string target, string mask)
        {
            await SendAsync("WHOIS", target, mask);
        }

        /// <summary>
        /// Retrieve information about a user who has previously logged off. This will typically indicate when the user was last seen.
        /// </summary>
        /// <param name="nickname">The nickname of the user.</param>
        public async Task WhowasAsync(string nickname)
        {
            await SendAsync("WHOWAS", nickname);
        }

        /// <summary>
        /// Mark this session "away" so that users receive an automated response when sending a query.
        /// </summary>
        /// <param name="text">The text to send to users who query the session.</param>
        public async Task AwayAsync(string text)
        {
            await SendAsync("AWAY", text);
        }

        /// <summary>
        /// Mark the session as no longer "away".
        /// </summary>
        public async Task UnAwayAsync()
        {
            await SendAsync("AWAY");
        }

        /// <summary>
        /// Retrieve very basic user and host information about one or more users.
        /// </summary>
        /// <param name="nicknames">The nicknames for which to retrieve information.</param>
        public async Task UserHostAsync(params string[] nicknames)
        {
            await SendAsync("USERHOST", nicknames);
        }

        /// <summary>
        /// Set or unset modes for a channel.
        /// </summary>
        /// <param name="channel">The channel on which to set modes.</param>
        /// <param name="modes">The list modes to set or unset.</param>
        public async Task ModeAsync(string channel, IEnumerable<IrcChannelMode> modes)
        {
            var ircChannelModes = modes as IList<IrcChannelMode> ?? modes.ToList();
            if (!ircChannelModes.Any())
            {
                await SendAsync("MODE", new IrcTarget(channel));
                return;
            }

            var enumerator = ircChannelModes.GetEnumerator();
            var modeChunk = new List<IrcChannelMode>();
            int i = 0;
            while (enumerator.MoveNext())
            {
                modeChunk.Add(enumerator.Current);
                if (++i == 3)
                {
                    await SendAsync("MODE", new IrcTarget(channel), IrcChannelMode.RenderModes(modeChunk));
                    modeChunk.Clear();
                    i = 0;
                }
            }
            if (modeChunk.Count > 0)
            {
                await SendAsync("MODE", new IrcTarget(channel), IrcChannelMode.RenderModes(modeChunk));
            }
        }

        /// <summary>
        /// Set or unset modes for a channel.
        /// </summary>
        /// <param name="channel">The channel on which to set modes.</param>
        /// <param name="modeSpec">The mode specification in the format +/-[modes] [parameters].</param>
        /// <remarks>
        /// Examples of the modeSpec parameter:
        ///   +nst
        ///   +i-ns
        ///   -i+l 500
        ///   +bb a@b.c x@y.z
        /// </remarks>
        public async Task ModeAsync(string channel, string modeSpec)
        {
            await ModeAsync(channel, IrcChannelMode.ParseModes(modeSpec));
        }

        /// <summary>
        /// Set or unset modes for the session.
        /// </summary>
        /// <param name="modes">The collection of modes to set or unset.</param>
        public async Task ModeAsync(IEnumerable<IrcUserMode> modes)
        {
            await SendAsync("MODE", new IrcTarget(Nickname), IrcUserMode.RenderModes(modes));
        }

        /// <summary>
        /// Set or unset modes for the session.
        /// </summary>
        /// <param name="modeSpec">The mode specification in the format +/-[modes] [parameters].</param>
        /// <remarks>
        /// Examples of modeSpec parameter:
        /// +im
        /// +iw-m
        /// -mw
        /// </remarks>
        public async Task ModeAsync(string modeSpec)
        {
            await ModeAsync(IrcUserMode.ParseModes(modeSpec));
        }

        /// <summary>
        /// Retrieve the modes for the specified channel.
        /// </summary>
        /// <param name="channel">The channel for which to retrieve modes.</param>
        public async Task ModeAsync(IrcTarget channel)
        {
            if (channel.IsChannel)
            {
                await SendAsync("MODE", channel);
            }
        }

        /// <summary>
        /// Retrieve a list of channels matching the specified mask.
        /// </summary>
        /// <param name="mask">The channel name or names to list (supports wildcards).</param>
        /// <param name="target">The name of the server to query.</param>
        public async Task ListAsync(string mask, string target)
        {
            await SendAsync("LIST", mask, target);
        }

        /// <summary>
        /// Retrieve a list of channels matching the specified mask.
        /// </summary>
        /// <param name="mask">The channel name or names to list (supports wildcards).</param>
        public async Task ListAsync(string mask)
        {
            await SendAsync("LIST", mask);
        }

        /// <summary>
        /// Retrieves a list of all channels.
        /// </summary>
        public async Task ListAsync()
        {
            await SendAsync("LIST");
        }

        /// <summary>
        /// Add a handler to capture a specific IRC code. This can be called from components that issue a command and are expecting
        /// some result code to be sent in the future.
        /// </summary>
        /// <param name="capture">An object encapsulating the handler and its options.</param>
        /// <remarks>
        /// A handler can prevent other components from processing a message. For example,
        /// a component that retrieves the hostname of a user with the USERHOST command can handle the response to prevent a client
        /// from displaying the result.
        /// </remarks>
        public void AddHandler(IrcCodeHandler capture)
        {
            lock (_captures)
            {
                _captures.Add(capture);
            }
        }

        /// <summary>
        /// Remove a handler.
        /// </summary>
        /// <param name="capture">The handler to remove. This must be the same object that was added previously.</param>
        /// <returns>Returns true if the handler was removed, false if it had not been added.</returns>
        public bool RemoveHandler(IrcCodeHandler capture)
        {
            lock (_captures)
            {
                return _captures.Remove(capture);
            }
        }

        /// <summary>
        /// Reconnect to server (after any disconnection and no autoreconnect)
        /// </summary>
        public async Task Reconnect()
        {
            if (State == IrcConnectionState.Connected)
            {
                await QuitAsync("Reconnecting...");
            }
            await OnReconnect();
        }

        private void RaiseEvent<T>(EventHandler<T> evt, T e) where T : EventArgs
        {
            if (evt != null)
            {
                evt(this, e);
            }
        }

        private async void OnStateChanged()
        {
            if (State == IrcConnectionState.Connected && _findExternalAddress)
            {
                AddHandler(new IrcCodeHandler(async e =>
                {
                    e.Handled = true;
                    if (e.Message.Parameters.Count < 2)
                    {
                        return true;
                    }

                    var parts = e.Message.Parameters[1].Split('@');
                    if (parts.Length > 1)
                    {
                        IPAddress external;
                        if (!IPAddress.TryParse(parts[1], out external))
                        {
                            try
                            {
                                var dns = await Dns.GetHostEntryAsync(parts[1]);
                                ExternalAddress = dns.AddressList[0];
                            }
                            catch (SocketException se)
                            {
                                if (Debugger.IsAttached)
                                {
                                    Debug.WriteLine("Failed to get external address: " + se);
                                }
                                ExternalAddress = external;
                            }

                        }
                        else
                        {
                            ExternalAddress = external;
                        }
                    }
                    return true;
                }, IrcCodeHandlerPriority.Normal, IrcCode.RPL_USERHOST));
                await UserHostAsync(Nickname);
            }

            RaiseEvent(StateChanged, EventArgs.Empty);

            if (State == IrcConnectionState.Disconnected && AutoReconnect && ForceDisconnect != true)
            {
                _reconnectTimer?.Dispose();
                _reconnectTimer = new Timer(async obj =>
                {
                    await OnReconnect();
                }, null, ReconnectWaitTime, Timeout.Infinite);
            }
        }

        private async Task OnReconnect()
        {
            if (State == IrcConnectionState.Disconnected)
            {
                State = IrcConnectionState.Connecting;
                await OpenSocketAsync();
            }
        }

        private void OnConnectionError(ErrorEventArgs e)
        {
            RaiseEvent(ConnectionError, e);
        }

        private async Task OnMessageReceived(IrcEventArgs e)
        {
            _isWaitingForActivity = false;

#if DEBUG
            if (Debugger.IsAttached)
            {
                Debug.WriteLine("RECV: {0}", e.Message);
            }
#endif

            RaiseEvent(RawMessageReceived, e);
            if (e.Handled)
            {
                return;
            }

            switch (e.Message.Command)
            {
                case "PING":
                    if (e.Message.Parameters.Count > 0)
                    {
                        await SendRawAsync("PONG", e.Message.Parameters.ToArray());
                    }
                    else
                    {
                        await SendRawAsync("PONG");
                    }
                    break;
                case "NICK":
                    OnNickChanged(e.Message);
                    break;
                case "PRIVMSG":
                    OnPrivateMessage(e.Message);
                    break;
                case "NOTICE":
                    OnNotice(e.Message);
                    break;
                case "QUIT":
                    OnQuit(e.Message);
                    break;
                case "JOIN":
                    OnJoin(e.Message);
                    break;
                case "PART":
                    OnPart(e.Message);
                    break;
                case "TOPIC":
                    OnTopic(e.Message);
                    break;
                case "INVITE":
                    OnInvite(e.Message);
                    break;
                case "KICK":
                    OnKick(e.Message);
                    break;
                case "MODE":
                    OnMode(e.Message);
                    break;
                default:
                    await OnOther(e.Message);
                    break;
            }
        }

        [UsedImplicitly]
        private void OnMessageSent(IrcEventArgs e)
        {
            RaiseEvent(RawMessageSent, e);
#if DEBUG
            if (Debugger.IsAttached)
            {
                Debug.WriteLine("SEND: {0}", e.Message);
            }
#endif
        }

        private void OnNickChanged(IrcMessage message)
        {
            var e = new IrcNickEventArgs(message);
            var handler = NickChanged;
            if (IsSelf(e.OldNickname))
            {
                Nickname = e.NewNickname;
                handler = SelfNickChanged;
            }
            RaiseEvent(handler, e);
        }

        private void OnPrivateMessage(IrcMessage message)
        {
            if (message.Parameters.Count > 1 && CtcpCommand.IsCtcpCommand(message.Parameters[1]))
            {
                OnCtcpCommand(message);
            }
            else
            {
                RaiseEvent(PrivateMessaged, new IrcMessageEventArgs(message));
            }
        }

        private void OnNotice(IrcMessage message)
        {
            if (message.Parameters.Count > 1 && CtcpCommand.IsCtcpCommand(message.Parameters[1]))
            {
                OnCtcpCommand(message);
            }
            else
            {
                RaiseEvent(Noticed, new IrcMessageEventArgs(message));
            }
        }

        private void OnQuit(IrcMessage message)
        {
            RaiseEvent(UserQuit, new IrcQuitEventArgs(message));
        }

        private void OnJoin(IrcMessage message)
        {
            var handler = Joined;
            var e = new IrcJoinEventArgs(message);
            if (IsSelf(e.Who.Nickname))
            {
                handler = SelfJoined;
            }
            RaiseEvent(handler, e);
        }

        private void OnPart(IrcMessage message)
        {
            var handler = Parted;
            var e = new IrcPartEventArgs(message);
            if (IsSelf(e.Who.Nickname))
            {
                handler = SelfParted;
            }
            RaiseEvent(handler, e);
        }

        private void OnTopic(IrcMessage message)
        {
            RaiseEvent(TopicChanged, new IrcTopicEventArgs(message));
        }

        private void OnInvite(IrcMessage message)
        {
            RaiseEvent(Invited, new IrcInviteEventArgs(message));
        }

        private void OnKick(IrcMessage message)
        {
            var handler = Kicked;
            var e = new IrcKickEventArgs(message);
            if (IsSelf(e.KickeeNickname))
            {
                handler = SelfKicked;
            }
            RaiseEvent(handler, e);
        }

        private void OnMode(IrcMessage message)
        {
            if (message.Parameters.Count > 0)
            {
                if (IrcTarget.IsChannelName(message.Parameters[0]))
                {
                    RaiseEvent(ChannelModeChanged, new IrcChannelModeEventArgs(message));
                }
                else
                {
                    var e = new IrcUserModeEventArgs(message);
                    UserModes = (from m in e.Modes.Where(newMode => newMode.Set).Select(newMode => newMode.Mode).Union(UserModes).Distinct()
                                      where !e.Modes.Any(newMode => !newMode.Set && newMode.Mode == m)
                                      select m).ToArray();

                    RaiseEvent(UserModeChanged, new IrcUserModeEventArgs(message));
                }
            }
        }

        private async Task OnOther(IrcMessage message)
        {
            int code;
            if (int.TryParse(message.Command, out code))
            {
                var e = new IrcInfoEventArgs(message);
                if (e.Code == IrcCode.RPL_WELCOME)
                {
                    if (e.Text.StartsWith("Welcome to the "))
                    {
                        var parts = e.Text.Split(' ');
                        NetworkName = parts[3];
                    }
                    State = IrcConnectionState.Connected;
                }

                if (_captures.Count > 0)
                {
                    var capturesToRemove = new List<IrcCodeHandler>();
                    List<IrcCodeHandler> copy;
                    lock (_captures)
                    {
                        copy = _captures.ToList();
                    }
                    foreach (var capture in copy.Where(c => c.Codes.Contains(e.Code)))
                    {
                        bool result = await capture.Handler(e).ConfigureAwait(false);
                        if (result)
                        {
                            // if it returns true remove the handler
                            capturesToRemove.Add(capture);
                        }
                        if (e.Handled)
                        {
                            break; // if its handled stop processing
                        }
                    }
                    lock (_captures)
                    {
                        _captures = _captures.Except(capturesToRemove).ToList();
                    }
               }
            RaiseEvent(InfoReceived, e);
            }
        }

        private void OnCtcpCommand(IrcMessage message)
        {
            RaiseEvent(CtcpCommandReceived, new CtcpEventArgs(message));
        }
    }
}
