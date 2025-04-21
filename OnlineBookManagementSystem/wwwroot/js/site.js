// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

const name = sessionStorage.getItem("userName");
const Role = sessionStorage.getItem("userRole");

$("#username").append(name);

$("#homeLink").click(function (event) {
    debugger;
    event.preventDefault(); // Prevent the default anchor tag action

    const token = sessionStorage.getItem("jwt");

    if (!token) {
        alert("Unauthorized! Please login.");
        return;
    }

    const role = sessionStorage.getItem("userRole");
    let targetUrl = role === "Admin" ? "/Books/AdminIndex" : "/Books/UserIndex";

    $.ajax({
        url: targetUrl,
        type: "GET",
        headers: {
            "Authorization": `Bearer ${token}`
        },
        success: function (response) {
            window.location.href = targetUrl; // Redirect after success
        },
        error: function (xhr, status, error) {
            alert("Error: You don't have access or session expired.");
        }
    });
});




function updateDateTime() {
    var time = new Date();
    var formattedTime = time.toLocaleTimeString(); // e.g., "3:24:15 PM"
    const options = { day: '2-digit', month: 'short', year: 'numeric' };
    const date = time.toLocaleDateString('en-GB', options).replace(/ /g, '-');
    $(".currentdate").html(date);
    $(".currenttime").html(formattedTime);

}
updateDateTime();
setInterval(updateDateTime, 1000);

function logout() {
    debugger;
    sessionStorage.clear();
   // document.cookie = "jwt=; path=/; expires=Thu, 01 Jan 1970 00:00:00 UTC;";
    window.location.href = "/Auth/Login";
}


