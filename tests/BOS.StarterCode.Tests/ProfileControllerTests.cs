using BOS.Auth.Client;
using BOS.StarterCode.Controllers;
using BOS.StarterCode.Models;
using BOS.StarterCode.Models.BOSModels;
using BOS.StarterCode.Tests.HelperClass;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
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
    public class ProfileControllerTests
    {
        private readonly AuthClient _bosAuthClient;

        public ProfileControllerTests()
        {
            var config = new ConfigurationBuilder().AddJsonFile("appsettings.test.json").Build();
            var bosAPIkey = config["BOS:APIkey"];

            HttpClient httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(config["BOS:ServiceBaseURL"] + config["BOS:AuthRelativeURL"]);
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", bosAPIkey);

            AuthClient authClient = new AuthClient(httpClient);
            _bosAuthClient = authClient;
        }

        [Fact]
        public async Task Index_returns_null_when_claims_is_empty()
        {
            //Arrange
            var controller = ConfigureController(false, false, null, null); //Creating the controller without Claims and Session objects

            //Act
            var result = await controller.Index();

            //Assert
            var viewResult = Assert.IsType<ViewResult>(result); //Asserting that the return is a View
            Assert.Null(viewResult.ViewData.Model); //Asserting that the returned model is null
        }

        [Fact]
        public async Task Index_returns_null_when_session_is_empty()
        {
            //Arrange
            var controller = ConfigureController(true, false, null, null); //Creating the controller with Claims and without Session objects

            //Act
            var result = await controller.Index();

            //Assert
            var viewResult = Assert.IsType<ViewResult>(result); //Asserting that the return is a View
            Assert.Null(viewResult.ViewData.Model); //Asserting that the returned model is null
        }

        [Fact]
        public async Task Index_returns_non_null_model_when_claims_and_sessions_are_not_empty_incorrect_module_code()
        {
            //Arrange
            Guid moduleId = Guid.NewGuid();

            Operation operation = new Operation //Creating an object of Operation model
            {
                Code = "ADD",
                Deleted = false,
                Id = Guid.NewGuid(),
                ModuleId = moduleId,
                Name = "Add Operation"
            };

            Module module = new Module //Creating an object of Module model with an invalid ModuleCode
            {
                Code = "SOMEMODULECODE",
                Deleted = false,
                Id = moduleId,
                Name = "Profile",
                Operations = new List<IA.Client.ClientModels.IOperation> { operation }
            };

            var controller = ConfigureController(true, true, null, new List<Module> { module });

            //Act
            var result = await controller.Index();

            //Assert
            var viewResult = Assert.IsType<ViewResult>(result); //Asserting that the return is a View
            dynamic model = Assert.IsAssignableFrom<ExpandoObject>(viewResult.ViewData.Model);
            Assert.Equal(model.CurrentModuleId, Guid.Empty);
            Assert.Null(model.UserInfo);
            Assert.True(model.Operations.Length == 0);
            Assert.NotNull(model.ModuleOperations);
            Assert.NotNull(model.Initials);
            Assert.NotNull(model.Username);
            Assert.NotNull(model.Roles);
        }

        [Fact]
        public async Task Index_returns_non_null_model_when_claims_and_sessions_are_not_empty_correct_module_code()
        {
            //Arrange
            Guid moduleId = Guid.NewGuid();

            Operation operation = new Operation //Creating an object of Operation model
            {
                Code = "ADD",
                Deleted = false,
                Id = Guid.NewGuid(),
                ModuleId = moduleId,
                Name = "Add Operation"
            };

            Module module = new Module //Creating an object of Module model with an invalid ModuleCode
            {
                Code = "MYPFL",
                Deleted = false,
                Id = moduleId,
                Name = "Profile",
                Operations = new List<IA.Client.ClientModels.IOperation> { operation }
            };

            var controller = ConfigureController(true, true, null, new List<Module> { module });

            //Act
            var result = await controller.Index();

            //Assert
            var viewResult = Assert.IsType<ViewResult>(result); //Asserting that the return is a View
            dynamic model = Assert.IsAssignableFrom<ExpandoObject>(viewResult.ViewData.Model);
            Assert.Null(model.UserInfo);
            Assert.NotNull(model.CurrentModuleId);
            Assert.NotNull(model.Operations);
            Assert.NotNull(model.ModuleOperations);
            Assert.NotNull(model.Initials);
            Assert.NotNull(model.Username);
            Assert.NotNull(model.Roles);
        }

        [Fact]
        public async Task ChangePassword_returns_null_when_claims_is_empty()
        {
            //Arrange
            var controller = ConfigureController(false, false, null, null); //Creating the controller without Claims and Session objects

            //Act
            var result = await controller.ChangePassword();

            //Assert
            var viewResult = Assert.IsType<ViewResult>(result); //Asserting that the return is a View
            Assert.Equal("ChangePassword", viewResult.ViewName);
            Assert.Null(viewResult.ViewData.Model); //Asserting that the returned model is null
        }

        [Fact]
        public async Task ChangePassword_returns_null_when_session_is_empty()
        {
            //Arrange
            var controller = ConfigureController(true, false, null, null); //Creating the controller with Claims and without Session objects

            //Act
            var result = await controller.ChangePassword();

            //Assert
            var viewResult = Assert.IsType<ViewResult>(result); //Asserting that the return is a View
            Assert.Equal("ChangePassword", viewResult.ViewName);
            Assert.Null(viewResult.ViewData.Model); //Asserting that the returned model is null
        }

        [Fact]
        public async Task ChangePassword_returns_non_null_model_when_claims_and_sessions_are_not_empty_incorrect_module_code()
        {
            //Arrange
            Guid moduleId = Guid.NewGuid();

            Operation operation = new Operation //Creating an object of Operation model
            {
                Code = "ADD",
                Deleted = false,
                Id = Guid.NewGuid(),
                ModuleId = moduleId,
                Name = "Add Operation"
            };

            Module module = new Module //Creating an object of Module model with an invalid ModuleCode
            {
                Code = "SOMEMODULECODE",
                Deleted = false,
                Id = moduleId,
                Name = "Profile",
                Operations = new List<IA.Client.ClientModels.IOperation> { operation }
            };

            var controller = ConfigureController(true, true, null, new List<Module> { module });

            //Act
            var result = await controller.ChangePassword();

            //Assert
            var viewResult = Assert.IsType<ViewResult>(result); //Asserting that the return is a View
            Assert.Equal("ChangePassword", viewResult.ViewName);

            dynamic model = Assert.IsAssignableFrom<ExpandoObject>(viewResult.ViewData.Model);
            Assert.Equal(model.CurrentModuleId, Guid.Empty);
            Assert.Null(model.UserInfo);
            Assert.True(model.Operations.Length == 0);
            Assert.NotNull(model.ModuleOperations);
            Assert.NotNull(model.Initials);
            Assert.NotNull(model.Username);
            Assert.NotNull(model.Roles);
        }

        [Fact]
        public async Task ChangePassword_returns_non_null_model_when_claims_and_sessions_are_not_empty_correct_module_code()
        {
            //Arrange
            Guid moduleId = Guid.NewGuid();

            Operation operation = new Operation //Creating an object of Operation model
            {
                Code = "ADD",
                Deleted = false,
                Id = Guid.NewGuid(),
                ModuleId = moduleId,
                Name = "Add Operation"
            };

            Module module = new Module //Creating an object of Module model with an invalid ModuleCode
            {
                Code = "MYPFL",
                Deleted = false,
                Id = moduleId,
                Name = "Profile",
                Operations = new List<IA.Client.ClientModels.IOperation> { operation }
            };

            var controller = ConfigureController(true, true, null, new List<Module> { module });

            //Act
            var result = await controller.ChangePassword();

            //Assert
            var viewResult = Assert.IsType<ViewResult>(result); //Asserting that the return is a View
            Assert.Equal("ChangePassword", viewResult.ViewName);

            dynamic model = Assert.IsAssignableFrom<ExpandoObject>(viewResult.ViewData.Model);
            Assert.Null(model.UserInfo);
            Assert.NotNull(model.CurrentModuleId);
            Assert.NotNull(model.Operations);
            Assert.NotNull(model.ModuleOperations);
            Assert.NotNull(model.Initials);
            Assert.NotNull(model.Username);
            Assert.NotNull(model.Roles);
        }

        [Fact]
        public async Task UpdatePassword_returns_error_string_when_data_is_null()
        {
            //Arrange
            var controller = new ProfileController(_bosAuthClient);

            //Act
            var result = await controller.UpdatePassword(null);

            //Assert
            var messageResult = Assert.IsType<string>(result); //Asserting that the return is a String
            Assert.Contains("Data sent was inaccurate.", messageResult); //Asserting that message is equal as mentioned
        }

        [Fact]
        public async Task UpdatePassword_returns_error_string_when_passwordObj_is_null()
        {
            //Arrange
            var controller = new ProfileController(_bosAuthClient);
            JObject data = new JObject();

            //Act
            var result = await controller.UpdatePassword(data);

            //Assert
            var messageResult = Assert.IsType<string>(result); //Asserting that the return is a String
            Assert.Contains("Data sent was inaccurate.", messageResult); //Asserting that message is equal as mentioned
        }

        [Fact]
        public async Task UpdatePassword_returns_error_string_when_current_password_is_null()
        {
            //Arrange
            var controller = ConfigureController(false, false, null, null);
            ChangePassword passwordObj = new ChangePassword();

            dynamic inputData = new ExpandoObject();
            inputData.PasswordObj = passwordObj;

            JObject data = JObject.FromObject(inputData);

            //Act
            var result = await controller.UpdatePassword(data);

            //Assert
            var messageResult = Assert.IsType<string>(result); //Asserting that the return is a String
            Assert.Contains("Password(s) cannot be null", messageResult); //Asserting that message is equal as mentioned
        }

        [Fact]
        public async Task UpdatePassword_returns_error_string_when_new_password_is_null()
        {
            //Arrange
            var controller = ConfigureController(false, false, null, null);
            ChangePassword passwordObj = new ChangePassword
            {
                CurrentPassword = "password"
            };

            dynamic inputData = new ExpandoObject();
            inputData.PasswordObj = passwordObj;

            JObject data = JObject.FromObject(inputData);

            //Act
            var result = await controller.UpdatePassword(data);

            //Assert
            var messageResult = Assert.IsType<string>(result); //Asserting that the return is a String
            Assert.Contains("Password(s) cannot be null", messageResult); //Asserting that message is equal as mentioned
        }

        [Fact]
        public async Task UpdatePassword_returns_error_string_when_claims_is_null()
        {
            //Arrange
            var controller = ConfigureController(false, false, null, null);
            ChangePassword passwordObj = new ChangePassword
            {
                CurrentPassword = "password",
                NewPassword = "password"
            };

            dynamic inputData = new ExpandoObject();
            inputData.PasswordObj = passwordObj;

            JObject data = JObject.FromObject(inputData);

            //Act
            var result = await controller.UpdatePassword(data);

            //Assert
            var messageResult = Assert.IsType<string>(result); //Asserting that the return is a String
            Assert.Contains("Your session seems to have expired.", messageResult); //Asserting that message is equal as mentioned
        }

        [Fact]
        public async Task UpdatePassword_returns_error_string_when_userId_is_null()
        {
            //Arrange
            var controller = ConfigureController(true, false, "userId", null);
            ChangePassword passwordObj = new ChangePassword
            {
                CurrentPassword = "password",
                NewPassword = "password"
            };

            dynamic inputData = new ExpandoObject();
            inputData.PasswordObj = passwordObj;

            JObject data = JObject.FromObject(inputData);

            //Act
            var result = await controller.UpdatePassword(data);

            //Assert
            var messageResult = Assert.IsType<string>(result); //Asserting that the return is a String
            Assert.Contains("Your session seems to have expired.", messageResult); //Asserting that message is equal as mentioned
        }

        [Fact]
        public async Task UpdatePassword_returns_error_string_when_userId_is_in_incorrect_format()
        {
            //Arrange
            var controller = ConfigureController(true, false, "userIdIncorrectFormat", null);
            ChangePassword passwordObj = new ChangePassword
            {
                CurrentPassword = "password",
                NewPassword = "password"
            };

            dynamic inputData = new ExpandoObject();
            inputData.PasswordObj = passwordObj;

            JObject data = JObject.FromObject(inputData);

            //Act
            var result = await controller.UpdatePassword(data);

            //Assert
            var messageResult = Assert.IsType<string>(result); //Asserting that the return is a String
            Assert.Contains("Guid should contain 32 digits with 4 dashes (xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx).", messageResult); //Asserting that message is equal as mentioned
        }

        [Fact]
        public async Task UpdateProfileInfo_returns_error_string_when_data_is_null()
        {
            //Arrange
            var controller = new ProfileController(_bosAuthClient);

            //Act
            var result = await controller.UpdateProfileInfo(null);

            //Assert
            var messageResult = Assert.IsType<string>(result); //Asserting that the return is a String
            Assert.Contains("Data sent was inaccurate.", messageResult); //Asserting that message is equal as mentioned
        }

        [Fact]
        public async Task UpdateProfileInfo_returns_error_string_when_claims_is_null()
        {
            //Arrange
            var controller = new ProfileController(_bosAuthClient);
            JObject data = new JObject();

            //Act
            var result = await controller.UpdateProfileInfo(data);

            //Assert
            var messageResult = Assert.IsType<string>(result); //Asserting that the return is a String
            Assert.Contains("Unable to perform this action at this time", messageResult); //Asserting that message is equal as mentioned
        }

        [Fact]
        public async Task UpdateProfileInfo_returns_error_string_when_userid_is_null()
        {
            //Arrange
            var controller = ConfigureController(true, false, "userId", null);
            JObject data = new JObject();

            //Act
            var result = await controller.UpdateProfileInfo(data);

            //Assert
            //Assert
            var messageResult = Assert.IsType<string>(result); //Asserting that the return is a String
            Assert.Contains("Value cannot be null", messageResult); //Asserting that message is equal as mentioned
        }

        [Fact]
        public async Task UpdateProfileInfo_returns_error_string_when_userid_is_incorrect_format()
        {
            //Arrange
            var controller = ConfigureController(true, false, "userIdIncorrectFormat", null);
            JObject data = new JObject();

            //Act
            var result = await controller.UpdateProfileInfo(data);

            //Assert
            //Assert
            var messageResult = Assert.IsType<string>(result); //Asserting that the return is a String
            Assert.Contains("Guid should contain 32 digits with 4 dashes (xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx).", messageResult); //Asserting that message is equal as mentioned
        }

        [Fact]
        public async Task UpdateProfileInfo_returns_error_string_when_userid_is_invalid()
        {
            //Arrange
            var controller = ConfigureController(true, false, null, null);
            JObject data = new JObject();

            //Act
            var result = await controller.UpdateProfileInfo(data);

            //Assert
            //Assert
            var messageResult = Assert.IsType<string>(result); //Asserting that the return is a String
            Assert.Contains("No user with that Id was found to update.", messageResult); //Asserting that message is equal as mentioned
        }

        [Fact]
        public async Task UpdateProfileInfo_returns_error_string_when_username_is_null()
        {
            //Arrange
            var controller = ConfigureController(true, false, "userName", null);
            JObject data = new JObject();

            //Act
            var result = await controller.UpdateProfileInfo(data);

            //Assert
            //Assert
            var messageResult = Assert.IsType<string>(result); //Asserting that the return is a String
            Assert.Contains("No user with that Id was found to update.", messageResult); //Asserting that message is equal as mentioned
        }

        [Fact]
        public async Task UpdateProfileInfo_returns_error_string_when_email_is_null()
        {
            //Arrange
            var controller = ConfigureController(true, false, null, null);
            JObject data = new JObject();
            
            //Act
            var result = await controller.UpdateProfileInfo(data);

            //Assert
            //Assert
            var messageResult = Assert.IsType<string>(result); //Asserting that the return is a String
            Assert.Contains("No user with that Id was found to update.", messageResult); //Asserting that message is equal as mentioned
        }

        [Fact]
        public async Task UpdateProfileInfo_returns_error_string_when_firstname_is_null()
        {
            //Arrange
            var controller = ConfigureController(true, false, null, null);

            dynamic inputData = new ExpandoObject();
            inputData.Email = "some@email.com";

            JObject data = JObject.FromObject(inputData);

            //Act
            var result = await controller.UpdateProfileInfo(data);

            //Assert
            //Assert
            var messageResult = Assert.IsType<string>(result); //Asserting that the return is a String
            Assert.Contains("No user with that Id was found to update.", messageResult); //Asserting that message is equal as mentioned
        }

        [Fact]
        public async Task UpdateProfileInfo_returns_error_string_when_lastname_is_null()
        {
            //Arrange
            var controller = ConfigureController(true, false, null, null);

            dynamic inputData = new ExpandoObject();
            inputData.Email = "some@email.com";
            inputData.FirstName = "John";

            JObject data = JObject.FromObject(inputData);

            //Act
            var result = await controller.UpdateProfileInfo(data);

            //Assert
            var messageResult = Assert.IsType<string>(result); //Asserting that the return is a String
            Assert.Contains("No user with that Id was found to update.", messageResult); //Asserting that message is equal as mentioned
        }

        [Fact]
        public async Task UpdateUsername_returns_error_string_when_updated_username_is_null()
        {
            ///Arrange
            var controller = ConfigureController(true, false, "userName", null);

            //Act
            var result = await controller.UpdateUsername(null);

            //Assert
            var messageResult = Assert.IsType<string>(result); //Asserting that the return is a String
            Assert.Contains("Username cannot be empty", messageResult); //Asserting that message is equal as mentioned
        }

        [Fact]
        public async Task UpdateUsername_returns_error_string_when_claims_is_null()
        {
            //Arrange
            var controller = new ProfileController(_bosAuthClient);

            //Act
            var result = await controller.UpdateUsername("someusername");

            //Assert
            var messageResult = Assert.IsType<string>(result); //Asserting that the return is a String
            Assert.Contains("Unable to perform this action at this time", messageResult); //Asserting that message is equal as mentioned
        }

        [Fact]
        public async Task UpdateUsername_returns_error_string_when_userid_is_null()
        {
            //Arrange
            var controller = ConfigureController(true, false, "userId", null);

            //Act
            var result = await controller.UpdateUsername("someusername");

            //Assert
            var messageResult = Assert.IsType<string>(result); //Asserting that the return is a String
            Assert.Contains("Value cannot be null.", messageResult); //Asserting that message is equal as mentioned
        }

        [Fact]
        public async Task UpdateUsername_returns_error_string_when_username_is_null()
        {
            //Arrange
            var controller = ConfigureController(true, false, "userName", null);

            //Act
            var result = await controller.UpdateUsername("someusername");

            //Assert
            var messageResult = Assert.IsType<string>(result); //Asserting that the return is a String
            Assert.Contains("Unable to perform this action at this time", messageResult); //Asserting that message is equal as mentioned
        }


        [Fact]
        public async Task UpdateUsername_returns_error_string_when_data_is_correct()
        {
            //Arrange
            var controller = ConfigureController(true, false, null, null);

            //Act
            var result = await controller.UpdateUsername("someusername");

            //Assert
            var messageResult = Assert.IsType<string>(result); //Asserting that the return is a String
            Assert.Contains("No user with that Id was found to update.", messageResult); //Asserting that message is equal as mentioned
        }

        private ProfileController ConfigureController(bool isClaims, bool isSession, string excludeClaims, List<Module> modules)
        {
            var controller = new ProfileController(_bosAuthClient);
            ClaimsPrincipal principal = new ClaimsPrincipal();
            if (isClaims)
            {
                //Mocking the user claims
                var claims = new List<Claim>();
                if (excludeClaims == null)
                {
                    claims = new List<Claim>{
                        new Claim("Email", "some@email.com"),
                        new Claim("Initials", "JD"),
                        new Claim("Name", "John Doe"),
                        new Claim("Role", "Admin"),
                        new Claim("UserId", Guid.NewGuid().ToString()),
                        new Claim("Username", "SomeUserName"),
                        new Claim("IsAuthenticated", "True")
                    };
                }
                else
                {
                    switch (excludeClaims)
                    {
                        case "userId":
                            claims = new List<Claim>{
                                    new Claim("Email", "some@email.com"),
                                    new Claim("Initials", "JD"),
                                    new Claim("Name", "John Doe"),
                                    new Claim("Role", "Admin"),
                                    new Claim("Username", "SomeUserName"),
                                    new Claim("IsAuthenticated", "True")
                            };
                            break;
                        case "userIdIncorrectFormat":
                            claims = new List<Claim>{
                                    new Claim("Email", "some@email.com"),
                                    new Claim("Initials", "JD"),
                                    new Claim("Name", "John Doe"),
                                    new Claim("UserId", "SomeUserId"),
                                    new Claim("Role", "Admin"),
                                    new Claim("Username", "SomeUserName"),
                                    new Claim("IsAuthenticated", "True")
                            };
                            break;
                        case "userName":
                            claims = new List<Claim>{
                                new Claim("Email", "some@email.com"),
                                new Claim("Initials", "JD"),
                                new Claim("Name", "John Doe"),
                                new Claim("Role", "Admin"),
                                new Claim("UserId", Guid.NewGuid().ToString()),
                                new Claim("IsAuthenticated", "True")
                            };
                            break;
                       
                    }
                }

                var userIdentity = new ClaimsIdentity(claims, "Auth");
                principal = new ClaimsPrincipal(userIdentity);

                controller = new ProfileController(_bosAuthClient)
                {
                    ControllerContext = new ControllerContext()
                    {
                        HttpContext = new DefaultHttpContext() { User = principal }
                    }
                };
            }

            if (isSession)
            {
                //Mocking a session
                MockHttpSession mockSession = new MockHttpSession();
                mockSession["ModuleOperations"] = JsonConvert.SerializeObject(modules);

                controller = new ProfileController(_bosAuthClient)
                {
                    ControllerContext = new ControllerContext()
                    {
                        HttpContext = new DefaultHttpContext() { User = principal, Session = mockSession }
                    }
                };
            }
            return controller;
        }
    }
}
