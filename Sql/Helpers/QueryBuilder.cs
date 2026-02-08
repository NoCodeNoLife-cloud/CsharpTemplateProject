using System.Text;

namespace Sql.Helpers;

/// <summary>
/// Query builder for constructing SQL queries
/// </summary>
public class QueryBuilder
{
    private readonly StringBuilder _selectClause = new();
    private readonly StringBuilder _fromClause = new();
    private readonly StringBuilder _whereClause = new();
    private readonly StringBuilder _orderByClause = new();
    private readonly StringBuilder _groupByClause = new();
    private readonly StringBuilder _havingClause = new();
    private readonly StringBuilder _joinClauses = new();
    private readonly List<string> _parameters = [];
    private int _limit = -1;
    private int _offset = -1;
    private bool _distinct;

    /// <summary>
    /// Adds SELECT clause
    /// </summary>
    /// <param name="columns">Columns to select</param>
    /// <returns>QueryBuilder instance</returns>
    public QueryBuilder Select(params string[] columns)
    {
        if (columns == null || columns.Length == 0)
            throw new ArgumentException("Columns cannot be null or empty", nameof(columns));

        var columnList = string.Join(", ", columns);
        _selectClause.Append(_distinct ? $"SELECT DISTINCT {columnList}" : $"SELECT {columnList}");
        return this;
    }

    /// <summary>
    /// Adds SELECT clause with table alias
    /// </summary>
    /// <param name="tableAlias">Table alias</param>
    /// <param name="columns">Columns to select</param>
    /// <returns>QueryBuilder instance</returns>
    public QueryBuilder Select(string tableAlias, params string[] columns)
    {
        if (string.IsNullOrWhiteSpace(tableAlias))
            throw new ArgumentException("Table alias cannot be null or empty", nameof(tableAlias));

        if (columns == null || columns.Length == 0)
            throw new ArgumentException("Columns cannot be null or empty", nameof(columns));

        var aliasedColumns = columns.Select(col => $"{tableAlias}.{col}").ToArray();
        return Select(aliasedColumns);
    }

    /// <summary>
    /// Sets DISTINCT flag
    /// </summary>
    /// <returns>QueryBuilder instance</returns>
    public QueryBuilder Distinct()
    {
        _distinct = true;
        return this;
    }

    /// <summary>
    /// Adds FROM clause
    /// </summary>
    /// <param name="table">Table name</param>
    /// <param name="alias">Table alias (optional)</param>
    /// <returns>QueryBuilder instance</returns>
    public QueryBuilder From(string table, string? alias = null)
    {
        if (string.IsNullOrWhiteSpace(table))
            throw new ArgumentException("Table name cannot be null or empty", nameof(table));

        _fromClause.Append(alias != null ? $" FROM {table} AS {alias}" : $" FROM {table}");
        return this;
    }

    /// <summary>
    /// Adds INNER JOIN clause
    /// </summary>
    /// <param name="table">Table to join</param>
    /// <param name="condition">Join condition</param>
    /// <param name="alias">Table alias (optional)</param>
    /// <returns>QueryBuilder instance</returns>
    public QueryBuilder InnerJoin(string table, string condition, string? alias = null)
    {
        return AddJoin("INNER JOIN", table, condition, alias);
    }

    /// <summary>
    /// Adds LEFT JOIN clause
    /// </summary>
    /// <param name="table">Table to join</param>
    /// <param name="condition">Join condition</param>
    /// <param name="alias">Table alias (optional)</param>
    /// <returns>QueryBuilder instance</returns>
    public QueryBuilder LeftJoin(string table, string condition, string? alias = null)
    {
        return AddJoin("LEFT JOIN", table, condition, alias);
    }

    /// <summary>
    /// Adds RIGHT JOIN clause
    /// </summary>
    /// <param name="table">Table to join</param>
    /// <param name="condition">Join condition</param>
    /// <param name="alias">Table alias (optional)</param>
    /// <returns>QueryBuilder instance</returns>
    public QueryBuilder RightJoin(string table, string condition, string? alias = null)
    {
        return AddJoin("RIGHT JOIN", table, condition, alias);
    }

