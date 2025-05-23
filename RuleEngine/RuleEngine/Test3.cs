using Newtonsoft.Json;
using RulesEngine.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RuleEngine
{
    class Program3
    {
        private static readonly RulesEngine.Models.ReSettings reSettings = new RulesEngine.Models.ReSettings
        {
            CustomTypes = new[] { typeof(IdCardUtil) }
        };

        static async Task Main(string[] args)
        {
            //模拟用户的输入内容
            var userInput = new UserInput
            {
                IdNo = null,
                Age = 18
            };


            //定义规则
            // ""Expression"": ""IdNo.GetAgeByIdCard() < 18""  ，这一句可以和下面判断方法是相同的
            //var rulesStr = @"[{
            //        ""WorkflowName"": ""UserInputWorkflow"",
            //        ""Rules"": [
            //                        {
            //                            ""RuleName"": ""CheckAge"",
            //                            ""ErrorMessage"": ""年龄必须大于18岁."",
            //                            ""ErrorType"": ""Error"",
            //                            ""localParams"": [
            //                              {
            //                                    ""Name"": ""model1"",
            //                                    ""Expression"": ""Age!=0""
            //                              },
            //                              {
            //                                    ""Name"": ""model2"",
            //                                    ""Expression"": ""IdCardUtil.getAgeByIdCardNo(Test.IdNo) < 18""
            //                              }
            //                            ],
            //                            ""RuleExpressionType"": ""LambdaExpression"",
            //                            ""Expression"": ""model1 AND model2""
            //                        },
            //                        {
            //                            ""RuleName"": ""CheckIDNoIsEmpty"",
            //                            ""ErrorMessage"": ""身份证号不可以为空."",
            //                             ""ErrorType"": ""Error"",
            //                            ""RuleExpressionType"": ""LambdaExpression"",
            //                            ""Expression"": ""IdNo != null ""
            //                        }
            //                    ]
            //                }] ";



            string filePath = "Test3.json"; // 文件路径
            string rulesStr = File.ReadAllText(filePath);

            RulesEngine.Models.RuleParameter ruleParameter = new RulesEngine.Models.RuleParameter("Test", userInput);

            //反序列化Json格式规则字符串
            var workflowRules = JsonConvert.DeserializeObject<List<WorkflowRules>>(rulesStr);

            //初始化规则引擎
            var rulesEngine = new RulesEngine.RulesEngine(workflowRules.ToArray(), reSettings);



            //使用规则进行判断，并返回结果
            List<RuleResultTree> resultList = await rulesEngine.ExecuteAllRulesAsync("UserInputWorkflow", ruleParameter);

            //返回结果并展示
            foreach (var item in resultList)
            {
                Console.WriteLine("验证成功：{0}，消息：{1}", item.IsSuccess, item.ExceptionMessage);
            }

            Console.ReadLine();
        }
    }
}
