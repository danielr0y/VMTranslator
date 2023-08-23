namespace VMTranslator.Translator;

public class Parser : Nand2TetrisParser
{
    private StreamReader? _source;
    private CommandType _commandType;
    private string? _arg1;
    private short _arg2;

    public void Init(StreamReader source)
    {
        _source = source;
    }

    public bool HasMoreLines() => ( _source?.Peek() ?? -1 ) > 0;

    public void Advance()
    {
        do
        {
            ParseCommand(_source?.ReadLine() ?? "// bad line or no source");
        } while (CommandType() is VMTranslator.Translator.CommandType.SKIPPABLE);
    }

    private void ParseCommand(string line)
    {
        var commandAndComment = line.Split("//", 2);
        var command = commandAndComment[0];

        var commandParsed = command.Split(' ');

        _commandType = DetermineCommandType(commandParsed);

        switch (_commandType)
        {
            case VMTranslator.Translator.CommandType.SKIPPABLE:
            {
                break;
            }
            case VMTranslator.Translator.CommandType.C_ARITHMETIC:
            {
                _arg1 = commandParsed[0];
                break;
            }
            case VMTranslator.Translator.CommandType.C_PUSH:
            case VMTranslator.Translator.CommandType.C_POP:
            default:
            {
                _arg1 = commandParsed[1];
                _arg2 = Int16.Parse(commandParsed[2]);
                break;
            }
        }
    }

    private CommandType DetermineCommandType(string[] commandParsed)
    {
        var commandTypes = new Dictionary<string, CommandType>()
        {
            { "push", VMTranslator.Translator.CommandType.C_PUSH },
            { "pop", VMTranslator.Translator.CommandType.C_POP },
            { "add", VMTranslator.Translator.CommandType.C_ARITHMETIC },
            { "sub", VMTranslator.Translator.CommandType.C_ARITHMETIC },
            { "neg", VMTranslator.Translator.CommandType.C_ARITHMETIC },
            { "eq", VMTranslator.Translator.CommandType.C_ARITHMETIC },
            { "gt", VMTranslator.Translator.CommandType.C_ARITHMETIC },
            { "lt", VMTranslator.Translator.CommandType.C_ARITHMETIC },
            { "and", VMTranslator.Translator.CommandType.C_ARITHMETIC },
            { "or", VMTranslator.Translator.CommandType.C_ARITHMETIC },
            { "not", VMTranslator.Translator.CommandType.C_ARITHMETIC },
            { "skippable", VMTranslator.Translator.CommandType.SKIPPABLE },
        };

        var operation = commandParsed[0];

        if (String.IsNullOrWhiteSpace(operation) || !commandTypes.ContainsKey(operation))
        {
            operation = "skippable";
        }

        return commandTypes[operation];
    }

    public CommandType CommandType()
    {
        return _commandType;
    }

    public string Arg1()
    {
        return _arg1 ?? "";
    }

    public short Arg2()
    {
        return _arg2;
    }
}

public interface Nand2TetrisParser
{
    bool HasMoreLines();
    void Advance();
    CommandType CommandType();
    string Arg1();
    short Arg2();
}

public enum CommandType
{
    C_ARITHMETIC,
    C_PUSH,
    C_POP,
    C_LABEL,
    C_GOTO,
    C_IF,
    C_FUNCTION,
    C_RETURN,
    C_CALL,
    SKIPPABLE
}