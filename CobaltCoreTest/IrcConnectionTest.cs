using System;
using Cobalt.Core.Irc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Net.Sockets;
using System.Runtime.Remoting.Channels;
using System.Threading;
using CobaltCore.Ctcp;

namespace Cobalt.Core.Tests
{
    [TestClass]
    public class IrcConnectionTest
    {
        CancellationTokenSource stoken = new CancellationTokenSource();
        CancellationTokenSource stoken2 = new CancellationTokenSource();

        /// <summary>
        /// Tests a few events that are essential to any IRC client.
        /// </summary>
        [TestMethod]
        public async Task TestConnection()
        {
            try
            {
                IrcConnection c = new IrcConnection();
                Task timeoutTask = Task.Delay(20000, stoken.Token);
                bool connected = false;
                bool topic = false;
                bool ctcp = false;
                bool selfJoin = false;
                bool join = false;
                bool invite = false;
                c.StateChanged += (sender, e) =>
                {
                    if (c.State != IrcConnectionState.Connected) return;
                    Console.WriteLine("Handled State Changed");
                    connected = true;
                };
                c.SelfJoined += async (sender, e) =>
                {
                    await c.PrivateMessageAsync(new IrcTarget("#dev"), "Hello!");
                    selfJoin = true;
                };
                c.Invited += async (sender, e) =>
                {
                    await c.JoinAsync("#dev").ConfigureAwait(false);
                    await c.PrivateMessageAsync(new IrcTarget("#dev"), $"Invited to {e.Channel} by {e.From.Nickname}");
                    invite = true;
                };
                c.Joined += async (sender, e) =>
                {
                    if (e.Channel.Name == "Test")
                    {
                        await c.PrivateMessageAsync(new IrcTarget("#dev"), $"{e.Who} has joined {e.Channel.Name}.");
                        join = true;
                    }
                };
                c.CtcpCommandReceived += async (sender, e) =>
                {
                    await c.PrivateMessageAsync(new IrcTarget("#dev"), $"Recieved CTCP message from {e.From.Nickname}.");
                    ctcp = true;
                };
                c.TopicChanged += async (sender, e) =>
                {
                    if (e.Channel.Name == "#dev")
                    {
                        await c.PrivateMessageAsync(new IrcTarget("#dev"), $"New Topic in {e.Channel.Name}: {e.Text} (set by {e.Who})");
                    }
                    topic = true;
                    await c.QuitAsync("Bye");
                    stoken.Cancel();
                };

                await c.ConnectAsync("irc.memers.co", 6667, false, "Test", "test", "test", false);
                await timeoutTask;
                Assert.IsTrue(connected);
                Assert.IsTrue(topic);
                Assert.IsTrue(ctcp);
                Assert.IsTrue(selfJoin);
                //Someone needs to join AFTER it connectes, so no real way to test Invited and Joined at the same time
                //Assert.IsTrue(join);
                Assert.IsTrue(invite);
            }
            catch (TaskCanceledException e)
            {

            }
        }

        /// <summary>
        /// Used mainly to test the TestConnection() test, although it will also fail if something is wrong.
        /// </summary>
        [TestMethod]
        public async Task TestDriver()
        {
            try
            {
                IrcConnection c = new IrcConnection();
                Task timeoutTask = Task.Delay(20000, stoken2.Token);

                c.StateChanged += async (sender, e) =>
                {
                    if (c.State != IrcConnectionState.Connected) return;
                    Console.WriteLine("Handled State Changed");
                    await c.PrivateMessageAsync(new IrcTarget("NickServ"), "ID Kappa");
                    await c.JoinAsync("#dev").ConfigureAwait(false);
                };

                c.SelfJoined += async (sender, e) =>
                {
                    await c.PrivateMessageAsync(new IrcTarget("#dev"), "Sending commands to Test.");
                    await c.InviteAsync("#dev", "Test");
                    await c.SendCtcpAsync(new IrcTarget("Test"), new CtcpCommand("version"), false);
                    await c.TopicAsync("#dev", "Test");
                };

                await c.ConnectAsync("irc.memers.co", 6667, false, "Test2", "test2", "test2", false);
                await timeoutTask;
            }
            catch (TaskCanceledException e)
            {

            }
        }

        /// <summary>
        /// Tests what happens when a connection is canceled.
        /// </summary>
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
