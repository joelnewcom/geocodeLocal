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
String mode = "";
String parser = "";

if (args.Length == 1)
{
    inputFile = args[0];
    mode = "reset";
}

else if (args.Length == 3)
{
    inputFile = args[0];
    mode = args[1];
    parser = args[2];
}
else
{
    Console.WriteLine("Only three arguments are accepted: <inputFile> <mode>[reset:default, proceed] <parser>[zkp, samzurcher]");
    return;
}

if (!System.IO.File.Exists(inputFile))
{
    Console.WriteLine("File at {0} doesn't exists", inputFile);
    return;
}

if (!"reset".Equals(mode) && !"proceed".Equals(mode))
{
    Console.WriteLine("Only mode 'reset' or 'proceed' are allowed");
    return;
}

if (!"zkp".Equals(parser) && !"samzurcher".Equals(parser))
{
    Console.WriteLine("Only parser 'zkp' or 'samzurcher' are allowed");
    return;
}

ILineParser lineParser;
switch (parser)
{
    case "zkp":
        lineParser = new ZKPExtractParser();
        break;
    case "samzurcher":
        lineParser = new SamZurcherParser();
        break;
    default:
        throw new ArgumentException("no parser defined");
}

await new Runner(inputFile, mode, lineParser).run();