
toastr.options = {
    "closeButton": true,
    "debug": false,
    "newestOnTop": false,
    "progressBar": true,
    "positionClass": "toast-top-right",  // position of the toast
    "preventDuplicates": true,
    "showDuration": "300",  // Duration of the showing animation
    "hideDuration": "1000", // Duration of the hide animation
    "timeOut": "5000",      // Duration before it disappears
    "extendedTimeOut": "1000", // Duration for hover-to-pause
    "showEasing": "swing",
    "hideEasing": "linear",
    "showMethod": "fadeIn", // Method for showing the toast
    "hideMethod": "fadeOut" // Method for hiding the toast
};

$(document).ready(function () {
    const token = sessionStorage.getItem("jwt");

    if (!token) {
        // Redirect to login page if token is missing (i.e., session expired or user not logged in)
        window.location.href = "/Auth/Login";
    } else {

        $("#CreateCategory").click(function () {
            $("#CategorySave").show();
            $("#CategoryUpdate").hide();
            $("#categoryForm")[0].reset();
            $("#myModalLabel").text("Add New Category");

            EnableValidation();
        });

        $("#BackToList").click(function () {
            $("#categoryForm")[0].reset();
            $(".text-danger").text(""); // Clear validation error messages
        });
    }
});

function EnableValidation() {
    if (!$.validator || !$.validator.unobtrusive) {
        console.warn("Validation libraries not loaded.");
        return;
    }

    let form = $("#categoryForm");
    form.removeData("validator");
    form.removeData("unobtrusiveValidation");
    $.validator.unobtrusive.parse(form);
}

function GetDataByForm() {
    return {
        Id: $("#NewCategory_Id").val(),
        Name: $("#NewCategory_Name").val()
    };
}

//Save ajax
function SaveCategory() {
    const data = GetDataByForm();

    if (!ValidateCategoryData(data)) return;

    $.ajax({
        url: "/Category/CreateCategory",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify(data),
        success: function (response) {
            if (response.success) {
                $('#myModal').modal('hide');
                $("#categoryForm")[0].reset();
                window.location.href = 'DisplayCategory';
                toastr.success(response.message);
            } else {
                toastr.warning(response.message);
            }
        },
        error: function (xhr) {
            alert("An error occurred while saving data.");
            console.error(xhr);
        }
    });
}

function DeleteCategory(Id) {
    if (!confirm("Are you sure you want to delete this category?")) return;

    $.ajax({
        url: `/Category/DeleteCategory/${Id}`,
        type: "DELETE",
        success: function (response) {
            window.location.href = 'DisplayCategory';
            toastr.error("Successfully Deleted");
        },
        error: function (xhr) {
            toastr.error("Error deleting category.");
            
        }
    });
}

function UpdateCategory(Id) {
    $("#CategorySave").hide();
    $("#CategoryUpdate").show();
    $("#myModalLabel").text("Update Category");

    $.ajax({
        url: `/Category/GetCategoryById/${Id}`,
        type: "GET",
        success: function (response) {
            $('#myModal').modal('show');
            $("#NewCategory_Id").val(response.getCategory.id);
            $("#NewCategory_Name").val(response.getCategory.name);
            toastr.success("form Open");
            EnableValidation();
        },
        error: function (xhr) {
            toastr.error("Error loading category data.");
            
        }
    });
}

function UpdateCategoryInDB() {
    const data = GetDataByForm();

    if (!ValidateCategoryData(data)) return;

    $.ajax({
        url: "/Category/UpdateCategory",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify(data),
        success: function (response) {
            if (response.success) {
                toastr.warning("Update Successfully");
                $('#myModal').modal('hide');
                $("#categoryForm")[0].reset();
                window.location.href = 'DisplayCategory';
            } else {
                toastr.error(response.message);
            }
        },
        error: function (xhr) {
            toastr.error("An error occurred while updating data.");
            
        }
    });
}

// ----------------------------
// ✅ Client-side validation
// ----------------------------
function ValidateCategoryData(data) {
    let isValid = true;

    if (!data.Name || data.Name.trim().length < 2) {
        displayError('NewCategory_Name', 'Category name must be at least 2 characters.');
        isValid = false;
    } else {
        clearError('NewCategory_Name');
    }

    return isValid;
}

function displayError(elementId, message) {
    const spanId = $("#" + elementId).next("span");
    if (spanId) {
        spanId.text(message).show();
    }
}

function clearError(elementId) {
    const spanId = $("#" + elementId).next("span");
    if (spanId) {
        spanId.text("").hide();
    }
}
