using System.Linq.Expressions;

namespace DonationHub.Application.Common.Specifications;

public abstract class Specification<T>
{
    public abstract Expression<Func<T, bool>> ToExpression();

    public bool IsSatisfiedBy(T entity) => ToExpression().Compile()(entity);

    public Specification<T> And(Specification<T> other) => new CombinedSpecification<T>(this, other, true);
    public Specification<T> Or(Specification<T> other) => new CombinedSpecification<T>(this, other, false);
}

public sealed class ExpressionSpecification<T>(Expression<Func<T, bool>> expression) : Specification<T>
{
    public override Expression<Func<T, bool>> ToExpression() => expression;
}

internal sealed class CombinedSpecification<T>(Specification<T> left, Specification<T> right, bool isAnd)
    : Specification<T>
{
    public override Expression<Func<T, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var parameter = Expression.Parameter(typeof(T), "x");

        var lb = new ParameterReplacer(l.Parameters[0], parameter).Visit(l.Body);
        var rb = new ParameterReplacer(r.Parameters[0], parameter).Visit(r.Body);

        Expression body = isAnd ? Expression.AndAlso(lb, rb) : Expression.OrElse(lb, rb);
        return Expression.Lambda<Func<T, bool>>(body, parameter);
    }

    private sealed class ParameterReplacer(ParameterExpression from, ParameterExpression to) : ExpressionVisitor
    {
        protected override Expression VisitParameter(ParameterExpression node) => node == from ? to : base.VisitParameter(node);
    }
}

public static class SpecificationQueryableExtensions
{
    public static IQueryable<T> Satisfying<T>(this IQueryable<T> query, Specification<T> specification) =>
        query.Where(specification.ToExpression());
}