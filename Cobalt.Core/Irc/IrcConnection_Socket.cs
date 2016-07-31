using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Cobalt.Core.Network;

namespace Cobalt.Core.Irc
{
    public partial class IrcConnection
    {
        private const int HeartbeatInterval = 300000;
        private Stream _stream;

        private async Task OpenSocketAsync()
        {
            _wtoken = new CancellationTokenSource();
            if (string.IsNullOrEmpty(Hostname))
                throw new ArgumentNullException(nameof(Hostname));
            if (Port <= 0 || Port > 65535)
                throw new ArgumentOutOfRangeException(nameof(Port));

            if (!string.IsNullOrEmpty(Proxy?.ProxyHostname))
            {
                var proxy = new SocksTcpClient(Proxy);
                _tcpClient = await proxy.ConnectAsync(Hostname, Port);
            }
            else
            {
                _tcpClient = new TcpClient();
            }

            try
            {
                // try connecting
                State = IrcConnectionState.Connecting;
                var connectTask = _tcpClient.ConnectAsync(Hostname, Port);
                var connectTimeoutTask =
                    Task.Run(async () => await Task.Delay(5000, _wtoken.Token).ConfigureAwait(false));
                var firstCompletedTask = await Task.WhenAny(connectTask, connectTimeoutTask).ConfigureAwait(false);

                _wtoken.Token.ThrowIfCancellationRequested();

                if (firstCompletedTask == connectTimeoutTask)
                {
                    State = IrcConnectionState.Disconnected;
                    OnConnectionError(new ErrorEventArgs($"Connection to {Hostname}:{Port} timed out"));
                }
                else
                {
                    await connectTask; // we await the finished task so we can throw the exception
                    await SocketEntryAsync().ConfigureAwait(false);
                    await Socket_OnConnected();
                }
            }
            catch (SocketException e)
            {
                OnConnectionError(new ErrorEventArgs(e));
            }
            catch (SocksException e)
            {
                OnConnectionError(new ErrorEventArgs(e));
            }
            catch (IOException e)
            {
                OnConnectionError(new ErrorEventArgs(e));
            }
            catch (OperationCanceledException)
            {
                _tcpClient?.Close();
                _tcpClient = null;
            }
        }

        private void Close()
        {
            _wtoken?.Cancel();       
            State = IrcConnectionState.Disconnected;
        }

        private async Task SocketEntryAsync()
        {
            CancellationToken ct = _wtoken.Token;
            _stream = _tcpClient.GetStream();
            if (IsSecure)
            {
                var sslStream = new SslStream(_stream, true, (sender, cert, chain, sslPolicyErrors) =>                
                {
                    if (!AcceptInsecureCertificate)
                    {
                        if (sslPolicyErrors == SslPolicyErrors.None)
                        {
                            return true;
                        }
                        else
                        {
                            OnConnectionError(new ErrorEventArgs("Server has an invalid Ssl Certificate"));
                        }
                    }
                    return true; // accept all insecure certificates
                });
                await sslStream.AuthenticateAsClientAsync(Hostname).ConfigureAwait(false);
                _stream = sslStream;
            }

            await Task.Factory.StartNew(async () =>
            {
                await Task.Yield(); // yield to fire and forget
                try
                {
                    await SocketLoopAsync().ConfigureAwait(false);
                }
                catch (IOException ex)
                {
                    OnConnectionError(new ErrorEventArgs(ex));
                }
                catch (SocketException ex)
                {
                    OnConnectionError(new ErrorEventArgs(ex));
                }
                catch (OperationCanceledException)
                {
                    Debug.WriteLine("Operation canceled");
                }
                finally
                {
                    _tcpClient?.Close();
                    _tcpClient = null;
                }

            }, ct, TaskCreationOptions.LongRunning, TaskScheduler.Current).ConfigureAwait(false);            
        }

        private async Task SocketLoopAsync()
        {
            CancellationToken ct = _wtoken.Token;
                byte[] readBuffer = new byte[512];

                while (_tcpClient != null && _tcpClient.Connected && !ct.IsCancellationRequested)
                {
                    var heartBeatTask = Task.Delay(HeartbeatInterval, ct);
                    var readTask = _stream.ReadAsync(readBuffer, 0, 512, ct);

                    var completed = await Task.WhenAny(heartBeatTask, readTask).ConfigureAwait(false);

                    ct.ThrowIfCancellationRequested();

                    if (completed == heartBeatTask)
                    {
                        await Socket_OnHeartbeat().ConfigureAwait(false);
                    }
                    else if (completed == readTask)
                    {
                        var read = await readTask.ConfigureAwait(false);
                        if (read == 0)
                        {
                            // 0 bytes mean socket close
                            Close();
                        }
                        else
                        {
                            var gotCarriageReturn = false;
                            var input = new List<byte>();
                            for (int i = 0; i < read; i++)
                            {
                                byte cur = readBuffer[i];
                                switch (cur)
                                {
                                    case 0xa:
                                        if (gotCarriageReturn)
                                        {
                                            var incoming = IrcMessage.Parse(Encoding.UTF8.GetString(input.ToArray()));
                                            await OnMessageReceived(new IrcMessageEventArgs(incoming));
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
        }

        private Task PostMessageAsync(IrcMessage message)
        {
            if (_tcpClient.Connected && _stream != null)
            {
                var writeBuffer = new byte[Encoding.UTF8.GetMaxByteCount(512)];
                var output = message.ToString();
                var count = Encoding.UTF8.GetBytes(output, 0, output.Length, writeBuffer, 0);
                count = Math.Min(510, count);
                writeBuffer[count] = 0xd;
                writeBuffer[count + 1] = 0xa;
                return _stream.WriteAsync(writeBuffer, 0, count + 2, _wtoken.Token);
            }
            else
            {
                return default(Task);
            }
        }

        private async Task Socket_OnConnected()
        {
            if (!string.IsNullOrEmpty(_password))
            {
                await SendAsync(new IrcMessage("PASS", _password)).ConfigureAwait(false);
            }
            await SendAsync(new IrcMessage("USER", Username, _isInvisible ? "4" : "0", "*", FullName))
                    .ConfigureAwait(false);
            await SendAsync(new IrcMessage("NICK", Nickname)).ConfigureAwait(false);
            var addr = await Dns.GetHostEntryAsync(string.Empty).ConfigureAwait(false);
            InternalAddress =
                ExternalAddress = addr.AddressList.FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork);
        }

        private Task Socket_OnHeartbeat()
        {
            if (_isWaitingForActivity)
            {
                Close();
                return default(Task);
            }
            else
            {
                _isWaitingForActivity = true;
                return SendAsync("PING", Hostname);
            }
        }
    }
}