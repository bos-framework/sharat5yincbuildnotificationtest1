using BOS.Auth.Client;
using BOS.StarterCode.Controllers;
using BOS.StarterCode.Models.BOSModels;
using BOS.StarterCode.Tests.HelperClass;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CSharp.RuntimeBinder;
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
    public class RolesControllerTests
    {
        private readonly AuthClient _bosAuthClient;

        public RolesControllerTests()
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
            var controller = new RolesController(_bosAuthClient);

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
            var controller = ConfigureController(true, false, null, null);

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

            var controller = ConfigureController(true, true, null, new List<Module> { module });

            //Act
            var result = await controller.Index();

            //Assert
            var viewResult = Assert.IsType<ViewResult>(result); //Asserting that the return is a View
            dynamic model = Assert.IsAssignableFrom<ExpandoObject>(viewResult.ViewData.Model);
            Assert.Null(model.CurrentModuleId);
            Assert.True(model.Operations.Length == 0);
            Assert.NotNull(model.ModuleOperations);
            Assert.NotNull(model.Initials);
            Assert.NotNull(model.Username);
            Assert.NotNull(model.Roles);
            Assert.NotNull(model.RoleList);
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
                Code = "ROLES",
                Deleted = false,
                Id = moduleId,
                Name = "Module Name",
                Operations = new List<IA.Client.ClientModels.IOperation> { operation }
            };

            var controller = ConfigureController(true, true, null, new List<Module> { module });

            //Act
            var result = await controller.Index();

            //Assert
            var viewResult = Assert.IsType<ViewResult>(result); //Asserting that the return is a View
            dynamic model = Assert.IsAssignableFrom<ExpandoObject>(viewResult.ViewData.Model);
            Assert.NotNull(model.CurrentModuleId);
            Assert.True(model.Operations.Length > 0);
            Assert.NotNull(model.ModuleOperations);
            Assert.NotNull(model.Initials);
            Assert.NotNull(model.Username);
            Assert.NotNull(model.Roles);
            Assert.NotNull(model.RoleList);
        }

        [Fact]
        public async Task AddNewRole_returns_null_when_claims_is_empty()
        {
            //Arrange
            var controller = new RolesController(_bosAuthClient);

            //Act
            var result = await controller.AddNewRole();

            //Assert
            var viewResult = Assert.IsType<ViewResult>(result); //Asserting that the return is a View
            Assert.Equal("AddRole", viewResult.ViewName); //Asserting that the view is "AddRole"
            Assert.Null(viewResult.ViewData.Model); //Asserting that the returned model is null
        }

        [Fact]
        public async Task AddNewRole_returns_null_when_session_is_empty()
        {
            //Arrange
            var controller = new RolesController(_bosAuthClient);

            //Act
            var result = await controller.AddNewRole();

            //Assert
            var viewResult = Assert.IsType<ViewResult>(result); //Asserting that the return is a View
            Assert.Equal("AddRole", viewResult.ViewName); //Asserting that the view is "AddRole"
            Assert.Null(viewResult.ViewData.Model); //Asserting that the returned model is null
        }

        [Fact]
        public async Task AddNewRole_returns_non_null_model_when_claims_and_sessions_are_not_empty()
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
                Code = "ROLES",
                Deleted = false,
                Id = moduleId,
                Name = "Module Name",
                Operations = new List<IA.Client.ClientModels.IOperation> { operation }
            };

            var controller = ConfigureController(true, true, null, new List<Module> { module });

            //Act
            var result = await controller.AddNewRole();

            //Assert
            var viewResult = Assert.IsType<ViewResult>(result); //Asserting that the return is a View
            Assert.Equal("AddRole", viewResult.ViewName);

            dynamic model = Assert.IsAssignableFrom<ExpandoObject>(viewResult.ViewData.Model);
            Assert.NotNull(model.CurrentModuleId);
            Assert.True(model.Operations.Length > 0);
            Assert.NotNull(model.ModuleOperations);
            Assert.NotNull(model.Initials);
            Assert.NotNull(model.Username);
            Assert.NotNull(model.Roles);
            Assert.NotNull(model.RoleList);
        }

        [Fact]
        public async Task EditRole_returns_index_when_roleid_is_null()
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
                Code = "ROLES",
                Deleted = false,
                Id = moduleId,
                Name = "Module Name",
                Operations = new List<IA.Client.ClientModels.IOperation> { operation }
            };

            var controller = ConfigureController(true, true, null, new List<Module> { module });

            //Act
            var result = await controller.EditRole(null);

            //Assert
            var viewResult = Assert.IsType<ViewResult>(result); //Asserting that the return is a View
            Assert.Equal("Index", viewResult.ViewName); //Asserting that that the returned view is "Index"
            Assert.True(controller.ViewData.ModelState.Count == 1); //Asserts that there is a ModelStateError object

            dynamic model = Assert.IsAssignableFrom<ExpandoObject>(viewResult.ViewData.Model);
            Assert.NotNull(model.CurrentModuleId);
            Assert.True(model.Operations.Length > 0);
            Assert.NotNull(model.ModuleOperations);
            Assert.NotNull(model.Initials);
            Assert.NotNull(model.Username);
            Assert.NotNull(model.Roles);
            Assert.NotNull(model.RoleList);
        }

        [Fact]
        public async Task EditRole_returns_errorpage_when_claims_is_empty()
        {
            //Arrange
            var controller = ConfigureController(false, false, null, null);

            //Act
            var result = await controller.EditRole(Guid.NewGuid().ToString());

            //Assert
            var viewResult = Assert.IsType<ViewResult>(result); //Asserting that the return is a View
            Assert.Equal("ErrorPage", viewResult.ViewName); //Asserting that the returned view is ErrorPage

            dynamic model = Assert.IsAssignableFrom<ExpandoObject>(viewResult.ViewData.Model);
            Assert.NotNull(model.Message);
            Assert.Contains("No role exists with the provided Id", model.Message);
        }

        [Fact]
        public async Task EditRole_returns_null_when_session_is_empty()
        {
            //Arrange
            var controller = ConfigureController(true, false, null, null);

            //Act
            var result = await controller.EditRole(Guid.NewGuid().ToString());

            //Assert
            var viewResult = Assert.IsType<ViewResult>(result); //Asserting that the return is a View
            Assert.Equal("ErrorPage", viewResult.ViewName); //Asserting that the returned view is ErrorPage

            dynamic model = Assert.IsAssignableFrom<ExpandoObject>(viewResult.ViewData.Model);
            Assert.NotNull(model.Message);
            Assert.Contains("No role exists with the provided Id", model.Message);
        }

        [Fact]
        public async Task EditRole_returns_errorpage_when_claims_and_sessions_are_not_empty()
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
                Code = "ROLES",
                Deleted = false,
                Id = moduleId,
                Name = "Module Name",
                Operations = new List<IA.Client.ClientModels.IOperation> { operation }
            };

            var controller = ConfigureController(true, true, null, new List<Module> { module });

            //Act
            var result = await controller.EditRole(Guid.NewGuid().ToString());

            //Assert
            var viewResult = Assert.IsType<ViewResult>(result); //Asserting that the return is a View
            Assert.Equal("ErrorPage", viewResult.ViewName); //Asserting that the returned view is ErrorPage

            dynamic model = Assert.IsAssignableFrom<ExpandoObject>(viewResult.ViewData.Model);
            Assert.NotNull(model.Message);
            Assert.Contains("No role exists with the provided Id", model.Message);
        }

        [Fact]
        public async Task EditRole_returns_errorpage_when_roleid_is_incorrect()
        {
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
                Code = "ROLES",
                Deleted = false,
                Id = moduleId,
                Name = "Module Name",
                Operations = new List<IA.Client.ClientModels.IOperation> { operation }
            };

            var controller = ConfigureController(true, true, null, new List<Module> { module });

            //Act
            var result = await controller.EditRole(Guid.NewGuid().ToString());

            //Assert
            var viewResult = Assert.IsType<ViewResult>(result); //Asserting that the return is a View
            Assert.Equal("ErrorPage", viewResult.ViewName); //Asserting that the returned view is ErrorPage

            dynamic model = Assert.IsAssignableFrom<ExpandoObject>(viewResult.ViewData.Model);
            Assert.NotNull(model.Message);
            Assert.Contains("No role exists with the provided Id", model.Message);
        }

        [Fact]
        public void RoleManagePermissions_redirects_to_permissions_view_when_roleid_is_null()
        {
            //Arrange
            var controller = new RolesController(_bosAuthClient);

            //Act
            var result = controller.RoleManagePermissions(null, null);

            //Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result); //Asserting that the return is a View
            Assert.Equal("Permissions", redirectResult.ControllerName); //Asserting that the returned Controller is "Permissions"
            Assert.Equal("FetchPermissions", redirectResult.ActionName); //Asserting that the Action Methond of the controller is "FetchPermissions"
        }

        [Fact]
        public void RoleManagePermissions_redirects_to_permissions_view_when_rolename_is_null()
        {
            //Arrange
            var controller = new RolesController(_bosAuthClient);

            //Act
            var result = controller.RoleManagePermissions(null, null);

            //Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result); //Asserting that the return is a View
            Assert.Equal("Permissions", redirectResult.ControllerName); //Asserting that the returned Controller is "Permissions"
            Assert.Equal("FetchPermissions", redirectResult.ActionName); //Asserting that the Action Methond of the controller is "FetchPermissions"
        }

        [Fact]
        public void RoleManagePermissions_redirects_to_permissions_view()
        {
            //Arrange
            var controller = new RolesController(_bosAuthClient);

            //Act
            var result = controller.RoleManagePermissions(Guid.NewGuid().ToString(), "RoleName");

            //Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result); //Asserting that the return is a View
            Assert.Equal("Permissions", redirectResult.ControllerName); //Asserting that the returned Controller is "Permissions"
            Assert.Equal("FetchPermissions", redirectResult.ActionName); //Asserting that the Action Methond of the controller is "FetchPermissions"
        }

        [Fact]
        public async Task AddRole_returns_error_string_when_data_is_null()
        {
            //Arrange
            var controller = new RolesController(_bosAuthClient);

            //Act
            var result = await controller.AddRole(null);

            //Assert
            var messageResult = Assert.IsType<string>(result); //Asserting that the return is a String
            Assert.Contains("Data cannot be null", messageResult); //Asserting that message is equal as mentioned
        }

        [Fact]
        public async Task AddRole_returns_string_when_role_is_null()
        {
            //Arrange
            var controller = new RolesController(_bosAuthClient);

            dynamic roleObj = new ExpandoObject();
            JObject data = JObject.FromObject(roleObj);

            //Act
            var result = await controller.AddRole(data);

            //Assert
            var messageResult = Assert.IsType<string>(result); //Asserting that the return is a String
            Assert.Contains("The data for role is inaccurate", messageResult); //Asserting that message is equal as mentioned
        }

        [Fact]
        public async Task AddRole_returns_error_string_when_role_has_same_name()
        {
            //Arrange
            var controller = new RolesController(_bosAuthClient);

            Role role = new Role
            {
                Id = new Guid(),
                Name = "Admin"
            };

            dynamic roleObj = new ExpandoObject();
            roleObj.Role = role;

            JObject data = JObject.FromObject(roleObj);

            //Act
            var result = await controller.AddRole(data);

            //Assert
            var messageResult = Assert.IsType<string>(result); //Asserting that the return is a String
            Assert.Contains("A role with that name already exists", messageResult); //Asserting that message is equal as mentioned
        }

        [Fact]
        public async Task AddRole_returns_error_string_when_role_has_special_characters()
        {
            //Arrange
            var controller = new RolesController(_bosAuthClient);

            Role role = new Role
            {
                Id = new Guid(),
                Name = "Admin!@#$"
            };

            dynamic roleObj = new ExpandoObject();
            roleObj.Role = role;

            JObject data = JObject.FromObject(roleObj);

            //Act
            var result = await controller.AddRole(data);

            //Assert
            var messageResult = Assert.IsType<string>(result); //Asserting that the return is a String
            Assert.Contains("Name cannot be contain special characters", messageResult); //Asserting that message is equal as mentioned
        }

        [Fact]
        public async Task UpdateRole_returns_error_string_when_data_is_null()
        {
            //Arrange
            var controller = new RolesController(_bosAuthClient);

            //Act
            var result = await controller.UpdateRole(null);

            //Assert
            var messageResult = Assert.IsType<string>(result); //Asserting that the return is a String
            Assert.Contains("Data cannot be null", messageResult); //Asserting that message is equal as mentioned
        }

        [Fact]
        public async Task UpdateRole_returns_error_string_when_role_is_null()
        {
            //Arrange
            var controller = new RolesController(_bosAuthClient);

            dynamic roleObj = new ExpandoObject();
            JObject data = JObject.FromObject(roleObj);

            //Act
            var result = await controller.UpdateRole(data);

            //Assert
            var messageResult = Assert.IsType<string>(result); //Asserting that the return is a String
            Assert.Contains("The input data was inaccurate", messageResult); //Asserting that message is equal as mentioned
        }

        [Fact]
        public async Task UpdateRole_returns_error_string_when_roleId_is_null()
        {
            //Arrange
            var controller = new RolesController(_bosAuthClient);

            dynamic roleObj = new ExpandoObject();
            roleObj.Role = new Role(); 

            JObject data = JObject.FromObject(roleObj);

            //Act
            var result = await controller.UpdateRole(data);

            //Assert
            var messageResult = Assert.IsType<string>(result); //Asserting that the return is a String
            Assert.Contains("The input data was inaccurate", messageResult); //Asserting that message is equal as mentioned
        }

        [Fact]
        public async Task UpdateRole_returns_error_string_when_role_has_invalid_id()
        {
            //Arrange
            var controller = new RolesController(_bosAuthClient);

            Role role = new Role
            {
                Id = new Guid(),
                Name = "Some Role"
            };

            dynamic roleObj = new ExpandoObject();
            roleObj.Role = role;

            JObject data = JObject.FromObject(roleObj);

            //Act
            var result = await controller.UpdateRole(data);

            //Assert
            var messageResult = Assert.IsType<string>(result); //Asserting that the return is a String
            Assert.Contains("The input data was inaccurate", messageResult); //Asserting that message is equal as mentioned
        }

        [Fact]
        public async Task UpdateUserRoles_returns_string_when_updatedroles_is_null()
        {
            //Arrange
            var controller = new RolesController(_bosAuthClient);

            //Act
            var result = await controller.UpdateUserRoles(null);

            //Assert
            var messageResult = Assert.IsType<string>(result); //Asserting that the return is a String
            Assert.Contains("Roles to associate with the user cannot be empty", messageResult); //Asserting that message is equal as mentioned
        }

        [Fact]
        public async Task UpdateUserRoles_returns_string_when_updatedroles_count_is_zero()
        {
            //Arrange
            var controller = new RolesController(_bosAuthClient);
            List<Role> updatedRoles = new List<Role>();

            //Act
            var result = await controller.UpdateUserRoles(updatedRoles);

            //Assert
            var messageResult = Assert.IsType<string>(result); //Asserting that the return is a String
            Assert.Contains("Roles to associate with the user cannot be empty", messageResult); //Asserting that message is equal as mentioned
        }

        [Fact]
        public async Task UpdateUserRoles_returns_error_string_when_claims_is_null()
        {
            //Arrange
            var controller = ConfigureController(false, false, null, null); //No claims are being set here

            List<Role> updatedRoles = new List<Role>
            {
                new Role
                {
                    Id = Guid.NewGuid(),
                    Name = "Some Role"
                }
            };
            //Act
            var result = await controller.UpdateUserRoles(updatedRoles);

            //Assert
            var messageResult = Assert.IsType<string>(result); //Asserting that the return is a String
            Assert.Contains("No user with the provided Id was found", messageResult); //Asserting that message is equal as mentioned
        }

        [Fact]
        public async Task UpdateUserRoles_returns_error_string_when_userid_is_null()
        {
            //Arrange
            var controller = ConfigureController(true, false, "userId", null); //Setting the claims, but without UserId

            List<Role> updatedRoles = new List<Role>
            {
                new Role
                {
                    Id = Guid.NewGuid(),
                    Name = "Some Role"
                }
            };
            //Act
            var result = await controller.UpdateUserRoles(updatedRoles);

            //Assert
            var messageResult = Assert.IsType<string>(result); //Asserting that the return is a String
            Assert.Contains("No user with the provided Id was found", messageResult); //Asserting that message is equal as mentioned
        }

        [Fact]
        public async Task UpdateUserRoles_returns_error_string_when_userid_is_incorrect()
        {
            //Arrange
            var controller = ConfigureController(true, false, null, null); //Setting the claims, with UserId

            List<Role> updatedRoles = new List<Role>
            {
                new Role
                {
                    Id = Guid.NewGuid(),
                    Name = "Some Role"
                }
            };
            //Act
            var result = await controller.UpdateUserRoles(updatedRoles);

            //Assert
            var messageResult = Assert.IsType<string>(result); //Asserting that the return is a String
            Assert.Contains("No user with the provided Id was found", messageResult); //Asserting that message is equal as mentioned
        }

        [Fact]
        public async Task UpdateUserRoles_returns_error_string_when_roleid_is_incorrect()
        {
            //Arrange
            var controller = ConfigureController(true, false, null, null); //Setting the claims, with UserId

            List<Role> updatedRoles = new List<Role>
            {
                new Role
                {
                    Id = Guid.NewGuid(),
                    Name = "Some Role"
                }
            };
            //Act
            var result = await controller.UpdateUserRoles(updatedRoles);

            //Assert
            var messageResult = Assert.IsType<string>(result); //Asserting that the return is a String
            Assert.Contains("No user with the provided Id was found", messageResult); //Asserting that message is equal as mentioned
        }

        [Fact]
        public async Task UpdateUserRolesByAdmin_returns_error_string_when_data_is_null()
        {
            //Arrange
            var controller = new RolesController(_bosAuthClient);

            //Act
            var result = await controller.UpdateUserRolesByAdmin(null);

            //Assert
            var messageResult = Assert.IsType<string>(result); //Asserting that the return is a String
            Assert.Contains("Roles to associate with the user cannot be empty", messageResult); //Asserting that message is equal as mentioned
        }

        [Fact]
        public async Task UpdateUserRolesByAdmin_returns_error_string_when_updatedroles_is_null()
        {
            //Arrange
            var controller = new RolesController(_bosAuthClient);
            JObject data = new JObject();

            //Act
            var result = await controller.UpdateUserRolesByAdmin(null);

            //Assert
            var messageResult = Assert.IsType<string>(result); //Asserting that the return is a String
            Assert.Contains("Roles to associate with the user cannot be empty", messageResult); //Asserting that message is equal as mentioned
        }

        [Fact]
        public async Task UpdateUserRolesByAdmin_returns_error_string_when_userId_is_null()
        {
            //Arrange
            var controller = ConfigureController(false, false, null, null); //No claims are being set here
            List<Role> updatedRoles = new List<Role>
            {
                new Role
                {
                    Id = Guid.NewGuid(),
                    Name = "Some Role"
                }
            };

            dynamic roleObj = new ExpandoObject();
            roleObj.UpdatedRoles = updatedRoles;

            JObject data = JObject.FromObject(roleObj);

            //Act
            var result = await controller.UpdateUserRolesByAdmin(data);

            //Assert
            var messageResult = Assert.IsType<string>(result); //Asserting that the return is a String
            Assert.Contains("Incorrect user id", messageResult); //Asserting that message is equal as mentioned
        }

        [Fact]
        public async Task UpdateUserRolesByAdmin_returns_error_string_when_userid_is_null()
        {
            //Arrange
            var controller = ConfigureController(true, false, "userId", null); //No claims are being set here
            List<Role> updatedRoles = new List<Role>
            {
                new Role
                {
                    Id = Guid.NewGuid(),
                    Name = "Some Role"
                }
            };

            dynamic roleObj = new ExpandoObject();
            roleObj.UpdatedRoles = updatedRoles;

            JObject data = JObject.FromObject(roleObj);

            //Act
            var result = await controller.UpdateUserRolesByAdmin(data);

            //Assert
            var messageResult = Assert.IsType<string>(result); //Asserting that the return is a String
            Assert.Contains("Incorrect user id", messageResult); //Asserting that message is equal as mentioned
        }

        [Fact]
        public async Task UpdateUserRolesByAdmin_returns_error_string_when_userid_is_empty()
        {
            //Arrange
            var controller = ConfigureController(true, false, "userId", null); //No claims are being set here
            List<Role> updatedRoles = new List<Role>
            {
                new Role
                {
                    Id = Guid.NewGuid(),
                    Name = "Some Role"
                }
            };

            dynamic roleObj = new ExpandoObject();
            roleObj.UpdatedRoles = updatedRoles;
            roleObj.UserId = Guid.Empty;

            JObject data = JObject.FromObject(roleObj);

            //Act
            var result = await controller.UpdateUserRolesByAdmin(data);

            //Assert
            var messageResult = Assert.IsType<string>(result); //Asserting that the return is a String
            Assert.Contains("The input is not a valid Base-64 string", messageResult); //Asserting that message is equal as mentioned
        }

        [Fact]
        public async Task UpdateUserRolesByAdmin_returns_error_string_when_userid_is_in_incorrect_format()
        {
            //Arrange
            var controller = ConfigureController(true, false, "userId", null); //No claims are being set here
            List<Role> updatedRoles = new List<Role>
            {
                new Role
                {
                    Id = Guid.NewGuid(),
                    Name = "Some Role"
                }
            };

            dynamic roleObj = new ExpandoObject();
            roleObj.UpdatedRoles = updatedRoles;
            roleObj.UserId = "someuserid";

            JObject data = JObject.FromObject(roleObj);

            //Act
            var result = await controller.UpdateUserRolesByAdmin(data);

            //Assert
            var messageResult = Assert.IsType<string>(result); //Asserting that the return is a String
            Assert.Contains("The input is not a valid Base-64 string", messageResult); //Asserting that message is equal as mentioned
        }

        [Fact]
        public async Task UpdateUserRolesByAdmin_returns_error_string_when_role_count_is_zero()
        {
            //Arrange
            var controller = ConfigureController(true, false, null, null); //No claims are being set here
            List<Role> updatedRoles = new List<Role>();
            
            dynamic roleObj = new ExpandoObject();
            roleObj.UpdatedRoles = updatedRoles;

            JObject data = JObject.FromObject(roleObj);

            //Act
            var result = await controller.UpdateUserRolesByAdmin(data);

            //Assert
            var messageResult = Assert.IsType<string>(result); //Asserting that the return is a String
            Assert.Contains("Roles to associate with the user cannot be empty", messageResult); //Asserting that message is equal as mentioned
        }

        [Fact]
        public async Task DeleteRole_returns_error_string_when_role_is_null()
        {
            //Arrange
            var controller = ConfigureController(false, false, null, null); //No claims are being set here
            
            //Act
            var result = await controller.DeleteRole(null);

            //Assert
            var messageResult = Assert.IsType<string>(result); //Asserting that the return is a String
            Assert.Contains("The selected role has an inaccurate Id", messageResult); //Asserting that message is equal as mentioned
        }

        [Fact]
        public async Task DeleteRole_returns_error_string_when_role_is_in_incorrect_format()
        {
            //Arrange
            var controller = ConfigureController(false, false, null, null); //No claims are being set here

            //Act
            var result = await controller.DeleteRole("roleid-1234");

            //Assert
            var messageResult = Assert.IsType<string>(result); //Asserting that the return is a String
            Assert.Contains("Guid should contain 32 digits with 4 dashes (xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx).", messageResult); //Asserting that message is equal as mentioned
        }

        [Fact]
        public async Task DeleteRole_returns_error_string_when_role_is_invalid()
        {
            //Arrange
            var controller = ConfigureController(false, false, null, null); //No claims are being set here

            //Act
            var result = await controller.DeleteRole(Guid.NewGuid().ToString());

            //Assert
            var messageResult = Assert.IsType<string>(result); //Asserting that the return is a String
            Assert.Contains("No role exists for the provided Id", messageResult); //Asserting that message is equal as mentioned
        }

        private RolesController ConfigureController(bool isClaims, bool isSession, string excludeClaims, List<Module> modules)
        {
            var controller = new RolesController(_bosAuthClient);
            ClaimsPrincipal principal = new ClaimsPrincipal();
            if (isClaims)
            {
                //Mocking the user claims
                var claims = new List<Claim>();
                if (excludeClaims == null)
                {
                    claims = new List<Claim>{
                        new Claim("CreatedOn", DateTime.UtcNow.ToString()),
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
                                    new Claim("CreatedOn", DateTime.UtcNow.ToString()),
                                    new Claim("Email", "some@email.com"),
                                    new Claim("Initials", "JD"),
                                    new Claim("Name", "John Doe"),
                                    new Claim("Role", "Admin"),
                                    new Claim("Username", "SomeUserName"),
                                    new Claim("IsAuthenticated", "True")
                            };
                            break;
                    }
                }

                var userIdentity = new ClaimsIdentity(claims, "Auth");
                principal = new ClaimsPrincipal(userIdentity);

                controller = new RolesController(_bosAuthClient)
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

                controller = new RolesController(_bosAuthClient)
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
