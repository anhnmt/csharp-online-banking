using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Ajax.Utilities;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using OnlineBanking.BLL.Repositories;
using OnlineBanking.DAL;
using static System.String;

namespace Backend.Hubs
{
    [HubName("chatHub")]
    public class ChatHub : Hub
    {
        public static ChatHub Instance { get; private set; }

        #region Properties

        /// <summary>
        /// List of online users
        /// </summary>
        private static readonly List<UserViewModel> Connections = new List<UserViewModel>();

        #endregion

        private readonly IRepository<Accounts> accountRepo;
        private readonly IRepository<Channels> channelRepo;
        private readonly IRepository<Messages> messageRepo;
        private readonly IRepository<Notifications> notificationRepo;
        private readonly IRepository<Transactions> transactionRepo;

        public ChatHub()
        {
            Instance = this;
            accountRepo = new Repository<Accounts>();
            channelRepo = new Repository<Channels>();
            messageRepo = new Repository<Messages>();
            notificationRepo = new Repository<Notifications>();
            transactionRepo = new Repository<Transactions>();
        }

        public async Task SendPrivate(string message)
        {
            try
            {
                var account = FindAccountByAccountId(GetIntegerAccountId());
                var channel = FindChannelByAccountId(account.AccountId);

                // Create and save message in database
                var msg = new Messages
                {
                    AccountId = account.AccountId,
                    ChannelId = channel.ChannelId,
                    Content = Regex.Replace(message, @"(?i)<(?!img|a|/a|/img).*?>", Empty),
                };
                messageRepo.Add(msg);

                // Broadcast the message
                var messageViewModel = new MessageViewModel(msg, account.Name);
                await Clients.Group("channel-" + channel.ChannelId).newMessage(messageViewModel);
                await Clients.All.reloadChatData();
            }
            catch (Exception)
            {
                Clients.Caller.onError("Message can't not send!");
            }
        }

        public async Task SendToChannel(int channelId, string message)
        {
            try
            {
                var account = FindAccountByAccountId(GetIntegerAccountId());
                var channel = FindChannelByChannelId(channelId);

                // Create and save message in database
                var msg = new Messages
                {
                    AccountId = account.AccountId,
                    ChannelId = channel.ChannelId,
                    Content = Regex.Replace(message, @"(?i)<(?!img|a|/a|/img).*?>", Empty),
                };
                messageRepo.Add(msg);

                // Broadcast the message
                var messageViewModel = new MessageViewModel(msg, account.Name);
                await Clients.Group("channel-" + channelId).newMessage(messageViewModel);
                await Clients.All.reloadChatData();
            }
            catch (Exception)
            {
                Clients.Caller.onError("Message can't not send!");
            }
        }

        public IEnumerable<MessageViewModel> GetMessageHistory(int channelId, int messageId = 0)
        {
            if (Utils.IsNullOrEmpty(channelId) || channelId == 0) return null;
            IEnumerable<Messages> messageHistory;

            if (!Utils.IsNullOrEmpty(messageId) && messageId != 0)
            {
                messageHistory = messageRepo.Get()
                    .Where(m => m.Channel.ChannelId == channelId && m.MessageId < messageId)
                    .OrderByDescending(m => m.MessageId);
            }
            else
            {
                messageHistory = messageRepo.Get()
                    .Where(m => m.Channel.ChannelId == channelId)
                    .OrderByDescending(m => m.CreatedAt);
            }

            return messageHistory
                .Take(10)
                .AsEnumerable()
                .Reverse()
                .Select(x =>
                {
                    var account = FindAccountByAccountId(x.AccountId);

                    return new MessageViewModel(x, account?.Name);
                });
        }

        public IEnumerable<NotificationViewModel> GetNotificationsHistory(int accountId)
        {
            var notifications = notificationRepo.Get()
                .Where(m => m.AccountId == accountId);

            return notifications
                .OrderByDescending(m => m.CreatedAt)
                .Take(5)
                .AsEnumerable()
                .Select(x =>
                {
                    var pkObject = transactionRepo
                        .Get().FirstOrDefault(y => y.TransactionId == x.PkId);

                    return new NotificationViewModel(x, pkObject);
                });
        }

