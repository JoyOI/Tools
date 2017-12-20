using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using JoyOI.OnlineJudge.Models;
using JoyOI.ManagementService.SDK;
using Newtonsoft.Json;

namespace RejudgeTool
{
    class Program
    {
        static void Main(string[] args)
        {
            while (true)
            {
                var id = Guid.Parse(Console.ReadLine());
                var builder = new DbContextOptionsBuilder<OnlineJudgeContext>();
                var connStr = JsonConvert.DeserializeObject<dynamic>(File.ReadAllText("config.json")).Oj;
                builder.UseMySql((string)connStr);
                using (var db = new OnlineJudgeContext(builder.Options))
                {
                    var statemachineId = db.JudgeStatusStateMachines.Where(x => x.StatusId == id).Last().StateMachineId;
                    var mgmt = new ManagementServiceClient("https://mgmtsvc.1234.sh", @"C:\Users\Yuko\Documents\webapi-client.pfx", "123456");
                    mgmt.PatchStateMachineInstanceAsync(statemachineId, "Start");
                    Console.WriteLine("OK");
                }
            }
        }
    }
}
