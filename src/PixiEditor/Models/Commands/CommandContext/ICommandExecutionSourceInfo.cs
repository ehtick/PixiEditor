﻿using System.Text.Json.Serialization;

namespace PixiEditor.Models.Commands.CommandContext;

[JsonDerivedType(typeof(ShortcutSourceInfo))]
[JsonDerivedType(typeof(MenuSourceInfo))]
[JsonDerivedType(typeof(CommandBindingSourceInfo))]
[JsonDerivedType(typeof(SearchSourceInfo))]
public interface ICommandExecutionSourceInfo
{
    public CommandExecutionSourceType SourceType { get; }
}