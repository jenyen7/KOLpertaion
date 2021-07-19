using KOLperation.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KOLperation.Middleware
{
    public class HomeDataBase : IDatabase
    {
        public AModel GetDatabase()
        {
            AModel db = new AModel();
            return db;
        }
    }
}