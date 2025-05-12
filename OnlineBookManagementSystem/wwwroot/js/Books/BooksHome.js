const token = sessionStorage.getItem("jwt");
const role = sessionStorage.getItem("userRole");
const userName = sessionStorage.getItem("userName");



$(document).ready(function () {

    if (!token) {
        // Redirect to login page if token is missing (i.e., session expired or user not logged in)
        window.location.href = "/Auth/Login";
    }
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
        url: '/Books/GetAdminData',
        type: 'GET',
        headers: {
            'Authorization': 'Bearer ' + sessionStorage.getItem("jwt")
        },
        success: function (data) {
            const bookGrid = $('#bookGrid');
            bookGrid.empty();

            const categoryMap = new Map();

            // Group by categoryId and keep only one book per category
            data.forEach(function (book) {
                if (!categoryMap.has(book.categoryId)) {
                    categoryMap.set(book.categoryId, book); // Add only the first book of each category
                }
            });

            // Display one book per category
            categoryMap.forEach(function (book) {
                const bookCard = `
                    <div class="col-lg-3 col-md-4 col-sm-6 col-12">
                        <div class="book-card shadow-sm p-2 bg-white rounded position-relative h-100">
                            <div class="position-absolute end-0 top-0 me-1 mt-1 z-3">
                                <button class="btn btn-link p-0" data-bs-toggle="dropdown" aria-expanded="false">
                                    <i class="bi bi-three-dots-vertical"></i>
                                </button>
                                <ul class="dropdown-menu dropdown-menu-end border-0 shadow z-3">
                                    <li><a class="dropdown-item text-warning" href="/Books/GetBookDetails/${book.id}">Edit</a></li>
                                    <li><a class="dropdown-item text-danger" onclick="DeleteBook(${book.id})">Delete</a></li>
                                </ul>
                            </div>
                            <img src="${book.imgUrl}" alt="${book.title}" class="book-image" />
                            <h6 class="mt-2">${book.title}</h6>
                            <small class="text-muted">${book.author}</small>
                            <p class="mb-0 text-primary fw-bold">Price: ₹${book.price}</p>
                            <a onclick="OpenBookModal(${book.id})" class="stretched-link"></a>
                        </div>
                    </div>
                `;
                bookGrid.append(bookCard);
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
                    let arrivalCard = `
                        <div class="col-lg-3 col-md-4 col-sm-6 col-12">
                            <div class="book-card shadow-sm p-3 bg-light rounded text-center position-relative h-100">
                                <img src="${book.imgUrl}" alt="${book.title}" class="img-fluid" style="height: 90px;" />
                                <h6 class="mt-2 fw-semibold">${book.title}</h6>
                                <small class="text-muted d-block">${book.author}</small>
                                <div class="mt-2">
                                    <i class="bi ${book.isFavorite ? 'bi-heart-fill text-danger' : 'bi-heart'}"
                                       id="fav-icon-arrival-${book.id}"
                                       onclick="AddToFavorites(${book.id})"
                                       style="cursor: pointer;">
                                    </i>
                                </div>
                            </div>
                        </div>
                    `;

                    let recommendedCard = `
                        <div class="col-lg-3 col-md-4 col-sm-6 col-12">
                            <div class="book-card shadow p-3 bg-white rounded h-100 position-relative">
                                <div class="position-absolute end-0 top-0 me-1 mt-1 z-3">
                                    <button class="btn btn-link p-0" data-bs-toggle="dropdown" aria-expanded="false">
                                        <i class="bi bi-three-dots-vertical"></i>
                                    </button>
                                    <ul class="dropdown-menu dropdown-menu-end border-0 shadow z-3">
                                        <li><a class="dropdown-item text-warning" onclick="OpenBookModal(${book.id})">📖 Details</a></li>
                                        <li><a class="dropdown-item text-danger">
                                            <i class="bi ${book.isFavorite ? 'bi-heart-fill text-danger' : 'bi-heart'}"
                                               id="fav-icon-recommend-${book.id}"
                                               onclick="AddToFavorites(${book.id})"
                                               style="cursor: pointer;">
                                            </i> Favorite
                                        </a></li>
                                    </ul>
                                </div>

                                <img src="${book.imgUrl}" alt="${book.title}" class="book-image" />
                                <h6 class="mt-2 fw-semibold">${book.title}</h6>
                                <small class="text-muted d-block">${book.author}</small>

                                <div class="d-flex justify-content-between align-items-center mt-2">
                                    <span class="text-primary fw-bold">₹${book.price}</span>
                                    <i class="bi bi-cart z-3" style="font-size: 1.5rem; cursor: pointer;" onclick="AddtoCart(${book.id});" id="cart-icon-${book.id}"></i>
                                    <div id="cart-counter-${book.id}" class="d-none z-3">
                                        <button class="btn btn-sm" onclick="changeCartQuantity(${book.id}, 'decrease', 'Book')">−</button>
                                        <span id="cart-quantity-${book.id}">1</span>
                                        <button class="btn btn-sm" onclick="changeCartQuantity(${book.id}, 'increase', 'Book')">+</button>
                                    </div>
                                </div>

                                <a onclick="OpenBookModal(${book.id})" class="stretched-link"></a>
                            </div>
                        </div>
                    `;

                    if (i < 4) arrivals += arrivalCard;
                    recommended += recommendedCard;
                });
            } else {
                arrivals = "<div class='text-muted'>📭 No new arrivals found.</div>";
                recommended = "<div class='text-muted'>🕊️ No recommendations available.</div>";
            }

            $("#newArrivals").html(arrivals);
            $("#recommendedBooks").html(recommended);
        },
        error: function () {
            alert("⚠️ Failed to load books. Please try again.");
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
function getImageInputValue() {
    const fileInput = $("input[name='ImgUrl']")[0];
    const imageUrlInput = $("#imageUrl").val();
    const hasFile = fileInput && fileInput.files && fileInput.files.length > 0;

    if (hasFile) {
        return { type: "file", file: fileInput.files[0] };
    } else if (imageUrlInput) {
        return { type: "url", url: imageUrlInput };
    } else {
        const existing = $("#ExistingImgUrl").val();
        return { type: "existing", url: existing };
    }
}

function DataFilledByForm() {
    return {
        Id: parseInt($("#Book_Id").val()) || 0,
        Title: $("#Book_Title").val(),
        Author: $("#Book_Author").val(),
        Price: parseFloat($("#Book_Price").val()),
        Isbn: $("#Book_Isbn").val(),
        Stock: $("#Book_Stock").val().toString(),
        CategoryId: $("#Book_CategoryId").val()
    };
}

function appendFormDataWithBookData(formData, data) {
    for (const key in data) {
        formData.append(key, data[key]);
    }

    const imageData = getImageInputValue();
    if (imageData.type === "file") {
        formData.append("ImageFile", imageData.file);
    } else if (imageData.type === "url") {
        formData.append("ImgUrl", imageData.url);
    } else if (imageData.type === "existing") {
        formData.append("ExistingImgUrl", imageData.url);
    }
}

function SubmitData(event) {
    
    if (event) event.preventDefault();

    const form = document.getElementById("addBookForm");
    const formData = new FormData(form);
    const bookData = DataFilledByForm();

    appendFormDataWithBookData(formData, bookData);

    $.ajax({
        url: "/Books/AddBook",
        method: "POST",
        headers: {
            "Authorization": `Bearer ${token}`
        },
        data: formData,
        contentType: false,
        processData: false,
        success: function (response) {
            if (response.success) {
                alert("✅ Book added successfully!");
                $("#addBookForm")[0].reset();
            } else {
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
    
    if (event) event.preventDefault();

    const form = document.getElementById("addBookForm");
    const formData = new FormData(form);
    const bookData = DataFilledByForm();

    appendFormDataWithBookData(formData, bookData);

    $.ajax({
        url: "/Books/UpdateBookDetails",
        method: "POST",
        headers: {
            "Authorization": `Bearer ${token}`
        },
        data: formData,
        contentType: false,
        processData: false,
        success: function (response) {
            if (response.success) {
                alert("✅ Book updated successfully!");
                $("#addBookForm")[0].reset();
                if (response.redirectUrl) {
                    window.location.href = response.redirectUrl;
                }
            } else {
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
                else if (type == "favorite")
                    window.location.href = "/Books/Favorite";
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
    var shipping = subtotal + tax > 1000 ? 0 : 50;
    var grandTotal = subtotal + tax + shipping;

    // Update the totals on the page
    $('#subtotal').text('₹' + subtotal.toFixed(2));  // Format subtotal to 2 decimal places
    $('#tax').text('₹' + tax.toFixed(2));            // Format tax to 2 decimal places
    $('#grand-total').text('₹' + grandTotal.toFixed(2));
}


