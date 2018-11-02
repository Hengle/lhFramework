using System;
using System.Collections.Generic;

namespace lhFramework.Infrastructure.Managers
{
    public interface IScene
    {
        void Initialize();
        void Load();
        void Unload();
        void Dispose();
    }
}
