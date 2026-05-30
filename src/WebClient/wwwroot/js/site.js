// Modern JS enhancements for Vefa CustomAuth Quickstart

function showToast(message) {
    let toast = document.getElementById('custom-toast');
    if (!toast) {
        toast = document.createElement('div');
        toast.id = 'custom-toast';
        toast.className = 'custom-toast';
        document.body.appendChild(toast);
    }
    toast.innerHTML = `${message}`;
    toast.classList.add('show');
    
    setTimeout(() => {
        toast.classList.remove('show');
    }, 2500);
}

function copyToClipboard(text, label = 'Copied to clipboard') {
    navigator.clipboard.writeText(text).then(() => {
        showToast(label);
    }).catch(err => {
        console.error('Failed to copy: ', err);
    });
}

// Add event listeners to any element with data-copy attribute
document.addEventListener('DOMContentLoaded', () => {
    document.body.addEventListener('click', (e) => {
        const copyBtn = e.target.closest('[data-copy]');
        if (copyBtn) {
            const textToCopy = copyBtn.getAttribute('data-copy');
            const label = copyBtn.getAttribute('data-copy-label') || 'Copied to clipboard';
            copyToClipboard(textToCopy, label);
        }
    });
});
