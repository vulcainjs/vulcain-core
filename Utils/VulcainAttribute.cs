using System;
using System.Collections.Generic;
using System.Text;

namespace Vulcain.Core
{
    public abstract class VulcainAttribute: Attribute
    {
        internal abstract void Apply();
    }
}
