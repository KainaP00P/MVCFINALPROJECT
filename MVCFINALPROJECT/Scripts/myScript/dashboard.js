// dashboard.js
// Handles dashboard page behavior and summary card animations

$(document).ready(function () {

    // -------------------------------------------------------
    // AUTO DISMISS alert messages after 4 seconds
    // -------------------------------------------------------
    setTimeout(function () {
        $(".alert").fadeOut("slow");
    }, 4000);

    // -------------------------------------------------------
    // ANIMATE summary cards on page load
    // Counts up from 0 to the actual value
    // -------------------------------------------------------
    $(".summary-count").each(function () {

        var $this = $(this);
        var countTo = parseInt($this.data("count"));

        // Skip animation if value is 0
        if (isNaN(countTo) || countTo === 0) return;

        // Animate the number counting up
        $({ countNum: 0 }).animate({ countNum: countTo }, {
            duration: 1500,
            easing: "swing",
            step: function () {
                $this.text(Math.floor(this.countNum).toLocaleString());
            },
            complete: function () {
                $this.text(countTo.toLocaleString());
            }
        });
    });

    // -------------------------------------------------------
    // ANIMATE currency values (collections total)
    // -------------------------------------------------------
    $(".summary-currency").each(function () {

        var $this = $(this);
        var countTo = parseFloat($this.data("count"));

        if (isNaN(countTo) || countTo === 0) return;

        $({ countNum: 0 }).animate({ countNum: countTo }, {
            duration: 1500,
            easing: "swing",
            step: function () {
                // Format as currency while counting
                $this.text("₱" + this.countNum.toLocaleString("en-PH", {
                    minimumFractionDigits: 2,
                    maximumFractionDigits: 2
                }));
            },
            complete: function () {
                $this.text("₱" + countTo.toLocaleString("en-PH", {
                    minimumFractionDigits: 2,
                    maximumFractionDigits: 2
                }));
            }
        });
    });

    // -------------------------------------------------------
    // RECENT PAYMENTS TABLE - highlight rows on hover
    // -------------------------------------------------------
    $("#recentPaymentsTable tbody tr").hover(
        function () {
            $(this).addClass("active");
        },
        function () {
            $(this).removeClass("active");
        }
    );

    // -------------------------------------------------------
    // RECENT STUDENTS TABLE - highlight rows on hover
    // -------------------------------------------------------
    $("#recentStudentsTable tbody tr").hover(
        function () {
            $(this).addClass("active");
        },
        function () {
            $(this).removeClass("active");
        }
    );

    // -------------------------------------------------------
    // SEARCH filter for recent payments table
    // -------------------------------------------------------
    $("#searchPayments").on("keyup", function () {

        var value = $(this).val().toLowerCase();

        // Filter rows by matching any cell content
        $("#recentPaymentsTable tbody tr").filter(function () {
            $(this).toggle(
                $(this).text().toLowerCase().indexOf(value) > -1
            );
        });
    });

    // -------------------------------------------------------
    // SEARCH filter for recent students table
    // -------------------------------------------------------
    $("#searchStudents").on("keyup", function () {

        var value = $(this).val().toLowerCase();

        $("#recentStudentsTable tbody tr").filter(function () {
            $(this).toggle(
                $(this).text().toLowerCase().indexOf(value) > -1
            );
        });
    });

    // -------------------------------------------------------
    // REFRESH dashboard data every 5 minutes
    // Reloads the page to get latest counts
    // -------------------------------------------------------
    setTimeout(function () {
        location.reload();
    }, 300000); // 300000ms = 5 minutes

});