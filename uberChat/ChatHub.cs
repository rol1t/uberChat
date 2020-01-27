using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using uberChat.Models;
using System.Linq;
using System.Threading;

namespace uberChat
{
    public class ChatHub: Hub
    {
        static public List<User> OnlineUsers { get; set; } = new List<User>();

        public async Task SendMessage(string message)
        {
            var user = OnlineUsers.FirstOrDefault(usr => usr.Id == Context.ConnectionId);
            await Clients.Group(user.CurrentGroup).SendAsync("ReciveMessage", user.UserName, message);
        }

        public async Task Connect(UserViewModel user)
        {
            var newUser = new User { Id = Context.ConnectionId, UserName = user.Name };
            if (OnlineUsers.FirstOrDefault(usr => usr.Id == Context.ConnectionId) != null)
            {
                var old = OnlineUsers.FirstOrDefault(usr => usr.Id == Context.ConnectionId);
                old.UserName = user.Name;
                await Clients.All.SendAsync("UserConnected", GetOnlineUsers());
            } else
            {
                OnlineUsers.Add(newUser);
                await Clients.All.SendAsync("UserConnected", GetOnlineUsers());
            }
            await Groups.AddToGroupAsync(Context.ConnectionId, "global");
            newUser.CurrentGroup = "global";
            var group = Clients.Group("global");
            await group.SendAsync("Notify", $"{newUser.UserName} connected to group global");
        }

        private string GetOnlineUsers()
        {
            string users = "";
            foreach (var item in OnlineUsers)
            {
                users += $"{item.Id.GetHashCode().ToString().Substring(0,4)}#{item.UserName}<br>";
            }
            return users;
        }

        public async Task ConnectToGroup(string groupName)
        {
            var user = OnlineUsers.FirstOrDefault(usr => usr.Id == Context.ConnectionId);
            if (user.CurrentGroup != null)
            {
                await Groups.RemoveFromGroupAsync(user.Id, user.CurrentGroup);
                await Clients.Group(user.CurrentGroup).SendAsync("Notify", user.UserName + " disconected");
            }
            user.CurrentGroup = groupName;
            await Groups.AddToGroupAsync(user.Id, groupName);
            await Clients.Groups(groupName).SendAsync("Notify", $"{user.UserName} connected to group {groupName}");
        }

        public async Task SendPrivateMessage(string reciver, string message)
        {
            var claims = reciver.Split("#");
            var user = OnlineUsers.SingleOrDefault(usr => usr.UserName == claims[1] && usr.Id.GetHashCode().ToString().Substring(0, 4) == claims[0]);
            try
            {
                var client = Clients.Client(user.Id);
                await client.SendAsync("PrivateMessage", reciver, message);
                
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            var disconectedUser = OnlineUsers.FirstOrDefault(usr => usr.Id == Context.ConnectionId);
            OnlineUsers.Remove(disconectedUser);
            if (disconectedUser == null)
            {
                return null;
            }
            else
            {
                Clients.Group(disconectedUser.CurrentGroup).SendAsync("Notify", disconectedUser.UserName + " disconected");
            }
            return base.OnDisconnectedAsync(exception);
        }
       
    }
}