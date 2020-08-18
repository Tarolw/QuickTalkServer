using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace serverChat
{
    static class DESCryptography // класс для шифрования и расшифровки сообщений с помощью алгоритма шифрования Triple DES
    {

        public static byte[] Key { get; set; } // свойство для хранения секретного ключа
        public static byte[] Iv { get; set; } // свойство для хранения вектора инициализации

        /*
         *Triple DES — симметричный блочный шифр, созданный Уитфилдом Диффи, Мартином Хеллманом и Уолтом Тачманном в 1978
         * году на основе алгоритма DES с целью устранения главного недостатка последнего — малой длины ключа, который может
         * быть взломан методом полного перебора ключа.
         */

        // метод шифрует данные
        static public byte[] Encrypt(string text, byte[] key, byte[] iv)
        {
            byte[] result; // создаем массив байтов, куда будут записанны зашифрованные данные

            // using определяет область, по завершении которой объект удаляется
            // создаем экземпляр класса TripleDESCryptoServiceProvider для доступа к версии служб шифрования (CSP англ. cryptography service provider) Triple DES алгоритма.
            using (TripleDESCryptoServiceProvider des = new TripleDESCryptoServiceProvider()) 
            {

                des.IV = iv; // присваиваем вектор инициализации
                des.Key = key; // присваиваем секретный ключ

                // создаем симметричный TripleDES объект-шифратор с заданным ключем и вектором инициализации
                ICryptoTransform encryptor = des.CreateEncryptor(des.Key, des.IV);
                // создаем экземпляр класса MemoryStream (поток , резервным хранилищем которого является память) с расширяемой емкостью, инициализированной нулевым значением.
                using (MemoryStream stream = new MemoryStream()) 
                {
                    // определяем поток, который связывает потоки данных с криптографическими преобразованиями.
                    using (CryptoStream cstream = new CryptoStream(stream, encryptor, CryptoStreamMode.Write)) 
                    {
                        // создаем экземпляр класса StreamWriter (реализует TextWriter для записи символов в поток) для указанного потока, использует кодировку UTF-8 и размер буфера по умолчанию 
                        using (StreamWriter sw = new StreamWriter(cstream)) 
                        {
                            sw.Write(text); // зписываем в поток строку
                        }
                        result = stream.ToArray(); // записываем содержимое потока в массив байтов result
                    }
                }
            }
            return result; // возвращаем зашифрованные данные
        }

        // метод расшифровывает данные
        static public byte[] Decrypt(byte[] chiper, byte[] key, byte[] iv)
        {
            string text; // создаем переменную для расшифрованных данных
            using (TripleDESCryptoServiceProvider des = new TripleDESCryptoServiceProvider())
            {
                des.IV = iv; // присваиваем вектор инициализации
                des.Key = key; // присваиваем секретный ключ
                // создаем симметричный TripleDES объект-дешифратор с указанным ключем и вектором инициализации
                ICryptoTransform decryptor = des.CreateDecryptor(des.Key, des.IV);
                // создаем экземпляр класса MemoryStream (поток , резервным хранилищем которого является память) с 
                // расширяемой емкостью, инициализированной нулевым значением.
                using (MemoryStream stream = new MemoryStream(chiper)) 
                {
                    // определяем поток, который связывает потоки данных с криптографическими преобразованиями.
                    using (CryptoStream cstream = new CryptoStream(stream, decryptor, CryptoStreamMode.Read)) 
                    {
                        // реализуем объект TextReader, который считывает символы из потока байтов в кодировке UTF-8
                        using (StreamReader sr = new StreamReader(cstream)) 
                        {
                            // считываем все символы, начиная с текущей позиции до конца потока
                            text = sr.ReadToEnd(); 
                        }
                    }
                }
            }
            return Encoding.UTF8.GetBytes(text); // возвращаем расшифрованные данные
        }
    }
}
