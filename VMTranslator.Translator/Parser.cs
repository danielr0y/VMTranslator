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
        } while (_commandType is CommandType.SKIPPABLE );
    }

    private void ParseCommand(string line)
    {
        var commandAndComment = line.Split("//", 2);
        var command = commandAndComment[0];

        var commandParsed = command.Split(' ');

        _commandType = DetermineCommandType(commandParsed);

        (_arg1, _arg2) = SetupArgs(_commandType, commandParsed);
    }

    private CommandType DetermineCommandType(string[] commandParsed)
    {
        var commandTypes = new Dictionary<string, CommandType>()
        {
            { "push", CommandType.C_PUSH },
            { "pop", CommandType.C_POP },
            { "add", CommandType.C_ARITHMETIC },
            { "sub", CommandType.C_ARITHMETIC },
            { "neg", CommandType.C_ARITHMETIC },
            { "eq", CommandType.C_ARITHMETIC },
            { "gt", CommandType.C_ARITHMETIC },
            { "lt", CommandType.C_ARITHMETIC },
            { "and", CommandType.C_ARITHMETIC },
            { "or", CommandType.C_ARITHMETIC },
            { "not", CommandType.C_ARITHMETIC },
            { "label", CommandType.C_LABEL },
            { "goto", CommandType.C_GOTO },
            { "if-goto", CommandType.C_IF },
            { "call", CommandType.C_CALL },
            { "function", CommandType.C_FUNCTION },
            { "return", CommandType.C_RETURN },
            { "skippable", CommandType.SKIPPABLE },
        };

        var operation = commandParsed[0];

        if (String.IsNullOrWhiteSpace(operation) || !commandTypes.ContainsKey(operation))
        {
            operation = "skippable";
        }

        return commandTypes[operation];
    }

    private (string? _arg1, short _arg2) SetupArgs(CommandType commandType, string[] commandParsed)
    {
        switch (commandType)
        {
            case CommandType.C_ARITHMETIC: // eg. add
            {
                return (commandParsed[0], _arg2);
            }
            case CommandType.C_LABEL: // eg. label LABEL_NAME
            case CommandType.C_GOTO: // eg. goto LABEL_NAME
            case CommandType.C_IF: // eg. if-goto LABEL_NAME
            {
                return (commandParsed[1], _arg2);
            }
            case CommandType.C_PUSH: // eg. push constant 1
            case CommandType.C_POP: // eg. pop local 0
            case CommandType.C_CALL: // eg. call fName nArgs
            case CommandType.C_FUNCTION: // eg. function fName nVars
            {
                return (commandParsed[1], Int16.Parse(commandParsed[2]));
            }
            case CommandType.C_RETURN:
            case CommandType.SKIPPABLE:
            default:
            {
                return (_arg1, _arg2);
            }
        }
    }

    public CommandType GetCommandType()
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
    CommandType GetCommandType();
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