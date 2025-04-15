// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

const name = sessionStorage.getItem("userName");
const Role = sessionStorage.getItem("userRole");

$("#username").append(name);

const homeLink = document.getElementById("homeLink");

if (Role === "Admin") {
    homeLink.href = "/Books/AdminIndex";
} else {
    homeLink.href = "/Books/UserIndex";
}



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

