﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreneticScript.TagHandlers;

namespace FreneticScript.CommandSystem.QueueCmds
{
    // <--[command]
    // @Name scriptcache
    // @Arguments removefunction/removescript all/<function>/<script> (quiet_fail)
    // @Short Modifies the state of the script cache, EG clearing it.
    // @Updated 2014/06/23
    // @Authors mcmonkey
    // @Group Queue
    // @Description
    // The ScriptCache 'removefunction' command is used to remove specified functions from the ScriptCache.
    // Specify 'all' to remove all cached functions.
    // The ScriptCache 'removescript' command is similar to 'removefunction', except for cached standard scripts.
    // Specify 'quiet_fail' to not show errors if the function or script does not exist.
    // Note that this will not stop already running scripts. To do that, use <@link command stop>stop all<@/link>.
    // TODO: Explain more!
    // TODO: ScriptCache info tags!
    // @Example
    // // This example clears the script cache of all standard scripts
    // scriptcache clear scripts
    // @Example
    // // This example clears the script cache of all functions
    // scriptcache clear functions
    // @Example
    // This example removes function 'helloworld' from the script cache
    // scriptcache remove helloworld
    // @Example
    // TODO: More examples!
    // -->
    class ScriptCacheCommand : AbstractCommand
    {
        public ScriptCacheCommand()
        {
            Name = "scriptcache";
            Arguments = "removefunction/removescript all/<function>/<script> (quiet_fail)";
            Description = "Modifies the state of the script cache, EG clearing it.";
            // TODO: Lock() functionality to ensure async-friendliness
        }

        public override void Execute(CommandEntry entry)
        {
            if (entry.Arguments.Count < 1)
            {
                ShowUsage(entry);
                return;
            }
            string type = entry.GetArgument(0).ToLower();
            if (type == "removescript")
            {
                if (entry.Arguments.Count < 2)
                {
                    ShowUsage(entry);
                    return;
                }
                string target = entry.GetArgument(1).ToLower();
                if (target == "all")
                {
                    int count = entry.Queue.CommandSystem.Scripts.Count;
                    entry.Queue.CommandSystem.Scripts.Clear();
                    entry.Good("Script cache cleared of <{text_color.emphasis}>" +
                        count + "<{text_color.base}> script" + (count == 1 ? ".": "s."));
                }
                else
                {
                    if (entry.Queue.CommandSystem.Scripts.Remove(target))
                    {
                        entry.Good("Script '<{text_color.emphasis}>" +
                            TagParser.Escape(target) + "<{text_color.base}>' removed from the script cache.");
                    }
                    else
                    {
                        if (entry.Arguments.Count > 2 && entry.GetArgument(2).ToLower() == "quiet_fail")
                        {
                            entry.Good("Script '<{text_color.emphasis}>" +
                                TagParser.Escape(target) + "<{text_color.base}>' does not exist in the script cache!");
                        }
                        else
                        {
                            entry.Error("Script '<{text_color.emphasis}>" +
                                TagParser.Escape(target) + "<{text_color.base}>' does not exist in the script cache!");
                        }
                    }
                }
            }
            else if (type == "removefunction")
            {
                if (entry.Arguments.Count < 2)
                {
                    ShowUsage(entry);
                    return;
                }
                string target = entry.GetArgument(1).ToLower();
                if (target == "all")
                {
                    int count = entry.Queue.CommandSystem.Functions.Count;
                    entry.Queue.CommandSystem.Functions.Clear();
                    entry.Good("Script cache cleared of <{text_color.emphasis}>" +
                        count + "<{text_color.base}> function" + (count == 1 ? "." : "s."));
                }
                else
                {
                    if (entry.Queue.CommandSystem.Functions.Remove(target))
                    {
                        entry.Good("Function '<{text_color.emphasis}>" +
                            TagParser.Escape(target) + "<{text_color.base}>' removed from the script cache.");
                    }
                    else
                    {
                        if (entry.Arguments.Count > 2 && entry.GetArgument(2).ToLower() == "quiet_fail")
                        {
                            entry.Good("Function '<{text_color.emphasis}>" +
                                TagParser.Escape(target) + "<{text_color.base}>' does not exist in the script cache!");
                        }
                        else
                        {
                            entry.Error("Function '<{text_color.emphasis}>" +
                                TagParser.Escape(target) + "<{text_color.base}>' does not exist in the script cache!");
                        }
                    }
                }
            }
            else
            {
                ShowUsage(entry);
            }
        }
    }
}
/*

                else if (target == "functions")
                {
                    int count = entry.Queue.CommandSystem.Functions.Count;
                    entry.Queue.CommandSystem.Functions.Clear();
                    entry.Good("Script cache cleared of <{text_color.emphasis}>" +
                        count + "<{text_color.base}> function" + (count == 1 ? "." : "s."));
                }
                else if (target == "all")
                {
                    int countScripts = entry.Queue.CommandSystem.Scripts.Count;
                    int countFunctions = entry.Queue.CommandSystem.Functions.Count;
                    entry.Queue.CommandSystem.Scripts.Clear();
                    entry.Queue.CommandSystem.Functions.Clear();
                    entry.Good("Script cache cleared of <{text_color.emphasis}>" +
                        countScripts + "<{text_color.base}> script" + (countScripts == 1 ? "," : "s,")
                        + " and <{text_color.emphasis}>" + countFunctions + "<{text_color.base}> function" +
                        (countFunctions == 1 ? ".": "s."));
                }
*/