// ***********************************************************************
// ⚡ MvvmGen => https://github.com/thomasclaudiushuber/mvvmgen
// Copyright © by Thomas Claudius Huber
// Licensed under the MIT license => See LICENSE file in repository root
// ***********************************************************************

using System.Collections.Generic;
using MvvmGen.Model;

namespace MvvmGen.Generators;

internal static class CommandPropertyGenerator
{
    internal static void GenerateCommandProperties(this ViewModelBuilder vmBuilder, IEnumerable<CommandToGenerate>? commandsToGenerate)
    {
        if (commandsToGenerate is not null)
        {
            foreach (var commandToGenerate in commandsToGenerate)
            {
                vmBuilder.AppendLineBeforeMember();
                var commandType = (commandToGenerate) switch {
                    { ExecuteMethod.IsAwaitable: true, ExecuteMethod.HasParameter: true } => $"IAsyncRelayCommand<{commandToGenerate.ExecuteMethod.ParameterType}>",
                    { ExecuteMethod.IsAwaitable: true, ExecuteMethod.HasParameter: false } => "IAsyncRelayCommand",
                    { ExecuteMethod.IsAwaitable: false, ExecuteMethod.HasParameter: true } => $"IRelayCommand<{commandToGenerate.ExecuteMethod.ParameterType}>",
                    { ExecuteMethod.IsAwaitable: false, ExecuteMethod.HasParameter: false } => "IRelayCommand",
                };
                vmBuilder.AppendLine($"public {commandType} {commandToGenerate.PropertyName} {{ get; }}");
            }
        }
    }
}