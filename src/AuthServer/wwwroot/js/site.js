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

// Chrome-only autofill fix: Chrome paints autofilled values with the wrong
// (small) font on first load and only snaps to our CSS font-size after the
// user clicks. The `onAutofill` keyframe in site.css fires an animationstart
// event the moment Chrome fills a field — we catch it and force a repaint of
// that input so the correct font-size applies immediately, no click needed.
// We never read or rewrite the field value (Chrome hides autofilled passwords
// from JS until a user gesture), so this is purely a non-destructive reflow.
document.addEventListener('animationstart', (e) => {
    if (e.animationName !== 'onAutofill') return;
    const input = e.target;
    // Toggle a GPU layer + read layout to force Chrome to re-render the text.
    input.style.transform = 'translateZ(0)';
    void input.offsetHeight; // force reflow
    requestAnimationFrame(() => {
        input.style.transform = '';
    });
});

// Add event listeners to any element with data-copy attribute
document.addEventListener('DOMContentLoaded', () => {
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
