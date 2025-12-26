
$(document).ready(function () {
    const token = sessionStorage.getItem("jwt");

    if (!token) {
        console.error('JWT token not found');
        return;
    }

    const headers = {
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json'
    };

    // Chart 1: Monthly Book Uploads
    fetch('/Books/GetMonthlyBookUploads', { headers })
        .then(res => {
            if (!res.ok) throw new Error('Failed to fetch monthly uploads');
            return res.json();
        })
        .then(data => {
            createBarChart('chart1', data.labels, data.counts, 'Monthly Upload Trends');
        })
        .catch(err => console.error('Error loading monthly uploads:', err));

    // Chart 2: Books by Category
    fetch('/Books/GetBooksByCategory', { headers })
        .then(res => {
            if (!res.ok) throw new Error('Failed to fetch books by category');
            return res.json();
        })
        .then(data => {
            createBarChart('chart2', data.labels, data.counts, 'Book Distribution by Category');
        })
        .catch(err => console.error('Error loading books by category:', err));

    // Chart 3: Books by Author
    fetch('/Books/GetBooksByAuthor', { headers })
        .then(res => {
            if (!res.ok) throw new Error('Failed to fetch books by author');
            return res.json();
        })
        .then(data => {
            createBarChart('chart3', data.labels, data.counts, 'Books per Author');
        })
        .catch(err => console.error('Error loading books by author:', err));

    // Chart 4: Favorite vs Non-Favorite Books
    fetch('/Books/GetFavoriteBookStats', { headers })
        .then(res => {
            if (!res.ok) throw new Error('Failed to fetch favorite stats');
            return res.json();
        })
        .then(data => {
            createPieChart('chart4', data.labels, data.counts, 'Favorites vs Others');
        })
        .catch(err => console.error('Error loading favorite stats:', err));
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
