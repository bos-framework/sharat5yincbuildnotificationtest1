using BOS.StarterCode.Helpers;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Dynamic;

namespace BOS.StarterCode.Controllers
{
    /// <summary>
    /// Controller used by the navigation pane. This is just a transient controller
    /// </summary>
    public class NavigationController : Controller
    {
        private Logger Logger;

        public NavigationController()
        {
            Logger = new Logger();
        }

        /// <summary>
        /// Author: BOS Framework, Inc
        /// Description: Is triggered when the Navigation Menu options are clicked. Uses the selected modules 'code' to identify and navigate to the respective controller
        /// </summary>
        /// <param name="id"></param>
        /// <param name="code"></param>
        /// <param name="isDefault"></param>
        /// <returns></returns>
        public IActionResult NavigateToModule(Guid id, string code, bool isDefault)
        {
            try
            {
                //Based on the code of the selected module, it is navigated to the repective view/controller
                if (!string.IsNullOrWhiteSpace(code))
                {
                    //The module codes written here are the ones that BOS provides by default
                    switch (code)
                    {
                        case "MYPFL":
                            return RedirectToAction("Index", "Profile");
                        case "USERS":
                            return RedirectToAction("Index", "Users");
                        case "ROLES":
                            return RedirectToAction("Index", "Roles");
                        case "PRMNS":
                            return RedirectToAction("Index", "Permissions");
                        default:
                            return RedirectToAction("NavigationMenu", "Dashboard", new { selectedModuleId = id });
                    }
                }
                else
                {
                    //In case of a code, that is null, we navigat the user back to the Dashboard page with a proper message
                    ModelState.AddModelError("CustomError", "Please check the code of the selected module");
                    return RedirectToAction("NavigationMenu", "Dashboard", new { selectedModuleId = "" });
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("Navigation", "NavigateToModule", ex);

                dynamic model = new ExpandoObject();
                model.Message = ex.Message;
                model.StackTrace = ex.StackTrace;
                return View("ErrorPage", model);
            }
        }
    }
}