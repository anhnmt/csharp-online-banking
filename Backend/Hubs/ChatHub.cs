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
        public static ConcurrentDictionary<string, List<string>> allSessions = new ConcurrentDictionary<string, List<string>>();
        public static ConcurrentDictionary<string, List<string>> channelSessions = new ConcurrentDictionary<string, List<string>>();
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

                var account = findAccountByAccountId(accountId);

                if (!Utils.IsNullOrEmpty(account))
                {
                    userSendMessage(account, message);
                }
            }
        }

        public void Reply(int accountId, string message)
        {
            if (!Utils.IsNullOrEmpty(message))
            {
                var account = findAccountByAccountId(accountId);

                if (!Utils.IsNullOrEmpty(account))
                {
                    userSendMessage(account, message);
                }
            }
        }

        public void ConnectChannel(string channelId)
        {
            if (!Utils.IsNullOrEmpty(channelId))
            {
                addChannelSession(channelId, Context.ConnectionId);
            }
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
            var channel = findChannelByAccountId(account.AccountId);
            if (Utils.IsNullOrEmpty(channel))
            {
                channel = new Channels();
                channel.AccountId = account.AccountId;
                channelRepo.Add(channel);
            }

            addChannelSession(account.AccountId.ToString(), Context.ConnectionId);

            var messageObj = new Messages
            {
                AccountId = account.AccountId,
                ChannelId = channel.ChannelId,
                Content = message,
                Timestamp = DateTime.Now
            };

            if (messageRepo.Add(messageObj))
            {
                sendMessageToAdmin(channel.ChannelId, account, messageObj);
            }

        }

        private Channels findChannelByAccountId(int accountId)
        {
            return channelRepo.Get(x => x.AccountId == accountId).FirstOrDefault();
        }

        private Accounts findAccountByAccountId(int accountId)
        {
            return accountRepo.Get(x => x.AccountId == accountId).FirstOrDefault();
        }

        private void sendMessageToAdmin(int channelId, Accounts account, Messages message)
        {

            List<Accounts> _accounts = accountRepo.Get(x => x.RoleId == 1 || x.RoleId == 2 || x.AccountId == account.AccountId).ToList();
            //Clients.Caller.showErrorMessage("The user is no longer connected.");

            if (!Utils.IsNullOrEmpty(_accounts))
            {
                _accounts.ForEach(x =>
                {
                    allSessions.TryGetValue(x.AccountId.ToString(), out List<string> existingConnectionAccounts);

                    if (!Utils.IsNullOrEmpty(existingConnectionAccounts))
                    {
                        existingConnectionAccounts.ForEach(connectionId =>
                        {
                            Clients.Client(connectionId).addNewMessageToPage(account.Name, message.Content);
                        });
                    }
                });
            }
        }

        private void addConnection(string connection)
        {
            var accountId = getAccountId();

            allSessions.TryGetValue(accountId, out List<string> existingConnections);

            if (existingConnections == null)
            {
                existingConnections = new List<string>();
            }

            existingConnections.Add(connection);

            allSessions.TryAdd(accountId, existingConnections);

            addChannelSession(accountId, connection);
        }

        private void addChannelSession(string accountId, string connection)
        {
            removeConnection(channelSessions, connection);

            var account = findAccountByAccountId(int.Parse(accountId));

            if (!Utils.IsNullOrEmpty(account))
            {
                var channel = findChannelByAccountId(account.AccountId);

                if (!Utils.IsNullOrEmpty(channel))
                {
                    channelSessions.TryGetValue(channel.ChannelId.ToString(), out List<string> existingConnections);

                    if (existingConnections == null)
                    {
                        existingConnections = new List<string>();
                    }

                    existingConnections.Add(connection);

                    channelSessions.TryAdd(channel.ChannelId.ToString(), existingConnections);
                }
            }
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

            removeConnection(allSessions, connection);
            removeConnection(channelSessions, connection);

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
            allSessions.TryGetValue(accountId, out connectionAccounts);

            return connectionAccounts;
        }


        private KeyValuePair<string, List<string>> getConnectionKey(ConcurrentDictionary<string, List<string>> sessions, string connection)
        {
            return sessions.FirstOrDefault(x => x.Value.Contains(connection));
        }

        private void removeConnection(ConcurrentDictionary<string, List<string>> sessions, string connection)
        {
            var account = getConnectionKey(sessions, connection);

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
                    if (!Utils.IsNullOrEmpty(key))
                    {
                        sessions.TryRemove(key, out value);
                    }
                }
            }
        }
    }
}