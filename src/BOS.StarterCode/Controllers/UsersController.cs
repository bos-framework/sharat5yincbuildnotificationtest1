using BOS.Auth.Client;
using BOS.Auth.Client.ClientModels;
using BOS.Email.Client;
using BOS.Email.Client.ClientModels;
using BOS.StarterCode.Helpers;
using BOS.StarterCode.Models.BOSModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;

namespace BOS.StarterCode.Controllers
{
    /// <summary>
    /// All the user management enpoints are in this controller
    /// </summary>
    [Authorize(Policy = "IsAuthenticated")]
    public class UsersController : Controller
    {
        private readonly IAuthClient _bosAuthClient;
        private readonly IEmailClient _bosEmailClient;
        private readonly IConfiguration _configuration;

        private Logger Logger;

        public UsersController(IAuthClient authClient, IEmailClient bosEmailClient, IConfiguration configuration)
        {
            _bosAuthClient = authClient;
            _bosEmailClient = bosEmailClient;
            _configuration = configuration;

            Logger = new Logger();
        }

        /// <summary>
        /// Author: BOS Framework, Inc
        /// Description: Returns the View that lists all the users
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> Index()
        {
            return View(await GetPageData()); //Returns to the View with data that is required to render the page
        }

        /// <summary>
        /// Author: BOS Framework, Inc
        /// Description: Is triggered when the 'Add User' button is clicked. Returns the view with the form to add a new user.
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> AddNewUser()
        {
            try
            {
                /* -------- LOGIC --------
                 * Get all the data that is required to render the page
                 * In addition, also get the list of roles that a user can be assigned with
                 * Return the view with the data
                 */
                dynamic model = await GetPageData(); //Getting the data that is required to load on the page

                var availableRoles = await _bosAuthClient.GetRolesAsync<Role>(); //Making a BOS API call to get all the roles in the application
                if (availableRoles != null && availableRoles.IsSuccessStatusCode)
                {
                    if (model == null) //If the method call above returns a null, then we re-decalre the model object as dynamic
                    {
                        model = new ExpandoObject();
                    }
                    model.AvailableRoles = availableRoles.Roles; //Sending the dynamic object with the list of roles returned from the BOS API call
                }
                return View("AddUser", model); //Returning to the "AddUser" view, that contains the form to add a new user, together with the data required to paint the page
            }
            catch (Exception ex)
            {
                Logger.LogException("Users", "AddNewUser", ex);

                dynamic model = new ExpandoObject();
                model.Message = ex.Message;
                model.StackTrace = ex.StackTrace;
                return View("ErrorPage", model);
            }
        }

