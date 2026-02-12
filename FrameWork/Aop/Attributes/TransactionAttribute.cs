using System.Transactions;
using ArxOne.MrAdvice.Advice;

namespace CommonFramework.Aop.Attributes;

/// <summary>
/// Transaction advice that wraps method execution in a transaction scope
/// Applied at compile-time for automatic transaction management
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public class TransactionAttribute : Attribute, IMethodAdvice
{
    public IsolationLevel IsolationLevel { get; set; } = IsolationLevel.ReadCommitted;

    public void Advise(MethodAdviceContext context)
    {
        using var scope = new TransactionScope(
            TransactionScopeOption.Required,
            new TransactionOptions { IsolationLevel = IsolationLevel },
            TransactionScopeAsyncFlowOption.Enabled);

        context.Proceed(); // Execute the original method
        scope.Complete();
    }
}