﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreneticScript.TagHandlers.Common;
using FreneticScript.TagHandlers.Objects;
using FreneticScript.TagHandlers;
using System.Reflection;

namespace FreneticScript.CommandSystem.Arguments
{
    /// <summary>
    /// Part of an argument that contains tags.
    /// </summary>
    public class TagArgumentBit: ArgumentBit
    {
        /// <summary>
        /// The pieces that make up the tag.
        /// </summary>
        public TagBit[] Bits;

        /// <summary>
        /// The method that gets the result of this TagArgumentBit.
        /// </summary>
        public MethodInfo GetResultMethod;

        /// <summary>
        /// Calls the method directly.
        /// </summary>
        public MethodHandler GetResultHelper;

        /// <summary>
        /// Parse the tag.
        /// </summary>
        /// <param name="data">The relevant tag data.</param>
        /// <returns>The parsed final object.</returns>
        public delegate TemplateObject MethodHandler(TagData data);

        /// <summary>
        /// Constructs a TagArgumentBit.
        /// </summary>
        /// <param name="system">The relevant command system.</param>
        /// <param name="bits">The tag bits.</param>
        public TagArgumentBit(Commands system, TagBit[] bits)
        {
            Bits = bits;
            CommandSystem = system;
        }

        /// <summary>
        /// Gets the resultant type of this argument bit.
        /// </summary>
        /// <param name="values">The relevant variable set.</param>
        /// <returns>The tag type.</returns>
        public override TagType ReturnType(CILAdaptationValues values)
        {
            if (Bits.Length == 1)
            {
                // TODO: Generic handler?
                if (Start is LvarTagBase)
                {
                    int var = (int)(((Bits[0].Variable.Bits[0]) as TextArgumentBit).InputValue as IntegerTag).Internal;
                    // TODO: Simpler lookup for var types. Probably a var type array.
                    for (int n = 0; n < values.CLVariables.Count; n++)
                    {
                        for (int i = 0; i < values.CLVariables[n].LVariables.Count; i++)
                        {
                            if (values.CLVariables[n].LVariables[i].Item1 == var)
                            {
                                return values.CLVariables[n].LVariables[i].Item3;
                            }
                        }
                    }
                    return null; // TODO: Error?
                }
                return Start.ResultType;
            }
            return Bits[Bits.Length - 1].TagHandler.Meta.ReturnTypeResult;
        }

        /// <summary>
        /// The tag to fall back on if this tag fails.
        /// </summary>
        public Argument Fallback;

        /// <summary>
        /// The starting point for this tag.
        /// </summary>
        public TemplateTagBase Start = null;

        /// <summary>
        /// Parse the argument part, reading any tags.
        /// </summary>
        /// <param name="base_color">The base color for color tags.</param>
        /// <param name="mode">The debug mode to use when parsing tags.</param>
        /// <param name="error">What to invoke if there is an error.</param>
        /// <param name="cse">The relevant command stack entry, if any.</param>
        /// <returns>The parsed final object.</returns>
        public override TemplateObject Parse(string base_color, DebugMode mode, Action<string> error, CompiledCommandStackEntry cse)
        {
            return CommandSystem.TagSystem.ParseTags(this, base_color, mode, error, cse);
        }

        /// <summary>
        /// Returns the tag as tag input text.
        /// </summary>
        /// <returns>Tag input text.</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("<{");
            for (int i = 0; i < Bits.Length; i++)
            {
                sb.Append(Bits[i].ToString());
                if (i + 1 < Bits.Length)
                {
                    sb.Append(".");
                }
            }
            sb.Append("}>");
            return sb.ToString();
        }
    }
}
