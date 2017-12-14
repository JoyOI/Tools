using System;
using System.IO;
using System.Text.RegularExpressions;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;

namespace PhoneForceModifier
{
    class Config
    {
        public string Uc { get; set; }

        public string Blog { get; set; }

        public string Forum { get; set; }

        public string Oj { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Please input the user which you want to change his phone number");
            Console.Write("Username=");
            var username = Console.ReadLine();

            Console.WriteLine("Please input a new phone number");
            Console.Write("Phone=");
            var phone = Console.ReadLine();
            var regex = new Regex("[+]{0,1}[0-9]{4,16}");
            if (!regex.IsMatch(phone))
            {
                Console.WriteLine("The phone number is invalid.");
                Console.WriteLine("Press any key to exit.");
                Console.Read();
                return;
            }

            var config = JsonConvert.DeserializeObject<Config>(File.ReadAllText("config.json"));
            HandleUser(config.Uc, username, phone);
            HandleUser(config.Blog, username, phone);
            HandleUser(config.Forum, username, phone);
            HandleUser(config.Oj, username, phone);
            Console.WriteLine("Succeeded");
            Console.WriteLine("Press any key to exit.");
            Console.Read();
        }

        static void HandleUser(string connStr, string username, string phone)
        {
            using (var conn = new MySqlConnection(connStr))
            {
                conn.Open();
                using (var cmd = new MySqlCommand("UPDATE `AspNetUsers` SET `PhoneNumber` = @p1, `PhoneNumberConfirmed` = 1 WHERE `Username` = @p2", conn))
                {
                    cmd.Parameters.Add(new MySqlParameter("p1", phone));
                    cmd.Parameters.Add(new MySqlParameter("p2", username));
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}
