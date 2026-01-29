// Enhanced Admin Dashboard Charts with seed data
$(document).ready(function () {
    // Initialize charts with loading states
    initializeCharts();

    // Load chart data with seed values
    loadAllChartData();

    // Add chart refresh functionality
    setupChartRefresh();
});

// Color schemes for different chart types
const chartColors = {
    primary: ['#007bff', '#6610f2', '#6f42c1', '#e83e8c', '#dc3545', '#fd7e14', '#ffc107', '#28a745', '#20c997', '#17a2b8'],
    gradient: {
        blue: ['#007bff', '#0056b3'],
        green: ['#28a745', '#1e7e34'],
        orange: ['#fd7e14', '#e55100'],
        purple: ['#6f42c1', '#5a32a3']
    },
    status: {
        success: '#28a745',
        warning: '#ffc107',
        danger: '#dc3545',
        info: '#17a2b8'
    }
};

// Seed data for charts
const seedData = {
    monthly: [
        { month: '2024-01', count: 15 },
        { month: '2024-02', count: 23 },
        { month: '2024-03', count: 18 },
        { month: '2024-04', count: 31 },
        { month: '2024-05', count: 27 },
        { month: '2024-06', count: 35 },
        { month: '2024-07', count: 42 },
        { month: '2024-08', count: 38 },
        { month: '2024-09', count: 29 },
        { month: '2024-10', count: 33 },
        { month: '2024-11', count: 41 },
        { month: '2024-12', count: 47 }
    ],
    category: [
        { categoryName: 'Fiction', count: 125 },
        { categoryName: 'Science', count: 89 },
        { categoryName: 'Technology', count: 76 },
        { categoryName: 'History', count: 54 },
        { categoryName: 'Biography', count: 43 },
        { categoryName: 'Philosophy', count: 32 },
        { categoryName: 'Art', count: 28 },
        { categoryName: 'Business', count: 67 }
    ],
    author: [
        { authorName: 'J.K. Rowling', count: 12 },
        { authorName: 'Stephen King', count: 18 },
        { authorName: 'Agatha Christie', count: 15 },
        { authorName: 'Isaac Asimov', count: 22 },
        { authorName: 'George Orwell', count: 8 },
        { authorName: 'Jane Austen', count: 10 },
        { authorName: 'Mark Twain', count: 14 },
        { authorName: 'Charles Dickens', count: 16 }
    ],
    favorites: {
        favoriteCount: 156,
        totalCount: 514
    },
    revenue: [
        { month: '2024-01', revenue: 2450.75 },
        { month: '2024-02', revenue: 3120.50 },
        { month: '2024-03', revenue: 2890.25 },
        { month: '2024-04', revenue: 4230.80 },
        { month: '2024-05', revenue: 3750.60 },
        { month: '2024-06', revenue: 4580.90 },
        { month: '2024-07', revenue: 5120.45 },
        { month: '2024-08', revenue: 4890.30 },
        { month: '2024-09', revenue: 3960.75 },
        { month: '2024-10', revenue: 4320.85 },
        { month: '2024-11', revenue: 5240.60 },
        { month: '2024-12', revenue: 6150.95 }
    ],
    orderStatus: [
        { status: 'Completed', count: 342 },
        { status: 'Processing', count: 89 },
        { status: 'Pending', count: 156 },
        { status: 'Cancelled', count: 23 }
    ]
};

function initializeCharts() {
    // Show loading spinners for all charts
    const chartIds = ['monthlyChart', 'categoryChart', 'chart1', 'chart2', 'chart3', 'chart4', 'revenueChart', 'orderStatusChart'];
    chartIds.forEach(id => {
        const canvas = document.getElementById(id);
        if (canvas) {
            showChartLoading(id);
        }
    });
}

function showChartLoading(canvasId) {
    const canvas = document.getElementById(canvasId);
    if (!canvas) return;

    const ctx = canvas.getContext('2d');
    ctx.clearRect(0, 0, canvas.width, canvas.height);
    ctx.fillStyle = '#f8f9fa';
    ctx.fillRect(0, 0, canvas.width, canvas.height);
    ctx.fillStyle = '#6c757d';
    ctx.font = '16px Arial';
    ctx.textAlign = 'center';
    ctx.fillText('Loading...', canvas.width / 2, canvas.height / 2);
}

function loadAllChartData() {
    console.log('Loading charts with seed data...');

    // Add a small delay to simulate loading
    setTimeout(() => {
        // Load main dashboard charts with seed data
        loadMonthlyChart();
        loadCategoryChart();

        // Load additional analytics charts with seed data
        loadMonthlyTrendsChart();
        loadCategoryDistributionChart();
        loadAuthorAnalyticsChart();
        loadFavoriteStatsChart();

        // Load revenue and order status charts if available
        loadRevenueChart();
        loadOrderStatusChart();
    }, 500);
}

