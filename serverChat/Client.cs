using System;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Threading;

namespace serverChat
{
    public class Client
    {
        private Socket _handler; //  сокет
        private Thread _userThread; //  поток 
        public Client(Socket socket)
        {
            _handler = socket;
            _userThread = new Thread(Listener) // запускаем функцию прослушивания входящих сообщений от клиента в потоке
            {
                IsBackground = true // делаем поток фоновым
            }; 
            _userThread.Start(); // запускаем поток
        }

        public string _UserName { get; private set; } // имя пользователя
        public bool _AuthOn { get; set; } = false; // флаг авторизации
        public static bool _Reg { get; set; } = false; // флаг включения регистрации на сервере

        private void Listener() // метод приема сообщений от клиентов
        {
            while (true)
            {
                try
                {
                    byte[] buffer = new byte[65536];  // создаем буфер
                    int bytesRec = _handler.Receive(buffer); // принимаем зашифрованное сообщение
                    Array.Resize(ref buffer, bytesRec);
                    buffer = DESCryptography.Decrypt(buffer, DESCryptography.Key, DESCryptography.Iv); // расшифровываем сообщение
                    string data = Encoding.UTF8.GetString(buffer); // переводим массив байтов в текст
                    HandleCommand(data); // передаем сообщение в обработчик команд
                }
                catch { Server.EndClient(this); return; } // в случае ошибки отключаем клиента
            }
        }

        public void End() // метод закрывающий сокет и завершающий поток
        {
            try
            {
                _handler.Close(); // закрываем сокет и освобождаем все связанные ресурсы
                try
                {
                    _userThread.Abort(); // Вызывает исключение ThreadAbortException в вызвавшем его потоке для того, чтобы начать процесс завершения потока. 
                }
                catch
                {
                    // ---
                }
            }
            catch(Exception exp)
            {
                Console.WriteLine($"Ошибка в методе End {exp.Message}");
            }
        }

        private void HandleCommand(string data) // обработчик команд
        {
            if (data.Contains("#authoriz")) // авторизация
            {
                string NameAndPass = data.Split('&')[1];
                string Name = NameAndPass.Split('~')[0];
                string Password = NameAndPass.Split('~')[1];
                if(Authorization(Name, Password))
                {
                    Thread.Sleep(1000);
                    Server.SendOnlineUsersAllChats();
                    Thread.Sleep(1000);
                    Server.SendHistoryNewUser(this);
                }
                return;
            }
            else if (data.Contains("#registr")) // регистрация
            {
                string NameAndPass = data.Split('&')[1];
                string Name = NameAndPass.Split('~')[0];
                string Password = NameAndPass.Split('~')[1];
                if(Registration(Name, Password))
                {
                    Thread.Sleep(1000);
                    Server.SendOnlineUsersAllChats();
                    Thread.Sleep(1000);
                    Server.SendHistoryNewUser(this);
                }
                return;
            }
            else if (data.Contains("#newmsg")) // новое сообщение
            {
                string message = data.Split('&')[1];
                ChatController.AddMessage(_UserName, message, DateTime.Now.ToShortTimeString());
                return;
            }
        }
        
        private bool Authorization(string NameUser, string HashPassword) // авторизация
        {
            bool AlreadyAuth = false; // флаг "уже авторизирован"
            foreach(Client cl in Server.Clients)
            {
                // смотрим не авторизирован ли уже пользователь с таким именем
                if(cl._UserName == NameUser)
                {
                    AlreadyAuth = true;
                    // отправляем клиенту комаду о том, что пользователь уже авторизирован
                    Send("#alreadyauth&");
                }
            }
            if (!AlreadyAuth)
            { 
                int ID = 0;
                Guid pass = new Guid(HashPassword);
                using (Context db = new Context())
                {
                    // ищем в бд пару имя и хэш пароля
                    var ids = db.Accounts.Where(a => a.Name == NameUser && a.Password == pass); 
                    foreach (var id in ids)
                    {
                        ID = id.ID;
                    }
                }
                if (ID > 0) // если такая пара есть, то авторизируем пользователя
                {
                    _UserName = NameUser;
                    _AuthOn = true;
                    // отправляем клиенту сообщение об успешной авторизации
                    Send($"#auth&");
                    Console.WriteLine($"Пользоватеь {NameUser} авторизирован.");
                    return true;
                }
                else
                {
                    // отправляем клиенту сообщение о том, что неправильный логин или пароль
                    Send($"#badauth&");
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        private bool Registration(string NameUser, string HashPassword) // регистрация
        {
            if (_Reg)
            {
                int ID = 0;
                Guid pass = new Guid(HashPassword);
                using (Context db = new Context())
                {
                    // ищим в базе данных пользователя с таким именем
                    var ids = db.Accounts.Where(a => a.Name == NameUser);
                    foreach (var id in ids)
                    {
                        ID = id.ID;
                    }
                    // если ткой пользователь не найден то проводим регистрацию
                    if (ID == 0)
                    {
                        Account account = new Account
                        {
                            Name = NameUser,
                            Password = pass
                        };
                        db.Accounts.Add(account);
                        db.SaveChanges();
                        _UserName = NameUser;
                        _AuthOn = true;
                        // отправляем клиенту сообщение об успешной регистраци 
                        Send("#regandauth&");
                        Console.WriteLine($"Пользоватеь {NameUser} зарегистрирован и авторизирован.");
                        return true;
                    }
                    else
                    {
                        // отправляем клиенту сообщение о том, что пользователь с таким именем уже зарегистрирован
                        Send($"#badreg&");
                        return false;
                    }
                }
            }
            else
            {
                // отправляем клиенту 
                Send($"#regoff&");
                return false;
            }
        }

        public void SendHistory() // отправляет историю 100 последних сообщений
        {
            Send(ChatController.GetChat());  // передаем в метод Send историю сообщений
        }

        public void SendMessage() // отправляет сообщение
        {
            Send(ChatController.GetMessage());  // передаем в метод Send сообщение
        }

        public void SendOnlineUsers() // отправляет список пользователей онлайн
        {
            Send(ChatController.GetOnlineUsers());  // передаем в метод Send список пользователей
        }

        public void Send(string command)  // метод отправки данных клиенту 
        {
            {
                try
                {
                    if (command == string.Empty)
                    {
                        return;
                    }
                    byte[] buffer = DESCryptography.Encrypt(command, DESCryptography.Key, DESCryptography.Iv); // шифруем сообщение
                    int bytesSent = _handler.Send(buffer); // отправляем зашифрованное сообщение
                    if (bytesSent > 0) Console.WriteLine("Сообщение успешно отправленно.");
                }
                catch (Exception exp)
                {
                    Console.WriteLine($"Ошибка в методе Send: {exp.Message}.");
                    Server.EndClient(this); // В случае ошибки отключаем клиента
                }
            }
        }
    }
}
