using System;
using System.Collections.Generic;
using System.Net.Sockets;

namespace serverChat
{
    public static class Server
    {
        public static List<Client> Clients = new List<Client>(); // список клиентов

        public static void NewClient(Socket handle) //  добавляет клиента
        {
            try
            {
                Client newClient = new Client(handle); // создаем клиента передав ему сокет
                Clients.Add(newClient); // добавляем созданного клиента в список
                Console.WriteLine($"Подключился новый клиент: {handle.RemoteEndPoint}");
            }
            catch (Exception exp) 
            { 
                Console.WriteLine($"Ошибка подключения клиента: {exp.Message}");
            }
        }
        public static void EndClient(Client client) // метод отключения клиента
        {
            try
            {
                client.End(); // вызывем метод в котором закрываем сокет и завершаем поток
                Clients.Remove(client);  // удаляем клиента из списка
                SendOnlineUsersAllChats();
                Console.WriteLine($"Клиент { client._UserName } был отключён.");
            }
            catch (Exception exp)
            {
                Console.WriteLine($"Ошибка в EndClient: {exp.Message}");
            }
        }

        public static void SendHistoryNewUser(Client cl) // отправляет историю чата новому пользователю
        {
            try
            {
                int IndexNewUser = Clients.IndexOf(cl);  
                Clients[IndexNewUser].SendHistory(); // обновляем чат у нового клиента
            }
            catch (Exception exp)
            {
                Console.WriteLine($"Ошибка в методе UpdateNewChats: {exp.Message}");
            }
        }

        public static void SendMessageAllChats() // отправляет сообщение всем пользователям
        {
            try
            {
                int countUsers = Clients.Count;  // смотрим сколько клиентов подключено к серверу (сколько клиентов в списке Clients)

                for (int i = 0; i < countUsers; i++)
                {
                    // если клиент авторизирован, то отправляем 
                    if (Clients[i]._AuthOn == true)
                        Clients[i].SendMessage(); // обновляем чат у каждого клиента
                }
            }
            catch (Exception exp)
            {
                Console.WriteLine($"Ошибка в методе UpdateAllChats: {exp.Message}");
            }
        }

        public static void SendOnlineUsersAllChats() // отправляет всем список пользователей онлайн 
        {
            try
            {
                int countUsers = Clients.Count;  // смотрим сколько клиентов подключено к серверу (сколько клиентов в списке Clients)

                for (int i = 0; i < countUsers; i++)
                {
                    // если клиент авторизирован, то отправляем 
                    if (Clients[i]._AuthOn == true)
                        Clients[i].SendOnlineUsers(); // отправляем список пользователей онлайн 
                }
            }
            catch (Exception exp)
            {
                Console.WriteLine($"Ошибка в методе SendOnlineUsersAllChats: {exp.Message}");
            }
        }
    }
}
