// course.js
// Handles course management form validation and behavior

$(document).ready(function () {

    // -------------------------------------------------------
    // AUTO DISMISS alert messages after 4 seconds
    // -------------------------------------------------------
    setTimeout(function () {
        $(".alert").fadeOut("slow");
    }, 4000);

    // -------------------------------------------------------
    // SEARCH filter for course list table
    // -------------------------------------------------------
    $("#searchCourse").on("keyup", function () {

        var value = $(this).val().toLowerCase();

        // Filter rows by matching any cell content
        $("#courseTable tbody tr").filter(function () {
            $(this).toggle(
                $(this).text().toLowerCase().indexOf(value) > -1
            );
        });
    });

    // -------------------------------------------------------
    // COURSE FORM VALIDATION (Create and Edit)
    // -------------------------------------------------------
    $("#courseForm").submit(function (e) {

        var isValid = true;

        // Clear all previous error messages
        $(".field-error").text("");

        // Validate Course Code
        var courseCode = $("#CourseCode").val().trim();
        if (courseCode === "") {
            $("#courseCodeError").text("Course code is required.");
            isValid = false;
        } else if (courseCode.length > 20) {
            $("#courseCodeError").text("Course code must not exceed 20 characters.");
            isValid = false;
        }

        // Validate Course Name
        if ($("#CourseName").val().trim() === "") {
            $("#courseNameError").text("Course name is required.");
            isValid = false;
        }

        // Department is optional so no validation needed

        // Stop form submission if validation fails
        if (!isValid) {
            e.preventDefault();
        }
    });

    // -------------------------------------------------------
    // DELETE CONFIRMATION
    // Shows confirm dialog before deleting a course
    // -------------------------------------------------------
    $(".btn-delete").click(function (e) {

        var courseName = $(this).data("name");

        if (!confirm("Are you sure you want to delete course: "
            + courseName + "?\nThis cannot be undone.")) {
            e.preventDefault();
        }
    });

    // -------------------------------------------------------
    // TOGGLE STATUS CONFIRMATION
    // Shows confirm dialog before activating/deactivating
    // -------------------------------------------------------
    $(".btn-toggle").click(function (e) {

        var courseName = $(this).data("name");
        var status = $(this).data("status");

        // Set message based on current status
        var action = status == "1" ? "deactivate" : "activate";

        if (!confirm("Are you sure you want to " + action
            + " course: " + courseName + "?")) {
            e.preventDefault();
        }
    });

    // -------------------------------------------------------
    // AUTO UPPERCASE Course Code input
    // Course codes are typically in uppercase
    // -------------------------------------------------------
    $("#CourseCode").on("input", function () {
        $(this).val($(this).val().toUpperCase());
    });

    // -------------------------------------------------------
    // CHARACTER COUNTER for Course Code
    // Shows remaining characters allowed
    // -------------------------------------------------------
    $("#CourseCode").on("keyup", function () {

        var maxLength = 20;
        var currentLen = $(this).val().length;
        var remaining = maxLength - currentLen;

        // Update character counter display
        $("#courseCodeCounter").text(remaining + " characters remaining");

        // Warn when running low
        if (remaining <= 5) {
            $("#courseCodeCounter").css("color", "red");
        } else {
            $("#courseCodeCounter").css("color", "gray");
        }
    });

    // -------------------------------------------------------
    // FILTER courses by Active/Inactive status
    // -------------------------------------------------------
    $("#filterStatus").on("change", function () {

        var value = $(this).val();

        $("#courseTable tbody tr").each(function () {

            var statusCell = $(this).find(".status-badge").text().trim().toLowerCase();

            if (value === "") {
                // Show all rows
                $(this).show();
            } else if (value === "active" && statusCell === "active") {
                $(this).show();
            } else if (value === "inactive" && statusCell === "inactive") {
                $(this).show();
            } else {
                $(this).hide();
            }
        });
    });

});