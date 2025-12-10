using System.Text;
using System.Text.RegularExpressions;

namespace SqlFormatter.Services;

public class TSqlFormatterService
{
    private readonly string[] _majorKeywords = new[]
    {
        "SELECT", "FROM", "WHERE", "GROUP BY", "HAVING", "ORDER BY",
        "INNER JOIN", "LEFT JOIN", "RIGHT JOIN", "FULL JOIN", "CROSS JOIN",
        "UNION", "UNION ALL", "EXCEPT", "INTERSECT"
    };

    private readonly string[] _minorKeywords = new[]
    {
        "AND", "OR", "ON", "AS", "IN", "NOT IN", "EXISTS", "NOT EXISTS",
        "BETWEEN", "LIKE", "IS NULL", "IS NOT NULL", "DISTINCT", "TOP"
    };

    public string Format(string sql)
    {
        if (string.IsNullOrWhiteSpace(sql))
            return string.Empty;

        // Normalize whitespace
        sql = Regex.Replace(sql, @"\s+", " ").Trim();

        var result = new StringBuilder();
        var tokens = TokenizeSql(sql);
        var isInSelectClause = false;
        var isInInsertColumns = false;
        var isInValues = false;
        var isInSetClause = false;
        var previousToken = "";
        var parenDepth = 0;

        for (int i = 0; i < tokens.Count; i++)
        {
            var token = tokens[i];
            var upperToken = token.ToUpperInvariant();
            var nextToken = i < tokens.Count - 1 ? tokens[i + 1] : "";
            var nextUpperToken = nextToken.ToUpperInvariant();

            // Check if this is a two-word keyword
            var twoWordKeyword = GetTwoWordKeyword(upperToken, nextUpperToken);

            if (twoWordKeyword != null)
            {
                // Handle two-word major keyword
                if (result.Length > 0)
                {
                    result.AppendLine();
                }

                result.Append(twoWordKeyword);

                if (twoWordKeyword == "INSERT INTO")
                {
                    result.Append(" ");
                    isInSelectClause = false;
                    isInInsertColumns = false;
                    isInValues = false;
                    isInSetClause = false;
                }
                else if (twoWordKeyword == "DELETE FROM")
                {
                    result.AppendLine();
                    result.Append("    ");
                    isInSelectClause = false;
                    isInInsertColumns = false;
                    isInValues = false;
                    isInSetClause = false;
                }
                else
                {
                    result.AppendLine();
                    result.Append("    "); // 4 spaces indent

                    if (twoWordKeyword == "GROUP BY" || twoWordKeyword == "ORDER BY")
                    {
                        isInSelectClause = false;
                        isInSetClause = false;
                    }
                }

                i++; // Skip the next token as it's part of this keyword
            }
            else if (IsSingleWordMajorKeyword(upperToken))
            {
                // Handle single-word major keyword
                if (result.Length > 0 && upperToken != "VALUES" && upperToken != "SET")
                {
                    result.AppendLine();
                }

                result.Append(upperToken);

                if (upperToken == "SELECT")
                {
                    result.AppendLine();
                    result.Append("    "); // 4 spaces indent
                    isInSelectClause = true;
                    isInInsertColumns = false;
                    isInValues = false;
                    isInSetClause = false;
                }
                else if (upperToken == "FROM" || upperToken == "WHERE" || upperToken == "HAVING")
                {
                    result.AppendLine();
                    result.Append("    "); // 4 spaces indent
                    isInSelectClause = false;
                    isInInsertColumns = false;
                    isInValues = false;
                    isInSetClause = false;
                }
                else if (upperToken == "UPDATE")
                {
                    result.Append(" ");
                    isInSelectClause = false;
                    isInInsertColumns = false;
                    isInValues = false;
                    isInSetClause = false;
                }
                else if (upperToken == "SET")
                {
                    result.AppendLine();
                    result.Append("    ");
                    isInSelectClause = false;
                    isInInsertColumns = false;
                    isInValues = false;
                    isInSetClause = true;
                }
                else if (upperToken == "VALUES")
                {
                    result.AppendLine();
                    isInSelectClause = false;
                    isInInsertColumns = false;
                    isInValues = true;
                    isInSetClause = false;
                }
            }
            else if (token == ",")
            {
                result.Append(token);

                // After comma in SELECT clause, SET clause, or INSERT columns (not in VALUES), add newline and indent
                if ((isInSelectClause || isInSetClause || (isInInsertColumns && parenDepth > 0)) && !isInValues)
                {
                    result.AppendLine();
                    result.Append("    "); // 4 spaces indent
                }
                else
                {
                    result.Append(" ");
                }
            }
            else if (token == "(")
            {
                parenDepth++;

                // Check if we're in INSERT INTO statement and this is the column list
                if (!isInSelectClause && !isInSetClause && i > 0)
                {
                    var prevIdx = i - 1;
                    while (prevIdx >= 0 && (tokens[prevIdx] == " " || string.IsNullOrWhiteSpace(tokens[prevIdx])))
                        prevIdx--;

                    if (prevIdx >= 0)
                    {
                        // Look back for INSERT keyword
                        for (int j = Math.Max(0, i - 10); j < i; j++)
                        {
                            if (tokens[j].ToUpperInvariant() == "INSERT")
                            {
                                isInInsertColumns = true;
                                result.AppendLine();
                                result.Append(token);
                                result.AppendLine();
                                result.Append("    ");
                                continue;
                            }
                        }
                    }
                }

                if (!isInInsertColumns || parenDepth > 1)
                {
                    result.Append(token);
                }
            }
            else if (token == ")")
            {
                parenDepth--;

                if (isInInsertColumns && parenDepth == 0)
                {
                    result.AppendLine();
                    result.Append(token);
                    isInInsertColumns = false;
                }
                else
                {
                    result.Append(token);
                }

                if (parenDepth == 0 && isInValues)
                {
                    isInValues = false;
                }
            }
            else
            {
                // Regular token (column name, table name, value, etc.)
                if (result.Length > 0 && !result.ToString().EndsWith(" ") &&
                    !result.ToString().EndsWith("(") && previousToken != ",")
                {
                    result.Append(" ");
                }
                result.Append(token);
            }

            previousToken = token;
        }

        return result.ToString().Trim();
    }

