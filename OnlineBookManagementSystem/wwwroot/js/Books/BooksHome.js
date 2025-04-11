$(document).ready(function () {
    const token = sessionStorage.getItem("jwtToken");

    if (!token) {
        alert("Token not found. Please log in again.");
        window.location.href = "/Auth/Login";
        return;
    }

    $.ajax({
        url: "/Books/Index", // Or your protected API endpoint
        type: "GET",
        headers: {
            "Authorization": "Bearer " + token
        },
        success: function (response) {
            // Handle successful response (e.g., render HTML, show data, etc.)
            console.log("Authorized:", response);
        },
        error: function (xhr, status, error) {
            if (xhr.status === 401) {
                alert("Unauthorized: Please log in again.");
                window.location.href = "/Auth/Login";
            } else {
                alert("An error occurred: " + xhr.responseText);
                console.error(xhr, status, error);
            }
        }
    });

});