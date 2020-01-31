/* --------------------------- SIGNOUT USER -----------------------------------*/
//Called with the user clicks on the SignOut button
function InitiateSignOut() {
    //Method to display the confirmation box with a message and data to be passed.
    DisplayConfirmationDialog({
        Message: "Are you sure you want to signout?",
        CallFrom: "SignOut",
        OkData: { Label: "Yes", Data: null },
        CancelData: { Label: "No", Data: null }
    });
}

//Called after the user confirms the SignOut operation
function TriggerSignOut() {
    $.ajax({
        type: "POST",
        url: "/Auth/SignOut",
        success: function () {
            location.reload(); //Reloading the page on success, checks for the claims object on the server side and redirects to the login page if none is found.
        },
        failure: function (response) {
            console.log(response.Message);
        },
        error: function (response) {
            console.log(response.Message);
        }
    });
}
/* --------------------------- END SIGNOUT USER -----------------------------------*/

/* --------------------------- UPDATE PROFILE -----------------------------------*/
//Called when the user clicks on 'Update' button on the Profile view
function UpdateProfileInfo() {
    //Capturing the data entered into variables
    var firstName = $("#profileFirstName")[0].value;
    var lastName = $("#profileLastName")[0].value;
    var email = $("#profileEmail")[0].value;

    var flag = true; //Used for validation of form-data

    //Checking is firstName is null or empty
    if (firstName === null || firstName === "") {
        $("#spanFirstName")[0].innerHTML = "Required";
        flag = false;
    }

    //Checking is lastName is null or empty
    if (lastName === null || lastName === "") {
        $("#spanLastName")[0].innerHTML = "Required";
        flag = false;
    }

    //Checking is email is null or empty
    if (email === null || email === "") {
        $("#spanEmail")[0].innerHTML = "Required";
        flag = false;
    }
    else {
        //Checking if the entered email is in valid format
        var isEmailValid = IsEmailValid(email);
        if (!isEmailValid) {
            $("#spanEmail")[0].innerHTML = "Incorrect email format";
            flag = false;
        }
    }

    //Only if the form data is valid, make the API call to update the information
    if (flag) {
        var data = new Object();
        data.FirstName = firstName;
        data.LastName = lastName;
        data.Email = email;

        $.ajax({
            type: "POST",
            url: "/Profile/UpdateProfileInfo",
            data: JSON.stringify(data),
            contentType: 'application/json; charset=utf-8',
            success: function (response) {
                //Notify the user either on success or failure
                if (response.indexOf("success") > -1) {
                    DisplayDialog({ Success: true, Message: response });
                    $("#pInitials")[0].dataset["letters"] = firstName.charAt(0).toLocaleUpperCase() + lastName.charAt(0).toLocaleUpperCase();
                }
                else
                    DisplayDialog({ Success: false, Message: response });
            },
            failure: function (response) {
                console.log(response.Message);
            },
            error: function (response) {
                console.log(response.Message);
            }
        });
    }
}

//Called when the user clicks "Update Username" on the Profile view
function UpdateUsername() {
    //Making the API call to update the username only if the data entered is valid
    var username = $("#profileUsername")[0].value;
    if (username === null || username === "") {
        $("#spanUsername")[0].innerHTML = "Required";
    }
    else {
        $.ajax({
            type: "POST",
            url: "/Profile/UpdateUsername",
            data: JSON.stringify(username),
            contentType: 'application/json; charset=utf-8',
            success: function (response) {
                $("#strongUsername")[0].innerText = username;
                DisplayDialog({ Success: true, Message: response });
            },
            failure: function (response) {
                console.log(response.Message);
            },
            error: function (response) {
                console.log(response.Message);
            }
        });
    }
}
/* --------------------------- END UPDATE PROFILE -----------------------------------*/

/* --------------------------- PROFILE - CHANGE PASSWORD -----------------------------------*/
function ChangePassword() {
    var flag = true; //Flag used to validate form-data

    //Taking the entered data into variables
    var currentPassword = $("#currentPassword")[0].value;
    var newPassword = $("#newPassword")[0].value;
    var confiirmPassword = $("#confirmPassword")[0].value;

    //Validating the form-data
    //Checking for non-null, non-empty currentPassword
    if (currentPassword === null || currentPassword === "") {
        $("#spanCurrentPassword")[0].innerHTML = "Required";
        flag = false;
    }

    flag = IsPasswordValid(newPassword);

    //Matching new and confirmed password
    if (newPassword !== confiirmPassword) {
        $("#spanConfirmPassword")[0].innerHTML = "Passwords do not match";
        flag = false;
    }

    //Making the API call only if the form-data is valid
    if (flag) {
        var password = {
            "CurrentPassword": currentPassword,
            "NewPassword": newPassword,
            "ConfirmPassword": confiirmPassword
        };

        var data = new Object();
        data.PasswordObj = password;
        var jsonObject = JSON.stringify(data);

        $.ajax({
            type: "POST",
            url: "/Profile/UpdatePassword",
            data: jsonObject,
            contentType: 'application/json; charset=utf-8',
            dataType: "text",
            success: function (response) {
                //Notifying the user either on success or failure
                if (response.indexOf("success") > -1) {
                    DisplayDialog({ Success: true, Message: response });

                    setTimeout(function () {
                        window.history.back();
                    }, 2500);
                }
                else {
                    DisplayDialog({ Success: false, Message: response });
                }
            },
            failure: function (response) {
                console.log(response.Message);
            },
            error: function (response) {
                console.log(response.Message);
            }
        });
    }
}
/* --------------------------- END PROFILE - CHANGE PASSWORD -----------------------------------*/

