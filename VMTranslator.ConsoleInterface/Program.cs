using VMTranslator.Translator;

try
{
    /*
     * load sources files
     */
    string source;

    if (args.Length > 0)
    {
        string sourceArg = args[0];
        source = Path.GetFullPath(sourceArg);
    }
    else
    {
        source = Directory.GetCurrentDirectory();
    }

    bool sourceIsFilePath = Path.HasExtension(source);

    if ( !sourceIsFilePath && !Path.EndsInDirectorySeparator(source))
    {
        source = $"{source}/";
    }

    var sourceFolder = new DirectoryInfo(Path.GetDirectoryName(source) ??
                                   throw new Exception("Could not determine directory from command arguments"));

    var files = from file in Directory.EnumerateFiles(sourceFolder.FullName, "*.vm", SearchOption.TopDirectoryOnly)
        where !sourceIsFilePath || file == source // only include the one file if source is a file
        where Char.IsUpper(Path.GetFileName(file)[0])
        select file;

    if (!files.Any())
    {
        throw new Exception("Folder contains no .vm files beginning with an uppercase letter.");
    }


    /*
     * prepare the destination
     */
    var dest = Path.Combine(sourceFolder.FullName,
        $"{(sourceIsFilePath ? Path.GetFileNameWithoutExtension(source) : sourceFolder.Name)}.asm");

    // 'using' manages closing streams
    using var outfs = File.Create(dest);
    using var sw = new StreamWriter(outfs);


    /*
     * create the translator
     */
    bool shouldBootstrap = !sourceIsFilePath;
    var translator = new Translator(new Parser(), new CodeWriter(sw, shouldBootstrap));


    /*
     * do the translation
     */
    foreach (var file in files)
    {
        // 'using' managers closing streams
        using var infs = new FileStream(file, FileMode.Open, FileAccess.Read);
        using var sr = new StreamReader(infs);

        translator.Translate(sr, Path.GetFileNameWithoutExtension(file));
    }
}
catch (IndexOutOfRangeException)
{
    Console.WriteLine("use syntax: ConsoleInterface dirName");
}
catch (Exception e)
{
    Console.WriteLine(e);
}