        public void SendNotifications(List<Notifications> notifications)
        {
            try
            {
                if (!notificationRepo.AddRange(notifications)) return;

                notifications.ForEach(x =>
                {
                    Console.WriteLine("user-" + x.AccountId);

                    var pkObject = transactionRepo
                        .Get().FirstOrDefault(y => y.TransactionId == x.PkId);

                    Clients.Group("user-" + x.AccountId)
                        .newNotification(new NotificationViewModel(x, pkObject));
                });
            }
            catch (Exception)
            {
                Clients.Caller.onError("Notification can't not send!");
            }
        }

        //private void sendListOnline()
        //{
        //    Clients.All.onlineList();
        //}

        public IEnumerable<UserViewModel> GetOnlineAccounts()
        {
            return Connections;
        }

        #region OnConnected/OnReconnected/OnDisconnected

        public override Task OnConnected()
        {
            var connectionId = GetConnectionId();
            var accountId = GetIntegerAccountId();

            try
            {
                var account = accountRepo.Get().FirstOrDefault(u => u.AccountId == accountId);

                if (Utils.NotNullOrEmpty(account))
                {
                    var userViewModel = new UserViewModel(account, 0);

                    if (account.RoleId != (int) RoleStatus.Admin && account.RoleId != (int) RoleStatus.Support)
                    {
                        var channel = FindChannelByAccountId(accountId);
                        userViewModel.CurrentChannelId = channel.ChannelId;
                        Groups.Add(connectionId, "channel-" + channel.ChannelId);
                        Clients.Client(connectionId).historyMessages(GetMessageHistory(channel.ChannelId));
                        Clients.Client(connectionId)
                            .historyNotifications(GetNotificationsHistory(GetIntegerAccountId()));
                    }

                    var tempAccount = Connections.FirstOrDefault(u => u.AccountId == accountId);
                    Connections.Remove(tempAccount);

                    Connections.Add(userViewModel);
                    Clients.Client(connectionId).UpdateUser(userViewModel);

                    Groups.Add(connectionId, "user-" + accountId);
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
            var connectionId = GetConnectionId();
            var accountId = GetIntegerAccountId();

            try
            {
                var tempAccount = Connections.FirstOrDefault(u => u.AccountId == accountId);
                Connections.Remove(tempAccount);

                Connections.Add(tempAccount);
                Clients.Client(connectionId).UpdateUser(tempAccount);

                // Remove mapping
                if (tempAccount != null) Groups.Remove(connectionId, "user-" + accountId);
            }
            catch (Exception ex)
            {
                Clients.Caller.onError("OnDisconnected: " + ex.Message);
            }

            return base.OnDisconnected(stopCalled);
        }

        #endregion

        public Task Join(int channelId)
        {
            try
            {
                var connectionId = GetConnectionId();
                var account = Connections.FirstOrDefault(u => u.AccountId == GetIntegerAccountId());

                if (Utils.NotNullOrEmpty(account))
                {
                    if (account.CurrentChannelId != channelId)
                    {
                        // Join to new chat room
                        Leave(account.CurrentChannelId);
                        Groups.Add(connectionId, "channel-" + channelId);
                        Connections.Remove(account);

                        account.CurrentChannelId = channelId;

                        Connections.Add(account);
                        Clients.Client(connectionId).UpdateUser(account);

                        Clients.Client(connectionId).historyMessages(GetMessageHistory(channelId));

                        return Groups.Add(connectionId, channelId.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                Clients.Caller.onError("You failed to join the channel!" + ex.Message);
            }

            return null;
        }

        private Task Leave(int channelId)
        {
            return Groups.Remove(GetConnectionId(), "channel-" + channelId);
        }

        private Channels FindChannelByChannelId(int channelId)
        {
            return channelRepo.Get(x => x.ChannelId == channelId).FirstOrDefault();
        }

        private Channels FindChannelByAccountId(int accountId)
        {
            var channel = channelRepo.Get(x => x.UserId == accountId).FirstOrDefault();

            if (!Utils.IsNullOrEmpty(channel)) return channel;
            channel = new Channels(accountId);
            channelRepo.Add(channel);

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
            return Context.QueryString["userId"];
        }

        private string GetConnectionId()
        {
            return Context.ConnectionId;
        }
    }
}