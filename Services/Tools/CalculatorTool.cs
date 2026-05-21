using System.ComponentModel;
using System.Data;

namespace ChatApp.Services.Tools;

public sealed class CalculatorTool
{
    [Description("执行数学计算")]
    public static string Calculate([Description("数学表达式，如 2+3*4")] string expression)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(expression);

        try
        {
            var result = Convert.ToDecimal(new DataTable().Compute(expression, null));
            return $"计算结果: {expression} = {result}";
        }
        catch (DivideByZeroException)
        {
            return "错误: 除数不能为零";
        }
        catch (EvaluateException)
        {
            return $"错误: 无法解析表达式 '{expression}'";
        }
    }
}
