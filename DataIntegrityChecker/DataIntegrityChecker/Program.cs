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

namespace DataIntegrityChecker
{
    class Program
    {
        static void Main(string[] args)
        {
            MainAsync(default(CancellationToken)).Wait();
        }

        static async Task MainAsync(CancellationToken token)
        {
            var connStr = JsonConvert.DeserializeObject<dynamic>(File.ReadAllText("config.json")).Oj;
            var builder = new DbContextOptionsBuilder<OnlineJudgeContext>();
            builder.UseMySql((string)connStr);
            var cnt = 0;
            using (var db = new OnlineJudgeContext(builder.Options))
            {
                var problemIds = await db.Problems
                    .Where(x => x.Source == ProblemSource.Local)
                    .Select(x => x.Id)
                    .ToListAsync(token);

                var mgmt = new ManagementServiceClient("https://mgmtsvc.1234.sh", @"C:\Users\Yuko\Documents\webapi-client.pfx", "123456");

                var tasks = new List<Task>();
                var cancel = new CancellationTokenSource();
                var ret = new ConcurrentBag<(string, Guid)>();
                foreach (var id in problemIds)
                {
                    cnt++;
                    if (cnt % 10 == 0)
                    {
                        Console.WriteLine(cnt + " problems handled.");
                    }

                    foreach(var x in await db.TestCases.Where(x => x.ProblemId == id).ToListAsync(token))
                    tasks.Add(Task.Run(async ()=> {
                        try
                        {
                            await mgmt.GetBlobAsync(x.InputBlobId);
                        }
                        catch
                        {
                            ret.Add((x.ProblemId, x.InputBlobId));
                            Console.WriteLine("[Failed] " + x.ProblemId + ", input id = " + x.InputBlobId);
                        }
                        try
                        {
                            await mgmt.GetBlobAsync(x.OutputBlobId);
                        }
                        catch
                        {
                            ret.Add((x.ProblemId, x.OutputBlobId));
                            Console.WriteLine("[Failed] " + x.ProblemId + ", output id = " + x.OutputBlobId);
                        }
                    }));
                    await Task.WhenAll(tasks);
                }

                File.WriteAllText("result.txt", string.Join("\r\n", ret.Select(x => x.Item1 + " " + x.Item2)));
            }
        }
    }
}
