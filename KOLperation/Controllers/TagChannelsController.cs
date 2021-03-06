using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Web.Http.Description;
using KOLperation.Models;

namespace KOLperation.Controllers
{
    [RoutePrefix("api/[controller]")]
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class TagChannelsController : ApiController
    {
        private readonly AModel db = new AModel();
        private readonly string url = ConfigurationManager.AppSettings["ourIconUrl"].ToString();

        // GET: api/TagChannels
        public IHttpActionResult GetTagChannels()
        {
            return Ok(db.TagChannels.Select(c => new
            {
                c.TagId,
                c.FAid,
                c.TagName,
                Icon = url + c.TagIcon
            }));
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}