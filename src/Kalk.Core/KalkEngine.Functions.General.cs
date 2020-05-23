﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Consolus;
using Kalk.Core.Modules;
using Scriban;
using Scriban.Functions;
using Scriban.Parsing;
using Scriban.Runtime;
using Scriban.Syntax;

namespace Kalk.Core
{
    public partial class KalkEngine
    {
        private const string CategoryGeneral = "General";

        private int _evalDepth;

        private delegate object EvaluateDelegate(string text, bool output = false);

        public T GetOrCreateModule<T>() where T : KalkModule, new()
        {
            var typeOfT = typeof(T);
            if (_modules.TryGetValue(typeOfT, out var module))
            {
                return (T) module;
            }
            var moduleT = new T();
            module = moduleT;
            _modules.Add(typeOfT, module);

            if (!module.IsBuiltin)
            {
                Builtins.SetValue(module.Name, module, true);

                Descriptors.Add(module.Name, new KalkDescriptor()
                {
                    Names = { module.Name },
                    Category = "Modules (e.g `import Files`)",
                });
            }

            module.Initialize(this);

            if (module.IsBuiltin)
            {
                module.InternalImport();
            }

            return moduleT;
        }

        [KalkDoc("clipboard", CategoryGeneral)]
        public object Clipboard(object value = null)
        {
            if (value == null)
            {
                return GetClipboardText?.Invoke();
            }
            else
            {
                SetClipboardText?.Invoke(ObjectToString(value));
                return null;
            }
        }

        [KalkDoc("display", CategoryGeneral)]
        public void Display(ScriptVariable name = null)
        {
            var mode = name?.Name ?? Config.Display;

            if (!KalkDisplayModeHelper.TryParse(mode, out var fullMode))
            {
                throw new ArgumentException($"Invalid display name `{mode}`. Expecting `std` or `dev`.", nameof(name));
            }

            Config.Display = mode;
            WriteHighlightLine($"# Display mode: {mode} ({fullMode})");
        }

        [KalkDoc("echo", CategoryGeneral)]
        public void Echo(ScriptVariable value = null)
        {
            if (value == null)
            {
                WriteHighlightLine($"# Echo is {(EchoEnabled ? "on":"off")}.");
                return;
            }
            var mode = value.Name;
            switch (mode)
            {
                case "true":
                case "on":
                    EchoEnabled = true;
                    break;
                case "false":
                case "off":
                    EchoEnabled = false;
                    break;
                default:
                    throw new ArgumentException($"Invalid parameter. Only on/true or off/false are valid for echo.", nameof(value));
            }
        }

