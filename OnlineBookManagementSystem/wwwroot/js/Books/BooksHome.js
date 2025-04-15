const token = sessionStorage.getItem("jwt");
const role = sessionStorage.getItem("userRole");
const userName = sessionStorage.getItem("userName");

function SubmitData() {

 
    const bookData = DataFilledByForm();


    console.log("📘 Book Data:", bookData); // Debugging

    $.ajax({
        url: "/Books/AddBook", // Ensure this matches your controller's route
        method: "POST",
        headers: {
            "Authorization": `Bearer ${token}`
        },
        contentType: "application/json",
        data: JSON.stringify(bookData),
        success: function (response) {
            alert("✅ Book added successfully!");
            $("#addBookForm")[0].reset(); // Clear form
        },
        error: function (xhr, status, error) {
            console.error("❌ Error adding book:", error);
            alert("Something went wrong while adding the book.");
        }
    });
}

function DataFilledByForm() {
    const bookData = {
        Id : $("#Id").val(),
        Title: $("#Title").val(),
        Author: $("#Author").val(),
        Price: parseFloat($("#Price").val()),
        Isbn: $("#Isbn").val(),
        ImgUrl: $("#ImgUrl").val(),
        Stock: $("#Stock").val()
    };
    return bookData;
}


function UpdateData() {
    debugger;
    const bookData = DataFilledByForm();
    console.log("📘 Book Data:", bookData); // Debugging

    $.ajax({
        url: "/Books/UpdateBookDetails", // Ensure this matches your controller's route
        method: "POST",
        headers: {
            "Authorization": `Bearer ${token}`
        },
        contentType: "application/json",
        data: JSON.stringify(bookData),
        success: function (response) {

            debugger;
            alert("✅ Book Update successfully!");
            $("#addBookForm")[0].reset(); // Clear form
            window.location.href = response.redirectUrl;
        },
        error: function (xhr, status, error) {
            console.error("❌ Error adding book:", error);
            alert("Something went wrong while adding the book.");
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
            console.log(response)
            window.location.href = response.redirectUrl;
        },
        error: function (xhr, status, error) {
            console.error("❌ Error :", error);
            alert("Something went wrong.");
        }


    })
}

$(document).ready(function () {
   
    if (userName) {
        $("#greeting").text("Hello, " + userName);
    }

    if (role === "Admin") {
        // Show admin panel only, skip book data
        $("#adminSection").show();
    } else if (role === "User") {
        // Show books if the user is not admin
        $.ajax({
            url: "/books/GetBooks",
            method: "GET",
            headers: {
                "Authorization": `Bearer ${token}`
            },
            success: function (response) {
                let arrivals = "";
                let recommended = "";

                console.log(response.data);
                $.each(response.data, function (i, book) {
                    const html = `<div class="book-card">
                             <img src="${book.imgUrl}" alt="${book.title}" style="width:100%; height:200px; object-fit:cover;" />
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
    } else {
        alert("Unauthorized: Invalid role");
    }
});



//function GetBookDetails(id) {
  
//    $.ajax({
//        url: `/Books/GetBookDetails/${id}`,
//        type: "GET",
//        headers: {
//            "Authorization": `Bearer ${token}`
//        },
//        success: function (response) {
//            console.log(response)
            
           

//        },
//        error: function () {
//            alert("Failed to load books.");
//        },
//    });
//}