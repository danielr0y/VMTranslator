namespace VMTranslator.Translator;

public class CodeWriter : Nand2TetrisCodeWriter
{
    private StreamWriter? _output;
    private string? _fileName;
    private short _labelId = 0;

    public void Init(StreamWriter output, string fileName)
    {
        _output = output;
        _fileName = fileName;
    }

    public void WriteArithmetic(string command)
    {
        WriteLines(BuildArithmeticCodeBlock(command));
    }

    private string[] BuildArithmeticCodeBlock(string command)
    {
        switch (command)
        {
            case "add":
            {
                return new string[]
                {
                    "// add",
                    "    @SP",
                    "    AM=M-1",
                    "    D=M",
                    "    @SP",
                    "    AM=M-1",
                    "    M=M+D",
                    "    @SP",
                    "    M=M+1",
                };
            }
            case "sub":
            {
                return new string[]
                {
                    "// sub",
                    "    @SP",
                    "    AM=M-1",
                    "    D=M",
                    "    @SP",
                    "    AM=M-1",
                    "    M=M-D",
                    "    @SP",
                    "    M=M+1",
                };
            }
            case "neg":
            {
                return new string[]
                {
                    "// neg",
                    "    @SP",
                    "    AM=M-1",
                    "    M=-M",
                    "    @SP",
                    "    M=M+1",
                };
            }
            case "eq":
            {
                var eq = _labelId++;
                var eqEnd = _labelId++;

                return new string[]
                {
                    "// eq",
                    "    @SP",
                    "    AM=M-1",
                    "    D=M",
                    "    @SP",
                    "    AM=M-1",
                    "    D=M-D",
                   $"    @LABEL{eq}",
                    "    D;JEQ",
                    "    @SP",
                    "    A=M",
                    "    M=0",
                   $"    @LABEL{eqEnd}",
                    "    0;JMP",
                   $"(LABEL{eq})",
                    "    @SP",
                    "    A=M",
                    "    M=-1",
                   $"(LABEL{eqEnd})",
                    "    @SP",
                    "    M=M+1",
                };
            }
            case "gt":
            {
                var gt = _labelId++;
                var endGt = _labelId++;

                return new string[]
                {
                    "// gt",
                    "    @SP",
                    "    AM=M-1",
                    "    D=M",
                    "    @SP",
                    "    AM=M-1",
                    "    D=M-D",
                   $"    @LABEL{gt}",
                    "    D;JGT",
                    "    @SP",
                    "    A=M",
                    "    M=0",
                   $"    @LABEL{endGt}",
                    "    0;JMP",
                   $"(LABEL{gt})",
                    "    @SP",
                    "    A=M",
                    "    M=-1",
                   $"(LABEL{endGt})",
                    "    @SP",
                    "    M=M+1",
                };
            }
            case "lt":
            {
                var lt = _labelId++;
                var endLt = _labelId++;

                return new string[]
                {
                    "// lt",
                    "    @SP",
                    "    AM=M-1",
                    "    D=M",
                    "    @SP",
                    "    AM=M-1",
                    "    D=M-D",
                   $"    @LABEL{lt}",
                    "    D;JLT",
                    "    @SP",
                    "    A=M",
                    "    M=0",
                   $"    @LABEL{endLt}",
                    "    0;JMP",
                   $"(LABEL{lt})",
                    "    @SP",
                    "    A=M",
                    "    M=-1",
                   $"(LABEL{endLt})",
                    "    @SP",
                    "    M=M+1",
                };
            }
            case "and":
            {
                return new string[]
                {
                    "// and",
                    "    @SP",
                    "    AM=M-1",
                    "    D=M",
                    "    @SP",
                    "    AM=M-1",
                    "    M=M&D",
                    "    @SP",
                    "    M=M+1",
                };
            }
            case "or":
            {
                return new string[]
                {
                    "// or",
                    "    @SP",
                    "    AM=M-1",
                    "    D=M",
                    "    @SP",
                    "    AM=M-1",
                    "    M=M|D",
                    "    @SP",
                    "    M=M+1",
                };
            }
            case "not":
            {
                return new string[]
                {
                    "// not ",
                    "    @SP",
                    "    AM=M-1",
                    "    M=!M",
                    "    @SP",
                    "    M=M+1",
                };
            }
            default:
            {
                return Array.Empty<string>();
            }
        }
    }

