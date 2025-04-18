using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Quartz;
namespace QuartzTest
{
    //DisallowConcurrentExecution
    [PersistJobDataAfterExecution,DisallowConcurrentExecution]
    class JobLgoTask : IJob
    {
        public string Name { get; set; }
        public string Age { get; set; }

        public string IsRun { get; set; }

       
        public async Task Execute(IJobExecutionContext context)
        {
            await Console.Out.WriteAsync(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"));
            await Console.Out.WriteLineAsync($"name:{Name},age:{Age},IsRun:{IsRun}");
            context.JobDetail.JobDataMap["Age"] = Convert.ToInt32(Age) + 1;
            Thread.Sleep(2000);
        }
    }
}
