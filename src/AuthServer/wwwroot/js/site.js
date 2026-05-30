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

function quickFill(username, password) {
    const userField = document.querySelector('input[name="UserName"]');
    const passField = document.querySelector('input[name="Password"]');
    if (userField && passField) {
        userField.readOnly = false;
        passField.readOnly = false;
        userField.value = username;
        passField.value = password;
        
        // Trigger pulse/focus highlights
        userField.focus();
        setTimeout(() => {
            passField.focus();
        }, 150);
        
        showToast(`Filled credentials for ${username}!`);
    }
}

function unlockLoginFields() {
    document.querySelectorAll('input.form-control[readonly]').forEach((field) => {
        field.readOnly = false;
    });
}

// Add event listeners to any element with data-copy attribute
document.addEventListener('DOMContentLoaded', () => {
    document.querySelectorAll('input.form-control[readonly]').forEach((field) => {
        field.addEventListener('focus', unlockLoginFields, { once: true });
        field.addEventListener('pointerdown', unlockLoginFields, { once: true });
        field.addEventListener('keydown', unlockLoginFields, { once: true });
    });

    document.body.addEventListener('click', (e) => {
        const copyBtn = e.target.closest('[data-copy]');
        if (copyBtn) {
            const textToCopy = copyBtn.getAttribute('data-copy');
            const label = copyBtn.getAttribute('data-copy-label') || 'Copied to clipboard';
            copyToClipboard(textToCopy, label);
        }
        
        const quickFillBtn = e.target.closest('[data-quickfill-user]');
        if (quickFillBtn) {
            const user = quickFillBtn.getAttribute('data-quickfill-user');
            const pass = quickFillBtn.getAttribute('data-quickfill-pass');
            quickFill(user, pass);
        }
    });
});
