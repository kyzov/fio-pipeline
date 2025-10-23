using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FIOpipeline.Domain.Interfaces
{
    public interface IPersonProvider
    {
        IEnumerable<string> Validate(Person person);
    }
}
