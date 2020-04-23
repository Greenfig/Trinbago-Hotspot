using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Trinbago_MVC5.Controllers
{
    /// <summary>
    /// Manages the automated sending of notifications
    /// </summary>
    public class NotificationController : Controller
    {
        /// <summary>
        /// Sends a notification to the user that the ad is expiring soon
        /// </summary>
        /// <returns></returns>
        public ActionResult NotifyUserAdExpires()
        {
            return View();
        }
    }
}