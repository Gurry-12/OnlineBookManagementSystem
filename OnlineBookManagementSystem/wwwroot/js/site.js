const userName = sessionStorage.getItem("userName");
const Role = sessionStorage.getItem("userRole");

$("#username").append(userName);




$("#homeLink").click(function (event) {
    var token = sessionStorage.getItem("jwt");

    event.preventDefault(); // Prevent the default anchor tag action

    let targetUrl = Role === "Admin" ? "/Books/AdminIndex" : "/Books/UserIndex";

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
    sessionStorage.clear();
   // document.cookie = "jwt=; path=/; expires=Thu, 01 Jan 1970 00:00:00 UTC;";
    window.location.href = "/Auth/Login";
}

$(document).ready(function () {
    // Toggle the collapsed class on sidebar
    $("#toggleSidebar").click(function () {
        $("#sidebar").toggleClass("collapsed");
        $(".content").toggleClass("sidebar-collapsed");
    });

    $("#SupportloginDetail").html(Role);

    
});


