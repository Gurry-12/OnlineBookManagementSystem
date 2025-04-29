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
        $("#UserTimeBookOption").addClass("d-none").removeClass("d-flex");
        loadAdminBooks();
    } else if (role === "User") {
        $("#UserTimeBookOption").removeClass("d-none").addClass("d-flex");
        loadBooks();
        restoreCartUI();
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
                                
                                    <p class="mb-0 text-primary fw-bold">Price: ₹${book.price}</p>

                                

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
                <div class="book-card shadow-sm p-2 bg-white rounded position-relative">
                    <img src="${book.imgUrl}" alt="${book.title}" class="img-fluid" style="height: 80px;" />
                    <h6 class="mt-2">${book.title}</h6>
                    <small class="text-muted">${book.author}</small>
                    <div class="d-flex justify-content-between align-items-center mt-2">
                        <i class="bi ${book.isFavorite === true ? 'bi-heart-fill' : 'bi-heart'}"
                           id="fav-icon-arrival-${book.id}"
                           onclick="AddToFavorites(${book.id})"
                           style="cursor: pointer; ${book.isFavorite === true ? 'color: red;' : ''}">
                        </i>
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
                                <ul class="dropdown-menu dropdown-menu-end border-0 shadow  z-3  ">
                                    <li class="text-center"><a class="dropdown-item text-warning" onclick="OpenBookModal(${book.id})">Details</a></li>
                                    <li class="text-center" ><i class="bi ${book.isFavorite === true ? 'bi-heart-fill' : 'bi-heart'}" dropdown-item" id="fav-icon-recommend-${book.id}" onclick="AddToFavorites(${book.id})" style="cursor: pointer; ${book.isFavorite === true ? 'color: red;' : ''}"> Favorite</i> </li>
                                </ul>
                                </div>

                                <img src="${book.imgUrl}" alt="${book.title}" class="book-image" />
                                <h6 class="mt-2">${book.title}</h6>
                                <small class="text-muted">${book.author}</small>
                                <div class="d-flex justify-content-between align-items-center mt-2">
                                    <p class="mb-0 text-primary fw-bold">₹${book.price}</p>
                                    <i class="bi bi-cart z-2" style="font-size: 1.5rem;" onclick="AddtoCart(${book.id});" id="cart-icon-${book.id}"></i>
                                    <div id="cart-counter-${book.id}" class="d-none z-2">
                                       <button class="btn btn-sm" onclick="changeCartQuantity(${book.id}, 'decrease', 'Book')">-</button>
                                        <span id="cart-quantity-${book.id}">1</span>
                                        <button class="btn btn-sm" onclick="changeCartQuantity(${book.id}, 'increase', 'Book')">+</button>
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


//Add to favourite - frontend Only
function AddToFavorites(id) {
   
    const $arrivalIcon = $(`#fav-icon-arrival-${id}`);
    const $recommendIcon = $(`#fav-icon-recommend-${id}`);

    const toggleFavoriteIcon = ($icon) => {
        if ($icon.length === 0) return;
        if ($icon.hasClass("bi-heart")) {
            $icon.removeClass("bi-heart")
                .addClass("bi-heart-fill")
                .css("color", "red");
        } else {
            $icon.removeClass("bi-heart-fill")
                .addClass("bi-heart")
                .css("color", "");
        }
    };

    // AJAX call to backend
    $.ajax({
        type: "POST",
        url: `/Books/AddandRemoveFavorite/${id}`,
        
        success: function (response) {
            
            if (response.success) {
                toggleFavoriteIcon($arrivalIcon);
                toggleFavoriteIcon($recommendIcon);
            }
        },
        error: function (xhr, status, error) {
            console.error("Error updating favorite:", error);
        }
    });
}



// data for ajax - book adding and updating
function DataFilledByForm() {
    return {
        Id: parseInt($("#Book_Id").val()) || 0,  // ✅ Ensure it's a number
        Title: $("#Book_Title").val(),
        Author: $("#Book_Author").val(),
        Price: parseFloat($("#Book_Price").val()),
        Isbn: $("#Book_Isbn").val(),
        ImgUrl: $("#Book_ImgUrl").val(),
        Stock: $("#Book_Stock").val().toString(),
        CategoryId: $("#Book_CategoryId").val(),
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
        success: function (response) {
            if (response.success) {
                alert("✅ Book added successfully!");
                $("#addBookForm")[0].reset();
            }
            else {
                alert(response.message);
            }
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
            if (response.success) {
                alert("✅ Book updated successfully!");
                $("#addBookForm")[0].reset();
                if (response.redirectUrl) {
                    window.location.href = response.redirectUrl;
                }
            }
            else {
                alert(response.message);
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


// Cart Section - UserIndex to cart 
// Add to Cart Function
// Add to Cart
function AddtoCart(bookId) {

    $.ajax({
        type: "POST",
        url: "/Cart/AddOrUpdateCart",
        contentType: "application/json",
        data: JSON.stringify({ BookId: bookId }),
        success: function () {
            $("#cart-icon-" + bookId).hide();
            $("#cart-counter-" + bookId).removeClass("d-none");
            $("#cart-quantity-" + bookId).text("1");
            restoreCartUI();
        },
        error: function () {
            console.error("Error adding book to cart.");
        }
    });
}

// Change Quantity (Increase or Decrease)
function changeCartQuantity(bookId, action, type) {
    
    let quantity = parseInt($("#cart-quantity-" + bookId).text());

    if (action === "increase") {
        quantity++;
    } else if (action === "decrease") {
        quantity--;
    }
    
    
    $.ajax({
        type: "POST",
        url: "/Cart/UpdateQuantity",
        contentType: "application/json",
        data: JSON.stringify({ BookId: bookId, Quantity: quantity }),
        success: function () {
            if (quantity <= 0) {
                $("#cart-icon-" + bookId).show();
                $("#cart-counter-" + bookId).addClass("d-none");
                if (type == 'Cart')
                    window.location.href = "/Cart/CartIndexUser";
                else if (type == "Book")
                    window.location.href = "/Books/UserIndex";
                else if (type == 'profile')
                    window.location.href = `/Books/DisplayBookdetails/${BookId}`;
            } else {
                $("#cart-quantity-" + bookId).text(quantity);
            }

            updateCartTotals()
            restoreCartUI();

        },
        error: function () {
            console.error("Error updating quantity.");
        }
    });
}


// Restore Cart UI on Page Load
function restoreCartUI() {

    $.ajax({
        type: "GET",
        url: "/Cart/GetAllCartItems",
        success: function (cartItems) {
            let totalQuantity = 0;

            cartItems.forEach(item => {
                const bookId = item.bookId;
                const quantity = item.quantity;
                totalQuantity += quantity;

                // Update book card UI
                $("#cart-icon-" + bookId).hide();
                $("#cart-counter-" + bookId).removeClass("d-none");
                $("#cart-quantity-" + bookId).text(quantity);
            });

            // Update cart badge
            const badge = document.getElementById("cartItemCount");
            if (totalQuantity > 0) {
                badge.textContent = totalQuantity;
                badge.style.display = "inline-block";
            } else {
                badge.style.display = "none";
            }
        },
        error: function () {
            console.error("Failed to restore cart UI.");
        }
    });
}


// Remove from DB and cart page - 
function RemoveCartItems(bookid, userid) {

    var Data = {
        UserId: userid,
        BookId: bookid
    };

    console.log(Data)
    $.ajax({
        url: `/Cart/RemoveCartItems`,
        method: "DELETE",
        contentType: "application/json",
        data: JSON.stringify(Data),
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


// Function to update subtotal, tax, and grand total
function updateCartTotals() {

    var subtotal = 0;
    $('.cart-item').each(function () {
        var bookPrice = parseFloat($(this).find('.book-price').text().replace('₹', '').trim());
        // Get the quantity as a number
        var quantity = parseInt($(this).find('.cart-quantity').text().trim(), 10); // Convert to integer

        // Ensure that both values are valid numbers before using them
        if (!isNaN(bookPrice) && !isNaN(quantity)) {
            // Add to subtotal
            subtotal += bookPrice * quantity;
        }
        console.log(bookPrice)
        console.log(quantity)
       
    });

    var tax = subtotal * 0.10;
    var grandTotal = subtotal + tax;

    // Update the totals on the page
    $('#subtotal').text('₹' + subtotal.toFixed(2));  // Format subtotal to 2 decimal places
    $('#tax').text('₹' + tax.toFixed(2));            // Format tax to 2 decimal places
    $('#grand-total').text('₹' + grandTotal.toFixed(2));
}
