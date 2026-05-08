// assessment.js
// Handles assessment form validation, auto-computation and AJAX calls

$(document).ready(function () {

    // -------------------------------------------------------
    // AUTO DISMISS alert messages after 4 seconds
    // -------------------------------------------------------
    setTimeout(function () {
        $(".alert").fadeOut("slow");
    }, 4000);

    // -------------------------------------------------------
    // SEARCH filter for assessment list table
    // -------------------------------------------------------
    $("#searchAssessment").on("keyup", function () {

        var value = $(this).val().toLowerCase();

        $("#assessmentTable tbody tr").filter(function () {
            $(this).toggle(
                $(this).text().toLowerCase().indexOf(value) > -1
            );
        });
    });

    // -------------------------------------------------------
    // WHEN STUDENT IS SELECTED
    // Auto-fill course, year level, semester, school year
    // via AJAX call to AssessmentController.GetStudentInfo
    // -------------------------------------------------------
    $("#StudentID").on("change", function () {

        var studentID = $(this).val();

        // Clear fields if no student selected
        if (studentID === "" || studentID === "0") {
            clearFeeFields();
            return;
        }

        // AJAX call to get student info
        $.ajax({
            url: "/Assessment/GetStudentInfo",
            type: "GET",
            data: { studentID: studentID },
            success: function (response) {

                if (response.success) {
                    // Auto fill student related fields
                    $("#CourseID").val(response.courseID);
                    $("#YearLevel").val(response.yearLevel);
                    $("#Semester").val(response.semester);
                    $("#SchoolYear").val(response.schoolYear);

                    // Trigger fee schedule load after student info is set
                    loadFeeSchedule();

                } else {
                    alert("Student information not found.");
                }
            },
            error: function () {
                alert("Error loading student information.");
            }
        });
    });

    // -------------------------------------------------------
    // LOAD FEE SCHEDULE via AJAX
    // Called when course, school year, or semester changes
    // Fetches fee data from FeeScheduleController
    // -------------------------------------------------------
    function loadFeeSchedule() {

        var courseID = $("#CourseID").val();
        var schoolYear = $("#SchoolYear").val().trim();
        var semester = $("#Semester").val();

        // Need all 3 values to load fee schedule
        if (!courseID || !schoolYear || !semester) return;

        $.ajax({
            url: "/FeeSchedule/GetByCourseSemester",
            type: "GET",
            data: {
                courseID: courseID,
                schoolYear: schoolYear,
                semester: semester
            },
            success: function (response) {

                if (response.success) {
                    // Auto fill fee schedule fields
                    $("#FeeScheduleID").val(response.feeScheduleID);
                    $("#TuitionFeePerUnit").val(response.tuitionFeePerUnit);
                    $("#RegistrationFee").val(response.registrationFee);
                    $("#MiscellaneousFee").val(response.miscellaneousFee);
                    $("#LaboratoryFee").val(response.laboratoryFee);

                    // Recompute totals after loading fee schedule
                    computeAssessment();

                } else {
                    // No fee schedule found for this combination
                    clearFeeFields();
                    $("#feeScheduleAlert").show();
                }
            },
            error: function () {
                alert("Error loading fee schedule.");
            }
        });
    }

    // Trigger fee schedule load when these fields change
    $("#Semester, #SchoolYear").on("change blur", function () {
        loadFeeSchedule();
    });

    // -------------------------------------------------------
    // COMPUTE ASSESSMENT TOTALS
    // Runs whenever units, fees, scholarship, or discount changes
    // -------------------------------------------------------
    function computeAssessment() {

        var units = parseInt($("#UnitsEnrolled").val()) || 0;
        var tuition = parseFloat($("#TuitionFeePerUnit").val()) || 0;
        var regFee = parseFloat($("#RegistrationFee").val()) || 0;
        var miscFee = parseFloat($("#MiscellaneousFee").val()) || 0;
        var labFee = parseFloat($("#LaboratoryFee").val()) || 0;

        // Compute tuition based on units enrolled
        var tuitionTotal = tuition * units;
        var totalFee = tuitionTotal + regFee + miscFee + labFee;

        // Get scholarship deduction percent
        var scholarshipDeduction = 0;
        var scholarshipPercent = parseFloat(
            $("#ScholarshipID option:selected")
                .data("percent")) || 0;

        if (scholarshipPercent > 0) {
            scholarshipDeduction = (scholarshipPercent / 100) * totalFee;
        }

        // Get discount deduction (amount or percent)
        var discountDeduction = 0;
        var discountAmount = parseFloat(
            $("#DiscountID option:selected")
                .data("amount")) || 0;
        var discountPercent = parseFloat(
            $("#DiscountID option:selected")
                .data("percent")) || 0;

        if (discountPercent > 0) {
            discountDeduction = (discountPercent / 100) * totalFee;
        } else if (discountAmount > 0) {
            discountDeduction = discountAmount;
        }

        // Compute net assessment after deductions
        var netAssessment = totalFee - scholarshipDeduction - discountDeduction;

        // Update breakdown display fields
        $("#displayTuition").text(formatCurrency(tuitionTotal));
        $("#displayRegFee").text(formatCurrency(regFee));
        $("#displayMiscFee").text(formatCurrency(miscFee));
        $("#displayLabFee").text(formatCurrency(labFee));
        $("#displayTotal").text(formatCurrency(totalFee));
        $("#displayScholarship").text("- " + formatCurrency(scholarshipDeduction));
        $("#displayDiscount").text("- " + formatCurrency(discountDeduction));
        $("#displayNet").text(formatCurrency(netAssessment));

        // Update payment terms breakdown (Prelim/Mid/Semi/Final)
        $("#displayPrelim").text(formatCurrency(netAssessment * 0.53));
        $("#displayMidterm").text(formatCurrency(netAssessment * 0.64));
        $("#displaySemiFinal").text(formatCurrency(netAssessment * 0.75));
        $("#displayFinal").text(formatCurrency(netAssessment * 1.00));
    }

    // Trigger computation when any of these fields change
    $("#UnitsEnrolled, #ScholarshipID, #DiscountID").on("change input",
        computeAssessment);

    // -------------------------------------------------------
    // FORMAT currency helper function
    // -------------------------------------------------------
    function formatCurrency(value) {
        return "₱" + value.toLocaleString("en-PH", {
            minimumFractionDigits: 2,
            maximumFractionDigits: 2
        });
    }

    // -------------------------------------------------------
    // CLEAR all fee fields
    // Called when no student or fee schedule is found
    // -------------------------------------------------------
    function clearFeeFields() {
        $("#FeeScheduleID").val("");
        $("#TuitionFeePerUnit").val("");
        $("#RegistrationFee").val("");
        $("#MiscellaneousFee").val("");
        $("#LaboratoryFee").val("");
        $("#displayTuition, #displayRegFee").text("₱0.00");
        $("#displayMiscFee, #displayLabFee").text("₱0.00");
        $("#displayTotal, #displayNet").text("₱0.00");
        $("#displayScholarship, #displayDiscount").text("- ₱0.00");
        $("#displayPrelim, #displayMidterm").text("₱0.00");
        $("#displaySemiFinal, #displayFinal").text("₱0.00");
    }

    // -------------------------------------------------------
    // FORM VALIDATION before submit
    // -------------------------------------------------------
    $("#assessmentForm").submit(function (e) {

        var isValid = true;
        $(".field-error").text("");

        // Validate Student selection
        if ($("#StudentID").val() === "" || $("#StudentID").val() === "0") {
            $("#studentError").text("Please select a student.");
            isValid = false;
        }

        // Validate Fee Schedule is loaded
        if ($("#FeeScheduleID").val() === "" || $("#FeeScheduleID").val() === "0") {
            $("#feeScheduleError").text("No fee schedule found for selected " +
                "course, school year and semester.");
            isValid = false;
        }

        // Validate Units Enrolled
        var units = parseInt($("#UnitsEnrolled").val());
        if (isNaN(units) || units <= 0) {
            $("#unitsError").text("Units enrolled must be greater than 0.");
            isValid = false;
        } else if (units > 30) {
            $("#unitsError").text("Units enrolled cannot exceed 30.");
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
    // Run computation on page load (for edit form)
    // -------------------------------------------------------
    computeAssessment();

});