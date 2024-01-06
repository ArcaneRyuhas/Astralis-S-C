using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using MessageService;

namespace CallbackTestProject
{
    [TestClass]
    public class LobbyCallbackTests
    {

        [ClassInitialize]
        public void Initialize()
        {

        }

        [TestMethod]
        public void TestMetho()
        {

        }
    }

    public class LobbyCallbackImplementation
    {
        public bool BeenKicked { get; set; }

        public bool ConnectedToLobby { get; set; }

        public bool ShowConnectionInLobby { get; set; }

        public bool ShowDisconnectionInLobby { get; set; }

        public int TeamChanged { get; set; }

        public string Message { get; set; }

        public bool GameStarted { get; set; }

        public LobbyCallbackImplementation()
        {
            BeenKicked = false;
            ConnectedToLobby = false;
            ShowConnectionInLobby = false;
            ShowDisconnectionInLobby = false;
            TeamChanged = 0;
            Message = string.Empty;
            GameStarted = false;
        }


    }
}