    /// <summary>
    /// Adds WHERE clause
    /// </summary>
    /// <param name="condition">Condition expression</param>
    /// <param name="parameters">Parameter values</param>
    /// <returns>QueryBuilder instance</returns>
    public QueryBuilder Where(string condition, params object[] parameters)
    {
        if (string.IsNullOrWhiteSpace(condition))
            throw new ArgumentException("Condition cannot be null or empty", nameof(condition));
        ArgumentNullException.ThrowIfNull(parameters);

        AppendWhereClause(condition, parameters);
        return this;
    }

    /// <summary>
    /// Adds AND condition to WHERE clause
    /// </summary>
    /// <param name="condition">Condition expression</param>
    /// <param name="parameters">Parameter values</param>
    /// <returns>QueryBuilder instance</returns>
    public QueryBuilder AndWhere(string condition, params object[] parameters)
    {
        if (string.IsNullOrWhiteSpace(condition))
            throw new ArgumentException("Condition cannot be null or empty", nameof(condition));

        _whereClause.Append(_whereClause.Length > 0 ? " AND " : " WHERE ");

        _whereClause.Append(condition);
        AddParameters(parameters);
        return this;
    }

    /// <summary>
    /// Adds OR condition to WHERE clause
    /// </summary>
    /// <param name="condition">Condition expression</param>
    /// <param name="parameters">Parameter values</param>
    /// <returns>QueryBuilder instance</returns>
    public QueryBuilder OrWhere(string condition, params object[] parameters)
    {
        if (string.IsNullOrWhiteSpace(condition))
            throw new ArgumentException("Condition cannot be null or empty", nameof(condition));

        _whereClause.Append(_whereClause.Length > 0 ? " OR " : " WHERE ");

        _whereClause.Append(condition);
        AddParameters(parameters);
        return this;
    }

    /// <summary>
    /// Adds ORDER BY clause
    /// </summary>
    /// <param name="columns">Sort columns</param>
    /// <returns>QueryBuilder instance</returns>
    public QueryBuilder OrderBy(params string[] columns)
    {
        if (columns == null || columns.Length == 0)
            throw new ArgumentException("Columns cannot be null or empty", nameof(columns));

        _orderByClause.Append($" ORDER BY {string.Join(", ", columns)}");
        return this;
    }

    /// <summary>
    /// Adds ORDER BY DESC clause
    /// </summary>
    /// <param name="columns">Sort columns</param>
    /// <returns>QueryBuilder instance</returns>
    public QueryBuilder OrderByDesc(params string[] columns)
    {
        if (columns == null || columns.Length == 0)
            throw new ArgumentException("Columns cannot be null or empty", nameof(columns));

        var descColumns = columns.Select(col => $"{col} DESC").ToArray();
        _orderByClause.Append($" ORDER BY {string.Join(", ", descColumns)}");
        return this;
    }

    /// <summary>
    /// Adds GROUP BY clause
    /// </summary>
    /// <param name="columns">Group columns</param>
    /// <returns>QueryBuilder instance</returns>
    public QueryBuilder GroupBy(params string[] columns)
    {
        if (columns == null || columns.Length == 0)
            throw new ArgumentException("Columns cannot be null or empty", nameof(columns));

        _groupByClause.Append($" GROUP BY {string.Join(", ", columns)}");
        return this;
    }

    /// <summary>
    /// Adds HAVING clause
    /// </summary>
    /// <param name="condition">Condition expression</param>
    /// <param name="parameters">Parameter values</param>
    /// <returns>QueryBuilder instance</returns>
    public QueryBuilder Having(string condition, params object[] parameters)
    {
        if (string.IsNullOrWhiteSpace(condition))
            throw new ArgumentException("Condition cannot be null or empty", nameof(condition));

        _havingClause.Append($" HAVING {condition}");
        AddParameters(parameters);
        return this;
    }

    /// <summary>
    /// Sets LIMIT clause
    /// </summary>
    /// <param name="limit">Limit rows</param>
    /// <returns>QueryBuilder instance</returns>
    public QueryBuilder Limit(int limit)
    {
        if (limit < 0)
            throw new ArgumentException("Limit must be non-negative", nameof(limit));

        _limit = limit;
        return this;
    }

    /// <summary>
    /// Sets OFFSET clause
    /// </summary>
    /// <param name="offset">Offset value</param>
    /// <returns>QueryBuilder instance</returns>
    public QueryBuilder Offset(int offset)
    {
        if (offset < 0)
            throw new ArgumentException("Offset must be non-negative", nameof(offset));

        _offset = offset;
        return this;
    }

