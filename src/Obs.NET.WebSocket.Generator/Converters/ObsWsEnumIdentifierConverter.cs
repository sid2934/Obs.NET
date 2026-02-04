#region

using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using Obs.Generator.Models;

#endregion

namespace Obs.Generator.Converters;

/// <summary>
///     JsonConverter that resolves an enum identifier object whose enumValue can be:
///     <list type="bullet">
///         <item>
///             <description>Integer literal (number token or numeric string, supports unary minus e.g. <c>-1</c>)</description>
///         </item>
///         <item>
///             <description>Shift expression: <c>1 &lt;&lt; 4</c> (optionally parenthesized, e.g. <c>(1 &lt;&lt; 4)</c>)</description>
///         </item>
///         <item>
///             <description>Bitwise OR: <c>FlagA | FlagB</c></description>
///         </item>
///         <item>
///             <description>Combination: <c>(1 &lt;&lt; 3) | FlagA | -1</c> (parentheses allowed around sub-expressions)</description>
///         </item>
///         <item>
///             <description>Identifier references (must already have appeared earlier in the same enum's JSON array)</description>
///         </item>
///     </list>
///     Notes:
///     <list type="bullet">
///         <item>
///             <description>Unary minus has highest precedence (applies to the following factor).</description>
///         </item>
///         <item>
///             <description>Shift amount must currently be a non-negative integer literal.</description>
///         </item>
///     </list>
///     Forward references are not supported and will throw.
/// </summary>
internal sealed class ObsWsEnumIdentifierConverter : JsonConverter<ObsWsEnumIdentifier>
{
    // Registries are scoped by enumType. Use ThreadStatic to avoid cross-thread leakage.
    [ThreadStatic] private static Dictionary<string, Dictionary<string, int>>? _registriesByEnumType;

    [ThreadStatic] private static string? _currentEnumType;

    // Called by the ObsWsEnum converter to begin a new enum scope
    public static void BeginEnumScope(string enumType)
    {
        _registriesByEnumType ??= new Dictionary<string, Dictionary<string, int>>(StringComparer.Ordinal);
        _currentEnumType = enumType;
        // Start a fresh registry for this enum type (clears any previous data for same enumType during a fresh parse)
        _registriesByEnumType[enumType] = new Dictionary<string, int>(StringComparer.Ordinal);
    }

    // Called by the ObsWsEnum converter when leaving the enum scope
    public static void EndEnumScope()
    {
        _currentEnumType = null;
    }

    private static Dictionary<string, int> GetCurrentRegistry(bool createIfMissing = false)
    {
        if (_currentEnumType == null)
            throw new JsonException(
                "ObsWsEnumIdentifier encountered outside of an enum scope (missing enumType context)");

        _registriesByEnumType ??= new Dictionary<string, Dictionary<string, int>>(StringComparer.Ordinal);
        if (!_registriesByEnumType.TryGetValue(_currentEnumType, out var reg) && createIfMissing)
        {
            reg = new Dictionary<string, int>(StringComparer.Ordinal);
            _registriesByEnumType[_currentEnumType] = reg;
        }

        return reg ?? throw new JsonException($"No registry for enumType '{_currentEnumType}'");
    }

    public override ObsWsEnumIdentifier Read(ref Utf8JsonReader reader, Type typeToConvert,
        JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
            throw new JsonException("Expected start of object for ObsWsEnumIdentifier");

        var registry = GetCurrentRegistry(true);

        var description = string.Empty;
        var enumIdentifier = string.Empty;
        var rpcVersion = string.Empty;
        var deprecated = false;
        var initialVersion = string.Empty;
        int enumValue;
        var hasEnumValue = false;
        string? rawEnumValueString = null;
        int? rawEnumValueNumber = null;

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject) break;

            if (reader.TokenType != JsonTokenType.PropertyName) throw new JsonException("Expected property name");

