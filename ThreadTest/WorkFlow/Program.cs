using System.Xml.Linq;
using WorkflowCore.Interface;
using WorkflowCore.Models;
using WorkflowCore.Services;
namespace WorkFlow
{
    public class HelloWorld : IActivity
    {
        public string Id => "HelloWorld";
        public int Version => 1;

        public Task<ExecutionResult> ExecuteAsync(IStepExecutionContext context)
        {
            Console.WriteLine("Hello World!");
            return ExecutionResult.Next();
        }
    }

    public class MyWorkflow : IWorkflow<MyData>
    {
        public string Id => "MyWorkflow";
        public int Version => 1;
        public void Build(IWorkflowBuilder<MyData> builder)
        {
            builder
                .StartWith<HelloWorld>()
                .Then(context =>
                {
                    Console.WriteLine("Workflow completed.");
                    return ExecutionResult.Finish();
                });
        }
    }

    class Program
    {
        static async Task Main(string[] args)
        {
            var builder = new WorkflowBuilder<MyData>();
            builder.WorkflowId("myinstanceid");
            var workflowId = await builder.StartAsync();

            Console.WriteLine($"Workflow started with ID {workflowId}");
            Console.ReadKey();
        }
    }
}
