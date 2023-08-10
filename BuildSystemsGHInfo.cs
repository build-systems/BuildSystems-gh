using Grasshopper;
using Grasshopper.Kernel;
using System;
using System.Drawing;

namespace BuildSystemsGH
{
    public class BuildSystemsGHInfo : GH_AssemblyInfo
    {
        public override string Name => "Build Systems";

        //Return a 24x24 pixel bitmap to represent this GHA library.
        public override Bitmap Icon => null;

        //Return a short string describing the purpose of this GHA library.
        public override string Description => "";

        public override Guid Id => new Guid("36538369-6017-4b4c-9973-aee8f072399a");

        //Return a string identifying you or your company.
        public override string AuthorName => "";

        //Return a string representing your preferred contact details.
        public override string AuthorContact => "";
    }
}