using System;
using CobaltCore.Irc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Net.Sockets;
using System.Threading;

namespace CobaltCoreTest
{
    [TestClass]
    public class IrcConnectionTest
    {
        CancellationTokenSource stoken = new CancellationTokenSource();
        [TestMethod]
        public async Task TestConnection()
        {
            try
            {
                IrcConnection c = new IrcConnection();
                Task timeoutTask = Task.Delay(5000, stoken.Token);
                bool connected = false;
                c.StateChanged += async (sender, e) =>
                {
                    if (c.State != IrcConnectionState.Connected) return;
                    Console.WriteLine("Handled State Changed");
                    await c.JoinAsync("#dev").ConfigureAwait(false);
                };
                c.SelfJoined += async (sender, e) =>
                {
                    await c.PrivateMessageAsync(new IrcTarget("#dev"), "Hello!");
                    await c.QuitAsync("Bye");
                    connected = true;
                    stoken.Cancel();
                };
                await c.ConnectAsync("irc.memers.co", 6667, false, "Test", "test", "test", false);
                await timeoutTask;
                Assert.IsTrue(connected);
            }
            catch (TaskCanceledException e)
            {
                
            }       
        }

        [TestMethod]
        public async Task TestBadConnection()
        {
            try
            {
                Task t = Task.Delay(5000, stoken.Token);
                IrcConnection c = new IrcConnection();
                bool caughtError = false;
                c.ConnectionError += (sender, error) =>
                {
                    Debug.WriteLine(error.ToString());
                    Assert.IsTrue(error.Exception is SocketException);
                    SocketException se = error.Exception as SocketException;
                    Assert.IsTrue(se.SocketErrorCode == SocketError.ConnectionRefused);
                    caughtError = true;
                    stoken.Cancel();
                };
                await c.ConnectAsync("irc.memers.co", 6668, false, "Test", "Test", "Test", false);
                Assert.IsFalse(c.State == IrcConnectionState.Disconnected);
                await t;
                Assert.IsTrue(caughtError);
            }
            catch (TaskCanceledException e)
            {
                
            }
        }
    }
}