/* --------------------------- ADD USER -----------------------------------*/
//Handles the selection and de-selction of the the Checkbox to send email while adding a new user
function AddUserSendEmailClicked(obj) {
    //Checking if the target of the event is a span or the checkbox itself. When it is the span element, we trigger the click of the checkbox
    if (obj.id === "addUserSendEmailSpan") {
        $("#addUserSendEmail").trigger('click');
    }

    //If the check to send email is checked, we hide the form-fields for Passwords, else we show them
    if ($("#addUserSendEmail")[0].checked === true) {
        $("#addUserPasswordBase").addClass("none");
        $("#addUserConfirmPasswordBase").addClass("none");
    }
    else {
        $("#addUserPasswordBase").removeClass("none");
        $("#addUserConfirmPasswordBase").removeClass("none");
    }
}

//Called when the user clicks the "Add" button in the Add New User View
function AddUser() {
    //Validating the form-data
    var isFormDataValid = ValidateInput();

    //Only if the form data is valid, we make the API call
    if (isFormDataValid) {
        var user = {
            "FirstName": $("#addUserFirstName")[0].value,
            "LastName": $("#addUserLastName")[0].value,
            "Username": $("#addUserUsername")[0].value,
            "Email": $("#addUserEmail")[0].value,
            "Active" : $("#addUserActiveStatus")[0].checked
        };

        var password = $("#addUserPassword")[0].value; //Capturing the value of the password in another variable

        var isUserAllowed = $('#IsUserAllowed').val(); //Checking if the user is allowed to assign Roles to the user 
        if (isUserAllowed === "True") {
            var roleElements = $("#divRoleBase input:checked");
            var rolesList = []; //A user can be assigned with more than one roles, hence taking the roles into a list
            if (roleElements !== null && roleElements.length > 0) {
                for (var i = 0; i < roleElements.length; i++) {
                    var role = new Object();
                    role.Id = roleElements[i].id;
                    role.Name = roleElements[i].name;
                    rolesList.push(role);
                }
            }

            //Creating the JSON input object
            var data = new Object();
            data.Roles = rolesList;
            data.User = user;
            data.Password = password;
            data.IsEmailToSend = $("#addUserSendEmail")[0].checked;
            var jsonObject = JSON.stringify(data);

            $.ajax({
                type: "POST",
                url: "/Users/AddUser",
                data: jsonObject,
                contentType: 'application/json; charset=utf-8',
                dataType: "text",
                success: function (response) {
                    //Notifying the user on success or failure
                    if (response.indexOf("success") > -1) {
                        DisplayDialog({ Success: true, Message: response });

                        //Navigating the user back to the List view, only on Success
                        setTimeout(function () {
                            //window.history.back();
                            window.location.href = $("#navigation-tree").find("li .active").attr("href");
                        }, 2500);
                    }
                    else {
                        DisplayDialog({ Success: false, Message: response });
                    }
                },
                failure: function (response) {
                    console.log(response.Message);
                },
                error: function (response) {
                    console.log(response.Message);
                }
            });
        }
    }
}

