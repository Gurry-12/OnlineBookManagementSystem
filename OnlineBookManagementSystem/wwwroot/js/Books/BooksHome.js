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
                            <div class="book-card shadow-sm p-2 bg-white rounded position-relative h-100">
                            <div class="position-absolute end-0 top-0 me-1 mt-1 z-3">
                                <button class="btn btn-link p-0" data-bs-toggle="dropdown" aria-expanded="false">
                                    <i class="bi bi-three-dots-vertical"></i>
                                </button>
                                <ul class="dropdown-menu dropdown-menu-end border-0 shadow  z-3">
                                    <li><a class="dropdown-item text-warning" href="/Books/GetBookDetails/${book.id}">Edit</a></li>
                                    <li><a class="dropdown-item text-danger" onclick="DeleteBook(${book.id})">Delete</a></li>
                                </ul>
                                </div>
                                <img src="${book.imgUrl}" alt="${book.title}" class="book-image" />
                                <h6 class="mt-2">${book.title}</h6>
                                <small class="text-muted">${book.author}</small>
                                <div class="d-flex justify-content-between align-items-center mt-2">
                                    <p class="mb-0 text-primary fw-bold">Price: ${book.price}</p>
                                    <i class="bi bi-cart" style="font-size: 1.5rem;"></i>
                                </div>

                                <!-- Card overlay click --> 
                                <a onclick="OpenBookModal(${book.id})" class="stretched-link"></a> 
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


//for the User Index Books
function loadBooks() {
    const token = sessionStorage.getItem("jwt");

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
                    // HTML for New Arrivals (less information)
                    let arrivalCard = `
                        <div class="col-lg-3 col-md-4 col-sm-6 col-12">
                            <div class="book-card shadow-sm p-2 bg-white rounded position-relative h-100">
                                <img src="${book.imgUrl}" alt="${book.title}" class="book-image" />
                                <h6 class="mt-2">${book.title}</h6>
                                <small class="text-muted">${book.author}</small>
                                <div class="d-flex justify-content-between align-items-center mt-2">
                                   <i class="bi bi-heart" id="fav-icon-arrival-${book.id}"" onclick="AddToFavorites(${book.id});"></i>
                                </div>
                            </div>
                        </div>
                    `;

                    // HTML for Recommended Books (full details)
                    let recommendedCard = `
                        <div class="col-lg-3 col-md-4 col-sm-6 col-12">
                            <div class="book-card shadow-sm p-2 bg-white rounded position-relative h-100">
                            <div class="position-absolute end-0 top-0 me-1 mt-1 z-3">
                                <button class="btn btn-link p-0" data-bs-toggle="dropdown" aria-expanded="false">
                                    <i class="bi bi-three-dots-vertical"></i>
                                </button>
                                <ul class="dropdown-menu dropdown-menu-end border-0 shadow  z-3">
                                    <li><a class="dropdown-item text-warning" onclick="OpenBookModal(${book.id})">Details</a></li>
                                    <li><i class="bi bi-heart dropdown-item" id="fav-icon-recommend-${book.id}" onclick="AddToFavorites(${book.id})"> Favorite</i> </li>
                                </ul>
                                </div>

                                <img src="${book.imgUrl}" alt="${book.title}" class="book-image" />
                                <h6 class="mt-2">${book.title}</h6>
                                <small class="text-muted">${book.author}</small>
                                <div class="d-flex justify-content-between align-items-center mt-2">
                                    <p class="mb-0 text-primary fw-bold">₹${book.price}</p>
                                    <i class="bi bi-cart z-2" style="font-size: 1.5rem;" onclick="AddtoCart(${book.id });" id="cart-icon-${book.id}"></i>
                                    <div id="cart-counter-${book.id}" class="d-none z-2">
                                       <button class="btn btn-sm" onclick="changeCartQuantity(${book.id}, 'decrease')">-</button>
                                        <span id="cart-quantity-${book.id}">1</span>
                                        <button class="btn btn-sm" onclick="changeCartQuantity(${book.id}, 'increase')">+</button>
                                    </div>
                                </div>

                                <a onclick="OpenBookModal(${book.id})" class="stretched-link"></a>
                            </div>
                        </div>
                    `;

                    // Add the book to New Arrivals section (only first 4)
                    if (i < 4) arrivals += arrivalCard;

                    // Add the book to Recommended Books section (full details)
                    recommended += recommendedCard;
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

function AddToFavorites(id) {
    const arrivalIcon = document.getElementById(`fav-icon-arrival-${id}`);
    const recommendIcon = document.getElementById(`fav-icon-recommend-${id}`);

    const toggleFavoriteIcon = (icon) => {
        if (!icon) return;
        if (icon.classList.contains("bi-heart")) {
            icon.classList.remove("bi-heart");
            icon.classList.add("bi-heart-fill");
            icon.style.color = "red";
        } else {
            icon.classList.remove("bi-heart-fill");
            icon.classList.add("bi-heart");
            icon.style.color = "";
        }
    };

    toggleFavoriteIcon(arrivalIcon);
    toggleFavoriteIcon(recommendIcon);

    //alert("Toggled favorite status for book ID: " + id);
}



let cartData = {}; // Store cart data (item ID and quantity)

// Add to cart function
function AddtoCart(id) {

    if (!cartData[id]) {
        // Initialize the cart item with quantity 1
        cartData[id] = 1;

        // Hide the cart icon and show the counter
        $("#cart-icon-" + id).hide();
        $("#cart-counter-" + id).removeClass("d-none");

        // Set initial quantity to 1
        $("#cart-quantity-" + id).text(cartData[id]);
    }

    const payload = {
         BookId : id,
        Quantity : cartData[id],
    }

    console.log(payload);

    console.log(cartData);
}

// Change quantity function (increase/decrease)
function changeCartQuantity(id, action) {
    if (action === "increase") {
        cartData[id]++;
    } else if (action === "decrease" && cartData[id] > 0) {
        cartData[id]--;
    }
    console.log(cartData);

    // Update the displayed quantity
    $("#cart-quantity-" + id).text(cartData[id]);

    // If quantity reaches 0, show the cart icon again
    if (cartData[id] === 0) {
        $("#cart-icon-" + id).show();
        $("#cart-counter-" + id).addClass("d-none");
        delete cartData[id]; // Remove the item from the cart
    }
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