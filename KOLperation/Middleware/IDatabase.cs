using KOLperation.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KOLperation.Middleware
{
    public interface IDatabase
    {
        AModel GetDatabase();
    }
}