// report.js
// Handles report page filtering, printing and export behavior

$(document).ready(function () {

    // -------------------------------------------------------
    // AUTO DISMISS alert messages after 4 seconds
    // -------------------------------------------------------
    setTimeout(function () {
        $(".alert").fadeOut("slow");
    }, 4000);

    // -------------------------------------------------------
    // SEARCH filter for ledger report table
    // -------------------------------------------------------
    $("#searchLedger").on("keyup", function () {

        var value = $(this).val().toLowerCase();

        $("#ledgerTable tbody tr").filter(function () {
            $(this).toggle(
                $(this).text().toLowerCase().indexOf(value) > -1
            );
        });
    });

    // -------------------------------------------------------
    // SEARCH filter for assessment report table
    // -------------------------------------------------------
    $("#searchAssessmentReport").on("keyup", function () {

        var value = $(this).val().toLowerCase();

        $("#assessmentReportTable tbody tr").filter(function () {
            $(this).toggle(
                $(this).text().toLowerCase().indexOf(value) > -1
            );
        });
    });

    // -------------------------------------------------------
    // PRINT REPORT button
    // Hides filter controls then prints
    // -------------------------------------------------------
    $("#btnPrint").click(function () {

        // Hide filter section before printing
        $(".filter-section, .no-print").hide();

        // Trigger browser print dialog
        window.print();

        // Show filter section again after printing
        $(".filter-section, .no-print").show();
    });

    // -------------------------------------------------------
    // EXPORT TO CSV
    // Converts table data to downloadable CSV file
    // -------------------------------------------------------
    $("#btnExportCSV").click(function () {

        // Get the report table
        var table = $("#ledgerTable, #assessmentReportTable").first();

        if (table.length === 0) {
            alert("No table found to export.");
            return;
        }

        var csv = [];
        var rows = table.find("tr");

        // Loop through each row and cell
        rows.each(function () {

            var row = [];
            var cols = $(this).find("th, td");

            cols.each(function () {
                // Clean cell text and wrap in quotes
                var text = $(this).text().trim()
                    .replace(/"/g, '""');
                row.push('"' + text + '"');
            });

            csv.push(row.join(","));
        });

        // Create CSV file and trigger download
        var csvContent = csv.join("\n");
        var blob = new Blob([csvContent],
            { type: "text/csv;charset=utf-8;" });
        var url = URL.createObjectURL(blob);
        var link = document.createElement("a");

        // Set filename with current date
        var today = new Date();
        var filename = "report_" + today.toISOString().slice(0, 10) + ".csv";

        link.setAttribute("href", url);
        link.setAttribute("download", filename);
        link.style.display = "none";

        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
    });

    // -------------------------------------------------------
    // HIGHLIGHT rows with high remaining balance
    // Marks students with balance > 50% of assessment
    // -------------------------------------------------------
    $("#ledgerTable tbody tr").each(function () {

        var net = parseFloat($(this).find(".col-net")
            .text().replace(/[₱,]/g, "")) || 0;
        var balance = parseFloat($(this).find(".col-balance")
            .text().replace(/[₱,]/g, "")) || 0;

        if (net > 0 && balance > 0) {

            var percent = (balance / net) * 100;

            // Red highlight if more than 50% unpaid
            if (percent > 50) {
                $(this).addClass("danger");

                // Yellow highlight if between 1% and 50% unpaid
            } else if (percent > 0) {
                $(this).addClass("warning");

                // Green highlight if fully paid
            } else {
                $(this).addClass("success");
            }
        }
    });

    // -------------------------------------------------------
    // FILTER ledger by balance status
    // Shows only students with balance, fully paid, etc.
    // -------------------------------------------------------
    $("#filterBalance").on("change", function () {

        var value = $(this).val();

        $("#ledgerTable tbody tr").each(function () {

            var balance = parseFloat($(this).find(".col-balance")
                .text().replace(/[₱,]/g, "")) || 0;

            if (value === "") {
                // Show all
                $(this).show();

            } else if (value === "paid" && balance <= 0) {
                // Show only fully paid
                $(this).show();

            } else if (value === "unpaid" && balance > 0) {
                // Show only with remaining balance
                $(this).show();

            } else {
                $(this).hide();
            }
        });
    });

    // -------------------------------------------------------
    // FILTER assessment report by semester
    // -------------------------------------------------------
    $("#filterSemester").on("change", function () {

        var value = $(this).val().toLowerCase();

        $("#assessmentReportTable tbody tr").each(function () {

            var semCell = $(this).find(".col-semester")
                .text().trim().toLowerCase();

            if (value === "" || semCell === value) {
                $(this).show();
            } else {
                $(this).hide();
            }
        });
    });

    // -------------------------------------------------------
    // UPDATE grand totals after filtering
    // Recomputes totals based on visible rows only
    // -------------------------------------------------------
    function updateGrandTotals() {

        var totalNet = 0;
        var totalPaid = 0;
        var totalBalance = 0;

        // Loop through visible rows only
        $("#ledgerTable tbody tr:visible").each(function () {

            totalNet += parseFloat($(this).find(".col-net")
                .text().replace(/[₱,]/g, "")) || 0;
            totalPaid += parseFloat($(this).find(".col-paid")
                .text().replace(/[₱,]/g, "")) || 0;
            totalBalance += parseFloat($(this).find(".col-balance")
                .text().replace(/[₱,]/g, "")) || 0;
        });

        // Update footer totals
        $("#footerNet").text(formatCurrency(totalNet));
        $("#footerPaid").text(formatCurrency(totalPaid));
        $("#footerBalance").text(formatCurrency(totalBalance));
    }

    // Update totals when filters change
    $("#filterBalance, #searchLedger").on("change keyup", function () {
        setTimeout(updateGrandTotals, 100);
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

    // Run on page load
    updateGrandTotals();

});