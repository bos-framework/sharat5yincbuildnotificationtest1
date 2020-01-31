using BOS.StarterCode.Controllers;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace BOS.StarterCode.Tests
{
    public class NavigationControllerTests
    {
        [Fact]
        public void NavigateToModule_redirects_to_dashboard_view_with_modelstateerror_when_code_is_null()
        {
            //Arrange
            var controller = new NavigationController();

            //Act
            var result = controller.NavigateToModule(Guid.NewGuid(), null, false);

            //Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result); //Asserting that the return is a View
            Assert.Equal("Dashboard", redirectResult.ControllerName); //Asserting that the returned Controller is "Dashboard"
            Assert.Equal("NavigationMenu", redirectResult.ActionName); //Asserting that the Action Methond of the controller is "NavigationMenu"
            Assert.True(controller.ViewData.ModelState.Count == 1);
        }

        [Fact]
        public void NavigateToModule_redirects_to_dashboard_view_when_code_is_custom()
        {
            //Arrange
            var controller = new NavigationController();

            //Act
            var result = controller.NavigateToModule(Guid.NewGuid(), "SAMPLE-MODULE-CODE", false);

            //Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result); //Asserting that the return is a View
            Assert.Equal("Dashboard", redirectResult.ControllerName); //Asserting that the returned Controller is "Dashboard"
            Assert.Equal("NavigationMenu", redirectResult.ActionName); //Asserting that the Action Methond of the controller is "NavigationMenu"
        }

        [Fact]
        public void NavigateToModule_redirects_to_predefined_view_when_code_is_bos_defined_profile()
        {
            //Arrange
            var controller = new NavigationController();

            //Act
            var result = controller.NavigateToModule(Guid.NewGuid(), "MYPFL", false);

            //Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result); //Asserting that the return is a View
            Assert.Equal("Profile", redirectResult.ControllerName); //Asserting that the returned Controller is "Profile"
            Assert.Equal("Index", redirectResult.ActionName); //Asserting that the Action Methond of the controller is "Index"
        }

        [Fact]
        public void NavigateToModule_redirects_to_predefined_view_when_code_is_bos_defined_users()
        {
            //Arrange
            var controller = new NavigationController();

            //Act
            var result = controller.NavigateToModule(Guid.NewGuid(), "USERS", false);

            //Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result); //Asserting that the return is a View
            Assert.Equal("Users", redirectResult.ControllerName); //Asserting that the returned Controller is "Users"
            Assert.Equal("Index", redirectResult.ActionName); //Asserting that the Action Methond of the controller is "Index"
        }

        [Fact]
        public void NavigateToModule_redirects_to_predefined_view_when_code_is_bos_defined_roles()
        {
            //Arrange
            var controller = new NavigationController();

            //Act
            var result = controller.NavigateToModule(Guid.NewGuid(), "ROLES", false);

            //Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result); //Asserting that the return is a View
            Assert.Equal("Roles", redirectResult.ControllerName); //Asserting that the returned Controller is "Roles"
            Assert.Equal("Index", redirectResult.ActionName); //Asserting that the Action Methond of the controller is "Index"
        }

        [Fact]
        public void NavigateToModule_redirects_to_predefined_view_when_code_is_bos_defined_permissions()
        {
            //Arrange
            var controller = new NavigationController();

            //Act
            var result = controller.NavigateToModule(Guid.NewGuid(), "PRMNS", false);

            //Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result); //Asserting that the return is a View
            Assert.Equal("Permissions", redirectResult.ControllerName); //Asserting that the returned Controller is "Permissions"
            Assert.Equal("Index", redirectResult.ActionName); //Asserting that the Action Methond of the controller is "Index"
        }
    }
}
