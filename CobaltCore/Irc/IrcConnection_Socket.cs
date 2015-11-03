using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CobaltCore.Irc
{
    public partial class IrcConnection
    {
        private const int HeartbeatInterval = 300000;
        private Stream _stream;

        private async Task OpenSocketAsync()
        {
            _wtoken = new CancellationTokenSource();
            if (string.IsNullOrEmpty(this.Hostname))
                throw new ArgumentNullException("server");
            if (this.Port <= 0 || this.Port > 65535)
                throw new ArgumentOutOfRangeException("port");

            if (this.Proxy != null && !string.IsNullOrEmpty(Proxy.ProxyHostname))
            {

            }
            else
            {
                _tcpClient = new TcpClient();
            }

            try
            {
                // try connecting
                this.State = IrcConnectionState.Connecting;
                Task connectTask = _tcpClient.ConnectAsync(this.Hostname, this.Port);
                Task connectTimeoutTask = Task.Run(async () => await Task.Delay(5000, _wtoken.Token).ConfigureAwait(false));
                var firstCompletedTask = await Task.WhenAny(connectTask, connectTimeoutTask).ConfigureAwait(false);

                if (firstCompletedTask == connectTimeoutTask)
                {
                    this.State = IrcConnectionState.Disconnected;
                }
                else
                {
                    await SocketEntryAsync(_wtoken.Token).ConfigureAwait(false);
                    await this.Socket_OnConnected();
                }
            }
            catch (SocketException e)
            {

            }
        }

        private async Task SocketEntryAsync(CancellationToken ct)
        {
            _stream = _tcpClient.GetStream();
            if (this.IsSecure)
            {
                var sslStream = new SslStream(_stream, true, (sender, cert, chain, sslPolicyErrors) =>
                {
                    if (!AcceptInsecureCertificate)
                    {
                        return sslPolicyErrors == SslPolicyErrors.None;
#warning Incomplete code
                        // yield invalid ssl event
                    }
                    else
                    {
                        return true; // accept all insecure certificates
                    }
                });
                await sslStream.AuthenticateAsClientAsync(Hostname).ConfigureAwait(false);
                _stream = sslStream;
            }

            try
            {
                _socketLoopTask = Task.Run(async () =>
                {
                    await SocketLoopAsync(ct).ConfigureAwait(false);
                }, _wtoken.Token);                
            }
            catch (Exception e) 
            {
#warning Incomplete code
            }
        }

        private async Task SocketLoopAsync(CancellationToken ct)
        {
            byte[] readBuffer = new byte[512], writeBuffer = new byte[Encoding.UTF8.GetMaxByteCount(512)];

            while(_tcpClient.Connected && !_wtoken.Token.IsCancellationRequested)
            {
                var heartBeatTask = Task.Delay(HeartbeatInterval, ct);
                var readTask = _stream.ReadAsync(readBuffer, 0, 512, ct);

                var completed = await Task.WhenAny(heartBeatTask, readTask).ConfigureAwait(false);

                if(completed == heartBeatTask)
                {
                    this.Socket_OnHeartbeat();
                }
                else if(completed == readTask)
                {
                    int read = await readTask.ConfigureAwait(false);
                    if (read == 0)
                    {
                        // 0 bytes mean socket close
                        _tcpClient.Close();
                    }
                    else
                    {
                        bool gotCarriageReturn = false;
                        var input = new List<byte>();
                        foreach (var cur in readBuffer)
                        {
                            switch (cur)
                            {
                                case 0xa:
                                    if (gotCarriageReturn)
                                    {
                                        var incoming = IrcMessage.Parse(Encoding.UTF8.GetString(input.ToArray()));
                                        this.OnMessageReceived(new IrcMessageEventArgs(incoming));
                                        input.Clear();
                                    }
                                    break;
                                case 0xd:
                                    break;
                                default:
                                    input.Add(cur);
                                    break;
                            }
                            gotCarriageReturn = cur == 0xd;
                        }
                    }
                }                
            }
            this.State = IrcConnectionState.Disconnected;
        }

        public async Task PostMessageAsync(IrcMessage message)
        {
            if(_tcpClient.Connected && _stream != null)
            {
                try
                {
                    byte[] writeBuffer = new byte[Encoding.UTF8.GetMaxByteCount(512)];
                    string output = message.ToString();
                    int count = Encoding.UTF8.GetBytes(output, 0, output.Length, writeBuffer, 0);
                    count = Math.Min(510, count);
                    writeBuffer[count] = 0xd;
                    writeBuffer[count + 1] = 0xa;
                    await _stream.WriteAsync(writeBuffer, 0, count + 2, _wtoken.Token).ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    Console.WriteLine("WriteAsync failed ", e);
                }
            }
        }

        private async Task Socket_OnConnected()
        {
            if (!string.IsNullOrEmpty(_password))
            {
                await SendAsync(new IrcMessage("PASS", _password)).ConfigureAwait(false);
            }
            await SendAsync(new IrcMessage("USER", this.Username, _isInvisible ? "4" : "0", "*", this.FullName)).ConfigureAwait(false);
            await SendAsync(new IrcMessage("NICK", this.Nickname)).ConfigureAwait(false);
            var addr = await Dns.GetHostEntryAsync(string.Empty).ConfigureAwait(false);
            this.InternalAddress = this.ExternalAddress = addr.AddressList.Where((ip) => ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork).FirstOrDefault();
        }

        private async void Socket_OnHeartbeat()
        {
            if (_isWaitingForActivity)
            {
                _tcpClient.Close();
            }
            else
            {
                _isWaitingForActivity = true;
                await this.SendAsync("PING", this.Hostname).ConfigureAwait(false);
            }
        }
    }
}