using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace DurableFunctions
{
    public static class Functions
    {
        [FunctionName("YapaxiEntryFunction")]
        public static async Task Run([TimerTrigger("*/10 * * * * *")]TimerInfo myTimer, ILogger log, [OrchestrationClient]DurableOrchestrationClient starter)
        {
            log.LogInformation($"C# Timer trigger function started at: {DateTime.Now}");
            var instanceId = await starter.StartNewAsync(nameof(YapaxiOrchestrator), null);
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}, {instanceId}");
        }

        [FunctionName(nameof(YapaxiOrchestrator))]
        public static async Task YapaxiOrchestrator([OrchestrationTrigger] DurableOrchestrationContext context, ILogger log)
        {
            log.LogInformation("1 " + context.InstanceId);

            var val1 = await context.CallActivityAsync<string>(nameof(YapaxiActivity1), context.InstanceId);

            await Task.Run(async () =>
            {
                log.LogInformation("2 " + context.InstanceId);
                await Task.Delay(20000);
                log.LogInformation("3 " + context.InstanceId);
            });

            await context.CreateTimer(DateTime.UtcNow.AddSeconds(2), CancellationToken.None);

            var val2 = await context.CallActivityAsync<string>(nameof(YapaxiActivity2), context.InstanceId);

            log.LogInformation("4 " + context.InstanceId);
        }

        [FunctionName(nameof(YapaxiActivity1))]
        public static async Task<string> YapaxiActivity1(
            [ActivityTrigger] DurableActivityContext context, 
            [OrchestrationClient] DurableOrchestrationClient client
        )
        {
            var iid = context.GetInput<string>();
            return iid;
        }

        [FunctionName(nameof(YapaxiActivity2))]
        public static async Task<string> YapaxiActivity2(
            [ActivityTrigger] DurableActivityContext context,
            [OrchestrationClient] DurableOrchestrationClient client
        )
        {
            var iid = context.GetInput<string>();
            return iid;
        }
    }
}
