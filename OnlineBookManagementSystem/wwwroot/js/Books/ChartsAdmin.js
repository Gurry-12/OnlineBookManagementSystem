
$(document).ready(function () {
    // Chart 1: Monthly Book Uploads
    fetch('/Books/GetMonthlyBookUploads')
        .then(res => res.json())
        .then(data => {
            createBarChart('chart1', data.labels, data.counts, 'Monthly Upload Trends');
        });

    // Chart 2: Books by Category
    fetch('/Books/GetBooksByCategory')
        .then(res => res.json())
        .then(data => {
            createBarChart('chart2', data.labels, data.counts, 'Book Distribution by Category');
        });

    // Chart 3: Books by Author
    fetch('/Books/GetBooksByAuthor')
        .then(res => res.json())
        .then(data => {
            createBarChart('chart3', data.labels, data.counts, 'Books per Author');
        });

    // Chart 4: Favorite vs Non-Favorite Books
    fetch('/Books/GetFavoriteBookStats')
        .then(res => res.json())
        .then(data => {
            createPieChart('chart4', data.labels, data.counts, 'Favorites vs Others');
        });
});

function createBarChart(canvasId, labels, data, labelText) {
    const ctx = document.getElementById(canvasId).getContext('2d');
    new Chart(ctx, {
        type: 'bar',
        data: {
            labels,
            datasets: [{
                label: labelText,
                data,
                backgroundColor: 'rgba(75, 192, 192, 0.6)',
                borderColor: 'rgba(75, 192, 192, 1)',
                borderWidth: 1
            }]
        },
        options: {
            responsive: true,
            plugins: {
                legend: { display: false }
            },
            scales: {
                y: {
                    beginAtZero: true,
                    title: {
                        display: true,
                        text: 'Count'
                    }
                },
                x: {
                    title: {
                        display: true,
                        text: 'Category'
                    }
                }
            }
        }
    });
}

function createPieChart(canvasId, labels, data, labelText) {
    const ctx = document.getElementById(canvasId).getContext('2d');
    new Chart(ctx, {
        type: 'pie',
        data: {
            labels,
            datasets: [{
                label: labelText,
                data,
                backgroundColor: ['#36A2EB', '#FF6384'],
                borderWidth: 1
            }]
        },
        options: {
            responsive: true,
            plugins: {
                legend: { position: 'top' },
                title: {
                    display: true,
                    text: labelText
                }
            }
        }
    });
}
