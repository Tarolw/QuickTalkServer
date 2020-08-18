using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Security.Cryptography;
using System.IO;

namespace serverChat
{
    class Program
    {
        private static string _serverIp { get; set; } // IP
        private static int _serverPort { get; set; } = 8005; // порт
        private static Thread _serverThread; // поток в котором будем запускать сервер

        static void Main(string[] args)
        {
            Console.WriteLine(
                              "* Чтобы запустить сервер введите команду /start, \n" +
                              "* Перед запуском сервера вы можете именить порт командой /setport, \n" +
                              "* по умолчанию будет установлен 8005 порт.\n" +
                              "* Для просмотра списка команд введите /help."
                              );
            // cчитываем команды из консоли
            while (true)
            {
                HandlerCommands(Console.ReadLine());
            }
        }

        private static void HandlerCommands(string cmd) // обработчик команд
        {
            cmd = cmd.ToLower(); // приводит команду к нижнему регистру
            if (cmd.Contains("/help"))
            {
                Console.WriteLine(
                                 "* Список команд:\n" +
                                 "* /start - запустить сервер\n" +
                                 "* /getusers - посмотерть список пользователей онлайн\n" +
                                 "* /showkeyiv - показать секретный ключ и вектор инициализации\n" + 
                                 "* /createkey - сгенерировать секретныый ключ и записать его в key.dat\n" + 
                                 "* /createiv - сгенерировать вектор инициализации и записать его в iv.dat\n" +
                                 "* /deletekey - удалить секретныый ключ\n" +
                                 "* /deleteiv - удалить вектор инициализации\n" +
                                 "* /setport - задать порт\n" +
                                 "* /regoff - откючить регистрацию новых пользователей\n" +
                                 "* /regon - включить регистрацию новых пользователей\n"
                                 );
            }
            else if (cmd.Contains("/start"))
            {
                SetKeyIv(); // получаем ключ и вектор инициализации

                // запускаем функуцию startServer() в фоновом потоке
                _serverThread = new Thread(StartServer);
                _serverThread.IsBackground = true;
                _serverThread.Start();
            }
            else if (cmd.Contains("/getusers"))
            {
                int countUsers = Server.Clients.Count;  // вычисляем количество подключенных клиентов
                // выводим список клиентов
                for (int i = 0; i < countUsers; i++)
                {
                    Console.WriteLine($"[{i}]: {Server.Clients[i]._UserName}");
                }
            }
            else if (cmd.Contains("/regoff"))
            {
                if (Client._Reg == true)
                {
                    Client._Reg = false;
                    Console.WriteLine("Регистрация отключена!");
                }
                else
                {
                    Console.WriteLine("Регистрация уже была отключена!");
                }
            }
            else if (cmd.Contains("/regon"))
            {
                if (Client._Reg == false)
                {
                    Client._Reg = true;
                    Console.WriteLine("Регистрация включена!");
                }
                else
                {
                    Console.WriteLine("Регистрация уже была включена!");
                }
            }
            else if (cmd.Contains("/createkey"))
            {
                using (TripleDESCryptoServiceProvider des = new TripleDESCryptoServiceProvider()) // создаем экземпляр класса TripleDESCryptoServiceProvider для доступа к версии служб шифрования (CSP англ. cryptography service provider) Triple DES алгоритма.
                {
                    if (!File.Exists("key.dat"))
                    {
                        DESCryptography.Key = des.Key;                                                  
                        File.WriteAllText("key.dat", Convert.ToBase64String(DESCryptography.Key));
                        Console.WriteLine("Файл key.dat с новым секретным ключем создан.");
                    }
                    else
                    {
                        Console.WriteLine("Файл key.dat уже существует, удалите его командой /deletekey.");
                    }
                }
            }
            else if (cmd.Contains("/createiv"))
            {
                using (TripleDESCryptoServiceProvider des = new TripleDESCryptoServiceProvider()) // создаем экземпляр класса TripleDESCryptoServiceProvider для доступа к версии служб шифрования (CSP англ. cryptography service provider) Triple DES алгоритма.
                {
                    if (!File.Exists("iv.dat"))
                    {
                        DESCryptography.Iv = des.IV;
                        File.WriteAllText("iv.dat", Convert.ToBase64String(DESCryptography.Iv));
                        Console.WriteLine("Файл iv.dat с новым вектором инициализауии создан.");
                    }
                    else
                    {
                        Console.WriteLine("Файл iv.dat уже существует, удалите его командой /deleteiv.");
                    }
                }
            }
            else if (cmd.Contains("/showkeyiv"))
            {
                if (File.Exists("key.dat"))
                {
                    Console.WriteLine($"Секретныйы ключ: {File.ReadAllText("key.dat")}");
                }
                else
                {
                    Console.WriteLine("Файл key.dat отсутствует.");
                }
                if (File.Exists("iv.dat"))
                {
                    Console.WriteLine($"Вектор инициализации: {File.ReadAllText("iv.dat")}");
                }
                else
                {
                    Console.WriteLine("Файл iv.dat отсутствует.");
                }
            }
            else if (cmd.Contains("/deletekey"))
            {
                if (!File.Exists("key.dat"))
                {
                    Console.WriteLine("Файл key.dat не найден.");
                }
                else
                {
                    File.Delete("key.dat");
                    Console.WriteLine("Файл key.dat  удален.");
                }
            }
            else if (cmd.Contains("/deleteiv"))
            {
                if (!File.Exists("iv.dat"))
                {
                    Console.WriteLine("Файл iv.dat не найден.");
                }
                else
                {
                    File.Delete("iv.dat");
                    Console.WriteLine("Файл iv.dat  удален.");
                }
            }
            else if (cmd.Contains("/setport"))
            {
                    Console.WriteLine("Введите порт:");
                    _serverPort = Int32.Parse(Console.ReadLine());
                    Console.WriteLine($"Порт изменен на {_serverPort}");
            }
            else
            {
                Console.WriteLine($"Команды {cmd} не существует.");
                Console.WriteLine($"Введите /help, чтобы посмотреть список команд.");
            }
        }

