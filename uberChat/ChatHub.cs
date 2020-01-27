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
        static public List<Chat> Chats { get; set; } = new List<Chat>() { 
            new Chat { ChatId = 1, Name = "Global Chat", Messages = 
                new List<Message> { 
                    new Message { Sender = "server", Content = "init message" },
                    new Message { Sender = "server", Content = "init message" }
                } 
            },
            new Chat { ChatId = 2, Name = "Flood", Messages =
                new List<Message> {
                    new Message { Sender = "server", Content = "init flood" },
                    new Message { Sender = "server", Content = "init flood" }
                }
            }
        }; 

        public async Task SendMessage(string message)
        {
            var user = OnlineUsers.FirstOrDefault(usr => usr.Id == Context.ConnectionId);
            var chat = Chats.FirstOrDefault(ch => ch.ChatId == user.ChatId);
            chat.Messages.Add(new Message { Sender = user.UserName, Content = message });
            foreach (var item in chat.ConnectedUsers)
            {
                await Clients.Client(item.Id).SendAsync("UpdateChat", chat);
            }

        }

        public async Task Connect(UserViewModel user)
        {
            var newUser = new User { Id = Context.ConnectionId, UserName = user.Name };
            if(OnlineUsers.FirstOrDefault(usr => usr.Id == Context.ConnectionId) != null)
            {
                var old = OnlineUsers.FirstOrDefault(usr => usr.Id == Context.ConnectionId);
                old.UserName = user.Name;
                await Clients.All.SendAsync("UserConnected", GetOnlineUsers());
            }else
            {
                OnlineUsers.Add(newUser);
                await Clients.All.SendAsync("UserConnected", GetOnlineUsers());
            }
            var chat = Chats.FirstOrDefault(cht => cht.Name == "Global Chat");
            newUser.ChatId = chat.ChatId;
            chat.ConnectedUsers.Add(newUser); //бага тута
            //await Clients.Caller.SendAsync("ConnectToChat2", new { newUser.CurrentChat.Messages, Users = new[] { new User { UserName = "biba" } } });
            //var costilniyChat = new Chat {
            //    ConnectedUsers = newUser.CurrentChat.ConnectedUsers.Select(usr => new User
            //    {
            //        UserName = usr.UserName,
            //        MessageBox = newUser.MessageBox,
            //        Id = newUser.Id,
            //        CurrentChat = null
            //    }).ToList(),
            //    ChatId = newUser.CurrentChat.ChatId,
            //    Messages = newUser.CurrentChat.Messages,
            //    Name = newUser.CurrentChat.Name
            //};
            await Clients.Caller.SendAsync("ConnectToChat", chat);


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

        public async Task ConnectToChat(int chatId)
        {
            var user = OnlineUsers.FirstOrDefault(usr => usr.Id == usr.Id);
            if (user.ChatId.HasValue)
            {
                var chattmp = Chats.FirstOrDefault(ch => ch.ChatId == user.ChatId);
                chattmp.ConnectedUsers.Remove(user);
                user.ChatId = null;
            }
            var chat = Chats.FirstOrDefault(chat => chat.ChatId == chatId);
            chat.ConnectedUsers.Add(user);
            user.ChatId = chat == null? 1: chat.ChatId;
            if (chat != null)
            {
                await Clients.Caller.SendAsync("ConnectToChat", chat);
            }
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
            var chat = Chats.FirstOrDefault(ch => ch.ChatId == disconectedUser.ChatId);
            if (disconectedUser != null)
            {
                chat.ConnectedUsers.Remove(disconectedUser);
            }
            foreach (var item in chat.ConnectedUsers)
            {
                Clients.Client(item.Id).SendAsync("UpdateChat", chat);
            }
            Clients.All.SendAsync("UserConnected", GetOnlineUsers());
            return base.OnDisconnectedAsync(exception);
        }
       
    }
}