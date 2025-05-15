using Newtonsoft.Json;
using RulesEngine.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RuleEngine
{
    class Program2
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
            var rulesStr = @"[{
                    ""WorkflowName"": ""UserInputWorkflow"",
                    ""Rules"": [
                                 {
                                    ""RuleName"": ""CheckAge"",
                                    ""ErrorMessage"": ""年龄必须大于18岁."",
                                    ""ErrorType"": ""Error"",
                                    ""RuleExpressionType"": ""LambdaExpression"",
                                    ""Expression"": ""IdCardUtil.getAgeByIdCardNo(IdNo) < 18""
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
            var rulesEngine = new RulesEngine.RulesEngine(workflowRules.ToArray(), reSettings);



            //使用规则进行判断，并返回结果
            List<RuleResultTree> resultList = await rulesEngine.ExecuteAllRulesAsync("UserInputWorkflow", userInput);

            //返回结果并展示
            foreach (var item in resultList)
            {
                Console.WriteLine("验证成功：{0}，消息：{1}", item.IsSuccess, item.ExceptionMessage);
            }

            Console.ReadLine();
        }
    }



    public static class IdCardUtil
    {
        //此处使用了C#的方法扩展
        public static int GetAgeByIdCard(this string str)
        {
            //假设进行了相关处理，得到结果
            return 45;
        }

        //此处是正常的业务逻辑方法
        public static int getAgeByIdCardNo(string str)
        {
            return 50;
        }
    }
}
