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
            IrcConnection c = new IrcConnection();

            await c.ConnectAsync("irc.memers.co", 6667, false, "Test", "test", "test", false);
            c.StateChanged += async (sender, e) =>
            {
                if(c.State == IrcConnectionState.Connected)
                {
                    await c.JoinAsync("#dev");
                    await c.PrivateMessageAsync(new IrcTarget("#dev"), "Hello!");
                }
            };   
            await Task.Delay(10000);
            await c.QuitAsync("");
        }
    }
}
