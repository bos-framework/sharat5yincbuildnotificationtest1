using BOS.StarterCode.Controllers;
using BOS.StarterCode.Tests.HelperClass;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Security.Claims;
using Xunit;

namespace BOS.StarterCode.Tests
{
    public class DashboardControllerTests
    {
        [Fact]
        public void Index_returns_model_with_null_currentmoduleid_when_claims_is_empty()
        {
            //Arrange
            var controller = new DashboardController();

            //Act
            var result = controller.Index();

            //Assert
            var viewResult = Assert.IsType<ViewResult>(result); //Asserting that the return is a View
            dynamic model = Assert.IsAssignableFrom<ExpandoObject>(viewResult.ViewData.Model);
            Assert.True(model.CurrentModuleId == null);
        }

        [Fact]
        public void Index_returns_model_with_null_currentmoduleid_when_session_is_empty()
        {
            //Arrange
            var controller = new DashboardController();

            //Act
            var result = controller.Index();

            //Assert
            var viewResult = Assert.IsType<ViewResult>(result); //Asserting that the return is a View
            dynamic model = Assert.IsAssignableFrom<ExpandoObject>(viewResult.ViewData.Model);
            Assert.True(model.CurrentModuleId == null);
        }

        [Fact]
        public void Index_returns_non_null_model_claims_and_sessions_are_not_empty()
        {
            //Arrange
            var controller = ConfigureController();
            controller.ControllerContext.HttpContext.Items.Add("ModuleOperations", "ModuleOperations");

            //Act
            var result = controller.Index();

            //Assert
            var viewResult = Assert.IsType<ViewResult>(result); //Asserting that the return is a View
            dynamic model = Assert.IsAssignableFrom<ExpandoObject>(viewResult.ViewData.Model);
            Assert.True(model.CurrentModuleId == null);
            Assert.True(model.Username != null);
            Assert.True(model.Initials != null);
            Assert.True(model.Roles != null);
        }

        [Fact]
        public void NavigationMenu_redirects_to_error_view_when_moduleid_is_null()
        {
            //Arrange
            var controller = new DashboardController();

            //Act
            var result = controller.NavigationMenu(null);

            //Assert
            var viewResult = Assert.IsType<ViewResult>(result); //Asserting that the return is a View
            Assert.Equal("ErrorPage", viewResult.ViewName); //Asserting that the returned view is "Index"
        }

        [Fact]
        public void NavigationMenu_redirects_to_index_view_when_moduleid_is_not_null()
        {
            //Arrange
            var controller = ConfigureController();
            controller.ControllerContext.HttpContext.Items.Add("ModuleOperations", "ModuleOperations");

            //Act
            var result = controller.NavigationMenu(Guid.NewGuid().ToString());

            //Assert
            var viewResult = Assert.IsType<ViewResult>(result); //Asserting that the return is a View
            dynamic model = Assert.IsAssignableFrom<ExpandoObject>(viewResult.ViewData.Model);
            Assert.Equal("Index", viewResult.ViewName); //Asserting that the returned view is "Error Page"

            Assert.True(model.CurrentModuleId != null);
            Assert.True(model.Username != null);
            Assert.True(model.Initials != null);
            Assert.True(model.Roles != null);
        }


        private DashboardController ConfigureController()
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

            var controller = new DashboardController()
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
