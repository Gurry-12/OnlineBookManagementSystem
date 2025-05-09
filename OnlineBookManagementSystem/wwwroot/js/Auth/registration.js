﻿function displayError(elementId, message) {
    $('#' + elementId + 'Error').text(message).show();
}

function clearError(elementId) {
    $('#' + elementId + 'Error').text('').hide();
}

// Real-time validation
$('#Name').on('input', function () {
    $(this).val().length < 2 ? displayError('Name', 'name must be at least 2 characters long.') : clearError('Name');
});

$('#Email').on('input', function () {
    const pattern = /\S+@\S+\.\S+/;
    !pattern.test($(this).val()) ? displayError('Email', 'Please enter a valid email address.') : clearError('Email');
});

$('#Password').on('input', function () {
    $(this).val().length < 6 ? displayError('Password', 'Password must be at least 6 characters long.') : clearError('Password');
});



// Validate on form submit
$("#SubmitForm").click(function (event) {
    event.preventDefault();
    const data = {
        Name: $("#Name").val(),
        Email: $("#Email").val(),
        Password: $("#Password").val(),
        Role: $("#Role").val()
    };

    console.log(data);

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

    if (!isValid) {
        event.preventDefault();
        return;
    }

    $.ajax({
        url: "/Auth/SaveData",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify(data),
        success: function (response) {
            console.log(response);
            if (response.success) {
                alert(response.message);
                window.location.href = response.redirectUrl;
            } else {
                alert("Registration failed: " + response.message);
            }
        },
        error: function (xhr, status, error) {
            alert("An error occurred: " + xhr.responseText);
            console.error(xhr, status, error);
        }
    });
});

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
                // Store in sessionStorage
                sessionStorage.setItem("userName", response.userName);
                sessionStorage.setItem("userRole", response.role);
                sessionStorage.setItem("jwt", response.token);

                // Set token cookie (1 hour expiry, path for entire site)
                //const expiry = new Date();
                //expiry.setTime(expiry.getTime() + (60 * 60 * 1000)); // 1 hour
                document.cookie = `jwt=${response.token}; path=/;`;

                window.location.href = response.redirectUrl;
            } else {
                alert("Login failed: " + response.message);
            }
        },
        error: function () {
            alert("An error occurred. Please try again.");
        }
    });
});

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
            alert("User details updated successfully!");
        },
        error: function () {
            alert("Failed to update user details.");
        }


    });
}