using GeoCodeLocal;

Console.WriteLine(@"
 _       __     __                             __                        
| |     / /__  / /________  ____ ___  ___     / /_____                   
| | /| / / _ \/ / ___/ __ \/ __ `__ \/ _ \   / __/ __ \                  
| |/ |/ /  __/ / /__/ /_/ / / / / / /  __/  / /_/ /_/ /                  
|__/|__/\___/_/\___/\____/_/ /_/ /_/\___/   \__/\____/____          __   
  / __ \/ __/ __/ (_)___  ___     / ____/__  ____  / ____/___  ____/ /__ 
 / / / / /_/ /_/ / / __ \/ _ \   / / __/ _ \/ __ \/ /   / __ \/ __  / _ \
/ /_/ / __/ __/ / / / / /  __/  / /_/ /  __/ /_/ / /___/ /_/ / /_/ /  __/
\____/_/ /_/ /_/_/_/ /_/\___/   \____/\___/\____/\____/\____/\__,_/\___/ ");

String inputFile = "";
Mode mode;
Parser parser;

if (args.Length == 1)
{
    inputFile = args[0];
    mode = Mode.reset;
    parser = Parser.format1;
}

else if (args.Length == 3)
{
    inputFile = args[0];
    mode = (Mode)Enum.Parse(typeof(Mode), args[1]);
    parser = (Parser)Enum.Parse(typeof(Parser), args[2]);
}
else
{
    Console.WriteLine("Only three arguments are accepted: <inputFile> <mode>[reset:default, proceed] <parser>[format1:default, samzurcher]");
    return;
}

if (!System.IO.File.Exists(inputFile))
{
    Console.WriteLine("File at {0} doesn't exists", inputFile);
    return;
}

if (!Mode.reset.Equals(mode) && !Mode.proceed.Equals(mode))
{
    Console.WriteLine("Only mode 'reset' or 'proceed' are allowed");
    return;
}

if (!Parser.format1.Equals(parser) && !Parser.samzurcher.Equals(parser))
{
    Console.WriteLine("Only parser 'format1' or 'samzurcher' are allowed");
    return;
}

await new Runner(inputFile, mode, new LineParserFactory().create(parser)).run();