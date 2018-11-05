//
// This file is created by Frenetic LLC.
// This code is Copyright (C) 2016-2017 Frenetic LLC under the terms of a strict license.
// See README.md or LICENSE.txt in the source root for the contents of the license.
// If neither of these are available, assume that neither you nor anyone other than the copyright holder
// hold any right or permission to use this software until such time as the official license is identified.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreneticScript.TagHandlers.Objects;

namespace FreneticScript.TagHandlers.Common
{
    /// <summary>
    /// Handles the 'function' tag base.
    /// </summary>
    public class FunctionTagBase : TemplateTagBase
    {
        // <--[tagbase]
        // @Base function[<FunctionTag>]
        // @Group Common Base Types
        // @ReturnType FunctionTag
        // @Returns the input object as a FunctionTag.
        // -->

        /// <summary>
        /// Constructs the tag base.
        /// </summary>
        public FunctionTagBase()
        {
            Name = "function";
            ResultTypeString = FunctionTag.TYPE;
        }

        /// <summary>
        /// Handles the base input for a tag.
        /// </summary>
        /// <param name="data">The tag data.</param>
        /// <returns>The correct object.</returns>
        public static TemplateObject HandleOne(TagData data)
        {
            return FunctionTag.CreateFor(data.GetModifierObjectCurrent(), data);
        }
    }
}