    public void WritePushPop(CommandType command, string segment, short index)
    {
        WriteLines(BuildPushPopSubRoutine(command, segment, index));
    }

    private string[] BuildPushPopSubRoutine(CommandType command, string segment, short index)
    {
        switch (command)
        {
            case CommandType.C_PUSH:
                switch (segment)
                {
                    case "constant":
                        var constant = index;
                        return new string[]
                        {
                           $"// push constant {constant}",
                           $"    @{constant}",
                            "    D=A",
                            "    @SP",
                            "    A=M",
                            "    M=D",
                            "    @SP",
                            "    M=M+1",
                        };

                    case "pointer":
                        return new string[]
                        {
                            $"// push pointer {index}",
                           $"    @{GetPointerAssemblySymbol(index)}",
                            "    D=M",
                            "    @SP",
                            "    A=M",
                            "    M=D",
                            "    @SP",
                            "    M=M+1",
                        };

                    case "static":
                        return new string[]
                        {
                           $"// push static {index}",
                           $"    @{_fileName}.{index}",
                            "    D=M",
                            "    @SP",
                            "    A=M",
                            "    M=D",
                            "    @SP",
                            "    M=M+1",
                        };

                    case "temp":
                        return new string[]
                        {
                            $"// push temp {index}",
                            $"    @{5 + index}",
                             "    D=M",
                             "    @SP",
                             "    A=M",
                             "    M=D",
                             "    @SP",
                             "    M=M+1",
                        };

                    default:
                        return new string[]
                        {
                           $"// push {segment} {index}",
                           $"    @{GetSegmentAssemblySymbol(segment)}",
                            "    D=M",
                           $"    @{index}",
                            "    A=D+A",
                            "    D=M",
                            "    @SP",
                            "    A=M",
                            "    M=D",
                            "    @SP",
                            "    M=M+1",
                        };
                }

            case CommandType.C_POP:
                switch (segment)
                {
                    case "constant":
                        var constant = index;

                        return new string[]
                        {
                           $"// pop constant {constant}",
                            "    @SP",
                            "    AM=M-1",
                            "    D=M",
                           $"    @{constant}",
                            "    M=D",
                        };

                    case "pointer":
                        return new string[]
                        {
                            $"// pop pointer {index}",
                            "    @SP",
                            "    AM=M-1",
                            "    D=M",
                           $"    @{GetPointerAssemblySymbol(index)}",
                            "    M=D",
                        };

                    case "static":
                        return new string[]
                        {
                           $"// pop static {index}",
                            "    @SP",
                            "    AM=M-1",
                            "    D=M",
                           $"    @{_fileName}.{index}",
                            "    M=D",
                        };

                    case "temp":
                        return new string[]
                        {
                            $"// pop temp {index}",
                             "    @SP",
                             "    AM=M-1",
                             "    D=M",
                            $"    @{5 + index}",
                             "    M=D",
                        };

                    default:
                        return new string[]
                        {
                            $"// pop {segment} {index}",
                            $"    @{GetSegmentAssemblySymbol(segment)}",
                            "    D=M",
                            $"    @{index}",
                            "    D=D+A",
                            "    @R13",
                            "    M=D",
                            "    @SP",
                            "    AM=M-1",
                            "    D=M",
                            "    @R13",
                            "    A=M",
                            "    M=D",
                        };
                }
            default:
                return Array.Empty<string>();
        }
    }

    private string GetSegmentAssemblySymbol(string segment)
    {
        return new Dictionary<string, string>()
        {
            { "local", "LCL" },
            { "argument", "ARG" },
            { "this", "THIS" },
            { "that", "THAT" },
        }[segment];
    }

    private string GetPointerAssemblySymbol(short index)
    {
        return new string[]
        {
            "THIS",
            "THAT"
        }[index];
    }

    public void Close()
    {
        WriteLines(new string[]
        {
            "(END)",
            "    @END",
            "    0;JMP",
        });
    }

    private void WriteLines(string[] lines)
    {
        foreach (var line in lines)
        {
            _output?.WriteLine(line);
        }
    }
}

public interface Nand2TetrisCodeWriter
{
    void WriteArithmetic(string command);
    void WritePushPop(CommandType command, string segment, short index);
    void Close();
}