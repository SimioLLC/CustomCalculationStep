using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Reflection.Emit;

namespace EFInterfaceStep
{
    /// <summary>
    /// We add this so we can have a constructur that supplies a connect string.
    /// It is a 'partial' class so that if we re-generate Entity Framework we don't whack this.
    /// </summary>
    public partial class SimioCalculationStepSample2Entities : DbContext
    {
        public SimioCalculationStepSample2Entities(string connectString)
            : base(connectString)
        {
        }


    }
}