    private List<string> TokenizeSql(string sql)
    {
        var tokens = new List<string>();
        var currentToken = new StringBuilder();
        var inString = false;
        var stringChar = '\0';

        for (int i = 0; i < sql.Length; i++)
        {
            var ch = sql[i];

            if (inString)
            {
                currentToken.Append(ch);
                if (ch == stringChar)
                {
                    inString = false;
                }
            }
            else if (ch == '\'' || ch == '"')
            {
                if (currentToken.Length > 0)
                {
                    tokens.Add(currentToken.ToString());
                    currentToken.Clear();
                }
                inString = true;
                stringChar = ch;
                currentToken.Append(ch);
            }
            else if (ch == ',' || ch == '(' || ch == ')')
            {
                if (currentToken.Length > 0)
                {
                    tokens.Add(currentToken.ToString());
                    currentToken.Clear();
                }
                tokens.Add(ch.ToString());
            }
            else if (char.IsWhiteSpace(ch))
            {
                if (currentToken.Length > 0)
                {
                    tokens.Add(currentToken.ToString());
                    currentToken.Clear();
                }
            }
            else
            {
                currentToken.Append(ch);
            }
        }

        if (currentToken.Length > 0)
        {
            tokens.Add(currentToken.ToString());
        }

        return tokens;
    }

    private string? GetTwoWordKeyword(string token, string nextToken)
    {
        var combined = token + " " + nextToken;

        var twoWordKeywords = new[]
        {
            "GROUP BY", "ORDER BY", "INNER JOIN", "LEFT JOIN",
            "RIGHT JOIN", "FULL JOIN", "CROSS JOIN", "UNION ALL",
            "INSERT INTO", "DELETE FROM"
        };

        if (twoWordKeywords.Contains(combined))
            return combined;

        return null;
    }

    private bool IsSingleWordMajorKeyword(string token)
    {
        var singleWordKeywords = new[]
        {
            "SELECT", "FROM", "WHERE", "HAVING",
            "UNION", "EXCEPT", "INTERSECT",
            "UPDATE", "SET", "VALUES"
        };

        return singleWordKeywords.Contains(token);
    }
}