//Validating the "New User" form-data 
function ValidateInput() {
    var flag = true; //Used to finally set if the form-data is valid

    //If user has disabled sending of email, then the password cannot be null or empty, else skip this step
    if ($("#addUserSendEmail")[0].checked !== true) {

        flag = IsPasswordValid($("#addUserPassword")[0].value); //Validating if the Password conforms to the required condition
        if (!flag)
            $("#spanPassword")[0].innerHTML = "Passwords must be at least 8 characters and contain at least one upper case (A - Z), one lower case (a - z), one number(0 - 9) and an special character(e.g. !@#$ %^&*)";

        //Checking if the passwords match
        if ($("#addUserPassword")[0].value !== $("#addUserConfirmPassword")[0].value) {
            $("#confirmPasswordMismatch")[0].innerHTML = "Passwords do not match";
            flag = false;
        }
    }

    //Checking for non-null, non-empty firstName
    if ($("#addUserFirstName")[0].value === null || $("#addUserFirstName")[0].value === "") {
        $("#spanFirstName")[0].innerHTML = "Required";
        flag = false;
    }

    //Checking for non-null, non-empty lastName
    if ($("#addUserLastName")[0].value === null || $("#addUserLastName")[0].value === "") {
        $("#spanLastName")[0].innerHTML = "Required";
        flag = false;
    }

    //Checking for non-null, non-empty username
    if ($("#addUserUsername")[0].value === null || $("#addUserUsername")[0].value === "") {
        $("#spanUsername")[0].innerHTML = "Required";
        flag = false;
    }

    //Checking for non-null, non-empty email
    if ($("#addUserEmail")[0].value === null || $("#addUserEmail")[0].value === "") {
        $("#spanEmail")[0].innerHTML = "Required";
        flag = false;
    }
    else if ($("#addUserEmail")[0].value !== null && $("#addUserEmail")[0].value !== "") {
        //Validating the format of the email entered
        var isEmailValid = IsEmailValid($("#addUserEmail")[0].value);
        if (!isEmailValid) {
            $("#spanEmail")[0].innerHTML = "Incorrect email format";
            flag = false;
        }
    }

    var isUserAllowed = $('#IsUserAllowed').val(); //Checking if the user is allowed to assign roles
    if (isUserAllowed === "True") {
        //Validating that at least one role is assigned with the user
        var roleElements = $("#divRoleBase input:checked");
        if (roleElements.length === 0) {
            $("#spanRoles")[0].innerHTML = "The user must be associated with at least one role";
            flag = false;
        }
    }
    return flag;
}
/* --------------------------- END ADD USER -----------------------------------*/

/* --------------------------- EDIT USER -----------------------------------*/
//Called when the "Edit" menu item is clicked on the Users table. The input parameter is the rowId, which is the encrypted userid
function OnUserEditClicked(selectedrow) {
    var selectedUserId = selectedrow.id;
    selectedUserId = selectedUserId.replace(/\+/g, '%2B'); //Replacing the escape charaters
    $('#hiddenUserEdit').attr("href", "/Users/EditUser?userId=" + selectedUserId); //This is a work around for triggering the event because of the various classes used. This triggers the click of a hidden href element
    $('#hiddenUserEdit')[0].click();
}

//Called when the "Update Information" button is clicked
function UpdateUserInfo() {

    var isDataValid = ValidateEditUserInput(); //Validating the form-data

    //Only if the form-data is valid, we make the API call
    if (isDataValid) {
        var userId = $("#hiddenEditUserId")[0].value; //Getting the updated UserId
        var emailConfirmed = $("#hiddenEmailConfirmed")[0].value;
        var user = {
            "UpdatedId": userId,
            "FirstName": $("#editUserFirstName")[0].value,
            "LastName": $("#editUserLastName")[0].value,
            "Username": $("#editUserUsername")[0].value,
            "Email": $("#email")[0].value,
            "Active": $("#editUserActiveStatus")[0].checked,
            "EmailConfirmed": emailConfirmed
        };

        var jsonObject = JSON.stringify(user); //Setting the input parameter for the API
        $.ajax({
            type: "POST",
            url: "/Users/UpdateUserInfo",
            data: jsonObject,
            contentType: 'application/json; charset=utf-8',
            dataType: "text",
            success: function (response) {
                //Notifying the user on sucess or failure
                if (response.indexOf('success') > -1) {
                    DisplayDialog({ Success: true, Message: response });
                }
                else {
                    DisplayDialog({ Success: false, Message: response });
                }
            },
            failure: function (response) {
                console.log(response.Message);
            },
            error: function (response) {
                console.log(response.Message);
            }
        });

        var isUserAllowed = $('#IsUserAllowed').val(); //Checking if the user is allowed to assing roles
        if (isUserAllowed === "True") {
            var roleElements = $("#divRoleBase input:checked");
            var rolesList = [];
            if (roleElements !== null && roleElements.length > 0) {
                for (var i = 0; i < roleElements.length; i++) {
                    var role = new Object();
                    role.Id = roleElements[i].id;
                    role.Name = roleElements[i].name;
                    rolesList.push(role); //Users can be assigned with multiple roles and hence adding the roles to a list
                }
            }

            //Preparing the input object for the API
            var data = new Object();
            data.UpdatedRoles = rolesList;
            data.UserId = userId;

            jsonObject = null;
            jsonObject = JSON.stringify(data);

            $.ajax({
                type: "POST",
                url: "/Roles/UpdateUserRolesByAdmin",
                data: jsonObject,
                contentType: 'application/json; charset=utf-8',
                dataType: "text",
                success: function (response) {
                    //Notifying the user on success or failure
                    DisplayDialog({ Success: true, Message: response });

                    //Navigating the user back to the List view, only on Success
                    setTimeout(function () {
                        //window.history.back();
                        window.location.href = $("#navigation-tree").find("li .active").attr("href");
                    }, 2500);
                },
                failure: function (response) {
                    console.log(response.Message);
                },
                error: function (response) {
                    console.log(response.Message);
                }
            });
        }
    }
}

