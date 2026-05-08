// feeSchedule.js
// Handles fee schedule form validation and behavior

$(document).ready(function () {

    // -------------------------------------------------------
    // AUTO DISMISS alert messages after 4 seconds
    // -------------------------------------------------------
    setTimeout(function () {
        $(".alert").fadeOut("slow");
    }, 4000);

    // -------------------------------------------------------
    // SEARCH filter for fee schedule list table
    // -------------------------------------------------------
    $("#searchFeeSchedule").on("keyup", function () {

        var value = $(this).val().toLowerCase();

        // Filter rows by matching any cell content
        $("#feeScheduleTable tbody tr").filter(function () {
            $(this).toggle(
                $(this).text().toLowerCase().indexOf(value) > -1
            );
        });
    });

    // -------------------------------------------------------
    // FEE SCHEDULE FORM VALIDATION (Create and Edit)
    // -------------------------------------------------------
    $("#feeScheduleForm").submit(function (e) {

        var isValid = true;

        // Clear all previous error messages
        $(".field-error").text("");

        // Validate Course selection
        if ($("#CourseID").val() === "" || $("#CourseID").val() === "0") {
            $("#courseError").text("Please select a course.");
            isValid = false;
        }

        // Validate School Year format (YYYY-YYYY)
        var schoolYear = $("#SchoolYear").val().trim();
        if (schoolYear === "") {
            $("#schoolYearError").text("School year is required.");
            isValid = false;
        } else if (!/^\d{4}-\d{4}$/.test(schoolYear)) {
            $("#schoolYearError").text("Format must be YYYY-YYYY (e.g. 2024-2025).");
            isValid = false;
        }

        // Validate Semester selection
        if ($("#Semester").val() === "") {
            $("#semesterError").text("Please select a semester.");
            isValid = false;
        }

        // Validate Tuition Fee Per Unit
        var tuition = parseFloat($("#TuitionFeePerUnit").val());
        if (isNaN(tuition) || tuition <= 0) {
            $("#tuitionError").text("Tuition fee per unit must be greater than 0.");
            isValid = false;
        }

        // Validate Registration Fee
        var regFee = parseFloat($("#RegistrationFee").val());
        if (isNaN(regFee) || regFee < 0) {
            $("#regFeeError").text("Registration fee cannot be negative.");
            isValid = false;
        }

        // Validate Miscellaneous Fee
        var miscFee = parseFloat($("#MiscellaneousFee").val());
        if (isNaN(miscFee) || miscFee < 0) {
            $("#miscFeeError").text("Miscellaneous fee cannot be negative.");
            isValid = false;
        }

        // Validate Laboratory Fee
        var labFee = parseFloat($("#LaboratoryFee").val());
        if (isNaN(labFee) || labFee < 0) {
            $("#labFeeError").text("Laboratory fee cannot be negative.");
            isValid = false;
        }

        // Stop form submission if validation fails
        if (!isValid) {
            e.preventDefault();
        }
    });

    // -------------------------------------------------------
    // LIVE TOTAL COMPUTATION
    // Updates total fee preview as user types fee values
    // Uses a sample of 21 units for preview
    // -------------------------------------------------------
    function computeTotal() {

        var sampleUnits = parseInt($("#sampleUnits").val()) || 21;
        var tuition = parseFloat($("#TuitionFeePerUnit").val()) || 0;
        var regFee = parseFloat($("#RegistrationFee").val()) || 0;
        var miscFee = parseFloat($("#MiscellaneousFee").val()) || 0;
        var labFee = parseFloat($("#LaboratoryFee").val()) || 0;

        // Compute total based on sample units
        var tuitionTotal = tuition * sampleUnits;
        var grandTotal = tuitionTotal + regFee + miscFee + labFee;

        // Update preview fields
        $("#previewTuition").text("₱" + tuitionTotal.toLocaleString("en-PH", {
            minimumFractionDigits: 2, maximumFractionDigits: 2
        }));

        $("#previewTotal").text("₱" + grandTotal.toLocaleString("en-PH", {
            minimumFractionDigits: 2, maximumFractionDigits: 2
        }));

        // Show payment term breakdowns
        var prelim = grandTotal * 0.53;
        var midterm = grandTotal * 0.64;
        var semiFinal = grandTotal * 0.75;
        var final = grandTotal * 1.00;

        $("#previewPrelim").text("₱" + prelim.toLocaleString("en-PH", {
            minimumFractionDigits: 2, maximumFractionDigits: 2
        }));

        $("#previewMidterm").text("₱" + midterm.toLocaleString("en-PH", {
            minimumFractionDigits: 2, maximumFractionDigits: 2
        }));

        $("#previewSemiFinal").text("₱" + semiFinal.toLocaleString("en-PH", {
            minimumFractionDigits: 2, maximumFractionDigits: 2
        }));

        $("#previewFinal").text("₱" + final.toLocaleString("en-PH", {
            minimumFractionDigits: 2, maximumFractionDigits: 2
        }));
    }

    // Trigger computation on any fee field change
    $("#TuitionFeePerUnit, #RegistrationFee, #MiscellaneousFee, #LaboratoryFee")
        .on("input", computeTotal);

    // Also trigger when sample units change
    $("#sampleUnits").on("input", computeTotal);

    // Run on page load for edit form
    computeTotal();

    // -------------------------------------------------------
    // AUTO FORMAT School Year input
    // Automatically adds dash after 4 digits
    // -------------------------------------------------------
    $("#SchoolYear").on("input", function () {

        var val = $(this).val().replace(/[^0-9]/g, "");

        if (val.length > 4) {
            val = val.substring(0, 4) + "-" + val.substring(4, 8);
        }

        $(this).val(val);
    });

    // -------------------------------------------------------
    // NUMBERS ONLY for all fee input fields
    // Allows decimal point as well
    // -------------------------------------------------------
    $("#TuitionFeePerUnit, #RegistrationFee, #MiscellaneousFee, #LaboratoryFee")
        .on("keypress", function (e) {

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

    // -------------------------------------------------------
    // DELETE CONFIRMATION
    // Shows confirm dialog before deleting a fee schedule
    // -------------------------------------------------------
    $(".btn-delete").click(function (e) {

        var course = $(this).data("course");
        var year = $(this).data("year");

        if (!confirm("Are you sure you want to delete the fee schedule for "
            + course + " (" + year + ")?\nThis cannot be undone.")) {
            e.preventDefault();
        }
    });

    // -------------------------------------------------------
    // FILTER fee schedules by School Year
    // -------------------------------------------------------
    $("#filterYear").on("change", function () {

        var value = $(this).val().toLowerCase();

        $("#feeScheduleTable tbody tr").each(function () {

            var yearCell = $(this).find(".year-cell")
                .text().trim().toLowerCase();

            if (value === "" || yearCell === value) {
                $(this).show();
            } else {
                $(this).hide();
            }
        });
    });

});