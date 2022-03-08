namespace Zipper
{
    using CommandLine;

    /// <summary>
    ///     Supported commandline options
    /// </summary>
    public class CommandLineOptions
    {
        [Option('i', "input", HelpText = "File(s) to be zipped(comma separated)", Required=true)]
        public string Input { get; set; }

        [Option('l', "latest", HelpText = "Latest instance of file(s) only", Default=true)]
        public bool Latest { get; set; }

        [Option('o', "output", HelpText = "Target zip file", Required = true)]
        public string Output { get; set; }
    }
}
