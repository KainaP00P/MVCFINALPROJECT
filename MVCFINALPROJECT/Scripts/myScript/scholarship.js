// scholarship.js
// Handles scholarship management form validation and behavior

$(document).ready(function () {

    // -------------------------------------------------------
    // AUTO DISMISS alert messages after 4 seconds
    // -------------------------------------------------------
    setTimeout(function () {
        $(".alert").fadeOut("slow");
    }, 4000);

    // -------------------------------------------------------
    // SEARCH filter for scholarship list table
    // -------------------------------------------------------
    $("#searchScholarship").on("keyup", function () {

        var value = $(this).val().toLowerCase();

        // Filter rows by matching any cell content
        $("#scholarshipTable tbody tr").filter(function () {
            $(this).toggle(
                $(this).text().toLowerCase().indexOf(value) > -1
            );
        });
    });

    // -------------------------------------------------------
    // SCHOLARSHIP FORM VALIDATION (Create and Edit)
    // -------------------------------------------------------
    $("#scholarshipForm").submit(function (e) {

        var isValid = true;

        // Clear all previous error messages
        $(".field-error").text("");

        // Validate Scholarship Name
        if ($("#ScholarshipName").val().trim() === "") {
            $("#scholarshipNameError").text("Scholarship name is required.");
            isValid = false;
        }

        // Validate Discount Percent
        var percent = parseFloat($("#DiscountPercent").val());

        if ($("#DiscountPercent").val() === "") {
            $("#discountPercentError").text("Discount percent is required.");
            isValid = false;
        } else if (isNaN(percent)) {
            $("#discountPercentError").text("Please enter a valid number.");
            isValid = false;
        } else if (percent <= 0 || percent > 100) {
            $("#discountPercentError").text("Discount percent must be between 1 and 100.");
            isValid = false;
        }

        // Stop form submission if validation fails
        if (!isValid) {
            e.preventDefault();
        }
    });

    // -------------------------------------------------------
    // LIVE PREVIEW of discount percent
    // Shows what the scholarship covers as user types
    // -------------------------------------------------------
    $("#DiscountPercent").on("input", function () {

        var percent = parseFloat($(this).val());

        if (!isNaN(percent) && percent > 0 && percent <= 100) {

            var label = "";

            // Show label based on percentage value
            if (percent === 100) {
                label = "Full Scholarship (100%)";
            } else if (percent >= 75) {
                label = "75% or more - Major Scholarship";
            } else if (percent >= 50) {
                label = "50% or more - Half Scholarship";
            } else {
                label = "Partial Scholarship (" + percent + "%)";
            }

            $("#percentPreview").text(label).css("color", "green");

        } else {
            $("#percentPreview").text("").css("color", "");
        }
    });

    // -------------------------------------------------------
    // DELETE CONFIRMATION
    // Shows confirm dialog before deleting a scholarship
    // -------------------------------------------------------
    $(".btn-delete").click(function (e) {

        var name = $(this).data("name");

        if (!confirm("Are you sure you want to delete scholarship: "
            + name + "?\nThis cannot be undone.")) {
            e.preventDefault();
        }
    });

    // -------------------------------------------------------
    // TOGGLE STATUS CONFIRMATION
    // Shows confirm before activating/deactivating
    // -------------------------------------------------------
    $(".btn-toggle").click(function (e) {

        var name = $(this).data("name");
        var status = $(this).data("status");
        var action = status == "1" ? "deactivate" : "activate";

        if (!confirm("Are you sure you want to " + action
            + " scholarship: " + name + "?")) {
            e.preventDefault();
        }
    });

    // -------------------------------------------------------
    // FILTER scholarships by Active/Inactive status
    // -------------------------------------------------------
    $("#filterStatus").on("change", function () {

        var value = $(this).val();

        $("#scholarshipTable tbody tr").each(function () {

            var statusCell = $(this).find(".status-badge").text().trim().toLowerCase();

            if (value === "") {
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

    // -------------------------------------------------------
    // NUMBERS ONLY for Discount Percent input
    // Allows decimal point as well
    // -------------------------------------------------------
    $("#DiscountPercent").on("keypress", function (e) {

        var char = String.fromCharCode(e.which);

        // Allow numbers and one decimal point
        if (!/[0-9.]/.test(char)) {
            e.preventDefault();
        }

        // Prevent more than one decimal point
        if (char === "." && $(this).val().indexOf(".") !== -1) {
            e.preventDefault();
        }
    });

});