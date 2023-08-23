using System.Xml;

namespace VMTranslator.Translator;

public class Translator
{
    private readonly Parser _parser;
    private readonly CodeWriter _writer;

    public Translator(Parser parser, CodeWriter writer)
    {
        _parser = parser;
        _writer = writer;
    }

    /**
     * get the name of the input file, constructs a parser and creates an output file
     * while( HasMorelines() )
     *  parse each line into its fields and the
     *  generate a sequence of assembly instructions from them
     */
    public void Translate(StreamReader fsIn, StreamWriter fsOut, string fileName)
    {
        _parser.Init(fsIn);
        _writer.Init(fsOut, fileName);

        while (_parser.HasMoreLines())
        {
            _parser.Advance();

            switch (_parser.CommandType())
            {
                case CommandType.C_ARITHMETIC:
                    _writer.WriteArithmetic(_parser.Arg1());
                    break;
                case CommandType.C_PUSH:
                case CommandType.C_POP:
                    _writer.WritePushPop(_parser.CommandType(), _parser.Arg1(), _parser.Arg2());
                    break;
                case CommandType.C_FUNCTION:
                case CommandType.C_CALL:
                case CommandType.C_RETURN:
                    break;
            }
        }

        _writer.Close();
    }
}