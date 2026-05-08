// payment.js
// Handles payment form validation, AJAX calls and behavior

$(document).ready(function () {

    // -------------------------------------------------------
    // AUTO DISMISS alert messages after 4 seconds
    // -------------------------------------------------------
    setTimeout(function () {
        $(".alert").fadeOut("slow");
    }, 4000);

    // -------------------------------------------------------
    // SEARCH filter for payment list table
    // -------------------------------------------------------
    $("#searchPayment").on("keyup", function () {

        var value = $(this).val().toLowerCase();

        $("#paymentTable tbody tr").filter(function () {
            $(this).toggle(
                $(this).text().toLowerCase().indexOf(value) > -1
            );
        });
    });

    // -------------------------------------------------------
    // WHEN PAYMENT TERM IS SELECTED
    // Auto-fill amount due via AJAX call
    // to PaymentController.GetTermAmount
    // -------------------------------------------------------
    $("#TermID").on("change", function () {

        var termID = $(this).val();

        // Clear amount if no term selected
        if (termID === "" || termID === "0") {
            $("#AmountPaid").val("");
            $("#displayAmountDue").text("₱0.00");
            $("#displayTermName").text("");
            return;
        }

        // AJAX call to get term amount due
        $.ajax({
            url: "/Payment/GetTermAmount",
            type: "GET",
            data: { termID: termID },
            success: function (response) {

                if (response.success) {
                    var amountDue = parseFloat(response.amountDue);

                    // Auto fill amount paid with amount due
                    $("#AmountPaid").val(amountDue.toFixed(2));

                    // Update display fields
                    $("#displayAmountDue").text(formatCurrency(amountDue));
                    $("#displayTermName").text(response.termName);

                    // Update minimum amount hint
                    $("#amountHint").text(
                        "Minimum amount: " + formatCurrency(amountDue));

                } else {
                    alert("Term information not found.");
                }
            },
            error: function () {
                alert("Error loading term amount.");
            }
        });
    });

    // -------------------------------------------------------
    // LIVE COMPUTATION of change/excess amount
    // Shows how much change if student pays more than due
    // -------------------------------------------------------
    $("#AmountPaid").on("input", function () {

        var amountPaid = parseFloat($(this).val()) || 0;
        var amountDue = parseFloat(
            $("#displayAmountDue").text()
                .replace(/[₱,]/g, "")) || 0;

        if (amountPaid > 0 && amountDue > 0) {

            var change = amountPaid - amountDue;

            if (change > 0) {
                // Student paid more than required
                $("#displayChange").text(formatCurrency(change))
                    .css("color", "green");
                $("#changeLabel").text("Change:");

            } else if (change < 0) {
                // Student paid less than required
                $("#displayChange").text(formatCurrency(Math.abs(change)))
                    .css("color", "red");
                $("#changeLabel").text("Short by:");

            } else {
                // Exact payment
                $("#displayChange").text("Exact Payment")
                    .css("color", "blue");
                $("#changeLabel").text("");
            }
        } else {
            $("#displayChange").text("₱0.00").css("color", "");
            $("#changeLabel").text("");
        }
    });

    // -------------------------------------------------------
    // SET today's date as default payment date
    // -------------------------------------------------------
    if ($("#PaymentDate").val() === "") {

        var today = new Date();
        var dd = String(today.getDate()).padStart(2, "0");
        var mm = String(today.getMonth() + 1).padStart(2, "0");
        var yyyy = today.getFullYear();

        // Format as YYYY-MM-DD for date input
        $("#PaymentDate").val(yyyy + "-" + mm + "-" + dd);
    }

    // -------------------------------------------------------
    // PAYMENT FORM VALIDATION before submit
    // -------------------------------------------------------
    $("#paymentForm").submit(function (e) {

        var isValid = true;
        $(".field-error").text("");

        // Validate Term selection
        if ($("#TermID").val() === "" || $("#TermID").val() === "0") {
            $("#termError").text("Please select a payment term.");
            isValid = false;
        }

        // Validate Amount Paid
        var amountPaid = parseFloat($("#AmountPaid").val());
        if (isNaN(amountPaid) || amountPaid <= 0) {
            $("#amountError").text("Amount paid must be greater than 0.");
            isValid = false;
        }

        // Validate amount is not less than amount due
        var amountDue = parseFloat(
            $("#displayAmountDue").text()
                .replace(/[₱,]/g, "")) || 0;

        if (amountPaid < amountDue) {
            $("#amountError").text(
                "Amount paid cannot be less than " + formatCurrency(amountDue));
            isValid = false;
        }

        // Validate Payment Date
        if ($("#PaymentDate").val() === "") {
            $("#dateError").text("Payment date is required.");
            isValid = false;
        }

        // Validate Payment Method
        if ($("#PaymentMethod").val() === "") {
            $("#methodError").text("Please select a payment method.");
            isValid = false;
        }

        // Validate OR Number
        if ($("#ORNumber").val().trim() === "") {
            $("#orError").text("OR Number is required.");
            isValid = false;
        }

        // Stop submission if invalid
        if (!isValid) {
            e.preventDefault();

            // Scroll to first error
            $("html, body").animate({
                scrollTop: $(".field-error:not(:empty)").first().offset().top - 100
            }, 500);
        }
    });

    // -------------------------------------------------------
    // NUMBERS ONLY for Amount Paid input
    // Allows decimal point as well
    // -------------------------------------------------------
    $("#AmountPaid").on("keypress", function (e) {

        var char = String.fromCharCode(e.which);

        if (!/[0-9.]/.test(char)) {
            e.preventDefault();
        }

        // Prevent more than one decimal point
        if (char === "." && $(this).val().indexOf(".") !== -1) {
            e.preventDefault();
        }
    });

    // -------------------------------------------------------
    // FILTER payments by payment method
    // -------------------------------------------------------
    $("#filterMethod").on("change", function () {

        var value = $(this).val().toLowerCase();

        $("#paymentTable tbody tr").each(function () {

            var methodCell = $(this).find(".method-cell")
                .text().trim().toLowerCase();

            if (value === "" || methodCell === value) {
                $(this).show();
            } else {
                $(this).hide();
            }
        });
    });

    // -------------------------------------------------------
    // PRINT payment receipt button
    // -------------------------------------------------------
    $("#btnPrint").click(function () {
        window.print();
    });

    // -------------------------------------------------------
    // FORMAT currency helper function
    // -------------------------------------------------------
    function formatCurrency(value) {
        return "₱" + value.toLocaleString("en-PH", {
            minimumFractionDigits: 2,
            maximumFractionDigits: 2
        });
    }

});