        /// <summary>
        /// Author: BOS Framework, Inc
        /// Description: Is triggered when the 'Edit' link is clicked. Returns the view with the form to edit the selected user, with the information pre-filled.
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<IActionResult> EditUser(string userId)
        {
            try
            {
                /*-------- LOGIC ----------
                 * Confirm non-null userId sent as the input
                 * Decrypt the userId to get the actual userId
                 * Get the user's information and associated roles via BOS API call
                 * Get all the roles in the application
                 * Prepare the model object that is required to render the page
                 * Navigate to the "EditUsewr" view with data
                 */
                if (!string.IsNullOrEmpty(userId)) //Checking for a non-null, non-empty userId
                {
                    dynamic model = await GetPageData(); //Getting the data that is required for rendering the page
                    if (model == null)
                    {
                        model = new ExpandoObject(); //If the method returns null, then re-ininitate a dynamic object
                    }

                    StringConversion stringConversion = new StringConversion();
                    string actualUserId = stringConversion.DecryptString(userId); //The userID that is sent to the view is encrypted. Before sending it to the BOS API, we'll have to decrypt it
                    var userInfo = await _bosAuthClient.GetUserByIdWithRolesAsync<User>(Guid.Parse(actualUserId)); //Making an API call to BOS to get the user's information together with the associated roles

                    if (userInfo != null && userInfo.IsSuccessStatusCode && userInfo.User != null)
                    {
                        userInfo.User.UpdatedId = userId; //Setting rhe updated (encrypted) userID, so it can be used in the View
                        model.UserInfo = userInfo.User; //User's data is assigned to the model 

                        List<string> rolesList = new List<string>();
                        foreach (UserRole role in userInfo.User.Roles)
                        {
                            rolesList.Add(role.Role.Name);
                        }
                        model.RolesList = rolesList; //All the roles that the user is already associated with
                    }

                    var availableRoles = await _bosAuthClient.GetRolesAsync<Role>(); //Making a BOS API Call to fetch all the Roles in the application
                    if (availableRoles != null && availableRoles.IsSuccessStatusCode)
                    {
                        model.AvailableRoles = availableRoles.Roles; //On success, setting the complete Roles list 
                    }

                    return View("EditUser", model); //Returning to the "EditUser" view, that has the form to edit user's information and roles, with the data required  to render the page
                }
                else
                {
                    ModelState.AddModelError("CustomError", "The selected user has inaccurate id. Please try again.");
                    return View("Index", await GetPageData());
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("Users", "EditUser", ex);

                dynamic model = new ExpandoObject();
                model.Message = ex.Message;
                model.StackTrace = ex.StackTrace;
                return View("ErrorPage", model);
            }
        }

        /// <summary>
        /// Author: BOS Framework, Inc
        /// Description: Is triggered when the 'Save' button is clicked with the details of the new user
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<string> AddUser([FromBody] JObject data)
        {
            try
            {
                if (data != null) //Confirm non-null input data
                {
                    /*--------LOGIC----------
                     * Validate the data sent across the wire
                     * Convert it to the "User" object
                     * Create a new User record in BOS
                     * Update the user's info by making another BOS API call by ID (received as a respoonse from the previous API call)
                     * Associate roles to the user
                     * Send email with the verification link (if selected in the View)
                     *      • Generate a slug
                     *      • Get the TemplateId via BOS Email API
                     *      • Get the ServiceProviderId via BOS Email API
                     *      • Create the email object
                     *      • Make the BOS API call to send the Email
                     * Return a success message
                     */
                    User userObj = data["User"]?.ToObject<User>(); //Convert the input data into a user object 
                    List<Role> roleList = data["Roles"]?.ToObject<List<Role>>(); //Get the list of roles the user is assigned to
                    bool isEmailToSend = Convert.ToBoolean(data["IsEmailToSend"]?.ToString()); //Check if the Verification email has to be sent
                    string password = data["Password"]?.ToString();

                    if (isEmailToSend) //If Email is to be sent, then the password is the be auto-created, else, the password is to be set by the user who is creating the record
                    {
                        password = CreatePassword();
                    }
                    else
                    {
                        if (userObj != null) //Checking for a non-null userObj
                        {
                            userObj.EmailConfirmed = true;
                        }
                    }

                    //Have different level of if conditions so that the returned message is more accurate, given the fail of condition
                    if (userObj != null) //Checking for a non-null userObj
                    {
                        if (userObj.Username != null && userObj.Email != null && password != null) //Non-null values
                        {
                            if (roleList != null && roleList.Count > 0) //Non-null role List and with at least one record
                            {
                                var result = await _bosAuthClient.AddNewUserAsync<BOSUser>(userObj.Username, userObj.Email, password); //Making a BOS API call to add a new user record
                                if (result != null && result.IsSuccessStatusCode)
                                {
                                    User user = userObj;
                                    user.Id = result.User.Id; //On successful, the response's userId is taken into account

                                    var extendUserResponse = await _bosAuthClient.ExtendUserAsync(user); //Updating the user's inforamation through a BOS API call
                                    if (extendUserResponse != null && extendUserResponse.IsSuccessStatusCode)
                                    {
                                        //On successful updation of information of the user, we then update the roles
                                        var roleResponse = await _bosAuthClient.AssociateUserToMultipleRolesAsync(result.User.Id, roleList); //Making a BOS API call to associate the user with role(s)
                                        if (roleResponse != null && roleResponse.IsSuccessStatusCode)
                                        {
                                            //On success of the API call, we finally send the user an email with the verification link, if it is set to true
                                            if (isEmailToSend)
                                            {
                                                var slugResponse = await _bosAuthClient.CreateSlugAsync(userObj.Email); //Making a BOS API call to generate a slug
                                                if (slugResponse != null && slugResponse.IsSuccessStatusCode)
                                                {
                                                    var slug = slugResponse.Slug;

                                                    //Preparing the email object that's used as an input to the BOS Email API
                                                    Models.BOSModels.Email emailObj = new Models.BOSModels.Email
                                                    {
                                                        Deleted = false,
                                                        From = new From
                                                        {
                                                            Email = "startercode@bosframework.com",
                                                            Name = "StarterCode Team",
                                                        },
                                                        To = new List<To>
                                                        {
                                                            new To
                                                            {
                                                                Email = userObj.Email,
                                                                Name = userObj.FirstName + " " + userObj.LastName
                                                            }
                                                        }
                                                    };
                                                    var templateResponse = await _bosEmailClient.GetTemplateAsync<Template>(); //Making the BOS API call to get the list of all the templates
                                                    if (templateResponse != null && templateResponse.IsSuccessStatusCode)
                                                    {
                                                        //Selecting the templateID where the templatename is UserAddedBySuperAdmin
                                                        emailObj.TemplateId = templateResponse.Templates.Where(i => i.Name == "UserAddedBySuperAdmin").Select(i => i.Id).ToList()[0];
                                                    }
                                                    else
                                                    {
                                                        ModelState.AddModelError("CustomError", "Sorry! We could not send you an email. Please try again later");
                                                        return View("Index", await GetPageData());
                                                    }

                                                    var spResponse = await _bosEmailClient.GetServiceProviderAsync<ServiceProvider>(); //Making a BOS API call to get the ServiceProviderId
                                                    if (spResponse != null && spResponse.IsSuccessStatusCode)
                                                    {
                                                        emailObj.ServiceProviderId = spResponse.ServiceProvider[0].Id;
                                                    }
                                                    else
                                                    {
                                                        ModelState.AddModelError("CustomError", "Sorry! We could not send you an email. Please try again later");
                                                        return View("Index", await GetPageData());
                                                    }

                                                    //This is the list of key-value pair where the content will be replace with the 'Value' where the 'Key' matches in the content of the template 
                                                    emailObj.Substitutions = new List<Substitution>();
                                                    emailObj.Substitutions.Add(new Substitution { Key = "usersName", Value = user.FirstName + " " + user.LastName });
                                                    emailObj.Substitutions.Add(new Substitution { Key = "companyUrl", Value = _configuration["PublicUrl"] });
                                                    emailObj.Substitutions.Add(new Substitution { Key = "companyLogo", Value = _configuration["PublicUrl"] + "/images/logo.png" });
                                                    emailObj.Substitutions.Add(new Substitution { Key = "applicationName", Value = _configuration["ApplicationName"] });
                                                    emailObj.Substitutions.Add(new Substitution { Key = "applicationUrl", Value = _configuration["PublicUrl"] + "/Password/Reset?slug=" + slug.Value + "&set=true" });
                                                    emailObj.Substitutions.Add(new Substitution { Key = "emailAddress", Value = user.Email });
                                                    emailObj.Substitutions.Add(new Substitution { Key = "password", Value = "" });
                                                    emailObj.Substitutions.Add(new Substitution { Key = "thanksCredits", Value = "Team StarterCode" });

                                                    var emailResponse = await _bosEmailClient.SendEmailAsync<IEmail>(emailObj); //Making an API call to send Email
                                                    if (!emailResponse.IsSuccessStatusCode)
                                                    {
                                                        ModelState.AddModelError("CustomError", emailResponse.BOSErrors[0].Message);
                                                    }
                                                }
                                            }
                                            return "User added successfully"; //On success of all the APIs, we return an appropriate message
                                        }
                                    }
                                    return result != null ? result.BOSErrors[0].Message : "We are unable to add users at this time. Please try again.";
                                }

                                else
                                {
                                    return result != null ? result.BOSErrors[0].Message : "We are unable to add users at this time. Please try again.";
                                }
                            }
                            else
                            {
                                return "User has to be associated with at least one role";
                            }
                        }
                        else
                        {
                            return "Required data is missing. Please try again";
                        }
                    }
                    else
                    {
                        return "User data cannot be null. Please check and try again.";
                    }
                }
                else
                {
                    return "Data cannot be null. Please try again.";
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("Users", "AddUser", ex);
                return ex.Message;
            }
        }

        /// <summary>
        /// Author: BOS Framework, Inc
        /// Description: Is triggered after the confirmation on the UI to delete the selected user.
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<string> DeleteUser([FromBody]string userId)
        {
            try
            {
                if (!string.IsNullOrEmpty(userId)) //Confirming a non-null, non-empty userId
                {
                    StringConversion stringConversion = new StringConversion();
                    string actualUserId = stringConversion.DecryptString(userId); //Since the userId sent to the view is encrypted, before sending it to the BOS API, we have to decrypt it

                    var response = await _bosAuthClient.DeleteUserAsync(Guid.Parse(actualUserId)); //Making an API call to BOS to delete the user
                    if (response != null && response.IsSuccessStatusCode)
                    {
                        return "User deleted successfully"; //On success, return the message
                    }
                    else
                    {
                        return response != null ? response.BOSErrors[0].Message : "We are unable to delete this user at this time. Please try again."; //Else, return the BOS error message
                        //An example could be, if there is no user with the id
                    }
                }
                else
                {
                    return "UserId cannot be null. Please check and try again.";
                }

            }
            catch (Exception ex)
            {
                Logger.LogException("Users", "DeleteUser", ex);
                return ex.Message;
            }
        }

        /// <summary>
        /// Author: BOS Framework, Inc
        /// Description:  Is triggered when the 'Update' button is clicked with the updated details of the selected user
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<string> UpdateUserInfo([FromBody]JObject user)
        {
            try
            {
                if (user != null) //Confirm non-null input data
                {
                    StringConversion stringConversion = new StringConversion();
                    Guid myId = Guid.Parse(stringConversion.DecryptString(Convert.ToString(user["UpdatedId"]))); 
                    bool confirmed = false;
                    string emailConfirmed = Convert.ToString(user["EmailConfirmed"]);
                    if (!string.IsNullOrEmpty(emailConfirmed))
                    {
                        confirmed = true;
                    }
                    User edituser = new User { Id = myId, Active = Convert.ToBoolean(user["Active"]), Email = Convert.ToString(user["Email"]), FirstName = Convert.ToString(user["FirstName"]), LastName = Convert.ToString(user["LastName"]), Username = Convert.ToString(user["Username"]), EmailConfirmed = Convert.ToBoolean(confirmed), Deleted = false };
                    var extendUserResponse = await _bosAuthClient.ExtendUserAsync(edituser);
                    if (extendUserResponse != null && extendUserResponse.IsSuccessStatusCode)
                    {
                        return "User's information updated successfully"; //On success, returning the message
                    }
                    else
                    {
                        //Else, return the BOS error message. An example could be, if there is no user with the id
                        return extendUserResponse != null ? extendUserResponse.BOSErrors[0].Message : "We are unable to update this user's information at this time. Please try again.";
                    }
                }
                else
                {
                    return "User data cannot be null. Please try again";
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("Users", "UpdateUserInfo", ex);
                return ex.Message;
            }
        }

        /// <summary>
        /// Author: BOS Framework, Inc
        /// Description: Is triggered after the confirmation on the UI to either activate or deactivate the selected user.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<string> ChangeUserActiveStatus([FromBody]JObject data)
        {
            try
            {
                if (data != null)
                {
                    if (data["UserId"] == null)
                    {
                        return "UserId cannot be null";
                    }
                    else if (data["Action"] == null)
                    {
                        return "Action cannot be null";
                    }

                    StringConversion stringConversion = new StringConversion();
                    string actualUserId = stringConversion.DecryptString(data["UserId"]?.ToString()); //Since the userId sent to the view is encrypted, before sending it to the BOS API, we have to decrypt it

                    var action = data["Action"]?.ToString();

                    //Based on the action that has been requested, we either make a call to the BOS' ActivateUser API or DeactivateUser API
                    if (action == "activate")
                    {
                        var response = await _bosAuthClient.ActivateUserAsync(Guid.Parse(actualUserId)); //Making the BOS API call with the userId
                        if (response != null && response.IsSuccessStatusCode)
                        {
                            return "The user has been activated successfully"; //On success, returning an appropriate message
                        }
                        else
                        {
                            return response.BOSErrors[0].Message; //On error, returing the BOS error message
                        }
                    }
                    else if (action == "deactivate")
                    {
                        var response = await _bosAuthClient.DeactivateUserAsync(Guid.Parse(actualUserId));  //Making the BOS API call with the userId
                        if (response != null && response.IsSuccessStatusCode)
                        {
                            return "The user has been deactivated successfully"; //On success, returning an appropriate message
                        }
                        else
                        {
                            return response.BOSErrors[0].Message; //On error, returing the BOS error message
                        }
                    }
                    else
                    {
                        return "You are trying to perform an unrecognized operation";
                    }
                }
                else
                {
                    return "Data cannot be null";
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("Users", "ChangeUserActiveStatus", ex);
                return ex.Message;
            }
        }

        /// <summary>
        /// Author: BOS Framework, Inc
        /// Description: Private method to fetch the data necessary to render the page
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
                    Guid currentModuleId = new Guid(); // A new variable to save the current or selected module Id. This is being used, especially in the custom modules → more for the UI purposes
                    try
                    {
                        currentModuleId = moduleOperations.Where(i => i.Code == "USERS").Select(i => i.Id).ToList()[0]; //Selecting the moduledD for MyProfile 
                    }
                    catch
                    {
                        currentModuleId = Guid.Empty;
                    }

                    var operationsList = moduleOperations.Where(i => i.Id == currentModuleId).Select(i => i.Operations).ToList(); //Fetching the allowed operations in this module for the given user

                    string operationsString = string.Empty;
                    if (operationsList.Count > 0)
                    {
                        var currentOperations = operationsList[0];
                        operationsString = String.Join(",", currentOperations.Select(i => i.Code)); //Converting the list of operations to a string, so it can be used in the View
                    }

                    //Preparing the dynamic object that has data used for rendering the page
                    dynamic model = new ExpandoObject();
                    model.ModuleOperations = moduleOperations;
                    model.Operations = operationsString;
                    model.CurrentModuleId = currentModuleId;

                    //Checking for non-null claims object
                    if (User != null)
                    {
                        model.Initials = User.FindFirst(c => c.Type == "Initials")?.Value.ToString();
                        model.Username = User.FindFirst(c => c.Type == "Username")?.Value.ToString();
                        model.Roles = User.FindFirst(c => c.Type == "Role")?.Value.ToString();
                    }

                    StringConversion stringConversion = new StringConversion();
                    var userList = await _bosAuthClient.GetUsersWithRolesAsync<User>(); //Getting the list of all the users in the application using the BOS API
                    if (userList != null && userList.IsSuccessStatusCode)
                    {
                        var updatedUserList = userList.Users.Select(c => { c.UpdatedId = stringConversion.EncryptString(c.Id.ToString()); return c; }).ToList(); //Updating the user object with the encrypted userid, which will be used in the View and for passing into APIs from the view to the controller
                        model.UserList = updatedUserList;
                    }
                    return model; //Returning the mode with all the data that is required to render the page
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("Users", "GetPageData", ex);
                return null;
            }
        }

        /// <summary>
        /// Author: BOS Framework, Inc
        /// Description: Private method to generate password
        /// </summary>
        /// <returns></returns>
        private string CreatePassword()
        {
            //A private method to generate random passwords. This uses MS .Net Core's Identity reference
            PasswordOptions opts = new PasswordOptions()
            {
                RequiredLength = 10,
                RequiredUniqueChars = 4,
                RequireDigit = true,
                RequireLowercase = true,
                RequireNonAlphanumeric = true,
                RequireUppercase = true
            };

            string[] randomChars = new[] {
                "ABCDEFGHJKLMNOPQRSTUVWXYZ",    // uppercase 
                "abcdefghijkmnopqrstuvwxyz",    // lowercase
                "0123456789",                   // digits
                "!@$?_-"                        // non-alphanumeric
            };
            Random rand = new Random(Environment.TickCount);
            List<char> chars = new List<char>();

            if (opts.RequireUppercase)
            {
                chars.Insert(rand.Next(0, chars.Count),
                    randomChars[0][rand.Next(0, randomChars[0].Length)]);
            }

            if (opts.RequireLowercase)
            {
                chars.Insert(rand.Next(0, chars.Count),
                    randomChars[1][rand.Next(0, randomChars[1].Length)]);
            }

            if (opts.RequireDigit)
            {
                chars.Insert(rand.Next(0, chars.Count),
                    randomChars[2][rand.Next(0, randomChars[2].Length)]);
            }

            if (opts.RequireNonAlphanumeric)
            {
                chars.Insert(rand.Next(0, chars.Count),
                    randomChars[3][rand.Next(0, randomChars[3].Length)]);
            }

            for (int i = chars.Count; i < opts.RequiredLength
                || chars.Distinct().Count() < opts.RequiredUniqueChars; i++)
            {
                string rcs = randomChars[rand.Next(0, randomChars.Length)];
                chars.Insert(rand.Next(0, chars.Count),
                    rcs[rand.Next(0, rcs.Length)]);
            }

            return new string(chars.ToArray());
        }
    }
}

