using BOS.Auth.Client;
using BOS.Email.Client;
using BOS.StarterCode.Controllers;
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
    public class UsersControllerTests
    {
        private readonly AuthClient _bosAuthClient;
        private readonly EmailClient _bosEmailClient;
        private readonly IConfiguration _configuration;

        public UsersControllerTests()
        {
            var config = new ConfigurationBuilder().AddJsonFile("appsettings.test.json").Build();
            var bosAPIkey = config["BOS:APIkey"];
            string baseURL = config["BOS:ServiceBaseURL"];

            HttpClient httpClientAuth = new HttpClient();
            httpClientAuth.BaseAddress = new Uri(baseURL + config["BOS:AuthRelativeURL"]);
            httpClientAuth.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", bosAPIkey);
            AuthClient authClient = new AuthClient(httpClientAuth);
            _bosAuthClient = authClient;

            HttpClient httpClientEmail = new HttpClient();
            httpClientEmail.BaseAddress = new Uri(baseURL + config["BOS:EmailRelativeURL"]);
            httpClientEmail.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", bosAPIkey);
            EmailClient emailClient = new EmailClient(httpClientEmail);
            _bosEmailClient = emailClient;

            _configuration = null;
        }

        [Fact]
        public async Task Index_returns_null_when_claims_is_empty()
        {
            //Arrange
            var controller = new UsersController(_bosAuthClient, _bosEmailClient, _configuration);

            //Act
            var result = await controller.Index();

            //Assert
            var viewResult = Assert.IsType<ViewResult>(result); //Asserting that the return is a View
            Assert.Null(viewResult.ViewData.Model); //Asserting that the returned model is null
        }

        [Fact]
        public async Task Index_returns_when_session_is_empty()
        {
            //Arrange
            var controller = ConfigureController(true, false, null); //Creating the controller with Claims and without Sessions Object

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
                Name = "Module Name",
                Operations = new List<IA.Client.ClientModels.IOperation> { operation }
            };

            var controller = ConfigureController(true, true, new List<Module> { module });

            //Act
            var result = await controller.Index();

            //Assert
            var viewResult = Assert.IsType<ViewResult>(result); //Asserting that the return is a View
            dynamic model = Assert.IsAssignableFrom<ExpandoObject>(viewResult.ViewData.Model);
            //Asserting various values of the returned model
            Assert.Equal(Guid.Empty, model.CurrentModuleId); //CurrentModuleId is empty
            Assert.True(model.Operations.Length == 0); //Asserting that there are no operations
            Assert.NotNull(model.ModuleOperations);
            Assert.NotNull(model.Initials);
            Assert.NotNull(model.Username);
            Assert.NotNull(model.Roles);
            Assert.NotNull(model.UserList);
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
                Code = "USERS",
                Deleted = false,
                Id = moduleId,
                Name = "Users",
                Operations = new List<IA.Client.ClientModels.IOperation> { operation }
            };

            var controller = ConfigureController(true, true, new List<Module> { module });

            //Act
            var result = await controller.Index();

            //Assert
            var viewResult = Assert.IsType<ViewResult>(result); //Asserting that the return is a View
            dynamic model = Assert.IsAssignableFrom<ExpandoObject>(viewResult.ViewData.Model);
            //Asserting values of the returned model
            Assert.NotNull(model.CurrentModuleId); //CurrentModuleId is NOT empty
            Assert.True(model.Operations.Length > 0); //Looking for operations 
            Assert.NotNull(model.ModuleOperations);
            Assert.NotNull(model.Initials);
            Assert.NotNull(model.Username);
            Assert.NotNull(model.Roles);
            Assert.NotNull(model.UserList);
        }

        [Fact]
        public async Task AddNewUser_returns_availableRoles_when_claims_is_empty()
        {
            //Arrange
            var controller = new UsersController(_bosAuthClient, _bosEmailClient, _configuration);

            //Act
            var result = await controller.AddNewUser();

            //Assert
            var viewResult = Assert.IsType<ViewResult>(result); //Asserting that the return is a View
            Assert.Equal("AddUser", viewResult.ViewName); //Asseting that the returned view is "AddUser"

            dynamic model = Assert.IsAssignableFrom<ExpandoObject>(viewResult.ViewData.Model);
            Assert.NotNull(model.AvailableRoles); //Asserting that the returned model just has AvailableRoles
        }

        [Fact]
        public async Task AddNewUser_returns_availableRoles_when_session_is_empty()
        {
            //Arrange
            var controller = ConfigureController(true, false, null);

            //Act
            var result = await controller.AddNewUser();

            //Assert
            var viewResult = Assert.IsType<ViewResult>(result); //Asserting that the return is a View
            Assert.Equal("AddUser", viewResult.ViewName); //Asseting that the returned view is "AddUser"

            dynamic model = Assert.IsAssignableFrom<ExpandoObject>(viewResult.ViewData.Model);
            Assert.NotNull(model.AvailableRoles); //Asserting that the returned model just has AvailableRoles
        }

        [Fact]
        public async Task AddNewUser_returns_non_null_model_when_claims_and_sessions_are_not_empty_incorrect_module_code()
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
                Name = "Module Name",
                Operations = new List<IA.Client.ClientModels.IOperation> { operation }
            };

            var controller = ConfigureController(true, true, new List<Module> { module });

            //Act
            var result = await controller.AddNewUser();

            //Assert
            var viewResult = Assert.IsType<ViewResult>(result); //Asserting that the return is a View
            Assert.Equal("AddUser", viewResult.ViewName); //Asseting that the returned view is "AddUser"

            dynamic model = Assert.IsAssignableFrom<ExpandoObject>(viewResult.ViewData.Model);
            //Asserting various values of the returned model
            Assert.True(model.Operations.Length == 0); //Asserting that there are no operations associated
            Assert.Equal(Guid.Empty, model.CurrentModuleId); //Asserting that CurrentModuleId is Empty Guid
            Assert.NotNull(model.ModuleOperations);
            Assert.NotNull(model.Initials);
            Assert.NotNull(model.Username);
            Assert.NotNull(model.Roles);
            Assert.NotNull(model.UserList);
            Assert.NotNull(model.AvailableRoles);
        }

        [Fact]
        public async Task AddNewUser_returns_non_null_model_when_claims_and_sessions_are_not_empty_correct_module_code()
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
                Code = "USERS",
                Deleted = false,
                Id = moduleId,
                Name = "Users",
                Operations = new List<IA.Client.ClientModels.IOperation> { operation }
            };

            var controller = ConfigureController(true, true, new List<Module> { module });

            //Act
            var result = await controller.AddNewUser();

            //Assert
            var viewResult = Assert.IsType<ViewResult>(result); //Asserting that the return is a View
            Assert.Equal("AddUser", viewResult.ViewName); //Asseting that the returned view is "AddUser"

            dynamic model = Assert.IsAssignableFrom<ExpandoObject>(viewResult.ViewData.Model);
            //Asserting various values of the returned model
            Assert.NotEqual(Guid.Empty, model.CurrentModuleId); //Asserting that CurrentModuleId is Empty Guid
            Assert.NotNull(model.Operations); //Asserting that there are some operations associated
            Assert.NotNull(model.ModuleOperations);
            Assert.NotNull(model.Initials);
            Assert.NotNull(model.Username);
            Assert.NotNull(model.Roles);
            Assert.NotNull(model.UserList);
            Assert.NotNull(model.AvailableRoles);
        }

        [Fact]
        public async Task EditUser_returns_index_view_with_model_error_when_userid_is_null()
        {
            //Arrange
            var controller = new UsersController(_bosAuthClient, _bosEmailClient, _configuration);

            //Act
            var result = await controller.EditUser(null);

            //Assert
            var viewResult = Assert.IsType<ViewResult>(result); //Asserting that the return is a View
            Assert.Equal("Index", viewResult.ViewName); //Asserting that the returned Controller is "Home"
            Assert.True(controller.ViewData.ModelState.Count == 1); //Asserts that there is a ModelStateError object
        }

        [Fact]
        public async Task EditUser_returns_error_view_when_userid_is_in_incorrect_format()
        {
            //Arrange
            var controller = new UsersController(_bosAuthClient, _bosEmailClient, _configuration);

            //Act
            var result = await controller.EditUser("SomeRandomUserId");

            //Assert
            var viewResult = Assert.IsType<ViewResult>(result); //Asserting that the return is a View
            Assert.Equal("ErrorPage", viewResult.ViewName); //Asserting that the returned Controller is "Error Page"

            dynamic model = Assert.IsAssignableFrom<ExpandoObject>(viewResult.ViewData.Model);
            //Asserting various values of the returned model
            Assert.NotNull(model.Message);
            Assert.NotNull(model.StackTrace);
        }

        [Fact]
        public async Task EditUser_returns_non_null_model_when_userid_is_invalid()
        {
            //Arrange
            var controller = new UsersController(_bosAuthClient, _bosEmailClient, _configuration);

            //Act
            var result = await controller.EditUser("f4EcNSUeWjAwPTPMefW2659w3FX3EtblPwy0AMKAEFTgXn0o2OPxDjar0+K7zL/P1VIHQbODgHzP1hJpqcbpVQ==");

            //Assert
            var viewResult = Assert.IsType<ViewResult>(result); //Asserting that the return is a View
            dynamic model = Assert.IsAssignableFrom<ExpandoObject>(viewResult.ViewData.Model);
            Assert.NotNull(model.AvailableRoles); //Asserting various values of the returned model
        }

        [Fact]
        public async Task EditUser_returns_model_with_availableRoles_when_claims_is_empty()
        {
            //Arrange
            var controller = new UsersController(_bosAuthClient, _bosEmailClient, _configuration);

            //Act
            var result = await controller.EditUser("f4EcNSUeWjAwPTPMefW2659w3FX3EtblPwy0AMKAEFTgXn0o2OPxDjar0+K7zL/P1VIHQbODgHzP1hJpqcbpVQ==");

            //Assert
            var viewResult = Assert.IsType<ViewResult>(result); //Asserting that the return is a View
            Assert.Equal("EditUser", viewResult.ViewName); //Asserting that the returned Controller is "EditUser"

            dynamic model = Assert.IsAssignableFrom<ExpandoObject>(viewResult.ViewData.Model);
            Assert.NotNull(model.AvailableRoles); //Asserting that the returned model only has Available Roles
        }

        [Fact]
        public async Task EditUser_returns_model_with_availableRoles_when_session_is_empty_incorrect_modue_code()
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
                Name = "Module Name",
                Operations = new List<IA.Client.ClientModels.IOperation> { operation }
            };

            var controller = ConfigureController(true, true, new List<Module> { module });

            //Act
            var result = await controller.EditUser("f4EcNSUeWjAwPTPMefW2659w3FX3EtblPwy0AMKAEFTgXn0o2OPxDjar0+K7zL/P1VIHQbODgHzP1hJpqcbpVQ==");

            //Assert
            var viewResult = Assert.IsType<ViewResult>(result); //Asserting that the return is a View
            Assert.Equal("EditUser", viewResult.ViewName); //Asserting that the returned Controller is "EditUser"

            dynamic model = Assert.IsAssignableFrom<ExpandoObject>(viewResult.ViewData.Model);
            //Asserting various values of the returned model
            Assert.Equal(Guid.Empty, model.CurrentModuleId); //CurrentModuleId is empty
            Assert.True(model.Operations.Length == 0); //Asserting that there are no operations
            Assert.NotNull(model.ModuleOperations);
            Assert.NotNull(model.Initials);
            Assert.NotNull(model.Username);
            Assert.NotNull(model.Roles);
            Assert.NotNull(model.UserList);
            Assert.NotNull(model.AvailableRoles);
        }

        [Fact]
        public async Task EditUser_returns_model_with_availableRoles_when_session_is_empty_correct_modue_code()
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
                Code = "USERS",
                Deleted = false,
                Id = moduleId,
                Name = "Users",
                Operations = new List<IA.Client.ClientModels.IOperation> { operation }
            };

            var controller = ConfigureController(true, true, new List<Module> { module });

            //Act
            var result = await controller.EditUser("f4EcNSUeWjAwPTPMefW2659w3FX3EtblPwy0AMKAEFTgXn0o2OPxDjar0+K7zL/P1VIHQbODgHzP1hJpqcbpVQ==");

            //Assert
            var viewResult = Assert.IsType<ViewResult>(result); //Asserting that the return is a View
            Assert.Equal("EditUser", viewResult.ViewName); //Asserting that the returned Controller is "EditUser"

            dynamic model = Assert.IsAssignableFrom<ExpandoObject>(viewResult.ViewData.Model);
            //Asserting values of the returned model
            Assert.NotNull(model.CurrentModuleId); //CurrentModuleId is NOT empty
            Assert.True(model.Operations.Length > 0); //Looking for operations 
            Assert.NotNull(model.ModuleOperations);
            Assert.NotNull(model.Initials);
            Assert.NotNull(model.Username);
            Assert.NotNull(model.Roles);
            Assert.NotNull(model.UserList);
            Assert.NotNull(model.AvailableRoles);
        }

        [Fact]
        public async Task EditUser_returns_non_null_model_when_role_is_null()
        {
            //Arrange
            var controller = new UsersController(_bosAuthClient, _bosEmailClient, _configuration);

            //Act
            var result = await controller.EditUser("f4EcNSUeWjAwPTPMefW2659w3FX3EtblPwy0AMKAEFTgXn0o2OPxDjar0+K7zL/P1VIHQbODgHzP1hJpqcbpVQ==");

            //Assert
            var viewResult = Assert.IsType<ViewResult>(result); //Asserting that the return is a View
            dynamic model = Assert.IsAssignableFrom<ExpandoObject>(viewResult.ViewData.Model);
            Assert.NotNull(model.AvailableRoles); //Asserting various values of the returned model
        }

        [Fact]
        public async Task AddUser_returns_error_string_when_data_is_null()
        {
            //Arrange
            var controller = new UsersController(_bosAuthClient, _bosEmailClient, _configuration);

            //Act
            var result = await controller.AddUser(null);

            //Assert
            var messageResult = Assert.IsType<string>(result); //Asserting that the return is a String
            Assert.Contains("Data cannot be null", messageResult); //Asserting that message is equal as mentioned
        }

        [Fact]
        public async Task AddUser_returns_error_string_when_user_is_null()
        {
            //Arrange
            var controller = new UsersController(_bosAuthClient, _bosEmailClient, _configuration);
            JObject jObject = new JObject();

            //Act
            var result = await controller.AddUser(jObject);

            //Assert
            var messageResult = Assert.IsType<string>(result); //Asserting that the return is a String
            Assert.Contains("User data cannot be null", messageResult); //Asserting that message is equal as mentioned
        }

        [Fact]
        public async Task AddUser_returns_error_string_when_IsEmailToSend_is_false_and_password_is_null()
        {
            //Arrange
            var controller = new UsersController(_bosAuthClient, _bosEmailClient, _configuration);

            dynamic data = new ExpandoObject();
            data.User = new User();
            data.IsEmailToSend = false;
            JObject jObject = JObject.FromObject(data);

            //Act
            var result = await controller.AddUser(jObject);

            //Assert
            var messageResult = Assert.IsType<string>(result); //Asserting that the return is a String
            Assert.Contains("Required data is missing", messageResult); //Asserting that message is equal as mentioned
        }

        [Fact]
        public async Task AddUser_returns_error_string_when_username_is_null()
        {
            //Arrange
            var controller = new UsersController(_bosAuthClient, _bosEmailClient, _configuration);

            dynamic data = new ExpandoObject();
            data.User = new User();
            JObject jObject = JObject.FromObject(data);

            //Act
            var result = await controller.AddUser(jObject);

            //Assert
            var messageResult = Assert.IsType<string>(result); //Asserting that the return is a String
            Assert.Contains("Required data is missing", messageResult); //Asserting that message is equal as mentioned
        }

        [Fact]
        public async Task AddUser_returns_error_string_when_email_is_null()
        {
            //Arrange
            var controller = new UsersController(_bosAuthClient, _bosEmailClient, _configuration);

            User user = new User
            {
                Username = "someusername"
            };

            dynamic data = new ExpandoObject();
            data.User = user;
            JObject jObject = JObject.FromObject(data);

            //Act
            var result = await controller.AddUser(jObject);

            //Assert
            var messageResult = Assert.IsType<string>(result); //Asserting that the return is a String
            Assert.Contains("Required data is missing", messageResult); //Asserting that message is equal as mentioned
        }

        [Fact]
        public async Task AddUser_returns_error_string_when_password_is_null()
        {
            //Arrange
            var controller = new UsersController(_bosAuthClient, _bosEmailClient, _configuration);

            User user = new User
            {
                Username = "someusername",
                Email = "john@email.com",
            };

            dynamic data = new ExpandoObject();
            data.User = user;
            data.IsEmailToSend = false;
            JObject jObject = JObject.FromObject(data);

            //Act
            var result = await controller.AddUser(jObject);

            //Assert
            var messageResult = Assert.IsType<string>(result); //Asserting that the return is a String
            Assert.Contains("Required data is missing", messageResult); //Asserting that message is equal as mentioned
        }

        [Fact]
        public async Task AddUser_returns_error_string_when_roles_count_is_zero()
        {
            //Arrange
            var controller = new UsersController(_bosAuthClient, _bosEmailClient, _configuration);

            User user = new User
            {
                Username = "someusername",
                Email = "john@email.com",
            };

            dynamic data = new ExpandoObject();
            data.User = user;
            data.Password = "Password";
            data.Roles = new List<Role>();
            JObject jObject = JObject.FromObject(data);

            //Act
            var result = await controller.AddUser(jObject);

            //Assert
            var messageResult = Assert.IsType<string>(result); //Asserting that the return is a String
            Assert.Contains("User has to be associated with at least one role", messageResult); //Asserting that message is equal as mentioned
        }

        [Fact]
        public async Task AddUser_returns_error_string_when_roles_is_null()
        {
            //Arrange
            var controller = new UsersController(_bosAuthClient, _bosEmailClient, _configuration);

            User user = new User
            {
                Username = "someusername",
                Email = "john@email.com",
            };

            dynamic data = new ExpandoObject();
            data.User = user;
            data.Password = "Password";
            JObject jObject = JObject.FromObject(data);

            //Act
            var result = await controller.AddUser(jObject);

            //Assert
            var messageResult = Assert.IsType<string>(result); //Asserting that the return is a String
            Assert.Contains("User has to be associated with at least one role", messageResult); //Asserting that message is equal as mentioned
        }

        [Fact]
        public async Task DeleteUser_returns_error_string_when_userid_is_null()
        {
            //Arrange
            var controller = new UsersController(_bosAuthClient, _bosEmailClient, _configuration);

            //Act
            var result = await controller.DeleteUser(null);

            //Assert
            var messageResult = Assert.IsType<string>(result); //Asserting that the return is a String
            Assert.Contains("UserId cannot be null", messageResult); //Asserting that message is equal as mentioned
        }

        [Fact]
        public async Task DeleteUser_returns_string_when_userid_is_in_incorrect_format()
        {
            //Arrange
            var controller = new UsersController(_bosAuthClient, _bosEmailClient, _configuration);

            //Act
            var result = await controller.DeleteUser("someuserId");

            //Assert
            var messageResult = Assert.IsType<string>(result); //Asserting that the return is a String
            Assert.Contains("The input is not a valid Base-64 string", messageResult); //Asserting that message is equal as mentioned
        }

        [Fact]
        public async Task DeleteUser_returns_string_when_userid_is_invalid()
        {
            //Arrange
            var controller = new UsersController(_bosAuthClient, _bosEmailClient, _configuration);

            //Act
            var result = await controller.DeleteUser("f4EcNSUeWjAwPTPMefW2659w3FX3EtblPwy0AMKAEFTgXn0o2OPxDjar0+K7zL/P1VIHQbODgHzP1hJpqcbpVQ==");

            //Assert
            var messageResult = Assert.IsType<string>(result); //Asserting that the return is a String
            Assert.Contains("User not found for Id", messageResult); //Asserting that message is equal as mentioned
        }

        [Fact]
        public async Task UpdateUserInfo_returns_error_string_when_user_is_null()
        {
            //Arrange
            var controller = new UsersController(_bosAuthClient, _bosEmailClient, _configuration);

            //Act
            var result = await controller.UpdateUserInfo(null);

            //Assert
            var messageResult = Assert.IsType<string>(result); //Asserting that the return is a String
            Assert.Contains("User data cannot be null", messageResult); //Asserting that message is equal as mentioned
        }

        [Fact]
        public async Task UpdateUserInfo_returns_error_string_when_userid_is_null()
        {
            //Arrange
            var controller = new UsersController(_bosAuthClient, _bosEmailClient, _configuration);

            User user = new User();
            JObject jObject = JObject.FromObject(user);
            //Act
            var result = await controller.UpdateUserInfo(jObject);

            //Assert
            var messageResult = Assert.IsType<string>(result); //Asserting that the return is a String
            Assert.Contains("User data cannot be null", messageResult); //Asserting that message is equal as mentioned
        }

        [Fact]
        public async Task UpdateUserInfo_returns_error_string_when_userid_is_empty()
        {
            //Arrange
            var controller = new UsersController(_bosAuthClient, _bosEmailClient, _configuration);

            User user = new User() { Id = Guid.Empty };
            JObject jObject = JObject.FromObject(user);
            //Act
            var result = await controller.UpdateUserInfo(jObject);

            //Assert
            var messageResult = Assert.IsType<string>(result); //Asserting that the return is a String
            Assert.Contains("User data cannot be null", messageResult); //Asserting that message is equal as mentioned
        }

        [Fact]
        public async Task UpdateUserInfo_returns_error_string_when_userid_is_in_incorrect_format()
        {
            //Arrange
            var controller = new UsersController(_bosAuthClient, _bosEmailClient, _configuration);
            User user = new User
            {
                UpdatedId = "SomeRandomUserId"
            };
            JObject jObject = JObject.FromObject(user);
            //Act
            var result = await controller.UpdateUserInfo(jObject);

            //Assert
            var messageResult = Assert.IsType<string>(result); //Asserting that the return is a String
            Assert.Contains("Array dimensions exceeded supported range.", messageResult); //Asserting that message is equal as mentioned
        }

        [Fact]
        public async Task UpdateUserInfo_returns_string_when_userid_is_invalid()
        {
            //
            //Arrange
            var controller = new UsersController(_bosAuthClient, _bosEmailClient, _configuration);
            User user = new User
            {
                UpdatedId = "f4EcNSUeWjAwPTPMefW2659w3FX3EtblPwy0AMKAEFTgXn0o2OPxDjar0+K7zL/P1VIHQbODgHzP1hJpqcbpVQ=="
            };
            JObject jObject = JObject.FromObject(user);
            //Act
            var result = await controller.UpdateUserInfo(jObject);

            //Assert
            var messageResult = Assert.IsType<string>(result); //Asserting that the return is a String
            Assert.Contains("No user with that Id was found to update", messageResult); //Asserting that message is equal as mentioned
        }

        [Fact]
        public async Task ChangeUserActiveStatus_returns_error_string_when_data_is_null()
        {
            //Arrange
            var controller = new UsersController(_bosAuthClient, _bosEmailClient, _configuration);

            //Act
            var result = await controller.ChangeUserActiveStatus(null);

            //Assert
            var messageResult = Assert.IsType<string>(result); //Asserting that the return is a String
            Assert.Contains("Data cannot be null", messageResult); //Asserting that message is equal as mentioned
        }

        [Fact]
        public async Task ChangeUserActiveStatus_returns_error_string_when_userid_is_null()
        {
            //Arrange
            var controller = new UsersController(_bosAuthClient, _bosEmailClient, _configuration);
            JObject data = new JObject();

            //Act
            var result = await controller.ChangeUserActiveStatus(data);

            //Assert
            var messageResult = Assert.IsType<string>(result); //Asserting that the return is a String
            Assert.Contains("UserId cannot be null", messageResult); //Asserting that message is equal as mentioned
        }

        [Fact]
        public async Task ChangeUserActiveStatus_returns_error_string_when_action_is_null()
        {
            //Arrange
            var controller = new UsersController(_bosAuthClient, _bosEmailClient, _configuration);

            dynamic inputData = new ExpandoObject();
            inputData.UserId = "";
            JObject data = JObject.FromObject(inputData);

            //Act
            var result = await controller.ChangeUserActiveStatus(data);

            //Assert
            var messageResult = Assert.IsType<string>(result); //Asserting that the return is a String
            Assert.Contains("Action cannot be null", messageResult); //Asserting that message is equal as mentioned
        }

        [Fact]
        public async Task ChangeUserActiveStatus_returns_error_string_when_userid_is_in_incorrect_format()
        {
            //Arrange
            var controller = new UsersController(_bosAuthClient, _bosEmailClient, _configuration);

            dynamic inputData = new ExpandoObject();
            inputData.UserId = "someuserid";
            inputData.Action = "Some action";
            JObject data = JObject.FromObject(inputData);

            //Act
            var result = await controller.ChangeUserActiveStatus(data);

            //Assert
            var messageResult = Assert.IsType<string>(result); //Asserting that the return is a String
            Assert.Contains("The input is not a valid Base-64 string", messageResult); //Asserting that message is equal as mentioned
        }

        [Fact]
        public async Task ChangeUserActiveStatus_returns_error_string_when_action_is_invalid()
        {
            //Arrange
            var controller = new UsersController(_bosAuthClient, _bosEmailClient, _configuration);

            dynamic inputData = new ExpandoObject();
            inputData.UserId = "f4EcNSUeWjAwPTPMefW2659w3FX3EtblPwy0AMKAEFTgXn0o2OPxDjar0+K7zL/P1VIHQbODgHzP1hJpqcbpVQ==";
            inputData.Action = "Some action";
            JObject data = JObject.FromObject(inputData);

            //Act
            var result = await controller.ChangeUserActiveStatus(data);

            //Assert
            var messageResult = Assert.IsType<string>(result); //Asserting that the return is a String
            Assert.Contains("You are trying to perform an unrecognized operation", messageResult); //Asserting that message is equal as mentioned
        }

        

        [Fact]
        public async Task ChangeUserActiveStatus_returns_error_string_when_userid_is_invalid()
        {
            //Arrange
            var controller = new UsersController(_bosAuthClient, _bosEmailClient, _configuration);

            dynamic inputData = new ExpandoObject();
            inputData.UserId = "f4EcNSUeWjAwPTPMefW2659w3FX3EtblPwy0AMKAEFTgXn0o2OPxDjar0+K7zL/P1VIHQbODgHzP1hJpqcbpVQ==";
            inputData.Action = "activate";
            JObject data = JObject.FromObject(inputData);

            //Act
            var result = await controller.ChangeUserActiveStatus(data);

            //Assert
            var messageResult = Assert.IsType<string>(result); //Asserting that the return is a String
            Assert.Contains("User not found for Id", messageResult); //Asserting that message is equal as mentioned
        }

        private UsersController ConfigureController(bool isClaims, bool isSession, List<Module> modules)
        {
            var controller = new UsersController(_bosAuthClient, _bosEmailClient, _configuration);
            ClaimsPrincipal principal = new ClaimsPrincipal();
            if (isClaims)
            {
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
                principal = new ClaimsPrincipal(userIdentity);

                controller = new UsersController(_bosAuthClient, _bosEmailClient, _configuration)
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

                controller = new UsersController(_bosAuthClient, _bosEmailClient, _configuration)
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
