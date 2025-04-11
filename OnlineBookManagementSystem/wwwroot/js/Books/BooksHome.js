$(document).ready(function () {
    const role = sessionStorage.getItem("userRole");
    const userName = sessionStorage.getItem("userName");

    if (userName) {
        $("#greeting").text("Hello, " + userName);
    }

    if (role === "Admin") {
        $("#adminSection").show();
    }

    // Load books from DB
    $.ajax({
        url: "/api/books/GetAll",
        method: "GET",
        success: function (books) {
            let arrivals = "";
            let recommended = "";

            $.each(books, function (i, book) {
                const html = `<div class="book-card">
                                <img src="${book.imageURL}" alt="${book.title}" style="width:100%; height:200px; object-fit:cover;" />
                                <h6>${book.title}</h6>
                                <small>${book.author}</small>
                              </div>`;
                if (i < 4) arrivals += html;
                recommended += html;
            });

            $("#newArrivals").html(arrivals);
            $("#recommendedBooks").html(recommended);
        },
        error: function () {
            alert("Failed to load books.");
        }
    });
});





$("#addBookForm").on("submit", function (e) {
    e.preventDefault();

    const bookData = {
        title: $("#Title").val(),
        author: $("#Author").val(),
        price: parseFloat($("#Price").val()),
        isbn: $("#ISBN").val(),
        imageUrl: $("#ImageURL").val(),
        stock: parseInt($("#Stock").val())
    };

    console.log("📘 Book Data:", bookData); // For debugging

    // Send data to your API endpoint
    $.ajax({
        url: "/api/books/add", // Change to your actual API endpoint
        method: "POST",
        contentType: "application/json",
        data: JSON.stringify(bookData),
        success: function (response) {
            alert("✅ Book added successfully!");
            $("#addBookForm")[0].reset(); // Clear the form
        },
        error: function (xhr, status, error) {
            console.error("❌ Error adding book:", error);
            alert("Something went wrong while adding the book.");
        }
    });
