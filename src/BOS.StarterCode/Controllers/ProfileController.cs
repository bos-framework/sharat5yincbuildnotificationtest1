using BOS.Auth.Client;
using BOS.StarterCode.Helpers;
using BOS.StarterCode.Models;
using BOS.StarterCode.Models.BOSModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BOS.StarterCode.Controllers
{
    /// <summary>
    /// This controller has all the endpoints used for managing one's profile
    /// </summary>
    [Authorize(Policy = "IsAuthenticated")]
    public class ProfileController : Controller
    {
        private readonly IAuthClient _bosAuthClient;

        private Logger Logger;

        public ProfileController(IAuthClient authClient)
        {
            _bosAuthClient = authClient;
            Logger = new Logger();
        }

        /// <summary>
        /// Author: BOS Framework, Inc
        /// Description: Returns the view where the user can view and update his profile
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> Index()
        {
            //Returns view with data that is required to render the page which includes demographics, username and ability to change password
            return View(await GetPageData());
        }

        /// <summary>
        /// Author: BOS Framework, Inc
        /// Description: Trggers when the "Change Password" button is clicked
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> ChangePassword()
        {
            //Returns the "ChangePassword" view that contains the form to change users password, together with all the other data required to render the page
            return View("ChangePassword", await GetPageData());
        }

        /// <summary>
        /// Author: BOS Framework, Inc
        /// Description: Triggers when the user enters the new password to be saved in the database
        /// </summary>
        /// <param name="passwordObj"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<string> UpdatePassword([FromBody] JObject data)
        {
            try
            {
                if (data != null && data["PasswordObj"] != null) //Checks for non-null data and passwordObj
                {
                    ChangePassword passwordObj = data["PasswordObj"].ToObject<ChangePassword>(); //Converts to PasswordObj data to ChangePassword model object

                    if (passwordObj.CurrentPassword != null && passwordObj.NewPassword != null)
                    {
                        string userId = string.Empty;
                        //Checking for non-null claims object
                        if (User != null)
                        {
                            userId = User.FindFirst(c => c.Type == "UserId")?.Value.ToString(); //Getting the userId from the claims object
                        }

                        if (!string.IsNullOrEmpty(userId)) //If non-null userId, then proceed to change password
                        {
                            var response = await _bosAuthClient.ChangePasswordAsync(Guid.Parse(userId), passwordObj.CurrentPassword, passwordObj.NewPassword); //Making a BOS API call to change the password. This looks for current password and the updated password with the userId
                            if (response != null && response.IsSuccessStatusCode)
                            {
                                return "Password updated successfully"; //Returning the success message
                            }
                            else
                            {
                                return response.BOSErrors[0].Message;
                            }
                        }
                        else
                        {
                            return "Your session seems to have expired. Please login again";
                        }
                    }
                    else
                    {
                        return "Password(s) cannot be null";
                    }
                }
                else
                {
                    return "Data sent was inaccurate. Please try again.";
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("Profile", "UpdatePassword", ex);
                return ex.Message;
            }
        }

        /// <summary>
        /// Author: BOS Framework, Inc
        /// Description: Triggered when the user sends across the information to be updated in the database
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<string> UpdateProfileInfo([FromBody]JObject data)
        {
            try
            {
                if (data != null) //Checking for non-null data
                {
                    if (User != null) //Checking for claims
                    {
                        //Preparing the user object. Update requires all the data - to be updated and the original ones to be sent, else they will be set to null
                        User user = new User
                        {
                            Id = Guid.Parse(User.FindFirst(c => c.Type == "UserId")?.Value.ToString()),
                            CreatedOn = DateTime.UtcNow,
                            Deleted = false,
                            Email = data["Email"]?.ToString(),
                            FirstName = data["FirstName"]?.ToString(),
                            LastModifiedOn = DateTime.UtcNow,
                            LastName = data["LastName"]?.ToString(),
                            Username = User.FindFirst(c => c.Type == "Username")?.Value.ToString(),
                            Active = true,
                            EmailConfirmed = true
                        };

                        var extendUserResponse = await _bosAuthClient.ExtendUserAsync(user); //Making a call to the BOS API to update the user's information
                        if (extendUserResponse != null && extendUserResponse.IsSuccessStatusCode)
                        {
                            await UpdateClaims("UpdateInformation", user);
                            return "Your inforamtion has been updated successfully";
                        }
                        else
                        {
                            return extendUserResponse.BOSErrors[0].Message;
                        }
                    }
                    else
                    {
                        Logger.LogException("ProfileController", "UpdateProfileInfo", new Exception("Claims is null"));
                        return "Unable to perform this action at this time. Please try again";
                    }
                }
                else
                {
                    return "Data sent was inaccurate. Please try again.";
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("Profile", "UpdatePassword", ex);
                return ex.Message;
            }
        }

        /// <summary>
        /// Author: BOS Framework, Inc
        /// Description: Is triggered when the user clicks "Update Username" after entering the new username
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<string> UpdateUsername([FromBody]string username)
        {
            try
            {
                //Verifying that the username is not empty or null
                if (!string.IsNullOrEmpty(username))
                {

                    //Checking for non-null Claims object
                    if (User != null && User.FindFirst(c => c.Type == "Username") != null)
                    {
                        //If the updated username is same as the previously set username, we do not make the API call and instead show appropriate message to the user
                        if (!User.FindFirst(c => c.Type == "Username").Value.ToString().Equals(username))
                        {
                            string userId = User.FindFirst(c => c.Type == "UserId")?.Value.ToString(); //Getting the userId

                            var updatedUsernameResponse = await _bosAuthClient.UpdateUsernameAsync(Guid.Parse(userId), username); //Making the BOS API call to update Username
                            if (updatedUsernameResponse.IsSuccessStatusCode)
                            {
                                await UpdateClaims("UpdateUsername", new User() { Username = username });
                                return "Username updated successfully"; //Returing the success message
                            }
                            else
                            {
                                return updatedUsernameResponse.BOSErrors[0].Message;
                            }
                        }
                        else
                        {
                            return "No change to the username"; //When the updated username is equal to the current username
                        }
                    }
                    else
                    {
                        Logger.LogException("ProfileController", "UpdateUsername", new Exception("Claims is null"));
                        return "Unable to perform this action at this time. Please try again";
                    }
                }
                else
                {
                    return "Username cannot be empty.";
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("Auth", "RegisterUser", ex);
                return ex.Message;
            }
        }

        /// <summary>
        /// Author: BOS Framework, Inc
        /// Description: Fetches the data that is required for rendering on the page 
        /// </summary>
        /// <returns></returns>
        private async Task<dynamic> GetPageData()
        {
            try
            {
                //Checking if Session is enabled and is non-null
                if (HttpContext != null && HttpContext.Session != null)
                {
                    var moduleOperations = HttpContext.Session.GetObject<List<Module>>("ModuleOperations");  //This is the list of permitted modules and operations to the logged-in user
                    Guid currentModuleId = new Guid(); // A new variable to save the current or selected module Id. This is being used, especially in the custom modules → more for the UI purposes
                    try
                    {
                        currentModuleId = moduleOperations.Where(i => i.Code == "MYPFL").Select(i => i.Id).ToList()[0]; // Selecting the module ID for MyProfile 
                    }
                    catch
                    {
                        currentModuleId = Guid.Empty;
                    }

                    //Fetching the allowed operations in this module for the given user
                    string operationsString = string.Empty;
                    var operationsList = moduleOperations.Where(i => i.Id == currentModuleId).Select(i => i.Operations).ToList();
                    if (operationsList.Count > 0)
                    {
                        var currentOperations = operationsList[0];
                        operationsString = String.Join(",", currentOperations.Select(i => i.Code));
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

                        string userId = User.FindFirst(c => c.Type == "UserId")?.Value.ToString();
                        var userInfo = await _bosAuthClient.GetUserByIdWithRolesAsync<User>(Guid.Parse(userId));
                        if (userInfo.IsSuccessStatusCode)
                        {
                            model.UserInfo = userInfo.User; //Getting the user's information that needs to be displayed on the screen
                        }
                    }

                    //CASE: If you were ever to enable setting of Roles in My Profile, this section of code will be used. Updating the logic in the Razor view is also important
                    //var availableRoles = await _bosAuthClient.GetRolesAsync<Role>(); //
                    //if (availableRoles.IsSuccessStatusCode)
                    //{
                    //    model.AvailableRoles = availableRoles.Roles;
                    //}

                    return model; //Returning the model with data required to render the page
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("Profile", "GetPageData", ex);
                return null;
            }
        }

        private async Task UpdateClaims(string action, User user)
        {
            var claims = new List<Claim>();
            switch (action)
            {
                case "UpdateUsername":
                    //Getting information from the current Cliams object
                    string email = User.FindFirst(c => c.Type == "Email")?.Value.ToString();
                    string initials = User.FindFirst(c => c.Type == "Initials")?.Value.ToString();
                    string name = User.FindFirst(c => c.Type == "Name")?.Value.ToString();
                    string role = User.FindFirst(c => c.Type == "Role")?.Value.ToString();
                    string userId = User.FindFirst(c => c.Type == "UserId")?.Value.ToString();

                    //Create Claims Identity. Saving all the information in the Claims object
                    claims = new List<Claim>{ new Claim("CreatedOn", DateTime.UtcNow.ToString()),
                                              new Claim("Email", email),
                                              new Claim("Initials", initials),
                                              new Claim("Name", name),
                                              new Claim("Role", role),
                                              new Claim("UserId", userId),
                                              new Claim("Username", user.Username), //This is the only value that needs to be updated
                                              new Claim("IsAuthenticated", "True")
                                            };

                    break;
                case "UpdateInformation":

                    string roleString = User.FindFirst(c => c.Type == "Role")?.Value.ToString();
                    
                    //Create Claims Identity. Saving all the information in the Claims object
                    claims = new List<Claim>{ new Claim("CreatedOn", DateTime.UtcNow.ToString()),
                                              new Claim("Email", user.Email),
                                              new Claim("Initials", user.FirstName[0].ToString() + user.LastName[0].ToString()),
                                              new Claim("Name", user.FirstName +" " + user.LastName),
                                              new Claim("Role", roleString),
                                              new Claim("UserId", user.Id.ToString()),
                                              new Claim("Username", user.Username.ToString()),
                                              new Claim("IsAuthenticated", "True")
                                            };
                    break;
            }
            var userIdentity = new ClaimsIdentity(claims, "Auth");
            ClaimsPrincipal principal = new ClaimsPrincipal(userIdentity);

            //Updating the claims object
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, new AuthenticationProperties
            {
                ExpiresUtc = DateTime.UtcNow.AddMinutes(3000),
                IsPersistent = false,
                AllowRefresh = false
            });
        }
    }
}