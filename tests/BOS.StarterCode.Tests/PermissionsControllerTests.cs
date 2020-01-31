using BOS.IA.Client;
using BOS.StarterCode.Controllers;
using BOS.StarterCode.Models.BOSModels.Permissions;
using BOS.StarterCode.Tests.HelperClass;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
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
    public class PermissionsControllerTests
    {
        private readonly IIAClient _bosIAClient;

        public PermissionsControllerTests()
        {
            var config = new ConfigurationBuilder().AddJsonFile("appsettings.test.json").Build();
            var bosAPIkey = config["BOS:APIkey"];

            HttpClient httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", bosAPIkey);
            httpClient.BaseAddress = new Uri(config["BOS:ServiceBaseURL"] + config["BOS:AuthRelativeURL"]);

            IAClient iaClient = new IAClient(httpClient);
            _bosIAClient = iaClient;
        }

        [Fact]
        public async Task FetchPermissions_returns_non_null_model_claims_is_empty()
        {
            //Arrange
            var controller = new PermissionsController(_bosIAClient);

            //Act
            var result = await controller.FetchPermissions(Guid.NewGuid().ToString(), null);

            //Assert
            var viewResult = Assert.IsType<ViewResult>(result); //Asserting that the return is a View
            dynamic model = Assert.IsAssignableFrom<ExpandoObject>(viewResult.ViewData.Model);
            Assert.True(model.OwnerId != null);
        }

        [Fact]
        public async Task FetchPermissions_returns_non_null_model_session_is_empty()
        {
            //Arrange
            var controller = new PermissionsController(_bosIAClient);

            //Act
            var result = await controller.FetchPermissions(Guid.NewGuid().ToString(), null);

            //Assert
            var viewResult = Assert.IsType<ViewResult>(result); //Asserting that the return is a View
            dynamic model = Assert.IsAssignableFrom<ExpandoObject>(viewResult.ViewData.Model);
            Assert.True(model.OwnerId != null);
        }

        [Fact]
        public async Task FetchPermissions_returns_non_null_model_claims_and_sessions_are_not_empty()
        {
            //Arrange
            var controller = ConfigureController();
            controller.ControllerContext.HttpContext.Items.Add("ModuleOperations", "ModuleOperations");

            //Act
            var result = await controller.FetchPermissions(Guid.NewGuid().ToString(), null);

            //Assert
            var viewResult = Assert.IsType<ViewResult>(result); //Asserting that the return is a View
            dynamic model = Assert.IsAssignableFrom<ExpandoObject>(viewResult.ViewData.Model);
            Assert.True(model.OwnerId != null);
        }

        [Fact]
        public async Task FetchPermissions_redirects_to_error_when_roleid_is_null()
        {
            //Arrange
            var controller = ConfigureController();
            controller.ControllerContext.HttpContext.Items.Add("ModuleOperations", "ModuleOperations");

            //Act
            var result = await controller.FetchPermissions(null, "roleName");

            //Assert
            var viewResult = Assert.IsType<ViewResult>(result); //Asserting that the return is a View
            dynamic model = Assert.IsAssignableFrom<ExpandoObject>(viewResult.ViewData.Model);
            Assert.True(model.CurrentModuleId == null);
            Assert.True(model.Username != null);
            Assert.True(model.Initials != null);
            Assert.True(model.Roles != null);

            Assert.Equal("Index", viewResult.ViewName); //Asserting that the returned Controller is "Index"
            Assert.True(controller.ViewData.ModelState.Count == 1);
        }

        [Fact]
        public async Task FetchPermissions_redirects_to_error_when_roleid_is_incorrect()
        {
            //Arrange
            var controller = ConfigureController();
            controller.ControllerContext.HttpContext.Items.Add("ModuleOperations", "ModuleOperations");

            //Act
            var result = await controller.FetchPermissions(null, "roleName");

            //Assert
            var viewResult = Assert.IsType<ViewResult>(result); //Asserting that the return is a View
            dynamic model = Assert.IsAssignableFrom<ExpandoObject>(viewResult.ViewData.Model);
            Assert.True(model.CurrentModuleId == null);
            Assert.True(model.Username != null);
            Assert.True(model.Initials != null);
            Assert.True(model.Roles != null);

            Assert.Equal("Index", viewResult.ViewName); //Asserting that the returned Controller is "Index"
            Assert.True(controller.ViewData.ModelState.Count == 1);
        }

        [Fact]
        public void BackToRoles_returns_roles_view()
        {
            //Arrange
            var controller = new PermissionsController(_bosIAClient);

            //Act
            var result = controller.BackToRoles();

            //Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result); //Asserting that the return is a View
            Assert.Equal("Roles", redirectResult.ControllerName); //Asserting that the returned Controller is "Role"
            Assert.Equal("Index", redirectResult.ActionName); //Asserting that the Action Methond of the controller is "Index"
        }

        [Fact]
        public async Task UpdatePermissions_returns_error_string_when_data_is_null()
        {
            //Arrange
            var controller = new PermissionsController(_bosIAClient);

            //Act
            var result = await controller.UpdatePermissions(null);

            //Assert
            var messageResult = Assert.IsType<string>(result); //Asserting that the return is a String
            Assert.Equal("Permission set cannot be empty", messageResult); //Asserting that message is equal as mentioned
        }

        [Fact]
        public async Task UpdatePermissions_returns_error_string_when_modules_does_not_exist()
        {
            //Arrange
            var controller = new PermissionsController(_bosIAClient);

            dynamic permissionObj = new ExpandoObject();
            JObject data = JObject.FromObject(permissionObj);

            //Act
            var result = await controller.UpdatePermissions(data);

            //Assert
            var messageResult = Assert.IsType<string>(result); //Asserting that the return is a String
            Assert.Contains("Object reference not set to an instance of an object", messageResult); //Asserting that message is equal as mentioned
        }

        [Fact]
        public async Task UpdatePermissions_returns_error_string_when_modules_is_null()
        {
            //Arrange
            var controller = new PermissionsController(_bosIAClient);

            dynamic permissionObj = new ExpandoObject();
            permissionObj.Modules = null;
            JObject data = JObject.FromObject(permissionObj);  

            //Act
            var result = await controller.UpdatePermissions(data);

            //Assert
            var messageResult = Assert.IsType<string>(result); //Asserting that the return is a String
            Assert.Contains("Value cannot be null", messageResult); //Asserting that message is equal as mentioned
        }

        [Fact]
        public async Task UpdatePermissions_returns_error_string_when_operations_does_not_exist()
        {
            //Arrange
            var controller = new PermissionsController(_bosIAClient);

            dynamic permissionObj = new ExpandoObject();
            JObject data = JObject.FromObject(permissionObj);
            permissionObj.Modules = new List<PermissionsSet>(); 
            //Act
            var result = await controller.UpdatePermissions(data);

            //Assert
            var messageResult = Assert.IsType<string>(result); //Asserting that the return is a String
            Assert.Contains("Object reference not set to an instance of an object", messageResult); //Asserting that message is equal as mentioned
        }

        [Fact]
        public async Task UpdatePermissions_returns_error_string_when_operations_is_null()
        {
            //Arrange
            var controller = new PermissionsController(_bosIAClient);

            dynamic permissionObj = new ExpandoObject();
            permissionObj.Modules = new List<PermissionsSet>();
            permissionObj.Operations = null;
            JObject data = JObject.FromObject(permissionObj);

            //Act
            var result = await controller.UpdatePermissions(data);

            //Assert
            var messageResult = Assert.IsType<string>(result); //Asserting that the return is a String
            Assert.Contains("Value cannot be null", messageResult); //Asserting that message is equal as mentioned
        }

        [Fact]
        public async Task UpdatePermissions_returns_error_string_when_ownerId_does_not_exist()
        {
            //Arrange
            var controller = new PermissionsController(_bosIAClient);

            dynamic permissionObj = new ExpandoObject();
            permissionObj.Modules = new List<PermissionsSet>();
            permissionObj.Operations = new List<PermissionsOperation>();
            JObject data = JObject.FromObject(permissionObj);

            //Act
            var result = await controller.UpdatePermissions(data);

            //Assert
            var messageResult = Assert.IsType<string>(result); //Asserting that the return is a String
            Assert.Contains("Object reference not set to an instance of an object", messageResult); //Asserting that message is equal as mentioned
        }

        [Fact]
        public async Task UpdatePermissions_returns_error_string_when_ownerId_is_null()
        {
            //Arrange
            var controller = new PermissionsController(_bosIAClient);

            dynamic permissionObj = new ExpandoObject();
            permissionObj.Modules = new List<PermissionsSet>();
            permissionObj.Operations = new List<PermissionsOperation>();
            permissionObj.OwnerId = null;
            JObject data = JObject.FromObject(permissionObj);

            //Act
            var result = await controller.UpdatePermissions(data);

            //Assert
            var messageResult = Assert.IsType<string>(result); //Asserting that the return is a String
            Assert.Contains("Unrecognized Guid format", messageResult); //Asserting that message is equal as mentioned
        }

        [Fact]
        public async Task UpdatePermissions_returns_error_string_when_data_passed_is_incorrect()
        {
            //Arrange
            var controller = new PermissionsController(_bosIAClient);

            dynamic permissionObj = new ExpandoObject();
            permissionObj.Modules = new List<PermissionsSet>();
            permissionObj.Operations = new List<PermissionsOperation>();
            permissionObj.OwnerId = Guid.NewGuid();
            JObject data = JObject.FromObject(permissionObj);

            //Act
            var result = await controller.UpdatePermissions(data);

            //Assert
            var messageResult = Assert.IsType<string>(result); //Asserting that the return is a String
            //Assert.Contains("Unrecognized Guid format", messageResult); //Asserting that message is equal as mentioned
        }

        private PermissionsController ConfigureController()
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
            ClaimsPrincipal principal = new ClaimsPrincipal(userIdentity);

            //Mocking a session
            MockHttpSession mockSession = new MockHttpSession();
            mockSession["Key"] = "KeySession";

            var controller = new PermissionsController(_bosIAClient)
            {
                ControllerContext = new ControllerContext()
                {
                    HttpContext = new DefaultHttpContext() { User = principal, Session = mockSession }
                }
            };

            return controller;
        }
    }
}
