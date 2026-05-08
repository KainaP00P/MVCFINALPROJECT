// account.js
// Handles login form validation and password change behavior

$(document).ready(function () {

    // -------------------------------------------------------
    // LOGIN FORM VALIDATION
    // Runs before the form is submitted
    // -------------------------------------------------------
    $("#loginForm").submit(function (e) {

        var username = $("#Username").val().trim();
        var password = $("#Password").val().trim();
        var isValid = true;

        // Clear previous error messages
        $(".field-error").text("");

        // Validate username field
        if (username === "") {
            $("#usernameError").text("Username is required.");
            isValid = false;
        }

        // Validate password field
        if (password === "") {
            $("#passwordError").text("Password is required.");
            isValid = false;
        }

        // Stop form submission if validation fails
        if (!isValid) {
            e.preventDefault();
        }
    });

    // -------------------------------------------------------
    // CHANGE PASSWORD FORM VALIDATION
    // -------------------------------------------------------
    $("#changePasswordForm").submit(function (e) {

        var current = $("#CurrentPassword").val().trim();
        var newPass = $("#NewPassword").val().trim();
        var confirm = $("#ConfirmPassword").val().trim();
        var isValid = true;

        // Clear previous error messages
        $(".field-error").text("");

        // Validate current password
        if (current === "") {
            $("#currentPassError").text("Current password is required.");
            isValid = false;
        }

        // Validate new password length
        if (newPass === "") {
            $("#newPassError").text("New password is required.");
            isValid = false;
        } else if (newPass.length < 6) {
            $("#newPassError").text("Password must be at least 6 characters.");
            isValid = false;
        }

        // Validate confirm password matches new password
        if (confirm === "") {
            $("#confirmPassError").text("Please confirm your new password.");
            isValid = false;
        } else if (newPass !== confirm) {
            $("#confirmPassError").text("Passwords do not match.");
            isValid = false;
        }

        // Stop form submission if validation fails
        if (!isValid) {
            e.preventDefault();
        }
    });

    // -------------------------------------------------------
    // TOGGLE PASSWORD VISIBILITY
    // Eye icon on password fields
    // -------------------------------------------------------
    $(".toggle-password").click(function () {

        // Get the target input field from data attribute
        var targetID = $(this).data("target");
        var input = $("#" + targetID);

        // Switch between text and password type
        if (input.attr("type") === "password") {
            input.attr("type", "text");
            $(this).html('<i class="glyphicon glyphicon-eye-close"></i>');
        } else {
            input.attr("type", "password");
            $(this).html('<i class="glyphicon glyphicon-eye-open"></i>');
        }
    });

    // -------------------------------------------------------
    // AUTO DISMISS alert messages after 4 seconds
    // -------------------------------------------------------
    setTimeout(function () {
        $(".alert").fadeOut("slow");
    }, 4000);

});