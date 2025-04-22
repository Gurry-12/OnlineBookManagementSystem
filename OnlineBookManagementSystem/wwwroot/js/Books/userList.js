$(document).ready(function () {
    const role = sessionStorage.getItem("userRole");
    const token = sessionStorage.getItem("jwt");

    if (role !== "Admin") {
        alert("Access Denied. Admins only.");
        window.location.href = "/Auth/Index"; // redirect if not admin
        return;
    }

    $.ajax({
        
        url: "/Books/GetAllUsers", // Adjust this to your actual API route
        method: "GET",
        headers: { "Authorization": `Bearer ${token}` },
        success: function (response) {
            console.log(response)
            let html = "";

            $.each(response.users, function (i, user) {
                html += `<div class="col-md-4">
                                <div class="card mb-3 p-3 shadow-sm">
                                    <h5>${user.name}</h5>
                                    <p class="text-muted">${user.email}</p>
                                    <p><strong>Role:</strong> ${user.role}</p>
                                    <p><strong>Cart Items:</strong> ${user.cartItemCount}</p>
                                </div>
                            </div>`;
            });

            $("#userList").html(html);
        },
        error: function () {
            alert("Failed to load users.");
        }
    });
});