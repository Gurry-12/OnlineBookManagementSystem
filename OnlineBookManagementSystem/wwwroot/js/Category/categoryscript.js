
// Modern category management script for JWT-based authentication
$(document).ready(function () {
    // Configure toastr
    toastr.options = {
        closeButton: true,
        progressBar: true,
        positionClass: "toast-top-right",
        timeOut: 4000,
        extendedTimeOut: 1000,
        showMethod: "fadeIn",
        hideMethod: "fadeOut"
    };

    // Check authentication
    if (!isLoggedIn()) {
        window.location.href = "/Auth/Login";
        return;
    }

    // Initialize category form handlers
    initializeCategoryHandlers();
});

// Check if user is logged in (using new auth system)
function isLoggedIn() {
    const accessToken = getAccessToken();
    const refreshToken = localStorage.getItem('refreshToken');
    return accessToken !== null || refreshToken !== null;
}

// Get access token from HttpOnly cookie
function getAccessToken() {
    const match = document.cookie.match(new RegExp('(^| )accessToken=([^;]+)'));
    return match ? match[2] : null;
}

// Initialize category form handlers
function initializeCategoryHandlers() {
    // Create category button
    $("#CreateCategory").on('click', function () {
        $("#CategorySave").show();
        $("#CategoryUpdate").hide();
        $("#categoryForm")[0].reset();
        $("#myModalLabel").text("Add New Category");
        clearAllErrors();
    });

    // Back to list button
    $("#BackToList").on('click', function () {
        $("#categoryForm")[0].reset();
        clearAllErrors();
    });

    // Save category button
    $("#CategorySave").on('click', function (e) {
        e.preventDefault();
        SaveCategory();
    });

    // Update category button
    $("#CategoryUpdate").on('click', function (e) {
        e.preventDefault();
        UpdateCategoryInDB();
    });

    // Form validation on input
    $("#NewCategory_Name").on('input blur', function () {
        validateCategoryName();
    });
}

// Get form data
function GetDataByForm() {
    return {
        Id: parseInt($("#NewCategory_Id").val()) || 0,
        Name: $("#NewCategory_Name").val().trim()
    };
}

// Save category with modern fetch API
async function SaveCategory() {
    const data = GetDataByForm();

    if (!ValidateCategoryData(data)) return;

    const $button = $("#CategorySave");
    const originalText = $button.html();
    $button.prop('disabled', true).html('<i class="bi bi-hourglass-split me-1"></i>Saving...');

    try {
        const response = await fetch('/Category/CreateCategory', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
            },
            body: JSON.stringify(data)
        });

        const result = await response.json();

        if (result.success) {
            $('#myModal').modal('hide');
            $("#categoryForm")[0].reset();
            toastr.success(result.message || 'Category created successfully!');

            // Redirect after short delay
            setTimeout(() => {
                window.location.href = '/Category/DisplayCategory';
            }, 1500);
        } else {
            toastr.error(result.message || 'Failed to create category');
        }
    } catch (error) {
        console.error('Save category error:', error);
        toastr.error('Network error. Please try again.');
    } finally {
        $button.prop('disabled', false).html(originalText);
    }
}

// Delete category with confirmation
async function DeleteCategory(id) {
    if (!confirm("Are you sure you want to delete this category?")) return;

    try {
        const response = await fetch(`/Category/DeleteCategory/${id}`, {
            method: 'DELETE',
            headers: {
                'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
            }
        });

        if (response.ok) {
            toastr.success("Category deleted successfully!");
            setTimeout(() => {
                window.location.href = '/Category/DisplayCategory';
            }, 1500);
        } else {
            const result = await response.json();
            toastr.error(result.message || 'Failed to delete category');
        }
    } catch (error) {
        console.error('Delete category error:', error);
        toastr.error('Network error. Please try again.');
    }
}

// Update category - load data into form
async function UpdateCategory(id) {
    $("#CategorySave").hide();
    $("#CategoryUpdate").show();
    $("#myModalLabel").text("Update Category");

    try {
        const response = await fetch(`/Category/GetCategoryById/${id}`, {
            method: 'GET'
        });

        const result = await response.json();

        if (result.success && result.getCategory) {
            $('#myModal').modal('show');
            $("#NewCategory_Id").val(result.getCategory.id);
            $("#NewCategory_Name").val(result.getCategory.name);
            clearAllErrors();
        } else {
            toastr.error('Failed to load category data');
        }
    } catch (error) {
        console.error('Load category error:', error);
        toastr.error('Network error. Please try again.');
    }
}

// Update category in database
async function UpdateCategoryInDB() {
    const data = GetDataByForm();

    if (!ValidateCategoryData(data)) return;

    const $button = $("#CategoryUpdate");
    const originalText = $button.html();
    $button.prop('disabled', true).html('<i class="bi bi-hourglass-split me-1"></i>Updating...');

    try {
        const response = await fetch('/Category/UpdateCategory', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
            },
            body: JSON.stringify(data)
        });

        const result = await response.json();

        if (result.success) {
            toastr.success(result.message || 'Category updated successfully!');
            $('#myModal').modal('hide');
            $("#categoryForm")[0].reset();

            setTimeout(() => {
                window.location.href = '/Category/DisplayCategory';
            }, 1500);
        } else {
            toastr.error(result.message || 'Failed to update category');
        }
    } catch (error) {
        console.error('Update category error:', error);
        toastr.error('Network error. Please try again.');
    } finally {
        $button.prop('disabled', false).html(originalText);
    }
}

// Client-side validation
function ValidateCategoryData(data) {
    let isValid = true;

    if (!data.Name || data.Name.length < 2) {
        displayError('NewCategory_Name', 'Category name must be at least 2 characters.');
        isValid = false;
    } else if (data.Name.length > 50) {
        displayError('NewCategory_Name', 'Category name cannot exceed 50 characters.');
        isValid = false;
    } else {
        clearError('NewCategory_Name');
    }

    return isValid;
}

// Validate category name on input
function validateCategoryName() {
    const name = $("#NewCategory_Name").val().trim();

    if (name.length === 0) {
        clearError('NewCategory_Name');
        return;
    }

    if (name.length < 2) {
        displayError('NewCategory_Name', 'Category name must be at least 2 characters.');
    } else if (name.length > 50) {
        displayError('NewCategory_Name', 'Category name cannot exceed 50 characters.');
    } else {
        clearError('NewCategory_Name');
    }
}

// Display error message
function displayError(elementId, message) {
    const $element = $("#" + elementId);
    let $errorSpan = $element.siblings('.text-danger');

    if ($errorSpan.length === 0) {
        $errorSpan = $('<span class="text-danger small"></span>');
        $element.after($errorSpan);
    }

    $errorSpan.text(message).show();
    $element.addClass('is-invalid');
}

// Clear error message
function clearError(elementId) {
    const $element = $("#" + elementId);
    const $errorSpan = $element.siblings('.text-danger');

    $errorSpan.text('').hide();
    $element.removeClass('is-invalid');
}

// Clear all error messages
function clearAllErrors() {
    $('.text-danger').text('').hide();
    $('.form-control').removeClass('is-invalid');
}
