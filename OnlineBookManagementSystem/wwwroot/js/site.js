// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

const name = sessionStorage.getItem("userName");
$("#username").append(name);

const Role = sessionStorage.getItem("userRole");
const homeLink = document.getElementById("homeLink");

if (Role === "Admin") {
    homeLink.href = "/Books/AdminIndex";
} else {
    homeLink.href = "/Books/UserIndex";
}