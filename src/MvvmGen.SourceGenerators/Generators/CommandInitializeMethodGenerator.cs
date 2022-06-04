// ***********************************************************************
// ⚡ MvvmGen => https://github.com/thomasclaudiushuber/mvvmgen
// Copyright © by Thomas Claudius Huber
// Licensed under the MIT license => See LICENSE file in repository root
// ***********************************************************************

using System.Collections.Generic;
using System.Linq;
using MvvmGen.Model;

namespace MvvmGen.Generators
{
    internal static class CommandInitializeMethodGenerator
    {
        internal static void GenerateCommandInitializeMethod(this ViewModelBuilder vmBuilder, ICollection<CommandToGenerate> commandsToGenerate, ICollection<InjectionToGenerate> injectionsToGenerate)
        {
            if (commandsToGenerate.Any())
            {
                vmBuilder.AppendLineBeforeMember();
                vmBuilder.AppendLine("private void InitializeCommands()");
                vmBuilder.AppendLine("{");
                vmBuilder.IncreaseIndent();
                foreach (var commandToGenerate in commandsToGenerate.Where(x => !x.IsSafeCommand))
                {
                    var command = commandToGenerate.ExecuteMethod switch
                    {
                        { IsAwaitable: true, HasParameter: true } => $"AsyncRelayCommand<{commandToGenerate.ExecuteMethod.ParameterType}>",
                        { IsAwaitable: true, HasParameter: false } => "AsyncRelayCommand",
                        { IsAwaitable: false, HasParameter: true } => $"RelayCommand<{commandToGenerate.ExecuteMethod.ParameterType}>",
                        { IsAwaitable: false, HasParameter: false } => "RelayCommand",
                    };
                    vmBuilder.Append($"{commandToGenerate.PropertyName} = new {command}({commandToGenerate.ExecuteMethod.Name}");
                    if (commandToGenerate.CanExecuteMethod.HasValue)
                    {
                        vmBuilder.Append($", {GetMethodCall(commandToGenerate.CanExecuteMethod.Value)}");
                    }
                    vmBuilder.AppendLine(");");
                }

                var safeCommands = commandsToGenerate.Where(x => x.IsSafeCommand).ToList();
                if (safeCommands.Any())
                {
                    var logger = injectionsToGenerate.First(x => x.Type.Contains("ILogger")).PropertyName;
                    var handler = injectionsToGenerate.First(x => x.Type == "MvvmGen.IExceptionHandler").PropertyName;
                    foreach (var commandToGenerate in safeCommands)
                    {
                        var command = commandToGenerate.ExecuteMethod switch
                        {
                            { IsAwaitable: true, HasParameter: true } => $"AsyncSafeCommand<{commandToGenerate.ExecuteMethod.ParameterType}>",
                            { IsAwaitable: true, HasParameter: false } => "AsyncSafeCommand",
                            { IsAwaitable: false, HasParameter: true } => $"SafeCommand<{commandToGenerate.ExecuteMethod.ParameterType}>",
                            { IsAwaitable: false, HasParameter: false } => "SafeCommand",
                        };
                        vmBuilder.Append($"{commandToGenerate.PropertyName} = new {command}({commandToGenerate.ExecuteMethod}, {logger}, {handler}");
                        if (commandToGenerate.CanExecuteMethod.HasValue)
                        {
                            vmBuilder.Append($", {GetMethodCall(commandToGenerate.CanExecuteMethod.Value)}");
                        }
                        vmBuilder.AppendLine(");");
                    }
                }

                vmBuilder.DecreaseIndent();
                vmBuilder.AppendLine("}");
            }
        }

        private static object GetMethodCall(MethodInfo methodInfo)
        {
            return methodInfo switch
            {
                { IsAwaitable: true, HasParameter: true } => $"async x => await {methodInfo.Name}(x)",
                { IsAwaitable: true, HasParameter: false } => $"async _ => await {methodInfo.Name}()",
                { IsAwaitable: false, HasParameter: true } => $"{methodInfo.Name}",
                { IsAwaitable: false, HasParameter: false } => $"_ => {methodInfo.Name}()",
            };
        }
    }
}
