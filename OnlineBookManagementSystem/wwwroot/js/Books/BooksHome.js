const token = sessionStorage.getItem("jwt");
const role = sessionStorage.getItem("userRole");
const userName = sessionStorage.getItem("userName");

$(document).ready(function () {
    if (userName) {
        $("#greeting").text("Hello, " + userName);
    }

    if (!token) {
        alert("Unauthorized access. Please log in.");
        return;
    }

    if (role === "Admin") {
        $("#adminSection").show();
        loadAdminBooks();
    } else if (role === "User") {
        loadBooks();
    } else {
        alert("Unauthorized: Invalid role");
    }
});


    function loadAdminBooks() {
        $.ajax({
            url: '/Books/GetAdminData', // Get book data via AJAX
            type: 'GET',
            headers: {
                'Authorization': 'Bearer ' + sessionStorage.getItem("jwt") // Include the JWT in the header
            },
            success: function (data) {
                // Loop through the data and display book cards
                let bookGrid = $('#bookGrid');
                bookGrid.empty(); // Clear any existing content in the book grid
                data.forEach(function (book) {
                    let bookCard = `
                        <div class="col-lg-3 col-md-4 col-sm-6 col-12">
                            <div class="book-card position-relative">
                                <button class="position-absolute translate-middle btn btn-link z-3 end-0" data-bs-toggle="dropdown" aria-expanded="false" style="top : 20px">
                                    <i class="bi bi-three-dots-vertical"></i>
                                </button>
                                <ul class="dropdown-menu border-0">
                                    <li><a class="dropdown-item text-warning" href="/Books/GetBookDetails/${book.id}">Edit</a></li>
                                    <li><a class="dropdown-item text-danger" onclick="DeleteBook(${book.id})">Delete</a></li>
                                </ul>

                                <img src="${book.imgUrl}" alt="${book.title}" class="book-image" />
                                <h6 class="mt-2">${book.title}</h6>
                                <small class="text-muted">${book.author}</small>
                                <div class="d-flex justify-content-between align-items-center mt-2">
                                    <p class="mb-0">Price: ${book.price}</p>
                                    <i class="bi bi-cart" style="font-size: 1.5rem;"></i>
                                </div>
                            </div>
                        </div>
                    `;
                    bookGrid.append(bookCard); // Add each book card to the grid
                });
            },
            error: function () {
                alert('Error loading book data.');
            }
        });
}

function loadBooks() {
    $.ajax({
        url: "/books/GetBooks",
        method: "GET",
        headers: {
            "Authorization": `Bearer ${token}`
        },
        success: function (response) {
            let arrivals = "";
            let recommended = "";

            if (response.data && response.data.length > 0) {
                $.each(response.data, function (i, book) {
                    const html = `<div class="book-card">
                        <img src="${book.imgUrl}" alt="${book.title}" style="width:100%; height:200px; object-fit:cover;" />
                        <h6>${book.title}</h6>
                        <small>${book.author}</small>
                    </div>`;

                    if (i < 4) arrivals += html;
                    recommended += html;
                });
            } else {
                arrivals = "<p>No new arrivals found.</p>";
                recommended = "<p>No recommendations available.</p>";
            }

            $("#newArrivals").html(arrivals);
            $("#recommendedBooks").html(recommended);
        },
        error: function () {
            alert("Failed to load books.");
        }
    });
}

function DataFilledByForm() {
    return {
        Id: parseInt($("#Id").val()) || 0,  // ✅ Ensure it's a number
        Title: $("#Title").val(),
        Author: $("#Author").val(),
        Price: parseFloat($("#Price").val()),
        Isbn: $("#Isbn").val(),
        ImgUrl: $("#ImgUrl").val(),
        Stock: $("#Stock").val().toString()
    };
}

function SubmitData(event) {
    if (event) event.preventDefault(); // ✅ Important line

    const bookData = DataFilledByForm();
    console.log("📘 Book Data:", JSON.stringify(bookData));

    $.ajax({
        url: "/Books/AddBook",
        method: "POST",
        headers: {
            "Authorization": `Bearer ${token}`
        },
        contentType: "application/json",
        data: JSON.stringify(bookData),
        success: function () {
            alert("✅ Book added successfully!");
            $("#addBookForm")[0].reset();
        },
        error: function (xhr, status, error) {
            console.error("❌ Error adding book:", error);
            alert("Something went wrong while adding the book.");
        }
    });
}

function UpdateData(event) {
    if (event) event.preventDefault(); // ✅ Important line

    const bookData = DataFilledByForm();
    console.log("📘 Final Book Data:", JSON.stringify(bookData));

    $.ajax({
        url: "/Books/UpdateBookDetails",
        method: "POST",
        headers: {
            "Authorization": `Bearer ${token}`
        },
        contentType: "application/json",
        data: JSON.stringify(bookData),
       
        success: function (response) {
            alert("✅ Book updated successfully!");
            $("#addBookForm")[0].reset();
            if (response.redirectUrl) {
                window.location.href = response.redirectUrl;
            }
        },
        error: function (xhr, status, error) {
            console.error("❌ Error updating book:", error);
            alert("Something went wrong while updating the book.");
        }
    });
}


function OpenBookModal(id) {
    $.ajax({
        url: `/Books/GetBook/${id}`,
        method: "GET",
        headers: {
            "Authorization": `Bearer ${token}`
        },
        success: function (response) {
            if (response.redirectUrl) {
                window.location.href = response.redirectUrl;
            } else {
                alert("Unexpected response format.");
            }
        },
        error: function (xhr, status, error) {
            console.error("❌ Error loading book:", error);
            alert("Something went wrong.");
        }
    });
}


function DeleteBook(Id) {

    $.ajax({
        url: `/Books/DeleteBook/${Id}`,
        method: "DELETE",
        headers: {
            "Authorization": `Bearer ${token}`
        },
        success: function (response) {
            alert("Book Deleted");
            window.location.href = response.redirectUrl;

        },
        error: function (xhr, status, error) {
            console.error("❌ Error loading book:", error);
            alert("Something went wrong.");
        }
    })
}