        private static void SetKeyIv() // получаем секретный ключ и вектор инициализации
        {
            using (TripleDESCryptoServiceProvider des = new TripleDESCryptoServiceProvider()) // создаем экземпляр класса TripleDESCryptoServiceProvider для доступа к версии служб шифрования (CSP англ. cryptography service provider) Triple DES алгоритма.
            {
                if (File.Exists("key.dat"))                                                         // если существует файл "key.dat", то
                    DESCryptography.Key = Convert.FromBase64String(File.ReadAllText("key.dat"));    // считываем из него секретный ключ
                else
                {
                    Console.WriteLine("Файл key.dat отсутствует, создайте его командой /createkey");
                }

                if (File.Exists("iv.dat"))                                                          // если существует файл "iv.dat", то
                    DESCryptography.Iv = Convert.FromBase64String(File.ReadAllText("iv.dat"));      // считываем из него вектор инициализации
                else
                {
                    Console.WriteLine("Файл iv.dat отсутствует, создайте его командой /createiv");
                }
            }
        }

        private static void StartServer() // метод запускает сервер
        {
            IPAddress[] ipv4Addresses = Array.FindAll(
            Dns.GetHostEntry(string.Empty).AddressList,
            a => a.AddressFamily == AddressFamily.InterNetwork);
            _serverIp = ipv4Addresses[0].ToString(); // считываем IP
            IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Parse(_serverIp), _serverPort); // помещаем айпи и порт в ipEndPoint (конечная точка)
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp); // создаем сокет, Stream потому что TCP. "iPAddress.AddressFamily", AddressFamily.InterNetwork (IPv4-адрес.)
            socket.Bind(ipEndPoint); // связываем сокет с точкой, по которой будем принимать данные
            socket.Listen(1000); // начинаем прослушивание, в скобках лимит одноверменных подключений к серверу
            Console.WriteLine($"Сервер запущен на IP: {ipEndPoint}");
            while (true)
            {
                try
                {
                    Socket user = socket.Accept();
                    Server.NewClient(user);
                }
                catch(Exception exp)
                {
                    Console.WriteLine($"Ошибка: {exp.Message}");
                }
            }

        }
    }
}
