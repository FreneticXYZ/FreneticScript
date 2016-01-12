﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreneticScript.TagHandlers;
using FreneticScript.CommandSystem.QueueCmds;
using FreneticScript.TagHandlers.Objects;
using FreneticScript.CommandSystem.Arguments;

namespace FreneticScript.CommandSystem
{
    /// <summary>
    /// Represents a set of commands to be run, and related information.
    /// </summary>
    public class CommandQueue
    {
        /// <summary>
        /// All commands in this queue, as strings.
        /// TODO: Replace list with more efficient handler (Linked list, perhaps?)
        /// </summary>
        public ListQueue<CommandEntry> CommandList;

        /// <summary>
        /// A list of all variables saved in this queue.
        /// </summary>
        public Dictionary<string, TemplateObject> Variables;

        /// <summary>
        /// Whether the queue can be delayed (EG, via a WAIT command).
        /// Almost always true.
        /// </summary>
        public bool Delayable = true;

        /// <summary>
        /// How long until the queue may continue.
        /// </summary>
        public float Wait = 0;

        /// <summary>
        /// Whether the queue is running.
        /// </summary>
        public bool Running = false;

        /// <summary>
        /// The last command to be run.
        /// </summary>
        public CommandEntry LastCommand;

        /// <summary>
        /// The command system running this queue.
        /// </summary>
        public Commands CommandSystem;

        /// <summary>
        /// The script that was used to build this queue.
        /// </summary>
        public CommandScript Script;

        /// <summary>
        /// How much debug information this queue should show.
        /// </summary>
        public DebugMode Debug;

        /// <summary>
        /// Whether commands in the queue will parse tags.
        /// </summary>
        public bool ParseTags = true;

        /// <summary>
        /// What was returned by the determine command for this queue.
        /// </summary>
        public List<string> Determinations = new List<string>();

        /// <summary>
        /// What function to invoke when output is generated.
        /// </summary>
        public Commands.OutputFunction Outputsystem = null;

        /// <summary>
        /// Constructs a new CommandQueue - generally kept to the FreneticScript internals.
        /// TODO: IList _commands -> ListQueue?
        /// </summary>
        public CommandQueue(CommandScript _script, IList<CommandEntry> _commands, Commands _system)
        {
            Script = _script;
            CommandSystem = _system;
            Variables = new Dictionary<string, TemplateObject>();
            Debug = DebugMode.FULL;
            for (int i = 0; i < _commands.Count; i++)
            {
                _commands[i].Queue = this;
                _commands[i].Output = CommandSystem.Output;
            }
            CommandList = new ListQueue<CommandEntry>(_commands);
        }

        /// <summary>
        /// Called when the queue is completed.
        /// </summary>
        public EventHandler<CommandQueueEventArgs> Complete;

        /// <summary>
        /// Starts running the command queue.
        /// </summary>
        public void Execute()
        {
            if (Running)
            {
                return;
            }
            Running = true;
            Tick(0f);
            if (Running)
            {
                CommandSystem.Queues.Add(this);
            }
        }

        /// <summary>
        /// Recalculates and advances the command queue.
        /// <param name="Delta">The time passed this tick.</param>
        /// </summary>
        public void Tick(float Delta)
        {
            if (Delayable && (LastCommand != null && LastCommand.Command.Waitable && LastCommand.WaitFor && !LastCommand.Finished))
            {
                return;
            }
            if (Delayable && Wait > 0f)
            {
                Wait -= Delta;
                if (Wait > 0f)
                {
                    return;
                }
                Wait = 0f;
            }
            while (CommandList.Length > 0)
            {
                CommandEntry CurrentCommand = CommandList.Pop();
                if (CurrentCommand == null)
                {
                    continue;
                }
                try
                {
                    CommandSystem.ExecuteCommand(CurrentCommand, this);
                }
                catch (Exception ex)
                {
                    if (!(ex is ErrorInducedException))
                    {
                        CurrentCommand.Error("Internal exception: " + ex.ToString());
                    }
                }
                LastCommand = CurrentCommand;
                if (Delayable && ((Wait > 0f) || (LastCommand.Command.Waitable && LastCommand.WaitFor && !LastCommand.Finished)))
                {
                    return;
                }
            }
            if (Complete != null)
            {
                Complete(this, new CommandQueueEventArgs(this));
            }
            Running = false;
        }
        