function loadMonthlyChart() {
    try {
        createEnhancedLineChart('monthlyChart', seedData.monthly, 'Monthly Book Uploads');
        console.log('Monthly chart loaded successfully');
    } catch (err) {
        console.error('Error loading monthly chart:', err);
        showChartError('monthlyChart', 'Failed to load monthly data');
    }
}

function loadCategoryChart() {
    try {
        createEnhancedDoughnutChart('categoryChart', seedData.category, 'Books by Category');
        console.log('Category chart loaded successfully');
    } catch (err) {
        console.error('Error loading category chart:', err);
        showChartError('categoryChart', 'Failed to load category data');
    }
}

function loadMonthlyTrendsChart() {
    try {
        createGradientBarChart('chart1', seedData.monthly, 'Monthly Upload Trends', chartColors.gradient.blue);
        console.log('Monthly trends chart loaded successfully');
    } catch (err) {
        console.error('Error loading monthly trends:', err);
    }
}

function loadCategoryDistributionChart() {
    try {
        createHorizontalBarChart('chart2', seedData.category, 'Book Distribution by Category');
        console.log('Category distribution chart loaded successfully');
    } catch (err) {
        console.error('Error loading category distribution:', err);
    }
}

function loadAuthorAnalyticsChart() {
    try {
        createPolarAreaChart('chart3', seedData.author, 'Books per Author');
        console.log('Author analytics chart loaded successfully');
    } catch (err) {
        console.error('Error loading author analytics:', err);
    }
}

function loadFavoriteStatsChart() {
    try {
        createAnimatedPieChart('chart4', seedData.favorites, 'Favorites vs Others');
        console.log('Favorite stats chart loaded successfully');
    } catch (err) {
        console.error('Error loading favorite stats:', err);
    }
}

function loadRevenueChart() {
    try {
        createRevenueChart('revenueChart', seedData.revenue, 'Monthly Revenue');
        console.log('Revenue chart loaded successfully');
    } catch (err) {
        console.error('Error loading revenue chart:', err);
        if (document.getElementById('revenueChart')) {
            showChartError('revenueChart', 'Revenue data not available');
        }
    }
}

function loadOrderStatusChart() {
    try {
        createOrderStatusChart('orderStatusChart', seedData.orderStatus, 'Order Status Distribution');
        console.log('Order status chart loaded successfully');
    } catch (err) {
        console.error('Error loading order status chart:', err);
        if (document.getElementById('orderStatusChart')) {
            showChartError('orderStatusChart', 'Order status data not available');
        }
    }
}

