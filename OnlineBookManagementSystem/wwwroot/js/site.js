// Modern JWT-based site functionality
const userName = sessionStorage.getItem("userName");
const userRole = sessionStorage.getItem("userRole");

// Update username display
if (userName) {
    $("#username").text(userName);
}

// Home link navigation with JWT authentication
$("#homeLink").click(function (event) {
    event.preventDefault();

    const token = sessionStorage.getItem("jwt");
    if (!token) {
        alert("Session expired. Please login again.");
        window.location.href = "/Auth/Login";
        return;
    }

    // Determine target URL based on role
    let targetUrl;
    switch (userRole) {
        case "SuperAdmin":
            targetUrl = "/SuperAdmin/Dashboard";
            break;
        case "Admin":
            targetUrl = "/Admin/Dashboard";
            break;
        case "User":
            targetUrl = "/User/Dashboard";
            break;
        default:
            targetUrl = "/Auth/Login";
    }

    // Navigate with JWT validation
    fetch(targetUrl, {
        method: "GET",
        headers: {
            "Authorization": `Bearer ${token}`,
            "Content-Type": "application/json"
        }
    })
        .then(response => {
            if (response.ok) {
                window.location.href = targetUrl;
            } else {
                throw new Error("Access denied or session expired");
            }
        })
        .catch(error => {
            console.error("Navigation error:", error);
            alert("Error: You don't have access or session expired.");
            window.location.href = "/Auth/Login";
        });
});

// Date and time display
function updateDateTime() {
    const time = new Date();
    const formattedTime = time.toLocaleTimeString();
    const options = { day: '2-digit', month: 'short', year: 'numeric' };
    const date = time.toLocaleDateString('en-GB', options).replace(/ /g, '-');

    $(".currentdate").html(date);
    $(".currenttime").html(formattedTime);
}

// Initialize and update time
updateDateTime();
setInterval(updateDateTime, 1000);

// Logout function
function logout() {
    sessionStorage.clear();
    localStorage.clear();
    window.location.href = "/Auth/Login";
}

// Document ready functions
$(document).ready(function () {
    // Sidebar toggle functionality
    $("#toggleSidebar").click(function () {
        $("#sidebar").toggleClass("collapsed");
        $(".content").toggleClass("sidebar-collapsed");
    });

    // Display user role
    if (userRole) {
        $("#SupportloginDetail").html(userRole);
    }
});


