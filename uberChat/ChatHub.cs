using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using uberChat.Models;
using System.Linq;

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
            var chat = user.CurrentChat;
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

            newUser.CurrentChat = Chats.FirstOrDefault(cht => cht.Name == "Global Chat");
            newUser.CurrentChat.ConnectedUsers.Add(newUser); //бага тута
            await Clients.Caller.SendAsync("ConnectToChat", newUser.CurrentChat);

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
            if (user.CurrentChat != null)
            {
                user.CurrentChat.ConnectedUsers.Remove(user);
                user.CurrentChat = null;
            }
            user.CurrentChat = Chats.FirstOrDefault(chat => chat.ChatId == chatId);
            if (user.CurrentChat != null)
            {
                await Clients.Caller.SendAsync("ConnectToChat", user.CurrentChat);
            }
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            var disconectedUser = OnlineUsers.FirstOrDefault(usr => usr.Id == Context.ConnectionId);
            OnlineUsers.Remove(disconectedUser);
            if (disconectedUser.CurrentChat != null)
            {
                disconectedUser.CurrentChat.ConnectedUsers.Remove(disconectedUser);
            }
            Clients.All.SendAsync("UserConnected", GetOnlineUsers());
            return base.OnDisconnectedAsync(exception);
        }
       
    }
}