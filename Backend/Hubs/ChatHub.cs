using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Newtonsoft.Json;
using OnlineBanking.BLL.Repositories;
using OnlineBanking.DAL;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Backend.Hubs
{
    [HubName("chatHub")]
    public class ChatHub : Hub
    {
        public static ConcurrentDictionary<string, List<string>> sessionConnections = new ConcurrentDictionary<string, List<string>>();
        private static List<Accounts> _accounts = new List<Accounts>();
        private static List<Channels> _channels = new List<Channels>();
        private IRepository<Accounts> accountRepo;
        private IRepository<Channels> channelRepo;
        private IRepository<Messages> messageRepo;

        public ChatHub()
        {
            this.accountRepo = new Repository<Accounts>();
            this.channelRepo = new Repository<Channels>();
            this.messageRepo = new Repository<Messages>();
        }

        public void Send(string message)
        {
            if (!Utils.IsNullOrEmpty(message))
            {

                var accountId = getIntegerAccountId();

                var account = accountRepo.Get(x => x.AccountId == accountId).FirstOrDefault();

                if (!Utils.IsNullOrEmpty(account))
                {
                    userSendMessage(account, message);
                }
            }


            //Clients.Caller.showErrorMessage("The user is no longer connected.");
            //foreach (var connectionId in _connections.GetConnections(who))
            //{
            //    Clients.Client(connectionId).addChatMessage(name + ": " + message);
            //}
        }

        public void Reply(int accountId, string message)
        {
            if (!Utils.IsNullOrEmpty(message))
            {
                var account = accountRepo.Get(x => x.AccountId == accountId).FirstOrDefault();

                if (!Utils.IsNullOrEmpty(account))
                {
                    userSendMessage(account, message);
                }
            }


            //Clients.Caller.showErrorMessage("The user is no longer connected.");
            //foreach (var connectionId in _connections.GetConnections(who))
            //{
            //    Clients.Client(connectionId).addChatMessage(name + ": " + message);
            //}
        }

        public IEnumerable<Accounts> GetOnlineUsers()
        {
            return _accounts;
        }

        public override Task OnConnected()
        {
            var connection = Context.ConnectionId;

            addConnection(connection);

            return base.OnConnected();
        }


        // Handler message from user
        private void userSendMessage(Accounts account, string message)
        {
            // check role user
            //if (account.RoleId == 3)
            //{
            var channel = findChannelByAccountId(account.AccountId);
            if (Utils.IsNullOrEmpty(channel))
            {
                channel.AccountId = account.AccountId;
                if (channelRepo.Add(channel))
                {
                    channel = findChannelByAccountId(account.AccountId);
                }
            }

            var messageObj = new Messages
            {
                AccountId = account.AccountId,
                ChannelId = channel.ChannelId,
                Content = message,
                Timestamp = DateTime.Now
            };

            if (messageRepo.Add(messageObj))
            {
                sendMessageToChannelId(channel.ChannelId, messageObj);
            }
            //}

        }

        private Channels findChannelByAccountId(int accountId)
        {
            return channelRepo.Get(x => x.AccountId == accountId).FirstOrDefault();
        }

        private void sendMessageToChannelId(int channelId, Messages message)
        {
            Clients.All.addNewMessageToPage("", "");
        }

        private void sendMessageToAccountId(int accountId)
        {
            Clients.All.addNewMessageToPage("", "");
        }

        private void addConnection(string connection)
        {
            var accountId = getAccountId();

            sessionConnections.TryGetValue(accountId, out List<string> existingConnectionAccounts);

            // happens on the very first connection from the user
            if (existingConnectionAccounts == null)
            {
                existingConnectionAccounts = new List<string>();
            }

            // First add to a List of existing user connections (i.e. multiple web browser tabs)
            existingConnectionAccounts.Add(connection);

            // Add to the global dictionary of connected users
            sessionConnections.TryAdd(accountId, existingConnectionAccounts);
        }

        public override Task OnReconnected()
        {
            var connection = Context.ConnectionId;

            addConnection(connection);

            return base.OnReconnected();
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            var connection = Context.ConnectionId;

            removeConnection(connection);

            return base.OnDisconnected(stopCalled);
        }

        private int getIntegerAccountId()
        {
            return int.Parse(getAccountId());
        }

        private string getAccountId()
        {
            return Context.QueryString["userId"].ToString();
        }
        private List<string> getConnectionByAccountId(string accountId)
        {
            List<string> connectionAccounts;
            sessionConnections.TryGetValue(accountId, out connectionAccounts);

            return connectionAccounts;
        }


        private KeyValuePair<string, List<string>> getConnectedAccount(string connection)
        {
            return sessionConnections.FirstOrDefault(x => x.Value.Contains(connection));
        }

        private void removeConnection(string connection)
        {
            var account = getConnectedAccount(connection);

            if (!Utils.IsNullOrEmpty(account))
            {
                var key = account.Key;
                var value = account.Value;

                if (!Utils.IsNullOrEmpty(value))
                {
                    value.Remove(connection);
                }
                else
                {
                    sessionConnections.TryRemove(key, out value);
                }
            }
        }
    }
}