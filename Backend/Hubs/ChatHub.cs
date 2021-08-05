using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
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
        public static ConcurrentDictionary<string, List<string>> ConnectedAccounts = new ConcurrentDictionary<string, List<string>>();
        private static List<Accounts> accounts = new List<Accounts>();
        private static List<Channels> channels = new List<Channels>();

        public void Send(string message)
        {
            var accountId = Context.QueryString["userId"].ToString();
            // Call the addNewMessageToPage method to update clients.
            Clients.All.addNewMessageToPage(accountId, message + "--" + Context.ConnectionId);

            //foreach (var connectionId in _connections.GetConnections(who))
            //{
            //    Clients.Client(connectionId).addChatMessage(name + ": " + message);
            //}
        }

        public IEnumerable<Accounts> GetOnlineUsers()
        {
            return accounts;
        }


        public string Ping(string connectionId)
        {
            List<string> existingUserConnectionIds;

            var accountId = Context.QueryString["userId"].ToString();
            ConnectedAccounts.TryGetValue(accountId, out existingUserConnectionIds);

            // happens on the very first connection from the user
            if (existingUserConnectionIds == null)
            {
                existingUserConnectionIds = new List<string>();
            }

            // First add to a List of existing user connections (i.e. multiple web browser tabs)
            existingUserConnectionIds.Add(connectionId);


            // Add to the global dictionary of connected users
            ConnectedAccounts.TryAdd(accountId, existingUserConnectionIds);

            return "success";
        }

        public override Task OnConnected()
        {
            var connection = Context.ConnectionId;

            //accounts.Add(Context.ConnectionId);
            return base.OnConnected();
        }

        public override Task OnReconnected()
        {
            //users.Remove(Context.ConnectionId);
            return base.OnReconnected();
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            this.removeConnection(Context.ConnectionId);

            return base.OnDisconnected(stopCalled);
        }

        private KeyValuePair<string, List<string>> getConnectedAccount(string connection)
        {
            return ConnectedAccounts.FirstOrDefault(x => x.Value.Contains(connection));
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
                    ConnectedAccounts.TryRemove(key, out value);
                }
            }
        }
    }
}