//Called from within the UpdateUserInfo method, to validate the edit form data
function ValidateEditUserInput() {
    var flag = true; //flag used to finally decide if form data is valid

    //Checking for non-null, non-empty firstName
    if ($("#editUserFirstName")[0].value === null || $("#editUserFirstName")[0].value === "") {
        $("#spanFirstName")[0].innerHTML = "Required";
        flag = false;
    }
    //Checking for non-null, non-empty LastName
    if ($("#editUserLastName")[0].value === null || $("#editUserLastName")[0].value === "") {
        $("#spanLastName")[0].innerHTML = "Required";
        flag = false;
    }

    //Checking for non-null, non-empty userName
    if ($("#editUserUsername")[0].value === null || $("#editUserUsername")[0].value === "") {
        $("#spanUsername")[0].innerHTML = "Required";
        flag = false;
    }

    //Checking for non-null, non-empty email
    if ($("#email")[0].value === null || $("#email")[0].value === "") {
        $("#spanEmail")[0].innerHTML = "Required";
        flag = false;
    }
    else {
        //Checking for the email format
        var isEmailValid = IsEmailValid($("#email")[0].value);
        if (!isEmailValid) {
            $("#spanEmail")[0].innerHTML = "Incorrect email format";
            flag = false;
        }
    }

    var isUserAllowed = $('#IsUserAllowed').val(); //Checking if the user is allowed to assign roles
    if (isUserAllowed === "True") {
        var roleElements = $("#divRoleBase input:checked");
        if (roleElements.length === 0) { //Checking to see that the user is assigned with at least one role
            $("#spanRoles")[0].innerHTML = "The user must be associated with at least one role";
            flag = false;
        }
    }
    return flag;
}
/* --------------------------- END EDIT USER -----------------------------------*/

/* --------------------------- CHANGE PASSWORD BY ADMIN -----------------------------------*/
//Called when the "Change Password" is clicked from the menu on the Users Table.
function OnChangePasswordClicked(selectedRow) {
    var selectedUserId = selectedRow.id;
    $('#hiddenUserIdChangePwd').val(selectedUserId);
    $('#myModal').modal('show'); //Showing the modal that contains the Change Password Form

    //Clearing the form-fields
    $('#password').val('');
    $('#confirmpassword').val('');
    document.getElementById("pass_type").innerHTML = "";
    document.getElementById("passwordMessage").innerHTML = "";
    document.getElementById("message").innerHTML = "";
}

//Called on click of the "Set" button in the password modal
function SetPassword() {
    var enteredPassword = $('#password').val(); //Assing the entered password into a variable

    //Checking for a non-null, non-empty password
    if (enteredPassword.length === 0) {
        $('#passwordMessage').html("Passwords must be at least 8 characters and contain at least one upper case (A - Z), one lower case (a - z), one number(0 - 9) and an special character(e.g. !@#$ %^&*)").css('color', 'red');
        $('#confirmpassword').val('');
        return false;
    }

    //Checking for the format of the password with the regular expression
    if (enteredPassword.length >= 0) {
        var flag = IsPasswordValid(enteredPassword); //Validating if the Password conforms to the required condition
        if (!flag) {
            $('#passwordMessage').html("Passwords must be at least 8 characters and contain at least one upper case (A - Z), one lower case (a - z), one number(0 - 9) and an special character(e.g. !@#$ %^&*)").css('color', 'red');
            $('#confirmpassword').val('');
        }
        else {
            $('#passwordMessage').html("");
        }
    }

    //Checking that the passwords match and then make the API call to change the password
    if (enteredPassword === $('#confirmpassword').val()) {
        var userId = $("#hiddenUserIdChangePwd").val();

        var data = {};
        data.userId = userId;
        data.password = enteredPassword;

        $.ajax({
            type: "POST",
            url: "/Auth/ForcePasswordChange",
            data: JSON.stringify(data),
            contentType: 'application/json; charset=utf-8',
            success: function (response) {
                //Notifying the user on success or failure
                if (response.indexOf("success") > -1) {
                    DisplayDialog({ Success: true, Message: response });
                    //On success, reset the values and hiding the modal
                    $('#hiddenUserIdChangePwd').val();
                    $('#myModal').modal('hide');
                }
                else
                    DisplayDialog({ Success: false, Message: response });
            },
            failure: function (response) {
                console.log(response.Message);
            },
            error: function (response) {
                console.log(response.Message);
            }
        });
    } else {
        $('#message').html('Passwords do not match').css('color', 'red');
        return false;
    }
}
/* --------------------------- END CHANGE PASSWORD BY ADMIN -----------------------------------*/

