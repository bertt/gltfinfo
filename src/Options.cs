using CommandLine;

namespace gltfinfo
{
    public class Options
    {
        [Option('i', "input", Required = true, HelpText = "Input path of the glb")]
        public string Input { get; set; }
    }
}
