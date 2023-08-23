using VMTranslator.Translator;

try
{
    var fileNameArg = args[0];
    var folderEndsAtIndex = fileNameArg.LastIndexOf('/');
    var folder = fileNameArg[..folderEndsAtIndex];
    var fileNameAndExtension = fileNameArg[(folderEndsAtIndex + 1)..];
    var fileNameAndExtensionSplit = fileNameAndExtension.Split('.', 2);
    var fileName = fileNameAndExtensionSplit[0];
    var fileExtension = fileNameAndExtensionSplit[1];

    if ( !String.Equals(fileExtension, "vm") )
    {
        throw new Exception("Unsupported file type. Supply a .vm file.");
    }

    if ( !Char.IsUpper(fileName[0]) )
    {
        throw new Exception("VM programs must start with an uppercase letter.");
    }

    string outputFileName = $"{folder}/{fileName}.asm";

    using FileStream infs = new FileStream(fileNameArg, FileMode.Open, FileAccess.Read);
    using StreamReader sr = new StreamReader(infs);

    using FileStream outfs = File.Create(outputFileName);
    using StreamWriter sw = new StreamWriter(outfs);

    var translator = new Translator(new Parser(), new CodeWriter());

    translator.Translate(sr, sw, fileName);
}
catch (Exception e)
{
    Console.WriteLine(e);
}