/* --------------------------- ACTIVATE/DEACTIVATE USER -----------------------------------*/
//Called when either the "Activate" or "Deactive" link is clicked from the menu on the Users Table.
function OnChangeActiveStatusClicked(selectedRow) {
    var selectedUserId = selectedRow.id;
    var action = selectedRow.innerText === "Activate" ? "activate" : "deactivate"; //Setting the action on the fly

    var data = { "UserId": selectedUserId, "Action": action }; //Preparing the data for the API 
    DisplayConfirmationDialog({ //Confirming from the user, the delete operation and passing the required data
        Message: "Are you sure you want to " + action + " the selected user?",
        CallFrom: "ChangeUserActiveStatus",
        OkData: { Label: "Yes", Data: data },
        CancelData: { Label: "No", Data: null }
    });
}

//Called after the confirmation from the user, making the API call
function TriggerChangeUserActiveStatus(data) {
    $.ajax({
        type: "POST",
        url: "/Users/ChangeUserActiveStatus",
        data: JSON.stringify(data),
        contentType: 'application/json; charset=utf-8',
        success: function (response) {
            //Notifying the user on either success or failure
            if (response.indexOf("success") > -1) {
                //On success, also removing the selected row from the Users table
                DisplayDialog({ Success: true, Message: response });

                //Updating the labels on the UI and the action based on the operation performed
                var selectedRow = $("#tableUsers tr[id='" + data.UserId + "']")[0].children[6]; //The 6th cell in the selected row is the one with the "Active/ Inactive" label.
                if (data.Action === "activate") {
                    selectedRow.innerText = "Active"; //Updating the label on the cell for the "Status" column when the previous status is inactive
                    selectedRow.style.color = "green"; //Updating the colour based on the status on the cell 
                    selectedRow.parentElement.getElementsByClassName("activestatus")[0].innerText = "Deactivate"; //Changing the label of the menu item 
                } else {
                    selectedRow.innerText = "Inactive"; //Updating the label on the cell for the "Status" column when the previous status is active
                    selectedRow.style.color = "darkgrey"; //Updating the colour based on the status on the cell 
                    selectedRow.parentElement.getElementsByClassName("activestatus")[0].innerText = "Activate"; //Changing the label of the menu item 
                }
            }
            else {
                DisplayDialog({ Success: false, Message: response });
            }
        },
        failure: function (response) {
            console.log(response.Message);
        },
        error: function (response) {
            console.log(response.Message);
        }
    });
}
/* --------------------------- END ACTIVATE/DEACTIVATE USER -----------------------------------*/

/* --------------------------- DELETE USER -----------------------------------*/
//Called when the 'Delete' menu item is selected on the Users table. The input parameter is the rowId, which is the encrypted userid
function OnUserDeleteClicked(selectedRow) {
    var selectedUserId = selectedRow.id;
    DisplayConfirmationDialog({ //Confirming from the user, the delete operation and passing the required data
        Message: "Are you sure you want to delete the selected user?",
        CallFrom: "DeleteUser",
        OkData: { Label: "Yes", Data: selectedUserId },
        CancelData: { Label: "No", Data: null }
    });
}

//Called after the confirmation from the user, making the API call
function TriggerDeleteUser(selectedUserId) {
    $.ajax({
        type: "POST",
        url: "/Users/DeleteUser",
        data: JSON.stringify(selectedUserId),
        contentType: 'application/json; charset=utf-8',
        success: function (response) {
            //Notifying the user on either success or failure
            if (response.indexOf("success") > -1) {
                //On success, also removing the selected row from the Users table
                DisplayDialog({ Success: true, Message: response });
                $("#tableUsers tr[id='" + selectedUserId + "']").remove();
            }
            else {
                DisplayDialog({ Success: false, Message: response });
            }
        },
        failure: function (response) {
            console.log(response.Message);
        },
        error: function (response) {
            console.log(response.Message);
        }
    });
}
/* --------------------------- END DELETE USER -----------------------------------*/

/* --------------------------- ADD ROLE -----------------------------------*/
//Is triggered when the reset link is clicked. This is to not select any of the admin configurations
function OnRestAdminSelectionClicked() {
    $('input[name="adminsettings"]').prop('checked', false);
}

