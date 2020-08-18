using System;
using System.Collections.Generic;

namespace serverChat
{
    public static class ChatController //Создаем статический класс ChatController
    {
        private const int _maxMessage = 100; // _maxMessage задаем максимальное количество сообщений
        public static List<Message> Chat = new List<Message>(); // список сообщений в чате (содержит структуры ( Имя + текст + время))
        public struct Message  // создаем структуру message
        {
            public string userName; // Имя
            public string data;     // Текст сообщения
            public string time;     // Время получения сообщения

            public Message(string name, string msg, string time) // метод принимающий имя и сообщение и время
            {
                userName = name;
                data = msg;
                this.time = time;
            }
        }

        public static void AddMessage(string userName, string msg, string time) // добавляет сообщение в список сообщений
        {
            try
            {
                if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(msg)) return; // если переданы не полные данные, то выходим из функции
                int countMessages = Chat.Count;  // смотрим сколько сообщений в чате (проверяем кол-во структур в списке)
                if (countMessages > _maxMessage)  // если больше в данном случае 100, то вызываем функцию очистки чата, которая очищает список List<message> Chat
                {
                    ClearChat();
                }

                // создаем новую структуру message заносим в нее Имя + Сообщение + Время и помещаем ее в список List<message> Chat
                Message newMessage = new Message(userName, msg, time);
                Chat.Add(newMessage); // добавляем структуру в список
                Console.WriteLine($"Новое сообщение от клиента {userName}"); 
                Server.SendMessageAllChats(); // Обновляем чат у всех клиентов

            }
            catch(Exception exp)
            {
                Console.WriteLine($"Ошибка в AddMessage {exp.Message}");
            }
        }

        public static void ClearChat() // очищает чат
        {
            Chat.RemoveRange(0, 1); // удалеем первое сообщение
        }

        public static string GetMessage()  // Получает последнее принятое сервером сообщение
        {
            try
            {
                string data = "#updatechat&";
                int indexLastMessages = Chat.Count - 1;
                int countMessages = Chat.Count; // смотрим сколько сообщений в чате ( сколько структур message в списке Chat)
                if (countMessages == 0) return string.Empty; // если сообщений нет, то метод возвращает пустую строку
                    data += $"{Chat[indexLastMessages].time}~ <{Chat[indexLastMessages].userName}> {Chat[indexLastMessages].data}|";
                return data;
            }
            catch (Exception exp)
            {
                // при ошибке возвращаем пустую строку
                Console.WriteLine($"Ошибка в getChat: {exp.Message}");
                return string.Empty;
            }
        }

        public static string GetOnlineUsers() // получает список пользователей онлайн
        {
            try
            {
                string data = "#getonline&";
                int countUsers = Server.Clients.Count;  // вычисляем количество подключенных клиентов
                for (int i = 0; i < countUsers; i++)
                {
                    if(Server.Clients[i]._AuthOn == true)
                        data += $"{Server.Clients[i]._UserName}|";
                }
                return data;
            }
            catch (Exception exp)
            {
                // при ошибке возвращаем пустую строку
                Console.WriteLine($"Ошибка в GetUsers: {exp.Message}");
                return string.Empty;
            }
        }

        public static string GetChat()  // Получить собщения
        {
            try
            {
                string data = "#history&";  
                int countMessages = Chat.Count; // смотрим сколько сообщений в чате ( сколько структур message в списке Chat)
                if (countMessages == 0) return string.Empty; // если сообщений нет, то метод возвращает пустую строку
                // формируем сообщения из данных структуры и заносим в переменную data, начиная с нулевого в списке и заканчивая количеством структур в списке
                for (int i = 0; i < countMessages; i++) 
                {
                    data += $"{Chat[i].time}~ <{Chat[i].userName}> {Chat[i].data}|";
                }
                return data;
            }
            catch (Exception exp)
            {
                // при ошибке возвращаем пустую строку
                Console.WriteLine($"Ошибка в getChaforNewUsert: {exp.Message}");
                return string.Empty;
            }
        }


    }
}
