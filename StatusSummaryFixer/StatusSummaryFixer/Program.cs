using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using JoyOI.OnlineJudge.Models;

namespace StatusSummaryFixer
{
    class Program
    {
        static void Main(string[] args)
        {
            var builder = new DbContextOptionsBuilder<OnlineJudgeContext>();
            Console.WriteLine("Please input the connection string of Joy OI online judge database.");
            Console.Write("Connection string=");
            builder.UseMySql(Console.ReadLine());
            builder.UseMySqlLolita();
            var db = new OnlineJudgeContext(builder.Options);
            Console.WriteLine("1=Fix specified user, 2=Fix register after time");
            Console.Write("Your choice=");
            var choice = Convert.ToInt16(Console.ReadLine());
            if (choice == 1)
            {
                Console.WriteLine("Please input the user which you want to fix.");
                Console.Write("Username=");
                var username = Console.ReadLine();
                FixOneUser(username, db);
            }
            else
            {
                Console.WriteLine("Please input the time(YYYY-MM-DD).");
                Console.Write("Time=");

                var time =Convert.ToDateTime(Console.ReadLine());
                var users = db.Users
                    .Where(x => x.RegisteryTime >= time)
                    .Select(x => x.UserName)
                    .ToList();

                foreach (var x in users)
                    FixOneUser(x, db);
            }
            Console.WriteLine("Hello World!");
        }

        static void FixOneUser(string username, OnlineJudgeContext db)
        {
            Console.WriteLine("Processing user: " + username);
            var summary = db.JudgeStatuses
                .Where(x => x.User.UserName == username && string.IsNullOrEmpty(x.ContestId) && !x.IsSelfTest)
                .OrderBy(x => x.Result)
                .GroupBy(x => x.ProblemId)
                .Select(x => new { x.Key, x.First().Result })
                .ToList();

            var ac = summary.Where(x => x.Result == JudgeResult.Accepted).Select(x => x.Key).ToList();
            var tried = summary.Select(x => x.Key).ToList();
            db.Users
                .Where(x => x.UserName == username)
                .SetField(x => x.PassedProblems).WithValue(JsonConvert.SerializeObject(ac))
                .SetField(x => x.TriedProblems).WithValue(JsonConvert.SerializeObject(tried))
                .Update();
            Console.WriteLine("User: " + username + " has been fixed");
        }
    }
}
