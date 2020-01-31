using BOS.Auth.Client;
using BOS.Email.Client;
using BOS.IA.Client;
using BOS.StarterCode.Controllers;
using BOS.StarterCode.Helpers;
using BOS.StarterCode.Models;
using BOS.StarterCode.Tests.HelperClass;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace BOS.StarterCode.Tests
{
    public class AuthControllerTests
    {
        private readonly IAuthClient _bosAuthClient;
        private readonly IIAClient _bosIAClient;
        private readonly IEmailClient _bosEmailClient;
        private readonly IConfiguration _configuration;

        public AuthControllerTests()
        {
            var config = new ConfigurationBuilder().AddJsonFile("appsettings.test.json").Build();
            var bosAPIkey = config["BOS:APIkey"];
            string baseURL = config["BOS:ServiceBaseURL"];

            HttpClient httpClientAuth = new HttpClient();
            httpClientAuth.BaseAddress = new Uri(baseURL + config["BOS:AuthRelativeURL"]);
            httpClientAuth.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", bosAPIkey);
            AuthClient authClient = new AuthClient(httpClientAuth);
            _bosAuthClient = authClient;

            HttpClient httpClientIA = new HttpClient();
            httpClientIA.BaseAddress = new Uri(baseURL + config["BOS:IARelativeURL"]);
            httpClientIA.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", bosAPIkey);
            IAClient iaClient = new IAClient(httpClientIA);
            _bosIAClient = iaClient;

            HttpClient httpClientEmail = new HttpClient();
            httpClientEmail.BaseAddress = new Uri(baseURL + config["BOS:EmailRelativeURL"]);
            httpClientEmail.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", bosAPIkey);
            EmailClient emailClient = new EmailClient(httpClientEmail);
            _bosEmailClient = emailClient;

            _configuration = null;
        }

        [Fact]
        public void Index_returns_login_view_when_not_authenticated()
        {
            //Arrange
            var controller = new AuthController(_bosAuthClient, _bosIAClient, _bosEmailClient, _configuration);

            //Act
            var result = controller.Index();

            //Assert
            var viewResult = Assert.IsType<ViewResult>(result); //Asserting that the return is a View
            Assert.Equal("Index", viewResult.ViewName); //Asserting that the returned view is "Index"
        }

        [Fact]
        public void Index_redirects_to_dashboard_view_when_authenticated()
        {
            //Arrange 
            //Mocking the user claims
            var claims = new List<Claim>{
                new Claim("CreatedOn", DateTime.UtcNow.ToString()),
                new Claim("Email", "some@email.com"),
                new Claim("Initials", "JD"),
                new Claim("Name", "John Doe"),
                new Claim("Role", "Admin"),
                new Claim("UserId", Guid.NewGuid().ToString()),
                new Claim("Username", "SomeUserName"),
                new Claim("IsAuthenticated", "True")
            };
            var userIdentity = new ClaimsIdentity(claims, "Auth");
            ClaimsPrincipal principal = new ClaimsPrincipal(userIdentity);

            var controller = new AuthController(_bosAuthClient, _bosIAClient, _bosEmailClient, _configuration)
            {
                ControllerContext = new ControllerContext()
                {
                    HttpContext = new DefaultHttpContext() { User = principal }
                }
            };

            //Act
            var result = controller.Index();

            //Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result); //Asserting that the return is a View
            Assert.Equal("Dashboard", redirectResult.ControllerName); //Asserting that the returned Controller is "Dashboard"
            Assert.Equal("Index", redirectResult.ActionName); //Asserting that the Action Methond of the controller is "Index"
        }

        [Fact]
        public async Task AuthenticateUser_without_cookie_consent_returns_message()
        {
            //Arrange 
            var controller = new AuthController(_bosAuthClient, _bosIAClient, _bosEmailClient, _configuration)
            {
                ControllerContext = new ControllerContext()
                {
                    HttpContext = new DefaultHttpContext()
                }
            };

            AuthModel authModel = new AuthModel();

            //Act
            var result = await controller.AuthenticateUser(authModel);

            //Assert
            var viewResult = Assert.IsType<ViewResult>(result); //Asserting that the return is a View
            Assert.Equal("Index", viewResult.ViewName); //Asserting that the returned Controller is "Index"
            Assert.True(controller.ViewData.ModelState.Count == 1); //Asserts that there is a ModelStateError object
        }

        [Fact]
        public async Task AuthenticateUser_null_authobj_returns_index_view_with_modelstate_error()
        {
            //Arrange 
            var controller = new AuthController(_bosAuthClient, _bosIAClient, _bosEmailClient, _configuration)
            {
                ControllerContext = new ControllerContext()
                {
                    HttpContext = new DefaultHttpContext()
                }
            };
            controller.ControllerContext.HttpContext.Request.Headers.Add("Cookie", new CookieHeaderValue(".AspNet.Consent", "true").ToString());

            AuthModel authModel = null;

            //Act
            var result = await controller.AuthenticateUser(authModel);

            //Assert
            var viewResult = Assert.IsType<ViewResult>(result); //Asserting that the return is a View
            Assert.Equal("Index", viewResult.ViewName); //Asserting that the returned Controller is "Index"
            Assert.True(controller.ViewData.ModelState.Count == 1); //Asserting that there is a ModelError object
        }

        [Fact]
        public async Task AuthenticateUser_redirects_to_error_view_when_username_is_null()
        {
            //Arrange 
            var controller = new AuthController(_bosAuthClient, _bosIAClient, _bosEmailClient, _configuration)
            {
                ControllerContext = new ControllerContext()
                {
                    HttpContext = new DefaultHttpContext()
                }
            };
            controller.ControllerContext.HttpContext.Request.Headers.Add("Cookie", new CookieHeaderValue(".AspNet.Consent", "true").ToString());

            AuthModel authModel = new AuthModel
            {
                Username = null,
                Password = "password"
            };

            //Act
            var result = await controller.AuthenticateUser(authModel);

            //Assert
            var viewResult = Assert.IsType<ViewResult>(result); //Asserting that the return is a View
            Assert.Equal("ErrorPage", viewResult.ViewName); //Asserting that the returned view is "Error Page"

            //await Assert.ThrowsAsync<ArgumentNullException>(() => controller.AuthenticateUser(authModel));
        }

        [Fact]
        public async Task AuthenticateUser_redirects_to_index_view_with_incorrect_credentials()
        {
            //Arrange 
            var controller = new AuthController(_bosAuthClient, _bosIAClient, _bosEmailClient, _configuration)
            {
                ControllerContext = new ControllerContext()
                {
                    HttpContext = new DefaultHttpContext()
                }
            };
            controller.ControllerContext.HttpContext.Request.Headers.Add("Cookie", new CookieHeaderValue(".AspNet.Consent", "true").ToString());

            AuthModel authModel = new AuthModel
            {
                Username = "username",
                Password = "password"
            };

            //Act
            var result = await controller.AuthenticateUser(authModel);

            //Assert
            var viewResult = Assert.IsType<ViewResult>(result); //Asserting that the return is a View
            Assert.Equal("Index", viewResult.ViewName); //Asserting that the returned view is "Index"
            Assert.True(controller.ViewData.ModelState.Count == 1); //Asserting that there is a ModelError object
        }

        [Fact]
        public async Task RegisterUser_redirects_to_error_view_when_registerobj_is_null()
        {
            //Arrange 
            var controller = new AuthController(_bosAuthClient, _bosIAClient, _bosEmailClient, _configuration);
            RegistrationModel registerObj = null;

            //Act
            var result = await controller.RegisterUser(registerObj);

            //Assert
            var viewResult = Assert.IsType<ViewResult>(result); //Asserting that the return is a View
            Assert.Equal("ErrorPage", viewResult.ViewName); //Asserting that the returned view is "Error Page"
        }

        [Fact]
        public async Task RegisterUser_redirects_to_error_view_when_email_is_null()
        {
            //Arrange 
            var controller = new AuthController(_bosAuthClient, _bosIAClient, _bosEmailClient, _configuration);
            RegistrationModel registerObj = new RegistrationModel
            {
                EmailAddress = null,
                FirstName = "John",
                LastName = "Doe"
            };

            //Act
            var result = await controller.RegisterUser(registerObj);

            //Assert
            var viewResult = Assert.IsType<ViewResult>(result); //Asserting that the return is a View
            Assert.Equal("ErrorPage", viewResult.ViewName); //Asserting that the returned view is "Error Page"
        }

        [Fact]
        public async Task RegisterUser_redirects_to_error_view_when_firstname_is_null()
        {
            //Arrange 
            var controller = new AuthController(_bosAuthClient, _bosIAClient, _bosEmailClient, _configuration);
            RegistrationModel registerObj = new RegistrationModel
            {
                EmailAddress = "john@doe.com",
                FirstName = null,
                LastName = "Doe"
            };

            //Act
            var result = await controller.RegisterUser(registerObj);

            //Assert
            var viewResult = Assert.IsType<ViewResult>(result); //Asserting that the return is a View
            Assert.Equal("ErrorPage", viewResult.ViewName); //Asserting that the returned view is "Error Page"
        }

        [Fact]
        public async Task RegisterUser_redirects_to_error_view_when_lastname_is_null()
        {
            //Arrange 
            var controller = new AuthController(_bosAuthClient, _bosIAClient, _bosEmailClient, _configuration);
            RegistrationModel registerObj = new RegistrationModel
            {
                EmailAddress = "john@doe.com",
                FirstName = "John",
                LastName = null
            };

            //Act
            var result = await controller.RegisterUser(registerObj);

            //Assert
            var viewResult = Assert.IsType<ViewResult>(result); //Asserting that the return is a View
            Assert.Equal("ErrorPage", viewResult.ViewName); //Asserting that the returned view is "Error Page"
        }

        [Fact]
        public async Task ForgotPasswordAction_redirects_to_error_view_when_object_is_null()
        {
            //Arrange 
            var controller = new AuthController(_bosAuthClient, _bosIAClient, _bosEmailClient, _configuration);
            ForgotPassword forgotPasswordObj = null;

            //Act
            var result = await controller.ForgotPasswordAction(forgotPasswordObj);

            //Assert
            var viewResult = Assert.IsType<ViewResult>(result); //Asserting that the return is a View
            Assert.Equal("ErrorPage", viewResult.ViewName); //Asserting that the returned view is "Error Page"
        }

        [Fact]
        public async Task ForgotPasswordAction_redirects_to_error_view_when_email_is_null()
        {
            //Arrange 
            var controller = new AuthController(_bosAuthClient, _bosIAClient, _bosEmailClient, _configuration);
            ForgotPassword forgotPasswordObj = new ForgotPassword
            {
                EmailAddress = null
            };

            //Act
            var result = await controller.ForgotPasswordAction(forgotPasswordObj);

            //Assert
            var viewResult = Assert.IsType<ViewResult>(result); //Asserting that the return is a View
            Assert.Equal("ErrorPage", viewResult.ViewName); //Asserting that the returned view is "Error Page"
        }

        [Fact]
        public async Task ForgotPasswordAction_redirects_to_error_view_when_email_is_in_incorrect_format()
        {
            //Arrange 
            var controller = new AuthController(_bosAuthClient, _bosIAClient, _bosEmailClient, _configuration);
            ForgotPassword forgotPasswordObj = new ForgotPassword
            {
                EmailAddress = null
            };

            //Act
            var result = await controller.ForgotPasswordAction(forgotPasswordObj);

            //Assert
            var viewResult = Assert.IsType<ViewResult>(result); //Asserting that the return is a View
            Assert.Equal("ErrorPage", viewResult.ViewName); //Asserting that the returned view is "Error Page"
        }

        [Fact]
        public async Task ForgotPasswordAction_redirects_to_login_view_when_email_is_invalid()
        {
            //Arrange 
            var controller = new AuthController(_bosAuthClient, _bosIAClient, _bosEmailClient, _configuration);
            ForgotPassword forgotPasswordObj = new ForgotPassword
            {
                EmailAddress = "notanemail"
            };

            //Act
            var result = await controller.ForgotPasswordAction(forgotPasswordObj);

            //Assert
            var viewResult = Assert.IsType<ViewResult>(result); //Asserting that the return is a View
            Assert.Equal("Index", viewResult.ViewName); //Asserting that the returned view is "Index"
            Assert.True(viewResult.ViewData["Message"] != null); //Asserting that the message is also returned to the view
        }

        [Fact]
        public async Task ForcePasswordChange_returns_error_message_when_data_is_null()
        {
            //Arrange 
            var controller = new AuthController(_bosAuthClient, _bosIAClient, _bosEmailClient, _configuration);
            JObject data = null;

            //Act
            var result = await controller.ForcePasswordChange(data);

            //Assert
            var errorMessage = Assert.IsType<string>(result); //Asserting that the return is a string
            Assert.Equal("Data cannot be null", errorMessage.ToString()); //Asserting that the returned message matches to the one mentioned
        }

        [Fact]
        public async Task ForcePasswordChange_returns_error_message_when_userId_is_null()
        {
            //Arrange 
            var controller = new AuthController(_bosAuthClient, _bosIAClient, _bosEmailClient, _configuration);

            dynamic passwordInfo = new ExpandoObject();
            passwordInfo.userId = null;
            passwordInfo.password = null;
            JObject data = JObject.FromObject(passwordInfo);

            //Act
            var result = await controller.ForcePasswordChange(data);

            //Assert
            var errorMessage = Assert.IsType<string>(result); //Asserting that the return is a string
            Assert.Contains("Something went wrong", errorMessage.ToString()); //Asserting that the returned message matches to the one mentioned
        }

        [Fact]
        public async Task ForcePasswordChange_returns_error_message_when_password_is_null()
        {
            //Arrange 
            var controller = new AuthController(_bosAuthClient, _bosIAClient, _bosEmailClient, _configuration);

            dynamic passwordInfo = new ExpandoObject();

            StringConversion stringConversion = new StringConversion();
            string userId = stringConversion.EncryptString(Guid.NewGuid().ToString());

            passwordInfo.userId = userId;
            passwordInfo.password = null;
            JObject data = JObject.FromObject(passwordInfo);

            //Act
            var result = await controller.ForcePasswordChange(data);

            //Assert
            var errorMessage = Assert.IsType<string>(result); //Asserting that the return is a string
            Assert.Contains("Something went wrong", errorMessage.ToString()); //Asserting that the returned message matches to the one mentioned
        }

        [Fact]
        public async Task ForcePasswordChange_returns_error_message_when_userid_is_in_incorrect_format()
        {
            //Arrange 
            var controller = new AuthController(_bosAuthClient, _bosIAClient, _bosEmailClient, _configuration);

            dynamic passwordInfo = new ExpandoObject();

            passwordInfo.userId = "some random string";
            passwordInfo.password = null;
            JObject data = JObject.FromObject(passwordInfo);

            //Act
            var result = await controller.ForcePasswordChange(data);

            //Assert
            var errorMessage = Assert.IsType<string>(result); //Asserting that the return is a string
            Assert.Contains("Something went wrong", errorMessage.ToString()); //Asserting that the returned message matches to the one mentioned
        }

        [Fact]
        public async Task ForcePasswordChange_returns_error_string_when_userid_is_invalid()
        {
            //Arrange 
            var controller = new AuthController(_bosAuthClient, _bosIAClient, _bosEmailClient, _configuration);

            dynamic passwordInfo = new ExpandoObject();

            StringConversion stringConversion = new StringConversion();
            string userId = stringConversion.EncryptString(Guid.NewGuid().ToString());

            passwordInfo.userId = userId;
            passwordInfo.password = "password";
            JObject data = JObject.FromObject(passwordInfo);

            //Act
            var result = await controller.ForcePasswordChange(data);

            //Assert
            var errorMessage = Assert.IsType<string>(result); //Asserting that the return is a string
            Assert.Contains("Something went wrong.", errorMessage.ToString()); //Asserting that the returned message matches to the one mentioned
        }

        [Fact]
        public void HasSessionExpired_throws_expection_when_httpcontext_is_null()
        {
            //Arrange 
            var controller = new AuthController(_bosAuthClient, _bosIAClient, _bosEmailClient, _configuration);

            //Act and Assert
            Assert.Throws<NullReferenceException>(() => controller.HasSessionExpired()); //Asserting that the expection thrown is NullReference
        }

        [Fact]
        public void HasSessionExpired_throws_expection_when_session_is_null()
        {
            //Arrange 
            //Mocking a session
            MockHttpSession mockSession = new MockHttpSession();
            mockSession["Key"] = "KeySession";

            var controller = new AuthController(_bosAuthClient, _bosIAClient, _bosEmailClient, _configuration)
            {
                ControllerContext = new ControllerContext()
                {
                    HttpContext = new DefaultHttpContext() { Session = mockSession }
                }
            };

            //Act and Assert
            Assert.Throws<KeyNotFoundException>(() => controller.HasSessionExpired()); //Asserting that the expection thrown is KeyNotFound
        }

        [Fact]
        public void HasSessionExpired_returns_false_when_session_found()
        {
            //Arrange 
            //Mocking a session
            MockHttpSession mockSession = new MockHttpSession();
            mockSession["ModuleOperations"] = JsonConvert.SerializeObject("modules");

            var controller = new AuthController(_bosAuthClient, _bosIAClient, _bosEmailClient, _configuration)
            {
                ControllerContext = new ControllerContext()
                {
                    HttpContext = new DefaultHttpContext() { Session = mockSession }
                }
            };

            //Act 
            var result = controller.HasSessionExpired();

            //Assert
            var viewResult = Assert.IsType<bool>(result);
            Assert.False(viewResult);
        }

        [Fact]
        public void HasSessionExpired_returns_true_when_session_not_found()
        {
            //Arrange
            var controller = new AuthController(_bosAuthClient, _bosIAClient, _bosEmailClient, _configuration)
            {
                ControllerContext = new ControllerContext()
                {
                    HttpContext = new DefaultHttpContext() { Session = null }
                }
            };

            //Act 
            var result = controller.HasSessionExpired();

            //Assert
            var viewResult = Assert.IsType<bool>(result);
            Assert.True(viewResult);
        }

        [Fact]
        public async Task SignOut_redirects_to_error_view_when_htttpcontext_is_null()
        {
            //Arrange 
            var controller = new AuthController(_bosAuthClient, _bosIAClient, _bosEmailClient, _configuration);

            //Act
            var result = await controller.SignOut();

            //Assert
            var viewResult = Assert.IsType<ViewResult>(result); //Asserting that the return is a View
            Assert.Equal("ErrorPage", viewResult.ViewName); //Asserting that the returned view is "ErrorPage"
        }
    }
}
