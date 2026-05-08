// discount.js
// Handles discount management form validation and behavior

$(document).ready(function () {

    // -------------------------------------------------------
    // AUTO DISMISS alert messages after 4 seconds
    // -------------------------------------------------------
    setTimeout(function () {
        $(".alert").fadeOut("slow");
    }, 4000);

    // -------------------------------------------------------
    // SEARCH filter for discount list table
    // -------------------------------------------------------
    $("#searchDiscount").on("keyup", function () {

        var value = $(this).val().toLowerCase();

        // Filter rows by matching any cell content
        $("#discountTable tbody tr").filter(function () {
            $(this).toggle(
                $(this).text().toLowerCase().indexOf(value) > -1
            );
        });
    });

    // -------------------------------------------------------
    // DISCOUNT TYPE TOGGLE
    // Shows either fixed amount OR percent input
    // based on what the user selects
    // -------------------------------------------------------
    $("#discountType").on("change", function () {

        var type = $(this).val();

        if (type === "amount") {
            // Show fixed amount, hide percent
            $("#amountGroup").show();
            $("#percentGroup").hide();
            $("#DiscountPercent").val("0");

        } else if (type === "percent") {
            // Show percent, hide fixed amount
            $("#amountGroup").hide();
            $("#percentGroup").show();
            $("#DiscountAmount").val("0");

        } else {
            // Show both if none selected
            $("#amountGroup").show();
            $("#percentGroup").show();
        }
    });

    // -------------------------------------------------------
    // TRIGGER discount type on page load
    // Sets correct visible fields when editing
    // -------------------------------------------------------
    $("#discountType").trigger("change");

    // -------------------------------------------------------
    // DISCOUNT FORM VALIDATION (Create and Edit)
    // -------------------------------------------------------
    $("#discountForm").submit(function (e) {

        var isValid = true;

        // Clear all previous error messages
        $(".field-error").text("");

        // Validate Discount Name
        if ($("#DiscountName").val().trim() === "") {
            $("#discountNameError").text("Discount name is required.");
            isValid = false;
        }

        // Get amount and percent values
        var amount = parseFloat($("#DiscountAmount").val()) || 0;
        var percent = parseFloat($("#DiscountPercent").val()) || 0;

        // Validate that at least one value is provided
        if (amount === 0 && percent === 0) {
            $("#discountValueError").text(
                "Please enter either a discount amount or percent.");
            isValid = false;
        }

        // Validate percent range if provided
        if (percent > 0 && (percent <= 0 || percent > 100)) {
            $("#discountPercentError").text(
                "Discount percent must be between 1 and 100.");
            isValid = false;
        }

        // Validate amount is positive if provided
        if (amount < 0) {
            $("#discountAmountError").text(
                "Discount amount cannot be negative.");
            isValid = false;
        }

        // Stop form submission if validation fails
        if (!isValid) {
            e.preventDefault();
        }
    });

    // -------------------------------------------------------
    // LIVE PREVIEW of discount value
    // Shows computed discount as user types
    // -------------------------------------------------------
    function updatePreview() {

        var amount = parseFloat($("#DiscountAmount").val()) || 0;
        var percent = parseFloat($("#DiscountPercent").val()) || 0;

        if (amount > 0) {
            // Fixed amount preview
            $("#discountPreview").text(
                "Fixed deduction of ₱" + amount.toLocaleString("en-PH", {
                    minimumFractionDigits: 2,
                    maximumFractionDigits: 2
                })
            ).css("color", "green");

        } else if (percent > 0 && percent <= 100) {
            // Percent preview
            $("#discountPreview").text(
                percent + "% deduction from total fee"
            ).css("color", "green");

        } else {
            $("#discountPreview").text("").css("color", "");
        }
    }

    // Update preview when values change
    $("#DiscountAmount, #DiscountPercent").on("input", updatePreview);

    // Run preview on page load (for edit form)
    updatePreview();

    // -------------------------------------------------------
    // DELETE CONFIRMATION
    // Shows confirm dialog before deleting a discount
    // -------------------------------------------------------
    $(".btn-delete").click(function (e) {

        var name = $(this).data("name");

        if (!confirm("Are you sure you want to delete discount: "
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
            + " discount: " + name + "?")) {
            e.preventDefault();
        }
    });

    // -------------------------------------------------------
    // FILTER discounts by Active/Inactive status
    // -------------------------------------------------------
    $("#filterStatus").on("change", function () {

        var value = $(this).val();

        $("#discountTable tbody tr").each(function () {

            var statusCell = $(this).find(".status-badge")
                .text().trim().toLowerCase();

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
    // NUMBERS ONLY for amount and percent inputs
    // Allows decimal point as well
    // -------------------------------------------------------
    $("#DiscountAmount, #DiscountPercent").on("keypress", function (e) {

        var char = String.fromCharCode(e.which);

        // Allow numbers and one decimal point only
        if (!/[0-9.]/.test(char)) {
            e.preventDefault();
        }

        // Prevent more than one decimal point
        if (char === "." && $(this).val().indexOf(".") !== -1) {
            e.preventDefault();
        }
    });

});