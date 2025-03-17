using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;

namespace IdentityServiceApi.Tests.Unit.Helpers
{
    /// <summary>
    ///     Custom implementation of IAsyncQueryProvider to enable async query execution in unit tests.
    /// </summary>
    internal class TestAsyncQueryProvider<TEntity> : IAsyncQueryProvider
    {
        private readonly IQueryProvider _inner;

        /// <summary>
        ///     Initializes a new instance of the <see cref="TestAsyncQueryProvider{TEntity}"/> class.
        /// </summary>
        /// <param name="inner">
        ///     The inner query provider.
        /// </param>
        internal TestAsyncQueryProvider(IQueryProvider inner)
        {
            _inner = inner;
        }

        /// <summary>
        ///     Creates a new query with the provided expression.
        /// </summary>
        /// <param name="expression">
        ///     The expression that represents the query.
        /// </param>
        /// <returns>
        ///     An <see cref="IQueryable{T}"/>.
        /// </returns>
        public IQueryable CreateQuery(Expression expression)
        {
            return new TestAsyncEnumerable<TEntity>(expression);
        }

        /// <summary>
        ///     Creates a new query with the provided expression.
        /// </summary>
        /// <typeparam name="TElement">
        ///     The element type of the query.
        /// </typeparam>
        /// <param name="expression">
        ///     The expression that represents the query.
        /// </param>
        /// <returns>
        ///     An <see cref="IQueryable{TElement}"/>.
        /// </returns>
        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            return new TestAsyncEnumerable<TElement>(expression);
        }

        /// <summary>
        ///     Executes the provided expression.
        /// </summary>
        /// <param name="expression">
        ///     The expression to execute.
        /// </param>
        /// <returns>
        ///     The result of the execution.
        /// </returns>
        public object Execute(Expression expression)
        {
            return _inner.Execute(expression);
        }

        /// <summary>
        ///     Executes the provided expression and returns the result of type TResult.
        /// </summary>
        /// <typeparam name="TResult">
        ///     The result type.
        /// </typeparam>
        /// <param name="expression">The expression to execute.
        /// </param>
        /// <returns>
        ///     The result of the execution.
        /// </returns>
        public TResult Execute<TResult>(Expression expression)
        {
            return _inner.Execute<TResult>(expression);
        }

        /// <summary>
        ///     Executes the provided expression asynchronously and returns the result of type TResult.
        /// </summary>
        /// <typeparam name="TResult">
        ///     The result type.
        /// </typeparam>
        /// <param name="expression">
        ///     The expression to execute.
        /// </param>
        /// <param name="cancellationToken">
        ///     A token to cancel the operation.
        /// </param>
        /// <returns>
        ///     The result of the execution.
        /// </returns>
        public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken)
        {
            return Task.FromResult(_inner.Execute<TResult>(expression)).Result;
        }

        /// <summary>
        ///     Executes the provided expression asynchronously and returns an <see cref="IAsyncEnumerable{TResult}"/>.
        /// </summary>
        /// <typeparam name="TResult">
        ///     The result type.
        /// </typeparam>
        /// <param name="expression">
        ///     The expression to execute.
        /// </param>
        /// <returns>
        ///     An <see cref="IAsyncEnumerable{TResult}"/> representing the asynchronous result.
        /// </returns>
        public static IAsyncEnumerable<TResult> ExecuteAsync<TResult>(Expression expression)
        {
            return new TestAsyncEnumerable<TResult>(expression);
        }
    }

    /// <summary>
    ///     Custom implementation of IQueryable and IAsyncEnumerable to enable async behavior in unit tests.
    /// </summary>
    /// <typeparam name="T">
    ///     The type of elements in the query.
    /// </typeparam>
    internal class TestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="TestAsyncEnumerable{T}"/> class.
        /// </summary>
        /// <param name="enumerable">
        ///     The enumerable collection to be queried.
        /// </param>
        public TestAsyncEnumerable(IEnumerable<T> enumerable)
            : base(enumerable)
        { }

        /// <summary>
        ///     Initializes a new instance of the <see cref="TestAsyncEnumerable{T}"/> class.
        /// </summary>
        /// <param name="expression">
        ///     The expression representing the query.
        /// </param>
        public TestAsyncEnumerable(Expression expression)
            : base(expression)
        { }

        /// <summary>
        ///     Returns an asynchronous enumerator to iterate through the collection.
        /// </summary>
        /// <param name="cancellationToken">
        ///     A token to cancel the operation.
        /// </param>
        /// <returns>
        ///     An <see cref="IAsyncEnumerator{T}"/> to iterate through the collection.
        /// </returns>
        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            return new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
        }

        /// <summary>
        ///     Gets the query provider for the queryable.
        /// </summary>
        IQueryProvider IQueryable.Provider
        {
            get { return new TestAsyncQueryProvider<T>(this); }
        }
    }

    /// <summary>
    ///     Custom implementation of IAsyncEnumerator to enable async iteration in unit tests.
    /// </summary>
    /// <typeparam name="T">
    ///     The type of elements being enumerated.
    /// </typeparam>
    internal class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
    {
        private readonly IEnumerator<T> _inner;

        /// <summary>
        ///     Initializes a new instance of the <see cref="TestAsyncEnumerator{T}"/> class.
        /// </summary>
        /// <param name="inner">
        ///     The inner enumerator to wrap.
        /// </param>
        public TestAsyncEnumerator(IEnumerator<T> inner)
        {
            _inner = inner;
        }

        /// <summary>
        ///     Gets the current element in the enumeration.
        /// </summary>
        public T Current => _inner.Current;

        /// <summary>
        ///     Disposes of the enumerator asynchronously.
        /// </summary>
        /// <returns>
        ///     A task that represents the completion of the dispose operation.
        /// </returns>
        public ValueTask DisposeAsync()
        {
            _inner.Dispose();
            return ValueTask.CompletedTask;
        }

        /// <summary>
        ///     Moves to the next element asynchronously.
        /// </summary>
        /// <returns>
        ///     A task that represents whether there are more elements.
        /// </returns>
        public ValueTask<bool> MoveNextAsync()
        {
            return new ValueTask<bool>(_inner.MoveNext());
        }
    }
}
