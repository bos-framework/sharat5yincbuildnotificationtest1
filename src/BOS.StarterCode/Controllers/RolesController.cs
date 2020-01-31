using BOS.Auth.Client;
using BOS.IA.Client.ClientModels;
using BOS.StarterCode.Helpers;
using BOS.StarterCode.Models.BOSModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BOS.StarterCode.Controllers
{
    /// <summary>
    /// The Roles Controller handles all the endpoints that deal with Role Management
    /// </summary>
    [Authorize(Policy = "IsAuthenticated")]
    public class RolesController : Controller
    {
        private readonly IAuthClient _bosAuthClient;

        private Logger Logger;

        public RolesController(IAuthClient authClient)
        {
            _bosAuthClient = authClient;

            Logger = new Logger();
        }

        /// <summary>
        /// Author: BOS Framework, Inc
        /// Description: Lists all the roles available in the application
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> Index()
        {
            return View(await GetPageData()); //Returns to the View with data that is required to render the page
        }

        /// <summary>
        /// Author: BOS Framework, Inc
        /// Description: Triggers when the "Add Role" button is clicked
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> AddNewRole()
        {
            return View("AddRole", await GetPageData()); //Returns to the "AddRole" view, that has the form to add a new role, togethwer with data that is required to render the page
        }

        /// <summary>
        /// Author: BOS Framework, Inc
        /// Description: Triggered when the "Edit Role" button is clicked
        /// </summary>
        /// <param name="roleId"></param>
        /// <returns></returns>
        public async Task<IActionResult> EditRole(string roleId)
        {
            try
            {
                if (!string.IsNullOrEmpty(roleId)) //Looking for a non-null roleId
                {
                    var roleInfo = await _bosAuthClient.GetRoleByIdAsync<Role>(Guid.Parse(roleId)); //Making a call to the BOS API to get data by the roleId
                    if (roleInfo != null && roleInfo.IsSuccessStatusCode)
                    {
                        dynamic model = await GetPageData(); //Getting the data that is required to load on the page
                        if (model == null)
                        {
                            model = new ExpandoObject(); //If null is returned from the method, then re-initate it as a dynamic object
                        }
                        model.RoleInfo = roleInfo.Role; //On successful API call, converting the response object to a "Role" object
                        return View("EditRole", model); //Returing to "EditRole" view with the data that is required to render the page
                    }
                    else
                    {
                        //If something goes wrong with the API, then we show the error returned by the API and navigate the user to the ErrorPage
                        dynamic model = new ExpandoObject();
                        model.Message = roleInfo.BOSErrors[0].Message;
                        model.StackTrace = roleInfo.BOSErrors[0].Message;
                        return View("ErrorPage", model);
                    }
                }
                else
                {
                    ModelState.AddModelError("CustomError", "The selected role has incorrect Id. Please try again");
                    return View("Index", await GetPageData());
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("Roles", "EditRole", ex);

                dynamic model = new ExpandoObject();
                model.Message = ex.Message;
                model.StackTrace = ex.StackTrace;
                return View("ErrorPage", model);
            }
        }

        /// <summary>
        /// Author: BOS Framework, Inc
        /// Description: Triggered when the "Manage Permissions" button is clicked. This naviogates the user to the Permissions Controller
        /// </summary>
        /// <param name="roleId"></param>
        /// <param name="roleName"></param>
        /// <returns></returns>
        public ActionResult RoleManagePermissions(string roleId, string roleName)
        {
            //Redirecting the to the  Permissions Controller with the roleId and roleName as input parameters. The endpoint then, fetches the permission set for the roleId and paints it on the page
            return RedirectToAction("FetchPermissions", "Permissions", new { roleId, roleName });
        }

        /// <summary>
        /// Author: BOS Framework, Inc
        /// Description: Triggers when the data for the new role is entered to be saved into the database
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<string> AddRole([FromBody]JObject data)
        {
            try
            {
                if (data != null) //Ensuring a non-null data
                {
                    /*---------LOGIC---------
                     * Verify that the input is not null
                     * Validate that the role Name is not empty
                     * Also verify that the name does not contain any special characters
                     * Make the BOS API call to AddRole
                     */

                    Role role = data["Role"]?.ToObject<Role>();
                    if (role != null) //Checking for a non-null Role object
                    {
                        if (!string.IsNullOrEmpty(role.Name)) //Confirming that the Name is non null
                        {
                            var regexItem = new Regex("^[a-zA-Z0-9 ]*$"); //Regular Expression to contain only alpha numeric characters, withour special characters
                            if (!regexItem.IsMatch(role.Name))
                            {
                                return ("Name cannot be contain special characters"); //If special characters are found, return an error message
                            }
                        }
                        else
                        {
                            return ("Name cannot be empty");//If name is empty, return an error message
                        }

                        var response = await _bosAuthClient.AddRoleAsync<Role>(role); //Making the BOS API call to Add a new role
                        if (response != null && response.IsSuccessStatusCode)
                        {
                            return ("The role was added successfully"); //On sucess, returning appropriate message
                        }
                        else
                        {
                            //Returns a message unsuccessful attempt at creating a new role. One of the cases could be if a role with same name already exists
                            return (response.BOSErrors[0].Message);
                        }
                    }
                    else
                    {
                        return ("The data for role is inaccurate. Please try again.");
                    }
                }
                else
                {
                    return ("Data cannot be null. Please try again.");
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("Roles", "AddRole", ex);
                return ex.Message;
            }
        }

        /// <summary>
        /// Author: BOS Framework, Inc
        /// Description: Triggers when the updated data for new role is entered to be saved into the database
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<string> UpdateRole([FromBody] JObject data)
        {
            try
            {
                if (data != null) //Checking for non-null input data
                {
                    Role role = data["Role"]?.ToObject<Role>(); //Converting the data into a Role object
                    if (role != null && role.Id != Guid.Empty) //Confirming non-null role object
                    {
                        var response = await _bosAuthClient.UpdateRoleAsync<Role>(role.Id, role); //Making the API call to BOS to add a new Role
                        if (response != null && response.IsSuccessStatusCode)
                        {
                            return "Role has been updated successfully"; //On success, returns appropriate message
                        }
                        else
                        {
                            return response.BOSErrors[0].Message; //Else, returns the BOS Error Message. One of the cases could be an existing role with the "updated" role name
                        }
                    }
                    else
                    {
                        return "The input data was inaccurate. Please try again.";
                    }
                }
                else
                {
                    return ("Data cannot be null. Please try again.");
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("Roles", "UpdateRole", ex);
                return ex.Message;
            }
        }

        /// <summary>
        /// Author: BOS Framework, Inc
        /// Description: Triggered from the Users Controller when a user is updting his own role is being updated
        /// </summary>
        /// <param name="updatedRoles"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<string> UpdateUserRoles([FromBody]List<Role> updatedRoles)
        {
            try
            {
                if (updatedRoles != null && updatedRoles.Count > 0) //Confirming that at least one role has been assigned to the user
                {
                    Guid userId = Guid.Empty;

                    if (User != null && User.FindFirst(c => c.Type == "UserId") != null) //Checking for non-null Claims object and within that userId
                    {
                        userId = Guid.Parse(User.FindFirst(c => c.Type == "UserId").Value.ToString());
                    }

                    var response = await _bosAuthClient.AssociateUserToMultipleRolesAsync(userId, updatedRoles); //Making a BOS API call to add/ update user's role
                    if (response.IsSuccessStatusCode)
                    {
                        return "User roles updated successfully"; //On success, returning a message
                    }
                    else
                    {
                        return response.BOSErrors[0].Message; //Else, returning the BOS error message. 
                    }
                }
                else
                {
                    return "Roles to associate with the user cannot be empty"; //Returning the message when the user is not associated with any role
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("Roles", "UpdateRole", ex);
                return ex.Message;
            }
        }

        /// <summary>
        /// Author: BOS Framework, Inc
        /// Description: Triggered from the Users Controller when a user's role is being updated by the Admin 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<string> UpdateUserRolesByAdmin([FromBody]JObject data)
        {
            try
            {
                if (data != null && data["UpdatedRoles"] != null) //Checking for non-null input data and roles list 
                {
                    List<Role> updatedRoles = data["UpdatedRoles"].ToObject<List<Role>>(); //Converting the data to a list of roles
                    Guid userId = Guid.Empty;
                    if (data["UserId"] != null)
                    {
                        var updatedUserId = data["UserId"].ToString(); //The userId sent to the View is in an encrypted format. So, we will have to decrypt it before sending it to the BOS API
                        StringConversion stringConversion = new StringConversion();
                        userId = Guid.Parse(stringConversion.DecryptString(updatedUserId));
                    }

                    if (updatedRoles.Count > 0) //Confirming that there is at least one role in the list
                    {
                        if (userId != Guid.Empty)
                        {
                            var response = await _bosAuthClient.AssociateUserToMultipleRolesAsync(userId, updatedRoles); //Making an API call to BOS to associate user with the roles
                            if (response != null && response.IsSuccessStatusCode)
                            {
                                return "User's roles updates successfully"; //On success, returing appropriate message
                            }
                            else
                            {
                                return response.BOSErrors[0].Message; //Else, return BOS error message
                            }
                        }
                        else
                        {
                            return "Incorrect user id";
                        }
                    }
                    else
                    {
                        return "Roles to associate with the user cannot be empty";
                    }
                }
                else
                {
                    return "Roles to associate with the user cannot be empty";
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("Roles", "UpdateRole", ex);
                return ex.Message;
            }
        }

        /// <summary>
        /// Author: BOS Framework, Inc
        /// Description: Triggers when a role is seleted to be deleted, after confirmation on the UI
        /// </summary>
        /// <param name="roleId"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<string> DeleteRole([FromBody]string roleId)
        {
            try
            {
                if (!string.IsNullOrEmpty(roleId)) //Checking for a non-null, non-empty roleId
                {
                    var response = await _bosAuthClient.DeleteRoleAsync(Guid.Parse(roleId)); //Making BOS API call to delete the role by Id
                    if (response.IsSuccessStatusCode)
                    {
                        return "Role deleted successfully"; //On success, returing the message

                    }
                    else
                    {
                        return response.BOSErrors[0].Message; //Else, returning BOS error message. An example coulb be that there is no role with the id
                    }
                }
                else
                {
                    return "The selected role has an inaccurate Id. Please check and try again.";
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("Roles", "DeleteRole", ex);
                return ex.Message;
            }
        }

        /// <summary>
        /// Author: BOS Framework, Inc
        /// Description: Fetches data necessary for rendering on the page
        /// </summary>
        /// <returns></returns>
        private async Task<dynamic> GetPageData()
        {
            try
            {
                //Checking if Sessions are enabled and non-null
                if (HttpContext != null && HttpContext.Session != null)
                {
                    var moduleOperations = HttpContext.Session.GetObject<List<Module>>("ModuleOperations"); //This is the list of permitted modules and operations to the logged-in user
                    List<Guid> currentModuleIds = new List<Guid>(); //Getting the moduleIDs for both Roles and Permissions because currently Permissions is an operation under Roles
                    try
                    {
                        currentModuleIds = moduleOperations.Where(i => i.Code == "ROLES" || i.Code == "PRMNS").Select(i => i.Id).ToList();
                    }
                    catch (ArgumentNullException)
                    {
                        currentModuleIds = null;
                    }

                    string operationsString = string.Empty; //Variable used to get all the operations that are allowed for the user in the module
                    var operationsList = moduleOperations.Where(i => currentModuleIds.Contains(i.Id)).Select(i => i.Operations).ToList();  //Getting the list of permitted operations for both the modules
                    if (operationsList.Count > 0)
                    {
                        var currentOperations = operationsList[0];
                        var currentOperations1 = new List<IOperation>();
                        try
                        {
                            currentOperations1 = operationsList[1];

                        }
                        catch
                        {
                            currentOperations1 = new List<IOperation>();
                        }
                        operationsString = String.Join(",", currentOperations.Select(i => i.Code)); //Converting the list of permitted operations to a string. This will be further used in the View
                        operationsString = operationsString + "," + String.Join(",", currentOperations1.Select(i => i.Code));
                    }

                    dynamic model = new ExpandoObject(); //Preparing the dynamic object that contains all the data that is required to paint information on the page
                    model.ModuleOperations = moduleOperations;
                    model.Operations = operationsString;
                    try
                    {
                        model.CurrentModuleId = moduleOperations.Where(i => i.Code == "ROLES").Select(i => i.Id).ToList()[0];
                    }
                    catch
                    {
                        model.CurrentModuleId = null;
                    }

                    //Checking for a non-null Claims object
                    if (User != null)
                    {
                        model.Initials = User.FindFirst(c => c.Type == "Initials")?.Value.ToString();
                        model.Username = User.FindFirst(c => c.Type == "Username")?.Value.ToString();
                        model.Roles = User.FindFirst(c => c.Type == "Role")?.Value.ToString();
                    }

                    var response = await _bosAuthClient.GetRolesAsync<Role>(); //Making a BOS API call to get a list of All the roles in the application
                    if (response.IsSuccessStatusCode)
                    {
                        model.RoleList = response.Roles; //On success, assign the model with the list of all roles returned from the API
                    }
                    return model;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("Roles", "GetPageData", ex);

                return null;
            }
        }
    }
}