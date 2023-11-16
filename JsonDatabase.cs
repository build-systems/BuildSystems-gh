using Grasshopper.Kernel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json.Linq;
using Rhino.Geometry;


namespace BuildSystemsGH
{
    public class JsonDatabase
    {
        private readonly string _databasePath;

        /// <summary>
        /// Constructor that assings the database folders path
        /// </summary>
        /// <param name="databasePath"></param>
        public JsonDatabase(string databasePath)
        {
            _databasePath = databasePath;
        }

        public List<string> GetFileNames(string libName, GH_ActiveObject gH_ActiveObject) 
        {
            try
            {
                // I didn't continue here because I don't know how to deal with errors in GH using an external class.
                // I wanted to create an error with AddRuntimeMessage() but then I would have to refer to GH_ActiveObject
                return null;
            }
            catch
            {
                return null;
            }
        }

    }
}
