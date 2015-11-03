using System;
using CobaltCore.Irc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace CobaltCoreTest
{
    [TestClass]
    public class IrcConnectionTest
    {
        [TestMethod]
        public async Task TestConnection()
        {
            Console.WriteLine("Handled State Changed");
            IrcConnection c = new IrcConnection();
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
            };
            await c.ConnectAsync("irc.memers.co", 6667, false, "Test", "test", "test", false);
            await Task.Delay(20000);
            Assert.IsTrue(connected);
        }
    }
}
