using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using RulesEngine.Models;

namespace NewRuleEngine
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("微软 Rules Engine 最新 API 演示程序");

            // 1. 创建规则引擎实例
            var workflows = new List<Workflow>();

            // 2. 定义规则 (可以来自JSON文件或数据库)
            var rules = new List<Rule>
            {
                new Rule
                {
                    RuleName = "DiscountRule1",
                    SuccessEvent = "10", // 成功时返回的折扣值
                    ErrorMessage = "不满足折扣条件",
                    Expression = "input1.Age >= 18 && input1.Age <= 30 && input1.MembershipYears >= 3",
                    RuleExpressionType = RuleExpressionType.LambdaExpression
                },
                new Rule
                {
                    RuleName = "DiscountRule2",
                    SuccessEvent = "15",
                    ErrorMessage = "不满足折扣条件",
                    Expression = "input1.Age > 30 && input1.MembershipYears >= 5",
                    RuleExpressionType = RuleExpressionType.LambdaExpression
                },
                new Rule
                {
                    RuleName = "DefaultDiscount",
                    SuccessEvent = "5", // 默认折扣
                    ErrorMessage = "无折扣",
                    Expression = "true", // 总是执行
                    RuleExpressionType = RuleExpressionType.LambdaExpression
                }
            };

            workflows.Add(new Workflow
            {
                WorkflowName = "DiscountWorkflow",
                Rules = rules
            });

            var reSettings = new ReSettings
            {
                CustomTypes = new Type[] { typeof(HelperFunctions) } // 如果需要自定义函数
            };

            var rulesEngine = new RulesEngine.RulesEngine(workflows.ToArray(), reSettings);

            // 3. 准备输入数据
            var input1 = new
            {
                Age = 25,
                MembershipYears = 4,
                Country = "USA"
            };

            // 4. 执行规则
            var ruleParams = new RuleParameter[] {
                new RuleParameter("input1", input1)
            };

            // 执行所有规则
            var resultList = await rulesEngine.ExecuteAllRulesAsync("DiscountWorkflow", ruleParams);

            // 5. 处理结果
            Console.WriteLine($"年龄: {input1.Age}, 会员年限: {input1.MembershipYears}");

            foreach (var result in resultList)
            {
                Console.WriteLine($"规则: {result.Rule.RuleName}, 是否成功: {result.IsSuccess}");
                if (result.IsSuccess)
                {
                    Console.WriteLine($"适用折扣: {result.Rule.SuccessEvent}%");
                }
            }

            // 获取第一个成功的规则结果
            var discount = resultList.FirstOrDefault(r => r.IsSuccess)?.Rule.SuccessEvent ?? "0";
            Console.WriteLine($"最终适用折扣: {discount}%");

            // 6. 演示动态输入
            Console.WriteLine("\n动态输入演示:");
            dynamic dynamicInput = new ExpandoObject();
            dynamicInput.Age = 35;
            dynamicInput.MembershipYears = 6;
            dynamicInput.Country = "UK";

            var dynamicParams = new RuleParameter[] {
                new RuleParameter("input1", dynamicInput)
            };

            var dynamicResults = await rulesEngine.ExecuteAllRulesAsync("DiscountWorkflow", dynamicParams);

            foreach (var result in dynamicResults)
            {
                if (result.IsSuccess)
                {
                    Console.WriteLine($"动态输入 - 适用折扣: {result.Rule.SuccessEvent}%");
                    break;
                }
            }

            // 7. 演示带自定义函数的规则
            Console.WriteLine("\n自定义函数演示:");
            workflows.Add(new Workflow
            {
                WorkflowName = "CustomFunctionWorkflow",
                Rules = new List<Rule>
                {
                    new Rule
                    {
                        RuleName = "CheckPremiumMember",
                        SuccessEvent = "20",
                        ErrorMessage = "不是高级会员",
                        Expression = "HelperFunctions.IsPremiumMember(input1.MembershipYears)",
                        RuleExpressionType = RuleExpressionType.LambdaExpression
                    }
                }
            });

            rulesEngine = new RulesEngine.RulesEngine(workflows.ToArray(), reSettings);

            var customFuncResult = await rulesEngine.ExecuteAllRulesAsync("CustomFunctionWorkflow", ruleParams);

            foreach (var result in customFuncResult)
            {
                if (result.IsSuccess)
                {
                    Console.WriteLine($"自定义函数 - 适用折扣: {result.Rule.SuccessEvent}%");
                }
            }
        }
    }

    // 自定义函数类
    public static class HelperFunctions
    {
        public static bool IsPremiumMember(int membershipYears)
        {
            return membershipYears >= 5;
        }
    }
}