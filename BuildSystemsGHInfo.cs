using Grasshopper;
using Grasshopper.Kernel;
using System;
using System.Drawing;
using System.Reflection;

namespace BuildSystemsGH
{
    public class BuildSystemsGHInfo : GH_AssemblyInfo
    {
        static BuildSystemsGHInfo()
        {
            AppDomain.CurrentDomain.AssemblyResolve += HandleAssemblyResolve;
        }

        private static Assembly HandleAssemblyResolve(object sender, ResolveEventArgs args)
        {
            if (args.Name.StartsWith("Newtonsoft.Json,"))
            {
                string resourceName = "BuildSystemsGH.Resources.Newtonsoft.Json.dll";
                using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
                {
                    if (stream == null) return null;
                    byte[] assemblyData = new byte[stream.Length];
                    stream.Read(assemblyData, 0, assemblyData.Length);
                    return Assembly.Load(assemblyData);
                }
            }
            return null;
        }

        public override string Name => "Build Systems";

        //Return a 24x24 pixel bitmap to represent this GHA library.
        public override Bitmap Icon => null;

        //Return a short string describing the purpose of this GHA library.
        public override string Description => "BuildSystem toolbox developed to speed-up our sustainable consulting services.";

        public override Guid Id => new Guid("36538369-6017-4b4c-9973-aee8f072399a");

        //Return a string identifying you or your company.
        public override string AuthorName => "Daniel Locatelli";

        //Return a string representing your preferred contact details.
        public override string AuthorContact => "daniel.locatelli@buildsystems.de";

        //Following Semantic Versioning (SemVer), initial was 0.1.0-alpha currently on 0.3.0-alpha (22/08/2023)
        public override string Version => "0.3.0";
    }
}