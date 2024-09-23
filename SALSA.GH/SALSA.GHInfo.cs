using Grasshopper;
using Grasshopper.Kernel;
using System;
using System.Drawing;

namespace SALSA.GH
{
    /// <summary>
    /// SALSA
    /// Sqlite Assistant as Lousy Software Acronym
    /// </summary>
    public class SALSA_GHInfo : GH_AssemblyInfo
    {
        public override string Name => "SALSA.GH";

        //Return a 24x24 pixel bitmap to represent this GHA library.
        public override Bitmap Icon => null;

        //Return a short string describing the purpose of this GHA library.
        public override string Description => "";

        public override Guid Id => new Guid("571ce378-0e2d-4294-9b73-37a2313fae96");

        //Return a string identifying you or your company.
        public override string AuthorName => "";

        //Return a string representing your preferred contact details.
        public override string AuthorContact => "";
    }
}