// Enhanced chart creation functions
function createEnhancedLineChart(canvasId, data, title) {
    const canvas = document.getElementById(canvasId);
    if (!canvas) return;

    const ctx = canvas.getContext('2d');

    // Create gradient
    const gradient = ctx.createLinearGradient(0, 0, 0, 400);
    gradient.addColorStop(0, 'rgba(0, 123, 255, 0.8)');
    gradient.addColorStop(1, 'rgba(0, 123, 255, 0.1)');

    new Chart(ctx, {
        type: 'line',
        data: {
            labels: data.map(item => item.month || item.label),
            datasets: [{
                label: 'Books Uploaded',
                data: data.map(item => item.count || item.value),
                backgroundColor: gradient,
                borderColor: '#007bff',
                borderWidth: 3,
                fill: true,
                tension: 0.4,
                pointBackgroundColor: '#007bff',
                pointBorderColor: '#ffffff',
                pointBorderWidth: 2,
                pointRadius: 6,
                pointHoverRadius: 8
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: {
                    display: false
                },
                tooltip: {
                    backgroundColor: 'rgba(0, 0, 0, 0.8)',
                    titleColor: '#ffffff',
                    bodyColor: '#ffffff',
                    borderColor: '#007bff',
                    borderWidth: 1
                }
            },
            scales: {
                y: {
                    beginAtZero: true,
                    grid: {
                        color: 'rgba(0, 0, 0, 0.1)'
                    },
                    ticks: {
                        color: '#6c757d'
                    }
                },
                x: {
                    grid: {
                        display: false
                    },
                    ticks: {
                        color: '#6c757d'
                    }
                }
            },
            animation: {
                duration: 200
            }
        }
    });
}

function createEnhancedDoughnutChart(canvasId, data, title) {
    const canvas = document.getElementById(canvasId);
    if (!canvas) return;

    const ctx = canvas.getContext('2d');

    new Chart(ctx, {
        type: 'doughnut',
        data: {
            labels: data.map(item => item.categoryName || item.label),
            datasets: [{
                data: data.map(item => item.count || item.value),
                backgroundColor: chartColors.primary,
                borderWidth: 2,
                borderColor: '#ffffff',
                hoverBorderWidth: 3,
                hoverBorderColor: '#ffffff'
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            cutout: '60%',
            plugins: {
                legend: {
                    position: 'bottom',
                    labels: {
                        padding: 20,
                        usePointStyle: true,
                        color: '#6c757d'
                    }
                },
                tooltip: {
                    backgroundColor: 'rgba(0, 0, 0, 0.8)',
                    titleColor: '#ffffff',
                    bodyColor: '#ffffff'
                }
            },
            animation: {
                duration: 200
            }
        }
    });
}
  

function createGradientBarChart(canvasId, data, title, gradientColors) {
    const canvas = document.getElementById(canvasId);
    if (!canvas) return;

    const ctx = canvas.getContext('2d');

    const gradient = ctx.createLinearGradient(0, 0, 0, 400);
    gradient.addColorStop(0, gradientColors[0]);
    gradient.addColorStop(1, gradientColors[1]);

    new Chart(ctx, {
        type: 'bar',
        data: {
            labels: data.map(item => item.month || item.label),
            datasets: [{
                label: title,
                data: data.map(item => item.count || item.value),
                backgroundColor: gradient,
                borderColor: gradientColors[0],
                borderWidth: 2,
                borderRadius: 8,
                borderSkipped: false
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: { display: false },
                tooltip: {
                    backgroundColor: 'rgba(0, 0, 0, 0.8)',
                    titleColor: '#ffffff',
                    bodyColor: '#ffffff'
                }
            },
            scales: {
                y: {
                    beginAtZero: true,
                    grid: { color: 'rgba(0, 0, 0, 0.1)' },
                    ticks: { color: '#6c757d' }
                },
                x: {
                    grid: { display: false },
                    ticks: { color: '#6c757d' }
                }
            },
            animation: {
                duration: 200
            }
        }
    });
}

function createHorizontalBarChart(canvasId, data, title) {
    const canvas = document.getElementById(canvasId);
    if (!canvas) return;

    const ctx = canvas.getContext('2d');

    new Chart(ctx, {
        type: 'bar',
        data: {
            labels: data.map(item => item.categoryName || item.label),
            datasets: [{
                label: title,
                data: data.map(item => item.count || item.value),
                backgroundColor: chartColors.primary.slice(0, data.length),
                borderColor: chartColors.primary.slice(0, data.length),
                borderWidth: 1,
                borderRadius: 4
            }]
        },
        options: {
            indexAxis: 'y',
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: { display: false },
                tooltip: {
                    backgroundColor: 'rgba(0, 0, 0, 0.8)',
                    titleColor: '#ffffff',
                    bodyColor: '#ffffff'
                }
            },
            scales: {
                x: {
                    beginAtZero: true,
                    grid: { color: 'rgba(0, 0, 0, 0.1)' },
                    ticks: { color: '#6c757d' }
                },
                y: {
                    grid: { display: false },
                    ticks: { color: '#6c757d' }
                }
            },
            animation: {
                duration: 200
            }
        }
    });
}

function createPolarAreaChart(canvasId, data, title) {
    const canvas = document.getElementById(canvasId);
    if (!canvas) return;

    const ctx = canvas.getContext('2d');

    new Chart(ctx, {
        type: 'polarArea',
        data: {
            labels: data.map(item => item.authorName || item.label),
            datasets: [{
                data: data.map(item => item.count || item.value),
                backgroundColor: chartColors.primary.slice(0, data.length).map(color => color + '80'),
                borderColor: chartColors.primary.slice(0, data.length),
                borderWidth: 2
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: {
                    position: 'bottom',
                    labels: {
                        padding: 15,
                        usePointStyle: true,
                        color: '#6c757d'
                    }
                },
                tooltip: {
                    backgroundColor: 'rgba(0, 0, 0, 0.8)',
                    titleColor: '#ffffff',
                    bodyColor: '#ffffff'
                }
            },
            scales: {
                r: {
                    beginAtZero: true,
                    grid: { color: 'rgba(0, 0, 0, 0.1)' },
                    ticks: { color: '#6c757d' }
                }
            },
            animation: {
                duration: 200
            }
        }
    });
}

function createAnimatedPieChart(canvasId, data, title) {
    const canvas = document.getElementById(canvasId);
    if (!canvas) return;

    const ctx = canvas.getContext('2d');

    // Handle different data structures
    let labels, values;
    if (data.favoriteCount !== undefined) {
        labels = ['Favorites', 'Others'];
        values = [data.favoriteCount, data.totalCount - data.favoriteCount];
    } else {
        labels = data.map(item => item.label);
        values = data.map(item => item.value);
    }

    new Chart(ctx, {
        type: 'pie',
        data: {
            labels: labels,
            datasets: [{
                data: values,
                backgroundColor: [chartColors.status.success, chartColors.status.info],
                borderColor: '#ffffff',
                borderWidth: 3,
                hoverBorderWidth: 4
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: {
                    position: 'bottom',
                    labels: {
                        padding: 20,
                        usePointStyle: true,
                        color: '#6c757d'
                    }
                },
                tooltip: {
                    backgroundColor: 'rgba(0, 0, 0, 0.8)',
                    titleColor: '#ffffff',
                    bodyColor: '#ffffff',
                    callbacks: {
                        label: function (context) {
                            const total = context.dataset.data.reduce((a, b) => a + b, 0);
                            const percentage = ((context.parsed / total) * 100).toFixed(1);
                            return `${context.label}: ${context.parsed} (${percentage}%)`;
                        }
                    }
                }
            },
            animation: {
                duration: 200
            }
        }
    });
}

function createRevenueChart(canvasId, data, title) {
    const canvas = document.getElementById(canvasId);
    if (!canvas) return;

    const ctx = canvas.getContext('2d');
    const gradient = ctx.createLinearGradient(0, 0, 0, 400);
    gradient.addColorStop(0, 'rgba(40, 167, 69, 0.8)');
    gradient.addColorStop(1, 'rgba(40, 167, 69, 0.1)');

    new Chart(ctx, {
        type: 'line',
        data: {
            labels: data.map(item => item.month),
            datasets: [{
                label: 'Revenue ($)',
                data: data.map(item => item.revenue),
                backgroundColor: gradient,
                borderColor: '#28a745',
                borderWidth: 3,
                fill: true,
                tension: 0.4,
                pointBackgroundColor: '#28a745',
                pointBorderColor: '#ffffff',
                pointBorderWidth: 2,
                pointRadius: 5,
                pointHoverRadius: 7
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: { display: false },
                tooltip: {
                    backgroundColor: 'rgba(0, 0, 0, 0.8)',
                    titleColor: '#ffffff',
                    bodyColor: '#ffffff',
                    callbacks: {
                        label: function (context) {
                            return `Revenue: ₹${context.parsed.y.toFixed(2)}`;
                        }
                    }
                }
            },
            scales: {
                y: {
                    beginAtZero: true,
                    grid: { color: 'rgba(0, 0, 0, 0.1)' },
                    ticks: {
                        color: '#6c757d',
                        callback: function (value) {
                            return '₹' + value.toFixed(0);
                        }
                    }
                },
                x: {
                    grid: { display: false },
                    ticks: { color: '#6c757d' }
                }
            },
            animation: {
                duration: 200
            }
        }
    });
}

function createOrderStatusChart(canvasId, data, title) {
    const canvas = document.getElementById(canvasId);
    if (!canvas) return;

    const ctx = canvas.getContext('2d');

    new Chart(ctx, {
        type: 'doughnut',
        data: {
            labels: data.map(item => item.status),
            datasets: [{
                data: data.map(item => item.count),
                backgroundColor: [
                    chartColors.status.success,
                    chartColors.status.warning,
                    chartColors.status.danger,
                    chartColors.status.info
                ],
                borderWidth: 2,
                borderColor: '#ffffff',
                hoverBorderWidth: 3
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            cutout: '50%',
            plugins: {
                legend: {
                    position: 'bottom',
                    labels: {
                        padding: 15,
                        usePointStyle: true,
                        color: '#6c757d'
                    }
                },
                tooltip: {
                    backgroundColor: 'rgba(0, 0, 0, 0.8)',
                    titleColor: '#ffffff',
                    bodyColor: '#ffffff',
                    callbacks: {
                        label: function (context) {
                            const total = context.dataset.data.reduce((a, b) => a + b, 0);
                            const percentage = ((context.parsed / total) * 100).toFixed(1);
                            return `${context.label}: ${context.parsed} (${percentage}%)`;
                        }
                    }
                }
            },
            animation: {
                duration: 200
            }
        }
    });
}

function showChartError(canvasId, message) {
    const canvas = document.getElementById(canvasId);
    if (!canvas) return;

    const ctx = canvas.getContext('2d');
    ctx.clearRect(0, 0, canvas.width, canvas.height);
    ctx.fillStyle = '#f8f9fa';
    ctx.fillRect(0, 0, canvas.width, canvas.height);
    ctx.fillStyle = '#dc3545';
    ctx.font = '14px Arial';
    ctx.textAlign = 'center';
    ctx.fillText(message || 'Error loading chart', canvas.width / 2, canvas.height / 2);
}

function setupChartRefresh() {
    // Add refresh button functionality if needed
    const refreshBtn = document.getElementById('refreshCharts');
    if (refreshBtn) {
        refreshBtn.addEventListener('click', function () {
            console.log('Refreshing charts...');
            initializeCharts();
            loadAllChartData();
        });
    }

    // Removed auto-refresh interval to prevent infinite chart growth
    console.log('Chart auto-refresh disabled to prevent infinite growth');
}