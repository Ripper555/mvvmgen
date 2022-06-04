using System;

namespace MvvmGen.Commands
{
    public interface IExceptionHandler
    {
        void Handle(Exception e);
    }
}
