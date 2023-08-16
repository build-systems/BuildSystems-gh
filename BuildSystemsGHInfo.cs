/*
Newtonsoft.Json license notice
The MIT License (MIT)

Copyright(c) 2007 James Newton-King

Permission is hereby granted, free of charge, to any person obtaining a copy of
this software and associated documentation files (the "Software"), to deal in
the Software without restriction, including without limitation the rights to
use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of
the Software, and to permit persons to whom the Software is furnished to do so,
subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

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
        public override string Description => "";

        public override Guid Id => new Guid("36538369-6017-4b4c-9973-aee8f072399a");

        //Return a string identifying you or your company.
        public override string AuthorName => "Daniel Locatelli";

        //Return a string representing your preferred contact details.
        public override string AuthorContact => "daniel.locatelli@buildsystems.de";
    }
}