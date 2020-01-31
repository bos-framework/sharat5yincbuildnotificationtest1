using BOS.Auth.Client;
using BOS.StarterCode.Helpers;
using BOS.StarterCode.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Dynamic;
using System.Threading.Tasks;

namespace BOS.StarterCode.Controllers
{
    /// <summary>
    /// Controller used for setting the password on registration and resetting on forgot password
    /// </summary>
    public class PasswordController : Controller
    {
        private readonly IAuthClient _bosAuthClient;

        private Logger Logger;

        public PasswordController(IAuthClient authClient)
        {
            _bosAuthClient = authClient;
            Logger = new Logger();
        }

        /// <summary>
        /// Author: BOS Framework, Inc
        /// Description: Is triggered when the user gets to the Verification Link. If the slug is valid it shows the view to set/ reset the password, else just shows a message
        /// </summary>
        /// <param name="slug"></param>
        /// <returns></returns>
        public async Task<IActionResult> Reset(string slug, string set)
        {
            try
            {
                //Checking for a non-null slug
                if (!string.IsNullOrWhiteSpace(slug))
                {
                    //Making an API call to verify if the Slug is still active and valid. 
                    var result = await _bosAuthClient.VerifySlugAsync(slug);

                    if (result != null && result.IsSuccessStatusCode)
                    {
                        Guid userId = result.UserId;
                        ViewBag.UserId = userId;
                        ViewBag.Set = set;

                       await _bosAuthClient.ConfirmUserEmailAddress(userId); //Making an API call to BOS to set the value true for email confirmation 

                    }
                    else
                    {
                        //Display this message when the slug being used is either inactive or has expired. The BOS default expiration for a slug is 48 hours since the time of its generation
                        ViewBag.Message = "The link has either expired or is invalid. If you have just registered, then get in touch with your admistrator for a new password. If you have forgotten your password, retry again in some time.";
                    }

                    return View("ResetPassword"); //Returning the ResetPassword View - where the end-user can set/ reset the password. The View then displays the form only if the slug is valid
                }
                else
                {
                    //Returning an error message when the slug is null
                    dynamic model = new ExpandoObject();
                    model.Message = "The slug string cannot be empty or null.";
                    model.StackTrace = "";
                    return View("ErrorPage", model);
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("Password", "Reset", ex);

                dynamic model = new ExpandoObject();
                model.Message = ex.Message;
                model.StackTrace = ex.StackTrace;
                return View("ErrorPage", model);
            }
        }

        /// <summary>
        /// Author: BOS Framework, Inc
        /// Description: Sets the password of the user, either after registration or forgot password operation is performed. 
        /// </summary>
        /// <param name="password"></param>
        /// <returns></returns>
        public async Task<IActionResult> ResetPassword(ChangePassword password)
        {
            try
            {
                if (password != null)
                {
                    string userId = password.UserId; //Getting the userId for whom the password needs to be reset
                    string newPassword = !string.IsNullOrWhiteSpace(password.NewPassword) ? password.NewPassword : null; //Checking for non-empty, non-null password string
                    if (newPassword != null)
                    {
                        var response = await _bosAuthClient.ForcePasswordChangeAsync(Guid.Parse(userId), newPassword); //Making the call to BOS API to change the password. Using the ForcePasswordChange method because in this scenario, there is no record of the current password
                        if (response != null && response.IsSuccessStatusCode)
                        {
                            ViewBag.SuccessMessage = "Password reset successfully";
                            return View();
                        }
                        else
                        {
                            //If not success, passing the error message from BOS to the Error Page for the user to know
                            dynamic model = new ExpandoObject();
                            model.Message = response != null ? response.BOSErrors[0].Message : "Unable to update the password at this time. Please try again later.";
                            model.StackTrace = "The password object that was provided was null";
                            return View("ErrorPage", model);
                        }
                    }
                    else
                    {
                        dynamic model = new ExpandoObject();
                        model.Message = "Passwords cannot be null";
                        model.StackTrace = "The updated password is either null or empty";
                        return View("ErrorPage", model);
                    }
                }
                else
                {
                    dynamic model = new ExpandoObject();
                    model.Message = "Passwords cannot be null";
                    model.StackTrace = "The password object that was provided was null";
                    return View("ErrorPage", model);
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("Password", "ResetPassword", ex);

                dynamic model = new ExpandoObject();
                model.Message = ex.Message;
                model.StackTrace = ex.StackTrace;
                return View("ErrorPage", model);
            }
        }

        /// <summary>
        /// Author: BOS Framework, Inc
        /// Description: Navigates the user back to the Login Screen
        /// </summary>
        /// <returns></returns>
        public IActionResult GotBackToLogin()
        {
            return RedirectToAction("Index", "Auth"); //Redirecting the user back to the Login Page
        }
    }
}