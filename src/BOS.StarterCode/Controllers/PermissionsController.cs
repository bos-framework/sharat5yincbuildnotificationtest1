using BOS.IA.Client;
using BOS.IA.Client.ClientModels;
using BOS.StarterCode.Helpers;
using BOS.StarterCode.Models.BOSModels;
using BOS.StarterCode.Models.BOSModels.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;

namespace BOS.StarterCode.Controllers
{
    /// <summary>
    /// This controller houses all the enpoints that handle the setting up and fetching of permissions
    /// </summary>
    [Authorize(Policy = "IsAuthenticated")]
    public class PermissionsController : Controller
    {
        private readonly IIAClient _bosIAClient;

        private Logger Logger;

        public PermissionsController(IIAClient iaClient)
        {
            _bosIAClient = iaClient;
            Logger = new Logger();
        }

        /// <summary>
        /// Author: BOS Framework, Inc
        /// Description: Returns the view that displays all the modules and operations that are associated with the given role, together with the complete list of modules and operations
        /// </summary>
        /// <param name="roleId"></param>
        /// <param name="roleName"></param>
        /// <returns></returns>
        public async Task<ActionResult> FetchPermissions(string roleId, string roleName)
        {
            try
            {
                var model = GetPageData(); //This mehod returns all the data that is required for loading the page
                if (model == null)
                {
                    model = new ExpandoObject(); //If the model object is null, we create a new dynamic object
                }

                /* ------------- LOGIC -------------
                     * Get the roleId and verify if it is non-null or non-empty
                     * Make a call to the BOS API to get the list of all the Modules and Operations that the role is permitted
                     * Get the absolute list of all the modules and permissions in the application
                     * Loop through the permitted modules and operations and set the 'IsPermitted' property to true. Based on this property the View is displayed
                     */
                if (!string.IsNullOrWhiteSpace(roleId)) //Checking for non-null roleId. This is the for which the permissions are set
                {
                    var ownerPermissionsresponse = await _bosIAClient.GetOwnerPermissionsSetsAsFlatAsync<PermissionsModule>(Guid.Parse(roleId)); //Making a BOS API call to get the permitted list of modules and operations. GetOwnerPermissionsSetsAsFlatAsync endpoint is called becasue it is easier to iterate through a non-nested list

                    //Declaring a few vairables that will help to get to the required output - i.e. a single list of all modules and operations (which is nested) but has a differentiating attribute of 'IsPermitted' 
                    List<Module> allModules = new List<Module>();
                    List<Operation> allOperations = new List<Operation>();
                    List<IPermissionsOperation> permittedOperations = new List<IPermissionsOperation>();
                    List<IPermissionsSet> permittedModules = new List<IPermissionsSet>();

                    if (ownerPermissionsresponse != null && ownerPermissionsresponse.IsSuccessStatusCode)
                    {
                        permittedModules = ownerPermissionsresponse.Permissions.Components; //Assiging the BOS API response of flat-listed modules to the variable
                        permittedOperations = ownerPermissionsresponse.Permissions.Operations;//Assiging the BOS API response of flat-listed operations to the variable
                    }

                    var modulesResponse = await _bosIAClient.GetModulesAsync<Module>(true, true); //Making another BOS API call to get the complete list of Modules in the application
                    if (modulesResponse != null && modulesResponse.IsSuccessStatusCode)
                    {
                        allModules = modulesResponse.Modules; //Assiging the BOS API response of complete list of nested modules to the variable
                    }

                    var operationsResponse = await _bosIAClient.GetOperationsAsync<Operation>(true, true); // Making another BOS API call to get the complete list of Operations in the application
                    if (operationsResponse != null && operationsResponse.IsSuccessStatusCode)
                    {
                        allOperations = operationsResponse.Operations;  //Assiging the BOS API response of complete list of nested operations to the variable
                    }

                    //Iterating through the permitted list of modules and finding it in the other complete list of modules to set the 'IsPermitted' property to True
                    foreach (PermissionsSet module in permittedModules)
                    {
                        var moduleObj = allModules.Where(x => x.Id == module.ComponentId).FirstOrDefault(); //However, this works only at level-0, in the nested list, so we have a few custom methods that help us iterate through the N-Level down nested list

                        if (moduleObj != null)
                        {
                            moduleObj.IsPermitted = true; //If the permitted moduleId is found in the list of all the Modules, we set its "IsPermitted" property to True  

                            //We repeat the process for Operations
                            if (moduleObj.Operations.Count > 0)
                            {
                                foreach (Operation operation in moduleObj.Operations)
                                {
                                    var operationObj = permittedOperations.FirstOrDefault(x => x.OperationId == operation.Id);
                                    if (operationObj != null)
                                    {
                                        operation.IsPermitted = true; //If the permitted OperationId is found in the list of all the Operations, we set its "IsPermitted" property to True 
                                    }
                                    else
                                    {
                                        if (operation.ChildOperations != null && operation.ChildOperations.Count > 0)
                                        {
                                            var operationsList = operation.ChildOperations;
                                            SetPermittedSubOperations(operation.Id, ref operationsList, ref permittedOperations);
                                        }
                                    }
                                    if (operation.ChildOperations != null && operation.ChildOperations.Count > 0)
                                    {
                                        var operationsList = operation.ChildOperations;
                                        SetPermittedSubOperations(operation.Id, ref operationsList, ref permittedOperations);
                                    }
                                }
                            }
                        }
                        else
                        {
                            //If it is not found, then we go to the next level of the nested list
                            SetPermittedSubModules(module.ComponentId, ref allModules, ref permittedOperations);
                        }
                    }

                    model.ModuleOperations = allModules[0].ChildComponents; //Finally, assigning the updated "allModules" nested-list to the model
                    model.OwnerId = roleId;
                    model.RoleName = roleName;
                    return View("Index", model); //Returing to the View with the sufficient data to render the page
                }
                else
                {
                    ModelState.AddModelError("CustomError", "The selected role does not have a verified Id. Please try again.");
                    return View("Index", model);
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("Permissions", "FetchPermissions", ex);

                dynamic model = new ExpandoObject();
                model.Message = ex.Message;
                model.StackTrace = ex.StackTrace;
                return View("ErrorPage", model);
            }
        }

        /// <summary>
        /// Author: BOS Framework, Inc
        /// Description: Navigates the user back to the Role view
        /// </summary>
        /// <returns></returns>
        public IActionResult BackToRoles()
        {
            return RedirectToAction("Index", "Roles"); //Helps redirect the user back to the Roles page
        }

        /// <summary>
        /// Author: BOS Framework, Inc
        /// Description: Saves all the changes made to the permissions for the given role
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<string> UpdatePermissions([FromBody] JObject data)
        {
            try
            {
                /* ------ LOGIC ----------
                 * Get the flat-listed modules from the View 
                 * Get the flat-listed operations from the View
                 * Get the RoleId which in this case is the OwnerId
                 * Prepare the input parameter to the BOS API 
                 * Make the API Call and return the status
                 */

                //Checking for non-null data
                if (data != null)
                {
                    //Setting the flat-listed modules that are permitted for the selected role
                    PermissionsModule permissionsModule = new PermissionsModule();
                    List<PermissionsSet> modules = data["Modules"].ToObject<List<PermissionsSet>>();
                    permissionsModule.Components = new List<IPermissionsSet>();
                    permissionsModule.Components.AddRange(modules);

                    //Setting the flat-listed operations that are permitted for the selected role
                    List<PermissionsOperation> operations = data["Operations"].ToObject<List<PermissionsOperation>>();
                    permissionsModule.Operations = new List<IPermissionsOperation>();
                    permissionsModule.Operations.AddRange(operations);

                    //Setting the RoleId to be the OwnerId
                    permissionsModule.OwnerId = Guid.Parse(data["OwnerId"].ToString());
                    permissionsModule.Type = SetType.Role;

                    var response = await _bosIAClient.AddPermissionsAsync<PermissionsModule>(permissionsModule); //Making the BOS API call to Add/ Update the API
                    if (response != null && response.IsSuccessStatusCode)
                    {
                        return "Permissions updated successfully"; //return the success message
                    }
                    else
                    {
                        return response.BOSErrors[0].Message;
                    }
                }
                else
                {
                    return "Permission set cannot be empty";
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("Permissions", "FetchPermissions", ex);
                return ex.Message;
            }
        }

        /// <summary>
        /// Author: BOS Framework, Inc
        /// Description: Fetches the data necessary for rendering on the page
        /// </summary>
        /// <returns></returns>
        private dynamic GetPageData()
        {
            try
            {
                //Checking if Sessions are enabled and non-null
                if (HttpContext != null && HttpContext.Session != null)
                {
                    dynamic model = new ExpandoObject();
                    try
                    {
                        var moduleOperations = HttpContext.Session.GetObject<List<Module>>("ModuleOperations"); //This is the list of permitted modules and operations to the logged-in user

                        Guid currentModuleId = new Guid(); //A new variable to save the current or selected module Id. This is being used, especially in the custom modules → more for the UI purposes
                        try
                        {
                            currentModuleId = moduleOperations.Where(i => i.Code == "ROLES").Select(i => i.Id).ToList()[0]; //Selecting the module ID for Roles currently because, Permissions, though a module, is currently programmed as an operation under Roles module
                        }
                        catch (ArgumentNullException)
                        {
                            currentModuleId = Guid.Empty;
                        }
                        model.ModuleOperations = moduleOperations;
                        model.CurrentModuleId = currentModuleId;
                    }
                    catch (Exception)
                    {
                        model.ModuleOperations = null;
                        model.CurrentModuleId = null;
                    }

                    //Checking for a non-null Claims object
                    if (User != null)
                    {
                        model.Initials = User.FindFirst(c => c.Type == "Initials")?.Value.ToString();
                        model.Username = User.FindFirst(c => c.Type == "Username")?.Value.ToString();
                        model.Roles = User.FindFirst(c => c.Type == "Role")?.Value.ToString();
                    }
                    return model; //Finally return the model with all the data required
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("Permissions", "GetPageData", ex);

                return null;
            }
        }

        /// <summary>
        /// Private method to loop through a List of Modules
        /// </summary>
        /// <param name="moduleId"></param>
        /// <param name="modules"></param>
        /// <param name="permittedOperations"></param>
        private void SetPermittedSubModules(Guid moduleId, ref List<Module> modules, ref List<IPermissionsOperation> permittedOperations)
        {
            if (modules.SelectMany(b => b.ChildComponents).Where(x => x.Id == moduleId).FirstOrDefault() is Module moduleObj)
            {
                moduleObj.IsPermitted = true;
                if (moduleObj.Operations.Count > 0)
                {
                    foreach (Operation operation in moduleObj.Operations)
                    {
                        var operationObj = permittedOperations.FirstOrDefault(x => x.OperationId == operation.Id);
                        if (operationObj != null)
                        {
                            operation.IsPermitted = true;
                        }
                    }
                }
            }
            else
            {
                var modulesList = modules.SelectMany(b => b.ChildComponents).ToList();
                if (modulesList.Count > 0)
                {
                    SetPermittedSubIModules(moduleId, ref modulesList, ref permittedOperations);
                }
            }
        }

        /// <summary>
        /// Private method to loop through a List of IModules
        /// </summary>
        /// <param name="moduleId"></param>
        /// <param name="modules"></param>
        /// <param name="permittedOperations"></param>
        private void SetPermittedSubIModules(Guid moduleId, ref List<IModule> modules, ref List<IPermissionsOperation> permittedOperations)
        {
            if (modules.SelectMany(b => b.ChildComponents).Where(x => x.Id == moduleId).FirstOrDefault() is Module moduleObj)
            {
                moduleObj.IsPermitted = true;
                if (moduleObj.Operations.Count > 0)
                {
                    foreach (Operation operation in moduleObj.Operations)
                    {
                        var operationObj = permittedOperations.FirstOrDefault(x => x.OperationId == operation.Id);
                        if (operationObj != null)
                        {
                            operation.IsPermitted = true;
                        }
                    }
                }
            }
            else
            {
                var modulesList = modules.SelectMany(b => b.ChildComponents).ToList() as List<IModule>;
                if (modulesList.Count > 0)
                {
                    SetPermittedSubIModules(moduleId, ref modulesList, ref permittedOperations);
                }
            }
        }

        private void SetPermittedSubOperations(Guid operationId, ref List<IOperation> operations, ref List<IPermissionsOperation> permittedOperations)
        {
            foreach (Operation operation in operations)
            {
                var operationObj = permittedOperations.FirstOrDefault(x => x.OperationId == operation.Id);
                if (operationObj != null)
                {
                    operation.IsPermitted = true; //If the permitted OperationId is found in the list of all the Operations, we set its "IsPermitted" property to True 
                }
                else
                {
                    if (operation.ChildOperations != null && operation.ChildOperations.Count > 0)
                    {
                        var operationsList = operation.ChildOperations;
                        SetPermittedSubOperations(operation.Id, ref operationsList, ref permittedOperations);
                    }
                }

                if (operation.ChildOperations != null && operation.ChildOperations.Count > 0)
                {
                    var operationsList = operation.ChildOperations;
                    SetPermittedSubOperations(operation.Id, ref operationsList, ref permittedOperations);
                }
            }
        }
    }
}