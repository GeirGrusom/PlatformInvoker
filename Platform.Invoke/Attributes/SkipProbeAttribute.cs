using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platform.Invoke.Attributes
{
    [Flags]
    public enum ProbeActions
    {
        None,
        Begin = 1,
        End = 2,
        BeginAndEnd = 3,
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class SkipProbeAttribute : Attribute
    {
        public ProbeActions SkipActions;

        public SkipProbeAttribute(ProbeActions actions)
        {
            this.SkipActions = actions;
        }

        public SkipProbeAttribute()
        {
            this.SkipActions = ProbeActions.BeginAndEnd;
        }
    }
}