        /// <summary>
        /// Handles an error as appropriate to the situation, in the current queue, from the current command.
        /// </summary>
        /// <param name="entry">The command entry that errored.</param>
        /// <param name="message">The error message.</param>
        public void HandleError(CommandEntry entry, string message)
        {
            bool hasnext = false;
            for (int i = 0; i < CommandList.Length; i++)
            {
                if (GetCommand(i).Command is TryCommand &&
                    GetCommand(i).Arguments[0].ToString() == "\0CALLBACK")
                {
                    hasnext = true;
                    break;
                }
            }
            if (hasnext)
            {
                entry.Good("Force-exiting try block.");
                while (CommandList.Length > 0)
                {
                    CommandEntry entr = GetCommand(0);
                    if (entr.Command is TryCommand &&
                        entr.Arguments[0].ToString() == "\0CALLBACK")
                    {
                        RemoveCommand(0);
                        break;
                    }
                    RemoveCommand(0);
                }
                if (CommandList.Length > 0)
                {
                    CommandEntry ce = GetCommand(0);
                    if (ce.Command is CatchCommand)
                    {
                        RemoveCommand(0);
                        ce.Queue = this;
                        ce.Output = CommandSystem.Output;
                        SetVariable("error_message", new TextTag(message));
                        if (ce.Block != null)
                        {
                            ce.Good("Trying block...");
                            CommandEntry callback = new CommandEntry("catch \0CALLBACK", null, ce,
                                ce.Command, new List<Argument>() { CommandSystem.TagSystem.SplitToArgument("\0CALLBACK") }, "catch", 0, ce.ScriptName, ce.ScriptLine);
                            ce.Block.Add(callback);
                            AddCommandsNow(ce.Block);
                        }
                        else
                        {
                            ce.Error("Catch invalid: No block follows!");
                        }
                    }
                }
            }
            else
            {
                entry.Bad(message);
            }
        }

        /// <summary>
        /// Gets the command at the specified index.
        /// </summary>
        /// <param name="index">The index of the command.</param>
        /// <returns>The specified command.</returns>
        public CommandEntry GetCommand(int index)
        {
            int x = 0;
            while (CommandList.Length > index + x && CommandList[index + x] == null)
            {
                x++;
            }
            return CommandList[index + x];
        }

        /// <summary>
        /// Removes the command at the specified index.
        /// </summary>
        /// <param name="index">The index of the command.</param>
        public void RemoveCommand(int index)
        {
            int x = 0;
            while (CommandList.Length > index + x && CommandList[index + x] == null)
            {
                x++;
            }
            CommandList[index + x] = null;
        }

        /// <summary>
        /// Adds a list of entries to be executed next in line.
        /// </summary>
        /// <param name="entries">Commands to be run.</param>
        public void AddCommandsNow(List<CommandEntry> entries)
        {
            for (int i = 0; i < entries.Count; i++)
            {
                entries[i].Queue = this;
                entries[i].Output = CommandSystem.Output;
            }
            CommandList.Insert(0, entries.ToArray());
        }

        /// <summary>
        /// Immediately stops the Command Queue.
        /// </summary>
        public void Stop()
        {
            CommandList.Clear();
        }

        /// <summary>
        /// Adds or sets a variable for tags in this queue to use.
        /// </summary>
        /// <param name="name">The name of the variable.</param>
        /// <param name="value">The value to set on the variable.</param>
        public void SetVariable(string name, TemplateObject value)
        {
            string namelow = name.ToLower();
            Variables.Remove(namelow);
            Variables.Add(namelow, value);
        }

        /// <summary>
        /// Gets the value of a variable saved on the queue.
        /// </summary>
        /// <param name="name">The name of the variable.</param>
        /// <returns>The variable's value.</returns>
        public TemplateObject GetVariable(string name)
        {
            string namelow = name.ToLower();
            TemplateObject value;
            if (Variables.TryGetValue(namelow, out value))
            {
                return value;
            }
            return null;
        }
    }

    /// <summary>
    /// An enumerattion of the possible debug modes a queue can have.
    /// </summary>
    public enum DebugMode : byte
    {
        /// <summary>
        /// Debug everything.
        /// </summary>
        FULL = 1,
        /// <summary>
        /// Only debug errors.
        /// </summary>
        MINIMAL = 2,
        /// <summary>
        /// Debug nothing.
        /// </summary>
        NONE = 3
    }

    /// <summary>
    /// A mini-class used for the callback for &amp;waitable commands.
    /// </summary>
    public class EntryFinisher
    {
        /// <summary>
        /// The entry being waited on.
        /// </summary>
        public CommandEntry Entry;

        /// <summary>
        /// Add this function as a callback to complete entry.
        /// </summary>
        public void Complete(object sender, CommandQueueEventArgs args)
        {
            Entry.Finished = true;
        }
    }
}