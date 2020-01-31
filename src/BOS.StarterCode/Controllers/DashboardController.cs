using BOS.StarterCode.Helpers;
using BOS.StarterCode.Models.BOSModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;

namespace BOS.StarterCode.Controllers
{
    [Authorize(Policy = "IsAuthenticated")]
    public class DashboardController : Controller
    {
        private Logger Logger; //Logger object that is used to either log messages or exceptions

        public DashboardController()
        {
            Logger = new Logger();
        }

        /// <summary>
        /// Author: BOS Framework, Inc
        /// Description: Returns the "Dashboard" view in the application with appropriate data
        /// </summary>
        /// <returns></returns>
        public IActionResult Index()
        {
            try
            {
                return View(GetPageData()); //Returns to the view with the data required to paint it
            }
            catch (Exception ex)
            {
                Logger.LogException("Home", "Index", ex);

                dynamic model = new ExpandoObject();
                model.Message = ex.Message;
                model.StackTrace = ex.StackTrace;
                return View("ErrorPage", model);
            }
        }

        /// <summary>
        /// Author: BOS Framework, Inc
        /// Description: Is triggered when a "Custom Module" is clicked in the Navigation menu.  
        /// </summary>
        /// <param name="selectedModuleId"></param>
        /// <returns></returns>
        public IActionResult NavigationMenu(string selectedModuleId)
        {
            try
            {

                //Preparing a dynamic object that has the data required to render the page
                var model = GetPageData();
                if (model == null)
                {
                    model = new ExpandoObject();
                }

                model.CurrentModuleId = selectedModuleId ?? throw new ArgumentNullException("The selected moduleId cannot be null"); //Setting the current moduleId to, in this case, will always be a non-BOS, custom moduleId

                try
                {
                    var moduleOperations = HttpContext.Session.GetObject<List<Module>>("ModuleOperations"); //This is the list of permitted modules and operations to the logged-in user
                    var operationsList = moduleOperations.Where(i => i.Id == Guid.Parse(selectedModuleId)).Select(i => i.Operations).ToList(); //Fetching the allowed operations in this module for the given user
                    
                    if (operationsList.Count > 0)
                    {
                        model.Operations = operationsList[0]; //Setting the current moduleId to, in this case, will always be a non-BOS, custom moduleId
                    }
                }
                catch
                {
                    model.Operations = null; //Setting the current moduleId to, in this case, will always be a non-BOS, custom moduleId
                }

                return View("Index", model); //Returing to the Index view with the data
            }
            catch (Exception ex)
            {
                Logger.LogException("Home", "NavigationMenu", ex);

                dynamic model = new ExpandoObject();
                model.Message = ex.Message;
                model.StackTrace = ex.StackTrace;
                return View("ErrorPage", model);
            }
        }

        /// <summary>
        /// Author: BOS Framework, Inc
        /// Description: Is a private method that fetches all the necessary information to render on the View
        /// </summary>
        /// <returns></returns>
        private dynamic GetPageData()
        {
            try
            {
                //Preparing the data that is required to render the page
                dynamic model = new ExpandoObject();

                //Checking for Claims and setting appropriate values
                if (User != null)
                {
                    model.Username = User.FindFirst(c => c.Type == "Username")?.Value.ToString();
                    model.Initials = User.FindFirst(c => c.Type == "Initials")?.Value.ToString();
                    model.Roles = User.FindFirst(c => c.Type == "Role")?.Value.ToString();
                }

                //Checking if Sessions are non-null and getting the list of Permitted Modules
                if (HttpContext != null && HttpContext.Session != null)
                {
                    try
                    {
                        model.ModuleOperations = HttpContext.Session.GetObject<List<Module>>("ModuleOperations");
                    }
                    catch (Exception)
                    {
                        model.ModuleOperations = null;
                    }
                }
                //Since the Dashboard is custom and used primarily as a transient controller, the CurrentModuleId is set to null
                model.CurrentModuleId = null;
                return model;
            }
            catch (Exception ex)
            {
                Logger.LogException("Home", "GetPageData", ex);
                return null;
            }
        }
    }
}