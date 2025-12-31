// Date and time (no jQuery)
function updateDateTime() {
    const now = new Date();
    const time = now.toLocaleTimeString();
    const options = { day: '2-digit', month: 'short', year: 'numeric' };
    const dateStr = now.toLocaleDateString('en-GB', options).replace(/ /g, '-');

    document.querySelectorAll('.currentdate').forEach(el => el.innerHTML = dateStr);
    document.querySelectorAll('.currenttime').forEach(el => el.innerHTML = time);
}

// Initial call + timer
updateDateTime();
setInterval(updateDateTime, 1000);

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