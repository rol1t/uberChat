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
        static public List<Message> Messages { get; set; } = new List<Message>();
        public async Task SendMessage(string message)
        {
            var user = OnlineUsers.FirstOrDefault(usr => usr.Id == Context.ConnectionId);
            Messages.Add(new Message
            {
                GroupName = user.CurrentGroup,
                Sender = user.UserName,
                Content = message,
                Id = Messages.Where(msg => msg.GroupName == user.CurrentGroup).Max(msg => msg.Id) + 1
            });
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
            await ConnectToGroup("global");
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
            user.CurrentGroup = groupName;
            if (!Messages.Any(msg => msg.GroupName == user.CurrentGroup))
            {
                int id = 0;
                Messages.Add(new Message
                {
                    Content = $"chat start here ({user.CurrentGroup})!",
                    GroupName = user.CurrentGroup,
                    Sender = "server",
                    Id = id++
                });
                Messages.Add(new Message
                {
                    Id = id++,
                    Content = $"пересчет работяг из ({user.CurrentGroup})!",
                    GroupName = user.CurrentGroup,
                    Sender = "server",

                });
                for (int i = 1; i <= 500; i++)
                {
                    Messages.Add(new Message
                    {
                        Id = id++,
                        Content = $"{i}!",
                        GroupName = user.CurrentGroup,
                        Sender = "server",

                    });
                }
                Messages.Add(new Message
                {
                    Id = id++,
                    Content = $"пересчет работяг из ({user.CurrentGroup}) окончен!",
                    GroupName = user.CurrentGroup,
                    Sender = "server",

                });
            }
            if (user.CurrentGroup != null)
            {
                await Groups.RemoveFromGroupAsync(user.Id, user.CurrentGroup);
                await Clients.Group(user.CurrentGroup).SendAsync("Notify", user.UserName + " disconected");
            }
            user.CurrentGroup = groupName;
            await Groups.AddToGroupAsync(user.Id, groupName);
            var messages = Messages.Where(msg => msg.GroupName == groupName)
                .TakeLast(10)
                .ToList();
            await Clients.Caller.SendAsync("LoadMessages", messages, (messages.Count() > 0 ? messages.Min(msg => msg.Id): 0));
            await Clients.Groups(groupName).SendAsync("Notify", $"{user.UserName} connected to group {groupName}");
        }

        //load more message. start on id
        public async Task LoadMore(int id)
        {
            var user = OnlineUsers.FirstOrDefault(usr => usr.Id == Context.ConnectionId);
            if (user == null)
            {
                return;
            }
            var messages = Messages.Where(msg => msg.GroupName == user.CurrentGroup && id > msg.Id)
                .TakeLast(10)
                .ToList();
            messages.Reverse();
            await Clients.Caller.SendAsync("LoadMoreMessages", messages, messages.Count() > 0 ? messages.Min(msg => msg.Id) : 0);
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