            var propName = reader.GetString()!;
            reader.Read();
            switch (propName)
            {
                case "description":
                    description = reader.TokenType == JsonTokenType.String
                        ? reader.GetString()!
                        : throw new JsonException();
                    break;
                case "enumIdentifier":
                    enumIdentifier = reader.TokenType == JsonTokenType.String
                        ? reader.GetString()!
                        : throw new JsonException();
                    break;
                case "rpcVersion":
                    rpcVersion = reader.TokenType == JsonTokenType.String
                        ? reader.GetString()!
                        : throw new JsonException();
                    break;
                case "deprecated":
                    deprecated = reader.TokenType switch
                    {
                        JsonTokenType.True => true, JsonTokenType.False => false, _ => throw new JsonException()
                    };
                    break;
                case "initialVersion":
                    initialVersion = reader.TokenType == JsonTokenType.String
                        ? reader.GetString()!
                        : throw new JsonException();
                    break;
                case "enumValue":
                    hasEnumValue = true;
                    if (reader.TokenType == JsonTokenType.Number)
                    {
                        if (!reader.TryGetInt32(out var num))
                            throw new JsonException("enumValue numeric literal not an int32");

                        rawEnumValueNumber = num;
                    }
                    else if (reader.TokenType == JsonTokenType.String)
                    {
                        rawEnumValueString = reader.GetString();
                    }
                    else
                    {
                        throw new JsonException("enumValue must be number or string");
                    }

                    break;
                default:
                    reader.Skip();
                    break;
            }
        }

        if (!hasEnumValue) throw new JsonException("enumValue required");

        // If enumValue was provided as a string that is exactly the same as the identifier name
        // (e.g. "enumIdentifier": "FOO", "enumValue": "FOO"), treat this as an implicit
        // auto-assigned integer value. Assign sequential ints per-enum starting at 0.
        if (rawEnumValueString != null)
        {
            var trimmed = rawEnumValueString.Trim();
            if (IsSimpleIdentifier(trimmed) && string.Equals(trimmed, enumIdentifier, StringComparison.Ordinal))
            {
                // next value is max(existing)+1 or 0 if none
                var next = registry.Count == 0 ? 0 : registry.Values.Max() + 1;
                enumValue = next;
                registry[enumIdentifier] = enumValue;

                return new ObsWsEnumIdentifier
                {
                    Description = description,
                    EnumIdentifier = enumIdentifier,
                    RpcVersion = rpcVersion,
                    Deprecated = deprecated,
                    InitialVersion = initialVersion,
                    EnumValue = enumValue
                };
            }
        }

        enumValue = rawEnumValueNumber.HasValue
            ? rawEnumValueNumber.Value
            : EvaluateExpression(rawEnumValueString!, registry);

        if (registry.ContainsKey(enumIdentifier))
            throw new JsonException(
                $"Duplicate enumIdentifier '{enumIdentifier}' detected in enum '{_currentEnumType}'");

        registry[enumIdentifier] = enumValue;

        return new ObsWsEnumIdentifier
        {
            Description = description,
            EnumIdentifier = enumIdentifier,
            RpcVersion = rpcVersion,
            Deprecated = deprecated,
            InitialVersion = initialVersion,
            EnumValue = enumValue
        };
    }

    private static bool IsSimpleIdentifier(string s)
    {
        if (string.IsNullOrEmpty(s)) return false;

        if (!(char.IsLetter(s[0]) || s[0] == '_')) return false;

        for (var i = 1; i < s.Length; i++)
            if (!(char.IsLetterOrDigit(s[i]) || s[i] == '_'))
                return false;

        return true;
    }

    public override void Write(Utf8JsonWriter writer, ObsWsEnumIdentifier value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteString("description", value.Description);
        writer.WriteString("enumIdentifier", value.EnumIdentifier);
        writer.WriteString("rpcVersion", value.RpcVersion);
        writer.WriteBoolean("deprecated", value.Deprecated);
        writer.WriteString("initialVersion", value.InitialVersion);
        writer.WriteNumber("enumValue", value.EnumValue);
        writer.WriteEndObject();
    }

    private static int EvaluateExpression(string expr, Dictionary<string, int> registry)
    {
        if (string.IsNullOrWhiteSpace(expr)) throw new JsonException("Empty enumValue expression");

        Tokenizer tokenizer = new(expr);
        var value = ParseExpr(tokenizer, registry);
        if (tokenizer.Current.Kind != TokenKind.End)
            throw new JsonException($"Unexpected token '{tokenizer.Current.Text}' at end of expression '{expr}'");

        return value;
    }

    private static int ParseExpr(Tokenizer tz, Dictionary<string, int> registry)
    {
        var v = ParseShift(tz, registry);
        while (tz.Current.Kind == TokenKind.Pipe)
        {
            tz.Next();
            var rhs = ParseShift(tz, registry);
            v |= rhs;
        }

        return v;
    }

    private static int ParseShift(Tokenizer tz, Dictionary<string, int> registry)
    {
        var v = ParseUnary(tz, registry);
        while (tz.Current.Kind == TokenKind.ShiftLeft)
        {
            tz.Next();
            if (tz.Current.Kind != TokenKind.Number) throw new JsonException("Expected shift amount integer");

            var shift = tz.Current.Value;
            if (shift < 0 || shift > 30) throw new JsonException($"Shift amount {shift} out of range (0-30)");

            tz.Next();
            v = v << shift;
        }

        return v;
    }

    private static int ParseUnary(Tokenizer tz, Dictionary<string, int> registry)
    {
        if (tz.Current.Kind == TokenKind.Minus)
        {
            tz.Next();
            var inner = ParseUnary(tz, registry); // right-associative for chains of '-'
            return -inner;
        }

        return ParseFactor(tz, registry);
    }

    private static int ParseFactor(Tokenizer tz, Dictionary<string, int> registry)
    {
        return tz.Current.Kind switch
        {
            TokenKind.Number => ConsumeNumber(tz),
            TokenKind.Identifier => ResolveIdentifier(tz, registry),
            TokenKind.LeftParen => ParseParenthesized(tz, registry),
            _ => throw new JsonException($"Unexpected token '{tz.Current.Text}'")
        };
    }

    private static int ParseParenthesized(Tokenizer tz, Dictionary<string, int> registry)
    {
        tz.Next(); // '('
        var inner = ParseExpr(tz, registry);
        if (tz.Current.Kind != TokenKind.RightParen) throw new JsonException("Expected ')' to close '(' in expression");

        tz.Next();
        return inner;
    }

    private static int ConsumeNumber(Tokenizer tz)
    {
        var v = tz.Current.Value;
        tz.Next();
        return v;
    }

    private static int ResolveIdentifier(Tokenizer tz, Dictionary<string, int> registry)
    {
        var name = tz.Current.Text;
        if (!registry.TryGetValue(name, out var v))
        {
            var known = registry.Count == 0 ? "(none)" : string.Join(", ", registry.Keys);
            throw new JsonException(
                $"Unknown enum identifier reference '{name}' in enum '{_currentEnumType}' (forward reference not supported). Known identifiers: {known}");
        }

        tz.Next();
        return v;
    }

