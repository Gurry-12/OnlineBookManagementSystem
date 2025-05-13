$(document).ready(function () {
    const role = sessionStorage.getItem("userRole");
    const token = sessionStorage.getItem("jwt");

    if (!token) {
        window.location.href = "/Auth/Login";
    }

    if (role !== "Admin") {
        alert("Access Denied. Admins only.");
        window.location.href = "/Auth/Index";
        return;
    }

    $.ajax({
        url: "/Books/GetAllUsers",
        method: "GET",
        headers: { "Authorization": `Bearer ${token}` },
        success: function (response) {
            let html = "";
            $.each(response.users, function (i, user) {
                const userId = user.id;
                const initials = user.name.split(' ').map(n => n[0]).join('').substring(0, 2).toUpperCase();

                html += `
    <div class="col-12 col-md-4 mb-3" id="user-${userId}">
        <div class="card user-card p-3">
            <div class="avatar-circle">${initials}</div>
            <h5 id="userName${userId}">${user.name}</h5>
            <p id="userEmail${userId}" class="text-muted">${user.email}</p>
            <p><strong>Role:</strong> <span class="badge bg-info">${user.role}</span></p>
            <p><strong>Cart Items:</strong> ${user.cartItemCount}</p>
            <button class="btn btn-outline-primary btn-sm" type="button" data-bs-toggle="modal" data-bs-target="#userModal${userId}">
                <i class="bi bi-pencil-square me-1"></i>Manage User
            </button>
        </div>
    </div>

    <!-- Modal -->
    <div class="modal fade" id="userModal${userId}" tabindex="-1" aria-labelledby="userModalLabel${userId}" aria-hidden="true">
      <div class="modal-dialog">
        <div class="modal-content">
          <div class="modal-header">
            <h5 class="modal-title" id="userModalLabel${userId}"><i class="bi bi-tools me-2"></i>Edit User</h5>
            <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
          </div>
          <div class="modal-body">
            <form id="editUserForm${userId}" class="edit-user-form">
              <div class="mb-3">
                <label for="newName${userId}" class="form-label">Name</label>
                <input type="text" class="form-control" id="newName${userId}" value="${user.name}" required>
              </div>
              <div class="mb-3">
                <label for="newEmail${userId}" class="form-label">Email</label>
                <input type="email" class="form-control" id="newEmail${userId}" value="${user.email}" required>
              </div>
              <div class="d-flex justify-content-between mt-3">
                  <button type="submit" class="btn btn-success btn-sm"><i class="bi bi-check-circle me-1"></i>Update</button>
                  <button class="btn btn-outline-danger btn-sm" onclick="deleteUser(${userId})"><i class="bi bi-trash me-1"></i>Delete</button>
              </div>
            </form>
          </div>
        </div>
      </div>
    </div>
                    `;
            });

            $("#userList").html(html);

            // Update User
            $(".edit-user-form").on("submit", function (event) {
                event.preventDefault();
                const form = $(this);
                const userId = form.attr("id").replace("editUserForm", "");
                const newName = $(`#newName${userId}`).val();
                const newEmail = $(`#newEmail${userId}`).val();

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
                    success: function () {
                        alert("User details updated successfully!");
                        $(`#userName${userId}`).text(newName);
                        $(`#userEmail${userId}`).text(newEmail);
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
            success: function () {
                alert("User deleted successfully!");
                location.reload();
            },
            error: function () {
                alert("Failed to delete user.");
            }
        });
    }
}