//Called when the "Create" button is clicked on "AddRole" view
function OnRoleAddClicked() {
    var roleName = $("#addRoleName")[0].value; //Assigning the role name to a variable and validating it
    if (roleName === null || roleName === "") {
        $("#spanRoleName")[0].innerHTML = "Required";
    }
    else {
        var pattern = new RegExp(/[~`!#$%\^&*+=\-\[\]@\\';,/{}|\\":<>\?]/); //unacceptable chars
        if (pattern.test(roleName)) {
            $("#spanRoleName")[0].innerHTML = "Role name cannot include special characters.";
        }
        else {
            //If the data in the form is valid, we make the API call
            var role = {
                "Name": roleName,
                "Description": $("#addRoleDescription")[0].value,
                "AuthLevel": $("#auth-level").val()
            };

            //Preparing the input object for the API
            var data = new Object();
            data.Role = role;
            var jsonObject = JSON.stringify(data);

            $.ajax({
                type: "POST",
                url: "/Roles/AddRole",
                data: jsonObject,
                contentType: 'application/json; charset=utf-8',
                dataType: "text",
                success: function (response) {
                    //Notifying the user on success or failure
                    if (response.indexOf("success") > -1) {
                        DisplayDialog({ Success: true, Message: response });

                        //On success, navigating the user back to the Roles List view
                        setTimeout(function () {
                            window.location.href = $("#navigation-tree").find("li .active").attr("href");
                            //if (window.location.href.indexOf('#') > -1) {
                            //    window.history.go(-2);
                            //} else {
                            //    window.history.back();
                            //}
                        }, 2500);
                    }
                    else {
                        DisplayDialog({ Success: false, Message: response });
                    }
                },
                failure: function (response) {
                    console.log(response.Message);
                },
                error: function (response) {
                    console.log(response.Message);
                }
            });
        }
    }
}
/* --------------------------- END ADD ROLE -----------------------------------*/

/* --------------------------- EDIT ROLE -----------------------------------*/
//Called when the "Edit" menu  item is clicked on the Roles table with the selected roleId as the input
function OnRoleEditClicked(selectedrow) {
    var selectedRoleId = selectedrow.id;
    $('#hiddenRoleEdit').attr("href", "/Roles/EditRole?roleId=" + selectedRoleId); //This is a work around to work with the anchor tag inside the table 
    $('#hiddenRoleEdit')[0].click(); //Triggers the click of a hidden anchor tag
}

//Called when "Update" button is clicked in the "Edit Role" view
function OnRoleUpdateClicked() {
    var roleName = $("#editRoleName")[0].value; //Assigning the form-data to a variable and then validated 
    if (roleName === null || roleName === "") {
        $("#spanRoleName")[0].innerHTML = "Required";
    }
    else {
        var pattern = new RegExp(/[~`!#$%\^&*+=\-\[\]@\\';,/{}|\\":<>\?]/); //unacceptable chars
        if (pattern.test(roleName)) {
            $("#spanRoleName")[0].innerHTML = "Role name cannot include special characters.";
        }
        else {
            //If the data is valid, we make the API call
            var role = {
                "Id": $("#hiddenEditUserId")[0].value,
                "Name": roleName,
                "Description": $("#editRoleDescription")[0].value,
                "AuthLevel": $("#auth-level").val(),
                "Rank": $("#editRoleRank")[0].value //Rank is a hidden field that is set by BOS
            };

            //Preparing the input object for the API
            var data = new Object();
            data.Role = role;
            var jsonObject = JSON.stringify(data);

            $.ajax({
                type: "POST",
                url: "/Roles/UpdateRole",
                data: jsonObject,
                contentType: 'application/json; charset=utf-8',
                dataType: "text",
                success: function (response) {
                    //Notifying the user on success and failure
                    if (response.indexOf("success") > -1) {
                        DisplayDialog({ Success: true, Message: response });

                        //On success, navigating the user back to the Roles List view
                        setTimeout(function () {
                            window.location.href = $("#navigation-tree").find("li .active").attr("href");
                            //if (window.location.href.indexOf('#') > -1) {
                            //    window.history.go(-2);
                            //} else {
                            //    window.history.back();
                            //}
                        }, 2500);
                    }
                    else {
                        DisplayDialog({ Success: false, Message: response });
                    }
                },
                failure: function (response) {
                    console.log(response.Message);
                },
                error: function (response) {
                    console.log(response.Message);
                }
            });
        }
    }
}
/* --------------------------- END EDIT ROLE -----------------------------------*/

/* --------------------------- DELETE ROLE -----------------------------------*/
//Called when the "Delete" option is selected on the Roles table view
function OnRoleDeleteClicked(selectedrow) {
    var selectedRoleId = selectedrow.id;
    //Confirming from the user the delete operation and passing required information
    DisplayConfirmationDialog({
        Message: "Are you sure you want to delete the selected role? \n Note: If this role is already associated with users, they no longer will have access to the previliges of this role.",
        CallFrom: "DeleteRole",
        OkData: { Label: "Yes", Data: selectedRoleId },
        CancelData: { Label: "No", Data: null }
    });
}

//Called after the user confirms the delete operation to make the API call, with seletedRoleId as the input
function TriggerDeleteRole(selectedRoleId) {
    $.ajax({
        type: "POST",
        url: "/Roles/DeleteRole",
        data: JSON.stringify(selectedRoleId),
        contentType: 'application/json; charset=utf-8',
        success: function (response) {
            //Notifies the user on success and failure
            if (response.indexOf("success") > -1) {
                DisplayDialog({ Success: true, Message: response });

                //On success, also removes the row of the deleted role from the table
                $("#tableRoles tr[id='" + selectedRoleId + "']").remove();
            }
            else
                DisplayDialog({ Success: false, Message: response });
        },
        failure: function (response) {
            console.log(response.Message);
        },
        error: function (response) {
            console.log(response.Message);
        }
    });
}
/* --------------------------- END DELETE ROLE -----------------------------------*/

/* --------------------------- ROLE - MANAGE PERMISSIONS -----------------------------------*/
//Called when "Manage Permissions" is clicked from the menu on the Roles table
function OnManagePermissionsClicked(selectedrow) {
    var roleId = selectedrow.id;
    var roleName = $('#' + roleId)[0].children[0].innerHTML;
    $('#hiddenRoleManagePerms').attr("href", "/Roles/RoleManagePermissions?roleId=" + roleId + '&roleName=' + roleName); //Is a work around to make call to the controller endpoint with RoleId and RoleName as parameters
    $('#hiddenRoleManagePerms')[0].click();
}
/* --------------------------- END ROLE - MANAGE PERMISSIONS -----------------------------------*/

/* --------------------------- MANAGE PERMISSIONS -----------------------------------*/
//Called when "Update Permissions" is clicked on the Manage Permissions view
function OnPermissionsSave() {
    var modulesList = $('.modulecheck:checkbox:checked'); //Getting a list of all the checked modules
    var operationsList = $('.operationcheck:checkbox:checked'); //Getting a list of all the checked operations
    var ownerId = $("#hiddenOwnerId").val(); //Getting the ownerId which in this case is te roleId

    var updatedModulesList = []; //Creating an empty array for a list of Module objects
    var updatedOperationsList = []; //Creating an empty array for a list of Operation objects

    var flag = true;
    var modules = "", operations = "";
    //Interating through the checked list of module checkbox to create module objects and save in the Modules list
    Array.prototype.forEach.call(modulesList, module => {

        //SAMPLE CODE: If you want to allow the selection of the sub-module only if the parent module is selected too.
        //if (module.getAttribute('ParentId') !== null) {
        //    const found = updatedModulesList.some(el => el.ModuleId === module.getAttribute('parentmoduleid'));
        //    if (!found) {
        //        if (modules === "") {
        //            modules = module.name;
        //        }
        //        else {
        //            modules = modules + ', ' + module.name;
        //        }
        //        flag = false;
        //    }
        //}

        var permissionSet = new Object();
        permissionSet.ComponentId = module.id;
        permissionSet.Name = module.name;
        permissionSet.Code = module.getAttribute('code');
        permissionSet.ParentId = module.getAttribute('parentmoduleid');
        updatedModulesList.push(permissionSet);
    });

    //Interating through the checked list of operation checkbox to create operation objects and save in the Modules list
    Array.prototype.forEach.call(operationsList, operation => {
        if (operation.getAttribute('parentoperationid') !== null) {
            const found = updatedOperationsList.some(el => el.OperationId === operation.getAttribute('parentoperationid'));
            if (!found) {
                if (operations === "") {
                    operations = operation.name;
                }
                else {
                    operations = operations + ', ' + operation.name;
                }
                flag = false;
            }
        }

        var permissionsOperation = new Object();
        permissionsOperation.ComponentId = operation.getAttribute('moduleid');
        permissionsOperation.Name = operation.name;
        permissionsOperation.Code = operation.getAttribute('code');
        permissionsOperation.IsDefault = operation.getAttribute('isdefault') === 'isdefault' ? 1 : 0;
        permissionsOperation.ParentOperationId = operation.getAttribute('parentoperationid');
        permissionsOperation.OperationId = operation.id;
        updatedOperationsList.push(permissionsOperation);
    });

    if (flag) {
        //Preparing the JSON input for the API
        var data = new Object();
        data.OwnerId = ownerId;
        data.Modules = updatedModulesList;
        data.Operations = updatedOperationsList;

        $.ajax({
            type: "POST",
            url: "/Permissions/UpdatePermissions",
            data: JSON.stringify(data),
            contentType: 'application/json; charset=utf-8',
            success: function (response) {
                //Notifying the user on Success and Failure
                if (response.indexOf("success") > -1)
                    DisplayDialog({ Success: true, Message: response });
                else
                    DisplayDialog({ Success: false, Message: response });
            },
            failure: function (response) {
                console.log(response.Message);
            },
            error: function (response) {
                console.log(response.Message);
            }
        });
    }
    else {
        var message;
        if (modules.length > 0) {
            message = "The module(s) <b>" + modules + " </b> cannot be assigned as their parent module(s) are not selected.";
            if (operations.length > 0) {
                message = message + "And, the operation(s) <b>" + operations + " </b> cannot be assigned as their parent operation(s) are not selected.";
            }
        }
        else {
            message = "The operation(s) <b>" + operations + " </b> cannot be assigned as their parent operation(s) are not selected.";
        }
        DisplayDialog({ Success: false, Message: message });
    }
}
/* --------------------------- END MANAGE PERMISSIONS -----------------------------------*/

/* --------------------------- NAVIGATION NENU - STATE MANAGEMENT -----------------------------------*/
//Called everytime the page is loaded, in order to maintain the state of the navigation pane
function NavigationState(currentModuleId) {
    var moduleDiv = $("#" + currentModuleId); //Get the HTML element by the selected moduleId

    if (moduleDiv[0].nodeName === "DIV") { //If the element happens to be of type 'DIV', it means that the module has sub-modules associated with it
        var parentId = moduleDiv[0].getAttribute("parentmoduleid");
        while (parentId !== null || parentId !== "null") {
            moduleDiv.toggle(200);
            moduleDiv.parent('li>a').addClass('active');
            NavigationState(parentId);
        }
    }
    else {
        parentId = moduleDiv[0].getAttribute("parentmoduleid");
        NavigationState(parentId);
    }
}
/* --------------------------- END NAVIGATION NENU - STATE MANAGEMENT -----------------------------------*/

/* --------------------------- DISPLAY MESSAGE FOR CUSTOM MODULES -----------------------------------*/
//For custom modules, it displays the message on the Dashboard and also sets the active class on the navigation pane on the selected module
function DisplayDashboardCustomMessage(currentModuleId) {
    var module = $("#" + currentModuleId);
    var moduleName = '';
    if (module[0].nodeName === "DIV") {
        moduleName = module[0].previousElementSibling.innerText;  //Getting the name of the selected module
        module[0].previousElementSibling.className = "active"; //Setting the active class on the navigation menu
    }
    else {
        moduleName = module.children('a')[0].innerText; //Getting the name of the selected module
        module.children('a')[0].className = "active"; //Setting the active class on the navigation menu
    }

    $("#divMessage").empty().append('You are in the <strong>' + moduleName + '</strong> module'); //Setting the text on Dashboad for custom module
}
/* --------------------------- END DISPLAY MESSAGE FOR CUSTOM MODULES -----------------------------------*/

/* --------------------------- Generic Methods ---------------------------*/
//Used for the Passwor dStrength Meter
function CheckPasswordStrength() {
    var val = null;

    if (document.getElementById("newPassword") !== null) {
        val = document.getElementById("newPassword").value;
    }
    else if (document.getElementById("addUserPassword") !== null) {
        val = document.getElementById("addUserPassword").value;
    }
    else if (document.getElementById("password") !== null) {
        val = document.getElementById("password").value;
    }

    var meter = document.getElementById("meter");
    var no = 0;
    if (val !== "") {
        // If the password length is less than or equal to 6
        if (val.length <= 6) no = 1;

        // If the password length is greater than 6 and contain any lowercase alphabet or any number or any special character
        if (val.length > 6 && (val.match(/[a-z]/) || val.match(/\d+/) || val.match(/.[!,@,#,$,%,^,&,*,?,_,~,-,(,)]/))) no = 2;

        // If the password length is greater than 6 and contain alphabet,number,special character respectively
        if (val.length > 6 && (val.match(/[a-z]/) && val.match(/\d+/) || val.match(/\d+/) && val.match(/.[!,@,#,$,%,^,&,*,?,_,~,-,(,)]/) || val.match(/[a-z]/) && val.match(/.[!,@,#,$,%,^,&,*,?,_,~,-,(,)]/))) no = 3;

        // If the password length is greater than 6 and must contain alphabets,numbers and special characters
        if (val.length > 6 && val.match(/[a-z]/) && val.match(/\d+/) && val.match(/.[!,@,#,$,%,^,&,*,?,_,~,-,(,)]/)) no = 4;

        if (no === 1) {
            $("#meter").animate({
                width: '50px'
            }, 300);
            meter.style.backgroundColor = "red";
            document.getElementById("pass_type").innerHTML = "Very Weak";
        }

        if (no === 2) {
            $("#meter").animate({
                width: '100px'
            }, 300);
            meter.style.backgroundColor = "#F5BCA9";
            document.getElementById("pass_type").innerHTML = "Weak";
        }

        if (no === 3) {
            $("#meter").animate({
                width: '150px'
            }, 300);
            meter.style.backgroundColor = "#FF8000";
            document.getElementById("pass_type").innerHTML = "Fair";
        }

        if (no === 4) {
            $("#meter").animate({
                width: '200px'
            }, 300);
            meter.style.backgroundColor = "#328c48";
            document.getElementById("pass_type").innerHTML = "Strong";
        }
    } else {
        meter.style.backgroundColor = "white";
        document.getElementById("pass_type").innerHTML = "";
    }
}

//Validates the email format
function IsEmailValid(email) {
    return /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email);
}

//Checks for conformance of the password requirement
function IsPasswordValid(password) {
    var flag = true;
    var strongRegex = new RegExp("^(?=.*[a-z])(?=.*[A-Z])(?=.*[0-9])(?=.*[!@#\$%\^&\*])(?=.{8,100})");
    if (!strongRegex.test(password)) {
        flag = false;
    }
    return flag;
}
/* --------------------------- End Generic Methods ---------------------------*/
