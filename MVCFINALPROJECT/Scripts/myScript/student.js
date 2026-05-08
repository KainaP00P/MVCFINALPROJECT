// student.js
// Handles student registration, edit form validation and behavior

$(document).ready(function () {

    // -------------------------------------------------------
    // AUTO DISMISS alert messages after 4 seconds
    // -------------------------------------------------------
    setTimeout(function () {
        $(".alert").fadeOut("slow");
    }, 4000);

    // -------------------------------------------------------
    // SEARCH filter for student list table
    // -------------------------------------------------------
    $("#searchStudent").on("keyup", function () {

        var value = $(this).val().toLowerCase();

        // Filter rows by matching any cell content
        $("#studentTable tbody tr").filter(function () {
            $(this).toggle(
                $(this).text().toLowerCase().indexOf(value) > -1
            );
        });
    });

    // -------------------------------------------------------
    // STUDENT REGISTRATION FORM VALIDATION
    // -------------------------------------------------------
    $("#studentForm").submit(function (e) {

        var isValid = true;

        // Clear all previous error messages
        $(".field-error").text("");

        // Validate First Name
        if ($("#FirstName").val().trim() === "") {
            $("#firstNameError").text("First name is required.");
            isValid = false;
        }

        // Validate Last Name
        if ($("#LastName").val().trim() === "") {
            $("#lastNameError").text("Last name is required.");
            isValid = false;
        }

        // Validate Gender selection
        if ($("#Gender").val() === "") {
            $("#genderError").text("Please select a gender.");
            isValid = false;
        }

        // Validate Birth Date
        if ($("#BirthDate").val() === "") {
            $("#birthDateError").text("Birth date is required.");
            isValid = false;
        }

        // Validate Address
        if ($("#Address").val().trim() === "") {
            $("#addressError").text("Address is required.");
            isValid = false;
        }

        // Validate Contact Number (numbers only, 11 digits)
        var contact = $("#ContactNumber").val().trim();
        if (contact === "") {
            $("#contactError").text("Contact number is required.");
            isValid = false;
        } else if (!/^[0-9]{11}$/.test(contact)) {
            $("#contactError").text("Contact number must be 11 digits.");
            isValid = false;
        }

        // Validate Email format
        var email = $("#Email").val().trim();
        if (email === "") {
            $("#emailError").text("Email is required.");
            isValid = false;
        } else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email)) {
            $("#emailError").text("Please enter a valid email address.");
            isValid = false;
        }

        // Validate Course selection
        if ($("#CourseID").val() === "" || $("#CourseID").val() === "0") {
            $("#courseError").text("Please select a course.");
            isValid = false;
        }

        // Validate Year Level
        if ($("#YearLevel").val() === "" || $("#YearLevel").val() === "0") {
            $("#yearLevelError").text("Please select a year level.");
            isValid = false;
        }

        // Validate Semester
        if ($("#Semester").val() === "") {
            $("#semesterError").text("Please select a semester.");
            isValid = false;
        }

        // Validate School Year format (e.g. 2024-2025)
        var schoolYear = $("#SchoolYear").val().trim();
        if (schoolYear === "") {
            $("#schoolYearError").text("School year is required.");
            isValid = false;
        } else if (!/^\d{4}-\d{4}$/.test(schoolYear)) {
            $("#schoolYearError").text("Format must be YYYY-YYYY (e.g. 2024-2025).");
            isValid = false;
        }

        // Validate Username (registration form only)
        if ($("#Username").length) {
            if ($("#Username").val().trim() === "") {
                $("#usernameError").text("Username is required.");
                isValid = false;
            } else if ($("#Username").val().trim().length < 4) {
                $("#usernameError").text("Username must be at least 4 characters.");
                isValid = false;
            }
        }

        // Validate Password (registration form only)
        if ($("#Password").length) {
            var password = $("#Password").val().trim();
            if (password === "") {
                $("#passwordError").text("Password is required.");
                isValid = false;
            } else if (password.length < 6) {
                $("#passwordError").text("Password must be at least 6 characters.");
                isValid = false;
            }
        }

        // Stop form submission if validation fails
        if (!isValid) {
            e.preventDefault();

            // Scroll to first error
            $("html, body").animate({
                scrollTop: $(".field-error:not(:empty)").first().offset().top - 100
            }, 500);
        }
    });

    // -------------------------------------------------------
    // DELETE CONFIRMATION
    // Shows confirm dialog before deleting a student
    // -------------------------------------------------------
    $(".btn-delete").click(function (e) {

        var studentName = $(this).data("name");

        if (!confirm("Are you sure you want to delete student: "
            + studentName + "?\nThis cannot be undone.")) {
            e.preventDefault();
        }
    });

    // -------------------------------------------------------
    // AUTO FORMAT School Year input
    // Automatically adds dash after 4 digits
    // -------------------------------------------------------
    $("#SchoolYear").on("input", function () {

        var val = $(this).val().replace(/[^0-9]/g, "");

        // Auto insert dash after 4 digits
        if (val.length > 4) {
            val = val.substring(0, 4) + "-" + val.substring(4, 8);
        }

        $(this).val(val);
    });

    // -------------------------------------------------------
    // CONTACT NUMBER - numbers only input
    // -------------------------------------------------------
    $("#ContactNumber").on("keypress", function (e) {

        // Allow only numeric keys
        if (!/[0-9]/.test(String.fromCharCode(e.which))) {
            e.preventDefault();
        }
    });

});