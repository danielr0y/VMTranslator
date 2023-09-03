namespace VMTranslator.Translator;

public class CodeWriter : Nand2TetrisCodeWriter
{
    private readonly StreamWriter? _output;
    private readonly Dictionary<string, short> _callRefs = new();
    private string _fileName = "";
    private string _functionName = "";

    public CodeWriter(StreamWriter output, bool shouldBootstrap)
    {
        _output = output;

        if (shouldBootstrap)
        {
            Bootstrap();
        }
    }

    private short GetCallRef(string id)
    {
        try
        {
            return ++_callRefs[id];
        }
        catch (Exception)
        {
            _callRefs.Add(id, 0);
            return GetCallRef(id);
        }
    }

    public void SetFileName(string fileName)
    {
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
            case "add": return new string[]
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
            case "sub": return new string[]
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
            case "neg": return new string[]
            {
                "// neg",
                "    @SP",
                "    AM=M-1",
                "    M=-M",
                "    @SP",
                "    M=M+1",
            };
            case "eq":
            {
                var callRef = GetCallRef($"{_functionName}${command}");

                return new string[]
                {
                    "// eq",
                    "    @SP",
                    "    AM=M-1",
                    "    D=M",
                    "    @SP",
                    "    AM=M-1",
                    "    D=M-D",
                   $"    @{_functionName}${command}_TRUE.{callRef}",
                    "    D;JEQ",
                    "    @SP",
                    "    A=M",
                    "    M=0",
                   $"    @{_functionName}${command}_CONVERGE.{callRef}",
                    "    0;JMP",
                   $"({_functionName}${command}_TRUE.{callRef})",
                    "    @SP",
                    "    A=M",
                    "    M=-1",
                   $"({_functionName}${command}_CONVERGE.{callRef})",
                    "    @SP",
                    "    M=M+1",
                };
            }
            case "gt":
            {
                var callRef = GetCallRef($"{_functionName}${command}");

                return new string[]
                {
                    "// gt",
                    "    @SP",
                    "    AM=M-1",
                    "    D=M",
                    "    @SP",
                    "    AM=M-1",
                    "    D=M-D",
                   $"    @{_functionName}${command}_TRUE.{callRef}",
                    "    D;JGT",
                    "    @SP",
                    "    A=M",
                    "    M=0",
                   $"    @{_functionName}${command}_CONVERGE.{callRef}",
                    "    0;JMP",
                   $"({_functionName}${command}_TRUE.{callRef})",
                    "    @SP",
                    "    A=M",
                    "    M=-1",
                   $"({_functionName}${command}_CONVERGE.{callRef})",
                    "    @SP",
                    "    M=M+1",
                };
            }
            case "lt":
            {
                var callRef = GetCallRef($"{_functionName}${command}");

                return new string[]
                {
                    "// lt",
                    "    @SP",
                    "    AM=M-1",
                    "    D=M",
                    "    @SP",
                    "    AM=M-1",
                    "    D=M-D",
                   $"    @{_functionName}${command}_TRUE.{callRef}",
                    "    D;JLT",
                    "    @SP",
                    "    A=M",
                    "    M=0",
                   $"    @{_functionName}${command}_CONVERGE.{callRef}",
                    "    0;JMP",
                   $"({_functionName}${command}_TRUE.{callRef})",
                    "    @SP",
                    "    A=M",
                    "    M=-1",
                   $"({_functionName}${command}_CONVERGE.{callRef})",
                    "    @SP",
                    "    M=M+1",
                };
            }
            case "and": return new string[]
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
            case "or": return new string[]
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
            case "not": return new string[]
            {
                "// not ",
                "    @SP",
                "    AM=M-1",
                "    M=!M",
                "    @SP",
                "    M=M+1",
            };
            default: return Array.Empty<string>();
        }
    }

    public void WritePushPop(CommandType command, string segment, short index)
    {
        WriteLines(BuildPushPopCodeBlock(command, segment, index));
    }

    private string[] BuildPushPopCodeBlock(CommandType command, string segment, short index)
    {
        switch (command)
        {
            case CommandType.C_PUSH:
                switch (segment)
                {
                    case "constant": return new string[]
                    {
                       $"// push constant {index}",
                       $"    @{index}",
                        "    D=A",
                        "    @SP",
                        "    A=M",
                        "    M=D",
                        "    @SP",
                        "    M=M+1",
                    };
                    case "pointer": return new string[]
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
                    case "static": return new string[]
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
                    case "temp": return new string[]
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
                    default: return new string[]
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
                    case "constant": return new string[]
                    {
                       $"// pop constant {index}",
                        "    @SP",
                        "    AM=M-1",
                        "    D=M",
                       $"    @{index}",
                        "    M=D",
                    };
                    case "pointer": return new string[]
                    {
                        $"// pop pointer {index}",
                        "    @SP",
                        "    AM=M-1",
                        "    D=M",
                       $"    @{GetPointerAssemblySymbol(index)}",
                        "    M=D",
                    };
                    case "static": return new string[]
                    {
                       $"// pop static {index}",
                        "    @SP",
                        "    AM=M-1",
                        "    D=M",
                       $"    @{_fileName}.{index}",
                        "    M=D",
                    };
                    case "temp": return new string[]
                    {
                        $"// pop temp {index}",
                         "    @SP",
                         "    AM=M-1",
                         "    D=M",
                        $"    @{5 + index}",
                         "    M=D",
                    };
                    default: return new string[]
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

            default: return Array.Empty<string>();
        }
    }

    public void WriteLabel(string label)
    {
        WriteLines(new string[]
        {
           $"// label {label}",
           $"({_functionName}${label})"
        });
    }

    public void WriteGoto(string label)
    {
        WriteLines(new string[]
        {
           $"// goto {label}",
           $"    @{_functionName}${label}",
            "    0;JMP",
        });
    }

    public void WriteIf(string label)
    {
        WriteLines(new string[]
        {
            $"// if-goto {label}",
            "    @SP",
            "    AM=M-1",
            "    D=M",
           $"    @{_functionName}${label}",
            "    D;JNE"
        });
    }

    public void WriteFunction(string functionName, short nVars)
    {
        _functionName = functionName;

        WriteLines(new string[]
        {
            $"// function {functionName} {nVars}",
            $"({functionName})",
            "    // initialise local variables",
        });

        for (int i = 0; i < nVars; i++)
        {
            WritePushPop(CommandType.C_PUSH, "constant", 0);
        }
    }

    public void WriteCall(string functionName, short nArgs)
    {
        var callRef = GetCallRef(functionName);

        WriteLines(new string[]
        {
           $"// call {functionName} {nArgs}",
            "    // push returnAddress",
           $"    @{functionName}$ret.{callRef}",
            "    D=A",
            "    @SP",
            "    A=M",
            "    M=D",
            "    @SP",
            "    M=M+1",
            "    // push LCL",
            "    @LCL",
            "    D=M",
            "    @SP",
            "    A=M",
            "    M=D",
            "    @SP",
            "    M=M+1",
            "    // push ARG",
            "    @ARG",
            "    D=M",
            "    @SP",
            "    A=M",
            "    M=D",
            "    @SP",
            "    M=M+1",
            "    // push THIS",
            "    @THIS",
            "    D=M",
            "    @SP",
            "    A=M",
            "    M=D",
            "    @SP",
            "    M=M+1",
            "    // push THAT",
            "    @THAT",
            "    D=M",
            "    @SP",
            "    A=M",
            "    M=D",
            "    @SP",
            "    M=M+1",
            "    // ARG = SP - 5 - nArgs",
            "    @SP",
            "    D=M",
           $"    @{5+nArgs}",
            "    D=D-A",
            "    @ARG",
            "    M=D",
            "    // LCL = SP",
            "    @SP",
            "    D=M",
            "    @LCL",
            "    M=D",
           $"    // goto {functionName}",
           $"    @{functionName}",
            "    0;JMP",
            "    // write return label",
           $"({functionName}$ret.{callRef})",
        });
    }

    public void WriteReturn()
    {
        WriteLines(new string[]
        {
            "// return",
            "    // frame = LCL",
            "    @LCL",
            "    D=M",
            "    @frame",
            "    M=D",
            "    // returnAddress = *(frame - 5)",
            "    @5",
            "    A=D-A",
            "    D=M",
            "    @returnAddress",
            "    M=D",
            "    // *ARG = pop()",
            "    @SP",
            "    AM=M-1",
            "    D=M",
            "    @ARG",
            "    A=M",
            "    M=D",
            "    // SP = ARG + 1",
            "    D=A+1",
            "    @SP",
            "    M=D",
            "    // THAT = *(frame - 1)",
            "    @frame",
            "    D=M",
            "    @1",
            "    A=D-A",
            "    D=M",
            "    @THAT",
            "    M=D",
            "    // THIS = *(frame - 2)",
            "    @frame",
            "    D=M",
            "    @2",
            "    A=D-A",
            "    D=M",
            "    @THIS",
            "    M=D",
            "    // ARG = *(frame - 3)",
            "    @frame",
            "    D=M",
            "    @3",
            "    A=D-A",
            "    D=M",
            "    @ARG",
            "    M=D",
            "    // LCL = *(frame - 4)",
            "    @frame",
            "    D=M",
            "    @4",
            "    A=D-A",
            "    D=M",
            "    @LCL",
            "    M=D",
            "    // goto returnAddress",
           $"    @returnAddress",
            "    A=M",
            "    0;JMP",
        });
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

    public void Bootstrap()
    {
        WriteLines(new string[]
        {
            "// bootstrap",
            "    // SP = 256",
            "    @256",
            "    D=A",
            "    @SP",
            "    M=D",
        });
        WriteCall("Sys.init", 0);
        Close();
    }

    public void Close()
    {
        WriteLines(new string[]
        {
            "// exit",
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
    void SetFileName(string fileName);
    void WriteArithmetic(string command);
    void WritePushPop(CommandType command, string segment, short index);
    void WriteLabel(string label);
    void WriteGoto(string label);
    void WriteIf(string label);
    void WriteCall(string functionName, short nArgs);
    void WriteFunction(string functionName, short nVars);
    void WriteReturn();
    void Close();
}