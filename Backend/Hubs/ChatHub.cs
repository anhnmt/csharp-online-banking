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
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Backend.Hubs
{
    [HubName("chatHub")]
    public class ChatHub : Hub
    {

        #region Properties
        /// <summary>
        /// List of online users
        /// </summary>
        public readonly static List<UserViewModel> _Connections = new List<UserViewModel>();

        /// <summary>
        /// List of all users
        /// </summary>
        public readonly static List<UserViewModel> _Accounts = new List<UserViewModel>();

        /// <summary>
        /// Mapping SignalR connections to application users.
        /// (We don't want to share connectionId)
        /// </summary>
        private readonly static ConcurrentDictionary<string, string> _ConnectionsMap = new ConcurrentDictionary<string, string>();
        #endregion

        private IRepository<Accounts> accountRepo;
        private IRepository<Channels> channelRepo;
        private IRepository<Messages> messageRepo;

        public ChatHub()
        {
            this.accountRepo = new Repository<Accounts>();
            this.channelRepo = new Repository<Channels>();
            this.messageRepo = new Repository<Messages>();
        }

        public int SendPrivate(string message)
        {
            try
            {
                var account = FindAccountByAccountId(GetIntegerAccountId());
                var channel = FindChannelByAccountId(account.AccountId);

                // Create and save message in database
                var msg = new Messages()
                {
                    AccountId = account.AccountId,
                    ChannelId = channel.ChannelId,
                    Content = Regex.Replace(message, @"(?i)<(?!img|a|/a|/img).*?>", String.Empty),
                    Timestamp = DateTime.Now,
                };
                messageRepo.Add(msg);

                // Broadcast the message
                var messageViewModel = new MessageViewModel(msg, account.Name);
                Clients.Group(channel.ChannelId.ToString()).newMessage(messageViewModel);
                Clients.All.reloadChatData();

                return msg.MessageId;
            }
            catch (Exception)
            {
                Clients.Caller.onError("Message can't not send!");
            }

            return 0;
        }

        public int SendToChannel(int channelId, string message)
        {
            try
            {
                var account = FindAccountByAccountId(GetIntegerAccountId());
                var channel = FindChannelByChannelId(channelId);

                // Create and save message in database
                var msg = new Messages()
                {
                    AccountId = account.AccountId,
                    ChannelId = channel.ChannelId,
                    Content = Regex.Replace(message, @"(?i)<(?!img|a|/a|/img).*?>", String.Empty),
                    Timestamp = DateTime.Now,
                };
                messageRepo.Add(msg);

                // Broadcast the message
                var messageViewModel = new MessageViewModel(msg, account.Name);
                Clients.Group(channelId.ToString()).newMessage(messageViewModel);
                Clients.All.reloadChatData();

                return msg.MessageId;
            }
            catch (Exception)
            {
                Clients.Caller.onError("Message can't not send!");
            }

            return 0;
        }

        public void Join(int channelId)
        {
            try
            {
                var account = _Connections.Where(u => u.AccountId == GetIntegerAccountId()).FirstOrDefault();

                if (!Utils.IsNullOrEmpty(account))
                {
                    if (account.CurrentChannelId != channelId)
                    {
                        // Remove user from others list
                        if (!string.IsNullOrEmpty(account.CurrentChannelId.ToString()))
                            Clients.Group(account.CurrentChannelId.ToString()).removeUser(account);

                        // Join to new chat room
                        Leave(account.CurrentChannelId);
                        Groups.Add(Context.ConnectionId, channelId.ToString());
                        account.CurrentChannelId = channelId;

                        // Tell others to update their list of users
                        Clients.Group(channelId.ToString()).addUser(account);
                    }
                }
            }
            catch (Exception ex)
            {
                Clients.Caller.onError("You failed to join the chat room!" + ex.Message);
            }
        }

        private void Leave(int channelId)
        {
            Groups.Remove(Context.ConnectionId, channelId.ToString());
        }

        //public IEnumerable<MessageViewModel> GetMessageHistory(int channelId)
        //{
        //    if (!Utils.IsNullOrEmpty(channelId) && channelId != 0)
        //    {
        //        var messageHistory = messageRepo.Get().Where(m => m.Channel.ChannelId == channelId)
        //        .OrderByDescending(m => m.Timestamp)
        //        .Take(20)
        //        .AsEnumerable()
        //        .Reverse()
        //        .Select(x => new MessageViewModel(x));

        //        return messageHistory;
        //    }

        //    return null;
        //}

        //private void sendListOnline()
        //{
        //    Clients.All.onlineList();
        //}

        public IEnumerable<UserViewModel> GetOnlineAccounts()
        {
            return _Accounts;
        }

        #region OnConnected/OnReconnected/OnDisconnected
        public override Task OnConnected()
        {
            var connection = Context.ConnectionId;
            var accountId = GetIntegerAccountId();

            try
            {
                var account = accountRepo.Get().Where(u => u.AccountId == accountId).FirstOrDefault();

                if (!Utils.IsNullOrEmpty(account))
                {
                    var userViewModel = new UserViewModel(account, 0);

                    if (account.RoleId != 1 && account.RoleId != 2)
                    {
                        var channel = FindChannelByAccountId(accountId);
                        userViewModel.CurrentChannelId = channel.ChannelId;
                        Join(channel.ChannelId);
                    }

                    var tempAccount = _Accounts.Where(u => u.AccountId == accountId).FirstOrDefault();
                    _Accounts.Remove(tempAccount);

                    _Accounts.Add(userViewModel);
                    Clients.All.UpdateUser(userViewModel);

                    _Connections.Add(userViewModel);
                    _ConnectionsMap.TryAdd(accountId.ToString(), connection);
                }
            }
            catch (Exception ex)
            {
                Clients.Caller.onError("OnConnected:" + ex.Message);
            }

            return base.OnConnected();
        }

        public override Task OnReconnected()
        {
            OnConnected();

            return base.OnReconnected();
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            var connection = Context.ConnectionId;
            var accountId = GetIntegerAccountId();

            try
            {
                var tempAccount = _Accounts.Where(u => u.AccountId == accountId).FirstOrDefault();
                _Accounts.Remove(tempAccount);

                Clients.All.UpdateUser(tempAccount);

                var account = _Connections.Where(u => u.AccountId == accountId).FirstOrDefault();
                _Connections.Remove(account);

                // Tell other users to remove you from their list
                Clients.OthersInGroup(account.CurrentChannelId.ToString()).removeUser(account);

                // Remove mapping
                _ConnectionsMap.TryRemove(account.AccountId.ToString(), out string value);

            }
            catch (Exception ex)
            {
                Clients.Caller.onError("OnDisconnected: " + ex.Message);
            }

            return base.OnDisconnected(stopCalled);
        }
        #endregion
        private Channels FindChannelByChannelId(int channelId)
        {
            return channelRepo.Get(x => x.ChannelId == channelId).FirstOrDefault();
        }

        private Channels FindChannelByAccountId(int accountId)
        {
            var channel = channelRepo.Get(x => x.UserId == accountId).FirstOrDefault();

            if (Utils.IsNullOrEmpty(channel))
            {
                channel = new Channels(accountId);
                channelRepo.Add(channel);
            }

            return channel;
        }

        private Accounts FindAccountByAccountId(int accountId)
        {
            return accountRepo.Get(x => x.AccountId == accountId).FirstOrDefault();
        }

        private int GetIntegerAccountId()
        {
            return int.Parse(GetAccountId());
        }

        private string GetAccountId()
        {
            return Context.QueryString["userId"].ToString();
        }
    }
}