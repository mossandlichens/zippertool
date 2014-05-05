namespace Zipper
{
    using CommandLine;
    using CommandLine.Text;

    /// <summary>
    ///     Supported commandline options
    /// </summary>
    public class CommandLineOptions
    {
        [Option('i', "input", HelpText = "File(s) to be zipped(comma separated)", Required=true)]
        public string Input { get; set; }

        [Option('l', "latest", HelpText = "Latest instance of file(s) only", DefaultValue=true)]
        public bool Latest { get; set; }

        [Option('o', "output", HelpText = "Target zip file", Required = true)]
        public string Output { get; set; }

        [Option('v', "verbose", DefaultValue = true,
          HelpText = "Prints all messages to standard output.")]
        public bool Verbose { get; set; }

        [ParserState]
        public IParserState LastParserState { get; set; }

        [HelpOption]
        public string GetUsage()
        {
            return HelpText.AutoBuild(this,
              (HelpText current) => HelpText.DefaultParsingErrorsHandler(this, current));
        }

    }
}
