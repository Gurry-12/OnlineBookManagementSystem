// Example: Approving a user
function approveUser(userId, role) {
    $.post('/SuperAdmin/ApproveUser', { userId: userId, role: role })
        .done(function (response) {
            if (response.success) {
                alert(response.message);
                location.reload();
            } else {
                alert('Error: ' + response.message);
            }
        });
}
// Date and time (no jQuery)
function updateDateTime() {
    const now = new Date();
    const time = now.toLocaleTimeString();
    const options = { day: '2-digit', month: 'short', year: 'numeric' };
    const dateStr = now.toLocaleDateString('en-GB', options).replace(/ /g, '-');

    document.querySelectorAll('.currentdate').forEach(el => el.innerHTML = dateStr);
    document.querySelectorAll('.currenttime').forEach(el => el.innerHTML = time);
}

// Initial call + timer - Changed to update every minute instead of every second to reduce performance impact
updateDateTime();
setInterval(updateDateTime, 60000); // Changed from 1000ms to 60000ms (1 minute)

// Sidebar toggle
document.getElementById('toggleSidebar')?.addEventListener('click', () => {
    document.getElementById('sidebar')?.classList.toggle('collapsed');
    document.querySelector('.content')?.classList.toggle('sidebar-collapsed');
});

// Display role (if you still have userRole variable)
if (typeof userRole !== 'undefined' && userRole) {
    const el = document.getElementById('SupportloginDetail');
    if (el) el.innerHTML = userRole;
}