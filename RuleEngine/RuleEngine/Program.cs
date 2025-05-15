using RulesEngine.Models;
using Newtonsoft.Json;

namespace RuleEngine
{
    class Program
    {
        static async Task Main1(string[] args)
        {
            //模拟用户的输入内容
            var userInput = new UserInput
            {
                IdNo = null,
                Age = 18
            };


            //定义规则
            var rulesStr = @"[{
                    ""WorkflowName"": ""UserInputWorkflow"",
                    ""Rules"": [
                                 {
                                    ""RuleName"": ""CheckAge"",
                                    ""ErrorMessage"": ""年龄必须大于18岁."",
                                    ""ErrorType"": ""Error"",
                                    ""RuleExpressionType"": ""LambdaExpression"",
                                    ""Expression"": ""Age > 18""
                                },
                                {
                                    ""RuleName"": ""CheckIDNoIsEmpty"",
                                    ""ErrorMessage"": ""身份证号不可以为空."",
                                     ""ErrorType"": ""Error"",
                                    ""RuleExpressionType"": ""LambdaExpression"",
                                    ""Expression"": ""IdNo != null""
                                }
                                ]
                            }] ";



            //反序列化Json格式规则字符串

            var workflowRules = JsonConvert.DeserializeObject<List<WorkflowRules>>(rulesStr);
            //初始化规则引擎
            var rulesEngine = new RulesEngine.RulesEngine(workflowRules.ToArray());

            //使用规则进行判断，并返回结果
            List<RuleResultTree> resultList = await rulesEngine.ExecuteAllRulesAsync("UserInputWorkflow", userInput);

            //返回结果并展示
            foreach (var item in resultList)
            {
                Console.WriteLine("验证成功：{0}，消息：{1}", item.IsSuccess, item.ExceptionMessage);
            }

            //RulesEngine.Models.Rule

            Console.ReadLine();
        }
    }

    public class UserInput
    {
        public string IdNo { get; set; }
        public int Age { get; set; }
    }
}
