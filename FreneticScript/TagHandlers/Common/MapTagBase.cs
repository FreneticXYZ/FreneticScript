﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreneticScript.TagHandlers.Objects;

namespace FreneticScript.TagHandlers.Common
{
    // <--[explanation]
    // @Name Maps
    // @Description
    // A map is a relationship between textual names and object values.
    // TODO: Explain better!
    // -->
    class MapTagBase : TemplateTagBase
    {
        // <--[tagbase]
        // @Base map[<MapTag>]
        // @Group Mathematics
        // @ReturnType MapTag
        // @Returns the specified input as a map.
        // <@link explanation maps>What are maps?<@/link>
        // -->
        public MapTagBase()
        {
            Name = "map";
        }

        public override TemplateObject Handle(TagData data)
        {
            TemplateObject modif = data.GetModifierObject(0);
            return (modif is MapTag ? modif: MapTag.For(modif.ToString())).Handle(data.Shrink());
        }
    }
}