
toastr.options = {
    "closeButton": true,
    "debug": false,
    "newestOnTop": false,
    "progressBar": true,
    "positionClass": "toast-top-right",  // position of the toast
    "preventDuplicates": true,
    "showDuration": "300",  // Duration of the showing animation
    "hideDuration": "1000", // Duration of the hide animation
    "timeOut": "3000",      // Duration before it disappears
    "extendedTimeOut": "1000", // Duration for hover-to-pause
    "showEasing": "swing",
    "hideEasing": "linear",
    "showMethod": "fadeIn", // Method for showing the toast
    "hideMethod": "fadeOut" // Method for hiding the toast
};
    function displayError(elementId, message) {
        $('#' + elementId + 'Error').text(message).show();
    }

    function clearError(elementId) {
        $('#' + elementId + 'Error').text('').hide();
    }

    // Real-time validation
    $('#Name').on('input', function () {
        $(this).val().length < 2 ? displayError('Name', 'Name must be at least 2 characters long.') : clearError('Name');
    });

    $('#Email').on('input', function () {
        const pattern = /\S+@\S+\.\S+/;
        !pattern.test($(this).val()) ? displayError('Email', 'Please enter a valid email address.') : clearError('Email');
    });

    $('#Password').on('input', function () {
        $(this).val().length < 6 ? displayError('Password', 'Password must be at least 6 characters long.') : clearError('Password');
    });

    // Submit form handler
    $("#SubmitForm").click(function (event) {
        event.preventDefault();

        const data = {
            Name: $("#Name").val(),
            Email: $("#Email").val(),
            Password: $("#Password").val(),
            Role: $("#Role").val()
        };

        let isValid = true;

        if (data.Name.length < 2) {
            displayError('Name', 'Name must be at least 2 characters long.');
            isValid = false;
        }

        if (!/\S+@\S+\.\S+/.test(data.Email)) {
            displayError('Email', 'Please enter a valid email address.');
            isValid = false;
        }

        if (data.Password.length < 6) {
            displayError('Password', 'Password must be at least 6 characters long.');
            isValid = false;
        }

        if (!isValid) return;

        $.ajax({
            url: "/Auth/SaveData",
            type: "POST",
            contentType: "application/json",
            data: JSON.stringify(data),
            success: function (response) {
                if (response.success) {
                    toastr.success("Account created successfully!");

                    setTimeout(function () {
                        document.body.classList.add("fade-out");
                        setTimeout(() => {
                            window.location.href = response.redirectUrl;
                        }, 500); // wait for fade-out
                    }, 1500); // wait for toast
                } else {
                    toastr.error("Something went wrong.");
                }
            },
            error: function (xhr, status, error) {
                toastr.error("Something went wrong.");
                console.error(xhr, status, error);
            }
        });
    });

    // Login handler
    $("#LoginData").click(function (event) {
        event.preventDefault();

        const data = {
            Email: $("#Email").val().trim(),
            Password: $("#Password").val().trim()
        };

        $.ajax({
            url: "/Auth/LoginData",
            type: "POST",
            contentType: "application/json",
            data: JSON.stringify(data),
            success: function (response) {
                if (response.success) {
                    toastr.info("Welcome back!");
                    sessionStorage.setItem("userName", response.userName);
                    sessionStorage.setItem("userRole", response.role);
                    sessionStorage.setItem("jwt", response.token);
                    document.cookie = `jwt=${response.token}; path=/;`;

                    setTimeout(function () {
                        document.body.classList.add("fade-out");
                        setTimeout(() => {
                            window.location.href = response.redirectUrl;
                        }, 500); // wait for fade-out
                    }, 1500); // wait for toast
                } else {
                    toastr.error("Login failed. Please check your credentials.");
                }
            },
            error: function () {
                toastr.error("An error occurred during login.");
            }
        });
    });

    // Update user profile
    function updateProfile() {
        const data = {
            id: $("#Id").val(),
            newName: $("#Name").val(),
            newEmail: $("#Email").val()
        }

        $.ajax({
            url: "/Auth/UpdateUserDetails",
            method: "POST",
            contentType: "application/json",
            data: JSON.stringify(data),
            success: function (response) {
                toastr.success("User details updated successfully!");
            },
            error: function () {
                toastr.error("Failed to update user details.");
            }
        });
    }