    /// <summary>
    /// Sets LIMIT and OFFSET clauses
    /// </summary>
    /// <param name="limit">Limit rows</param>
    /// <param name="offset">Offset value</param>
    /// <returns>QueryBuilder instance</returns>
    public QueryBuilder Limit(int limit, int offset)
    {
        return Limit(limit).Offset(offset);
    }

    /// <summary>
    /// Builds the complete SQL query
    /// </summary>
    /// <returns>Built SQL query string</returns>
    public string Build()
    {
        var query = new StringBuilder();

        // Build basic query parts
        if (_selectClause.Length > 0)
            query.Append(_selectClause);
        else
            query.Append("SELECT *");

        if (_fromClause.Length > 0)
            query.Append(_fromClause);

        if (_joinClauses.Length > 0)
            query.Append(_joinClauses);

        if (_whereClause.Length > 0)
            query.Append(_whereClause);

        if (_groupByClause.Length > 0)
            query.Append(_groupByClause);

        if (_havingClause.Length > 0)
            query.Append(_havingClause);

        if (_orderByClause.Length > 0)
            query.Append(_orderByClause);

        // Add LIMIT and OFFSET
        if (_limit < 0) return query.ToString().Trim();
        query.Append($" LIMIT {_limit}");
        if (_offset >= 0)
        {
            query.Append($" OFFSET {_offset}");
        }

        return query.ToString().Trim();
    }

    /// <summary>
    /// Gets the parameter list
    /// </summary>
    /// <returns>Parameter list</returns>
    public IReadOnlyList<string> GetParameters()
    {
        return _parameters.AsReadOnly();
    }

    /// <summary>
    /// Clears all query conditions
    /// </summary>
    /// <returns>QueryBuilder instance</returns>
    public QueryBuilder Clear()
    {
        _selectClause.Clear();
        _fromClause.Clear();
        _whereClause.Clear();
        _orderByClause.Clear();
        _groupByClause.Clear();
        _havingClause.Clear();
        _joinClauses.Clear();
        _parameters.Clear();
        _limit = -1;
        _offset = -1;
        _distinct = false;
        return this;
    }

    /// <summary>
    /// Clones the current query builder
    /// </summary>
    /// <returns>New QueryBuilder instance</returns>
    public QueryBuilder Clone()
    {
        var clone = new QueryBuilder();

        clone._selectClause.Append(_selectClause);
        clone._fromClause.Append(_fromClause);
        clone._whereClause.Append(_whereClause);
        clone._orderByClause.Append(_orderByClause);
        clone._groupByClause.Append(_groupByClause);
        clone._havingClause.Append(_havingClause);
        clone._joinClauses.Append(_joinClauses);
        clone._parameters.AddRange(_parameters);
        clone._limit = _limit;
        clone._offset = _offset;
        clone._distinct = _distinct;

        return clone;
    }

    #region Private Helper Methods

    /// <summary>
    /// Adds JOIN clause
    /// </summary>
    /// <param name="joinType">Join type</param>
    /// <param name="table">Table name</param>
    /// <param name="condition">Join condition</param>
    /// <param name="alias">Table alias</param>
    /// <returns>QueryBuilder instance</returns>
    private QueryBuilder AddJoin(string joinType, string table, string condition, string? alias)
    {
        if (string.IsNullOrWhiteSpace(table))
            throw new ArgumentException("Table name cannot be null or empty", nameof(table));

        if (string.IsNullOrWhiteSpace(condition))
            throw new ArgumentException("Join condition cannot be null or empty", nameof(condition));

        var tableSpec = alias != null ? $"{table} AS {alias}" : table;
        _joinClauses.Append($" {joinType} {tableSpec} ON {condition}");

        return this;
    }

    /// <summary>
    /// Appends WHERE clause
    /// </summary>
    /// <param name="condition">Condition expression</param>
    /// <param name="parameters">Parameter values</param>
    private void AppendWhereClause(string condition, object[] parameters)
    {
        _whereClause.Append(_whereClause.Length > 0 ? " AND " : " WHERE ");

        _whereClause.Append(condition);
        AddParameters(parameters);
    }

    /// <summary>
    /// Adds parameters
    /// </summary>
    /// <param name="parameters">Parameter values array</param>
    private void AddParameters(object[] parameters)
    {
        foreach (var param in parameters)
        {
            _parameters.Add(param.ToString() ?? "NULL");
        }
    }

    #endregion
}