        [KalkDoc("print", CategoryGeneral)]
        public void Print(object value)
        {
            var previousEcho = EchoEnabled;
            try
            {
                EchoEnabled = true;
                WriteHighlightLine(ObjectToString(value), highlight: false);
            }
            finally
            {
                EchoEnabled = previousEcho;
            }
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="expression"></param>
        [KalkDoc("help", CategoryGeneral)]
        public void Help(ScriptExpression expression = null)
        {
            var name = expression?.ToString();
            if (name != null && name != "help")
            {
                if (Descriptors.TryGetValue(name, out var descriptor))
                {
                    WriteHelpForDescriptor(descriptor);
                    return;
                }

                throw new ArgumentException($"The builtin function `{name}` does not exist", nameof(expression));
            }


            var random = new Random();
            WriteHighlightLine($"# help [name]");
            if (name == "help")
            {
                WriteHighlightLine($"#");
                WriteHighlightLine($"# Example");
                WriteHighlightLine($"  help # Display a list of function and topic names to get help from.");
                WriteHighlightLine($"  help {Descriptors.Select(x => x.Key).ElementAt(random.Next(0, Descriptors.Count - 1))} # Display the help for the specific function or topic.");
            }

            if (name == null)
            {
                var categoryToDescriptors = Descriptors.GroupBy(x => x.Value.Category).ToDictionary(x => x.Key, y => y.Select(x => x.Value).Distinct().ToList());
                WriteHighlightLine($"#");
                foreach (var categoryPair in categoryToDescriptors.OrderBy(x => x.Key))
                {
                    var list = categoryPair.Value;

                    // Exclude from the list modules that have been already imported
                    var names = list.SelectMany(x => x.Names).Where(funcName =>
                        Builtins.TryGetValue(funcName, out var funcObj) && (!(funcObj is KalkModuleWithFunctions module) || !module.IsImported)
                    ).OrderBy(x => x).ToList();

                    if (names.Count > 0)
                    {
                        WriteHighlightLine($"# {categoryPair.Key}");

                        WriteHighlightAligned("    - ", string.Join(", ", names));
                        WriteHighlightLine("");
                    }
                }
            }
        }

        private void WriteHelpForDescriptor(KalkDescriptor descriptor)
        {
            var parentless = descriptor.IsCommand && descriptor.Params.Count <= 1;
            var args = string.Join(", ", descriptor.Params.Select(x => x.IsOptional ? $"[{x.Name}]" : x.Name));

            var syntax = string.Join("/", descriptor.Names);

            if (!string.IsNullOrEmpty(args))
            {
                syntax += parentless ? $" {args}" : $"({args})";
            }

            WriteHighlightLine($"# {syntax}");
            WriteHighlightLine($"#");
            if (string.IsNullOrEmpty(descriptor.Description))
            {
                WriteHighlightLine($"#   No documentation available.");
            }
            else
            {
                WriteHighlightAligned($"#   ", descriptor.Description);
                if (descriptor.Params.Count > 0)
                {
                    WriteHighlightLine($"#");
                    WriteHighlightLine($"# Parameters");
                    foreach (var par in descriptor.Params)
                    {
                        WriteHighlightAligned($"#   - {par.Name}: ", par.Description);
                    }
                }
                if (!string.IsNullOrEmpty(descriptor.Returns))
                {
                    WriteHighlightLine($"#");
                    WriteHighlightLine($"# Returns");
                    WriteHighlightAligned($"#   ", descriptor.Returns);
                }
            }
        }

        /// <summary>
        /// Clear all defined variables.
        /// </summary>
        [KalkDoc("reset", CategoryGeneral)]
        public void Reset()
        {
            Variables.Clear();
        }

        /// <summary>
        /// Prints the version of kalk.
        /// </summary>
        [KalkDoc("version", CategoryGeneral)]
        public void Version()
        {
            var text = $"{ConsoleStyle.BrightRed}k{ConsoleStyle.BrightYellow}a{ConsoleStyle.BrightGreen}l{ConsoleStyle.BrightCyan}k{ConsoleStyle.Reset}  1.0.0 - Copyright (c) 2020 Alexandre Mutel";
            WriteHighlightLine(text, false);
        }

        /// <summary>
        /// Lists all user defined variables and functions.
        /// </summary>
        [KalkDoc("list", CategoryGeneral)]
        public void List()
        {
            // Highlight line per line
            if (Variables.Count == 0)
            {
                WriteHighlightLine("# No variables");
                return;
            }

            bool writeHeading = true;

            List<KeyValuePair<string, object>> functions = null;

            // Write variables
            foreach (var variableKeyPair in Variables)
            {
                if (variableKeyPair.Value is ScriptFunction function && !function.IsAnonymous)
                {
                    if (functions == null) functions = new List<KeyValuePair<string, object>>();
                    functions.Add(variableKeyPair);
                    continue;

                }
                if (writeHeading)
                {
                    WriteHighlightLine("# Variables");
                    writeHeading = false;
                }
                WriteHighlightVariableAndValueToConsole(variableKeyPair.Key, variableKeyPair.Value);
            }

            // Write functions
            if (functions != null)
            {
                WriteHighlightLine("# Functions");
                foreach (var variableKeyPair in functions)
                {
                    WriteHighlightVariableAndValueToConsole(variableKeyPair.Key, variableKeyPair.Value);
                }
            }
        }

        /// <summary>
        /// Deletes a user defined variable.
        /// </summary>
        /// <param name="variable">Name of the variable to delete.</param>
        [KalkDoc("del", CategoryGeneral)]
        public void DeleteVariable(ScriptVariable variable)
        {
            if (variable == null) throw new ArgumentNullException(nameof(variable));
            
            if (((IDictionary<string, object>)Variables).TryGetValue(variable.Name, out var previousValue))
            {
                Variables.Remove(variable.Name);
                if (previousValue is ScriptFunction function && !function.IsAnonymous)
                {
                    WriteHighlightLine($"# Function `{function}` deleted.");
                }
                else
                {
                    WriteHighlightLine($"# Variable `{variable.Name} == {ObjectToString(previousValue, true)}` deleted.");
                }
            }
            else
            {
                WriteHighlightLine($"# Variable `{variable.Name}` not found");
            }
        }

        /// <summary>
        /// Exit kalk.
        /// </summary>
        [KalkDoc("exit", CategoryGeneral)]
        public void Exit()
        {
            HasExit = true;
            ReplExit();
        }

        public void ClearHistory()
        {
            HistoryList.Clear();
        }
        
        /// <summary>
        /// Parse !0 history command.
        /// </summary>
        private bool TryParseSpecialHistoryBangCommand(string text, out Template template)
        {
            template = null;
            var matchHistory = MatchHistoryRegex.Match(text);
            if (matchHistory.Success)
            {
                var historyCmd = $"history({matchHistory.Groups[1].Value})";
                template = Template.Parse(historyCmd, null, _parserOptions, _lexerOptions);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Displays the command history.
        /// </summary>
        /// <param name="line">An optional parameter that indicates:
        ///
        /// - if it is &gt;= 0, the index of the history command to re-run. (e.g `history 1` to re-run the command 1 in the history)
        /// - if it is &lt; 0, how many recent lines to display. (e.g `history -3` would display the last 3 lines in the history)
        /// </param>
        [KalkDoc("history", CategoryGeneral)]
        public void History(object line = null)
        {
            // Always remove the history command (which is the command being executed
            HistoryList.RemoveAt(HistoryList.Count - 1);

            if (HistoryList.Count == 0)
            {
                WriteHighlightLine("# History empty");
                return;
            }

            if (line != null)
            {
                int lineNumber;
                try
                {
                    lineNumber = ToInt(new SourceSpan(), line);
                }
                catch
                {
                    throw new ArgumentException("Invalid history line number. Must be an integer.", nameof(line));
                }

                if (lineNumber >= 0 && lineNumber < HistoryList.Count)
                {
                    OnEnterNextText(HistoryList[lineNumber]);
                }
                else if (lineNumber < 0)
                {
                    lineNumber = HistoryList.Count + lineNumber;
                    if (lineNumber < 0) lineNumber = 0;
                    for (int i = lineNumber; i < HistoryList.Count; i++)
                    {
                        WriteHighlightLine($"{i}: {HistoryList[i]}");
                    }
                }
                else
                {
                    throw new ArgumentException($"Invalid history index. Check with `history` command.", nameof(line));
                }
            }
            else
            {
                for (int i = 0; i < HistoryList.Count; i++)
                {
                    WriteHighlightLine($"{i}: {HistoryList[i]}");
                }
            }
        }

        [KalkDoc("eval", CategoryGeneral)]
        public object EvaluateText(string text, bool output = false)
        {
            return EvaluateTextImpl(text, null, output);
        }

        [KalkDoc("load", CategoryGeneral)]
        public object LoadFile(string path, bool output = false)
        {
            var fullPath = FileModule.AssertReadFile(path);
            var text = File.ReadAllText(fullPath);
            return EvaluateTextImpl(text, path, output);
        }

        private object EvaluateTextImpl(string text, string filePath, bool output = false)
        {
            if (text == null) throw new ArgumentNullException(nameof(text));

            _evalDepth++;
            var shouldOutput = true;

            if (!(CurrentNode?.Parent is ScriptExpressionStatement))
            {
                shouldOutput = _evalDepth <= 1 || output;
            }
            var previous = EnableEngineOutput;

            try
            {
                EnableEngineOutput = shouldOutput;
                EnableOutput = Repl == null;
                var evaluate = Parse(text, filePath, false);
                if (evaluate.HasErrors)
                    throw new ArgumentException("This script has errors. Messages:\n" + string.Join("\n", evaluate.Messages), filePath != null ? nameof(filePath) : nameof(text));
                return Evaluate(evaluate.Page);
            }
            finally
            {
                _evalDepth--;
                EnableOutput = _evalDepth == 0;
                EnableEngineOutput = previous;
            }
        }

        private object EvaluateExpression(ScriptExpression expression)
        {
            var previousEngineOutput = EnableEngineOutput;
            var previousOutput = EnableOutput;
            try
            {
                EnableEngineOutput = false;
                EnableOutput = false;
                return Evaluate(expression);
            }
            finally
            {
                EnableOutput = previousOutput;
                EnableEngineOutput = previousEngineOutput;
            }
        }

        /// <summary>
        /// Clears the screen (by default) or the history (e.g clear history).
        /// </summary>
        /// <param name="what">An optional argument specifying what to clear. Can be of the following value:
        /// * screen: to clear the screen (default if not passed)
        /// * history: to clear the history
        /// </param>
        [KalkDoc("clear", CategoryGeneral)]
        public void Clear(ScriptExpression what = null)
        {
            if (what != null)
            {
                if (what is ScriptVariableGlobal variable)
                {
                    switch (variable.Name)
                    {
                        case "history":
                            ClearHistory();
                            return;
                        case "screen": goto clearScreen;
                    }
                }
                throw new ArgumentException("Unexpected argument. `clear` command accepts only `screen` or `history` arguments.");
            }

            clearScreen:

            OnClear?.Invoke();
        }

        /// <summary>
        /// Clears the screen.
        /// </summary>
        [KalkDoc("cls", CategoryGeneral)]
        public void Cls()
        {
            Clear(null);
        }

        /// <summary>
        /// Returns the last evaluated result.
        /// </summary>
        /// <returns>The last evaluated result as an object.</returns>
        [KalkDoc("out", CategoryGeneral)]
        public object Last()
        {
            return _lastResult;
        }

        /// <summary>
        /// Copies the last evaluated content to the clipboard.
        ///
        /// This is equivalent to `out |> clipboard`.
        /// </summary>
        [KalkDoc("out2clipboard", CategoryGeneral)]
        public void LastToClipboard()
        {
            Clipboard(Last());
        }
        
        private delegate void DefineShortcutDelegate(ScriptVariable name, params ScriptExpression[] shortcuts);

        [KalkDoc("shortcut", CategoryGeneral)]
        public void Shortcut(ScriptVariable name, params ScriptExpression[] shortcuts)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            if (shortcuts.Length == 0)
            {
                throw new ArgumentException("Invalid arguments. Missing a key associated to a symbol.", nameof(shortcuts));
            }

            if ((shortcuts.Length % 2) != 0)
            {
                throw new ArgumentException("Invalid arguments. Missing a symbol associated to a key.", nameof(shortcuts));
            }

            var keyList = new List<KalkShortcutKey>();
            for (int i = 0; i < shortcuts.Length; i += 2)
            {
                keyList.Add(KalkShortcutKey.Parse(shortcuts[i], shortcuts[i + 1]));
            }

            var shortcut = new KalkShortcut(name.Name, keyList, !_registerAsSystem);
            Shortcuts.AddSymbolShortcut(shortcut);
        }


        private delegate void DefineAliasDelegate(ScriptVariable name, params ScriptVariable[] aliases);

        [KalkDoc("alias", CategoryGeneral)]
        public void Alias(ScriptVariable name, params ScriptVariable[] aliases)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            if (aliases.Length == 0)
            {
                throw new ArgumentException("Invalid arguments. Missing a key associated to a symbol.", nameof(aliases));
            }

            var names = new List<string>();
            for (int i = 0; i < aliases.Length; i++)
            {
                names.Add(aliases[i].Name);
            }

            var alias = new KalkAlias(name.Name, names, !_registerAsSystem);
            Aliases.AddAlias(alias);
        }
    }
}