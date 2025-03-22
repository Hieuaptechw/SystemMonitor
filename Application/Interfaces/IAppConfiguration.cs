using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IAppConfiguration
    {
       
        string this[string key] { get; }
        string GetConnectionString(string name);
        void Reload();
    }
}
