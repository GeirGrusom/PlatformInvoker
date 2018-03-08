﻿using System;

namespace Platform.Invoke.Attributes
{
    /// <summary>
    /// Specifies probe actions for <see cref="SkipProbeAttribute"/>.
    /// </summary>
    [Flags]
    public enum ProbeActions
    {
        /// <summary>
        /// No probes are skipped. This is the behavior if the attribute is not set.
        /// </summary>
        None,
        /// <summary>
        /// Specifies that the begin probe should not be called.
        /// </summary>
        Begin = 1,
        /// <summary>
        /// Specifies that the end probe should not be called.
        /// </summary>
        End = 2,
        /// <summary>
        /// Specifies that no probes should be called.
        /// </summary>
        BeginAndEnd = 3,
    }

    /// <summary>
    /// Declares that this field should not invoke probe methods.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class SkipProbeAttribute : Attribute
    {
        /// <summary>
        /// Actions to skip.
        /// </summary>
        public ProbeActions SkipActions;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="actions"></param>
        public SkipProbeAttribute(ProbeActions actions)
        {
            this.SkipActions = actions;
        }
        /// <summary>
        /// 
        /// </summary>
        public SkipProbeAttribute()
        {
            this.SkipActions = ProbeActions.BeginAndEnd;
        }
    }
}
