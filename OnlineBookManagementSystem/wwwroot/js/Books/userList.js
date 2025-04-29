$(document).ready(function () {
    const role = sessionStorage.getItem("userRole");
    const token = sessionStorage.getItem("jwt");

    if (role !== "Admin") {
        alert("Access Denied. Admins only.");
        window.location.href = "/Auth/Index"; // redirect if not admin
        return;
    }

    $.ajax({
        url: "/Books/GetAllUsers",  // Adjust this to your actual API route
        method: "GET",
        headers: { "Authorization": `Bearer ${token}` },
        success: function (response) {
            console.log(response);
            let html = "";

            $.each(response.users, function (i, user) {
                // Generate a unique ID for each user's card and collapse section
                const userId = user.id; // Assuming 'id' is unique and can be used for updates
                console.log(userId)
                html += `
                <div class="col-12 col-md-4 mb-3" id="user-${userId}">
                    <div class="card p-3 shadow-sm">
                        <h5 id="userName${userId}">${user.name}</h5>
                        <p id="userEmail${userId}" class="text-muted">${user.email}</p>
                        <p><strong>Role:</strong> ${user.role}</p>
                        <p><strong>Cart Items:</strong> ${user.cartItemCount}</p>
                        
                        <!-- Button to toggle collapse -->
                        <button class="btn btn-primary" type="button" data-bs-toggle="collapse" data-bs-target="#collapseUser${userId}" aria-expanded="false" aria-controls="collapseUser${userId}">
                            Manage User
                        </button>

                        <!-- Collapsible content -->
                        <div class="collapse" id="collapseUser${userId}">
                            <div class="card card-body mt-2">
                                <form id="editUserForm${userId}" class="edit-user-form">
                                    <div class="mb-3">
                                        <label for="newName${userId}" class="form-label">Name</label>
                                        <input type="text" class="form-control" id="newName${userId}" value="${user.name}" required>
                                    </div>
                                    <div class="mb-3">
                                        <label for="newEmail${userId}" class="form-label">Email</label>
                                        <input type="email" class="form-control" id="newEmail${userId}" value="${user.email}" required>
                                    </div>
                                    <button type="submit" class="btn btn-success btn-sm">Update Details</button>
                                </form>
                                <button class="btn btn-danger btn-sm mt-2" onclick="deleteUser(${userId})">Delete User</button>
                            </div>
                        </div>
                    </div>
                </div>`;
            });

            $("#userList").html(html);

            // Bind form submission event after rendering
            $(".edit-user-form").on("submit", function (event) {
                event.preventDefault();

                const form = $(this);
                const userId = form.attr("id").replace("editUserForm", "");
                const newName = $(`#newName${userId}`).val();
                const newEmail = $(`#newEmail${userId}`).val();

                // Prepare data to send to the server with the userId
                const data = JSON.stringify({
                    id: userId,
                    newName: newName,
                    newEmail: newEmail
                });

                $.ajax({
                    url: "/Auth/UpdateUserDetails",
                    method: "POST",
                    
                    contentType: "application/json",
                    data: data,
                    success: function (response) {
                        alert("User details updated successfully!");

                        // Dynamically update the DOM with new user details
                        $(`#userName${userId}`).text(newName);
                        $(`#userEmail${userId}`).text(newEmail);

                        // Optionally, reload the page to reflect the changes from the backend
                        // location.reload();
                    },
                    error: function () {
                        alert("Failed to update user details.");
                    }
                });
            });
        },
        error: function () {
            alert("Failed to load users.");
        }
    });
});

function deleteUser(userId) {
    if (confirm("Are you sure you want to delete this user?")) {
        const token = sessionStorage.getItem("jwt");

        $.ajax({
            url: `/Auth/DeleteUser/${userId}`,
            method: "DELETE",
            headers: { "Authorization": `Bearer ${token}` },
            contentType: "application/json",
            data: JSON.stringify({ userId: userId }),
            success: function (response) {
                alert("User deleted successfully!");
                location.reload(); // Reload to reflect changes
            },
            error: function () {
                alert("Failed to delete user.");
            }
        });
    }
}