#if DEBUG
    internal static void __SelfTest()
    {
        Dictionary<string, int> reg = new(StringComparer.Ordinal) { { "FlagA", 1 }, { "FlagB", 2 }, { "FlagC", 4 } };
        string[] tests =
        {
            "1 << 4", "(1 << 4)", "1<<1 | 1<<2", "(1<<1) | (1<<2) | (1<<3)", "-1", "(-1)", "(1<<3) | -1",
            "FlagA | FlagB | (1<<5)"
        };
        foreach (var t in tests)
            try
            {
                var v = EvaluateExpression(t, reg);
                Console.WriteLine($"[ExprTest] {t} = {v}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ExprTest] {t} threw {ex.GetType().Name}: {ex.Message}");
            }
    }
#endif

    private enum TokenKind
    {
        End,
        Number,
        Identifier,
        Pipe,
        ShiftLeft,
        LeftParen,
        RightParen,
        Minus
    }

    private sealed class Tokenizer
    {
        private readonly string _s;
        private int _pos;

        public Tokenizer(string s)
        {
            _s = s;
            _pos = 0;
            Next();
        }

        public Token Current { get; private set; }

        public void Next()
        {
            SkipWs();
            if (_pos >= _s.Length)
            {
                Current = new Token(TokenKind.End, string.Empty, 0);
                return;
            }

            var c = _s[_pos];

            if (c == '-')
            {
                _pos++;
                Current = new Token(TokenKind.Minus, "-", 0);
                return;
            }

            if (char.IsDigit(c))
            {
                var start = _pos;
                while (_pos < _s.Length && char.IsDigit(_s[_pos])) _pos++;

                var text = _s.Substring(start, _pos - start);
                if (!int.TryParse(text, NumberStyles.None, CultureInfo.InvariantCulture, out var val))
                    throw new JsonException($"Invalid integer literal '{text}'");

                Current = new Token(TokenKind.Number, text, val);
                return;
            }

            if (char.IsLetter(c) || c == '_')
            {
                var start = _pos;
                while (_pos < _s.Length && (char.IsLetterOrDigit(_s[_pos]) || _s[_pos] == '_')) _pos++;

                var text = _s.Substring(start, _pos - start);
                Current = new Token(TokenKind.Identifier, text, 0);
                return;
            }

            if (c == '|')
            {
                _pos++;
                Current = new Token(TokenKind.Pipe, "|", 0);
                return;
            }

            if (c == '<' && _pos + 1 < _s.Length && _s[_pos + 1] == '<')
            {
                _pos += 2;
                Current = new Token(TokenKind.ShiftLeft, "<<", 0);
                return;
            }

            if (c == '(')
            {
                _pos++;
                Current = new Token(TokenKind.LeftParen, "(", 0);
                return;
            }

            if (c == ')')
            {
                _pos++;
                Current = new Token(TokenKind.RightParen, ")", 0);
                return;
            }

            throw new JsonException($"Unexpected character '{c}' in expression '{_s}' at position {_pos}");
        }

        private void SkipWs()
        {
            while (_pos < _s.Length && char.IsWhiteSpace(_s[_pos])) _pos++;
        }
    }

    private readonly record struct Token(TokenKind Kind, string Text, int Value);
}