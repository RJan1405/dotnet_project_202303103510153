document.addEventListener('DOMContentLoaded', function () {
    const root = document.documentElement;
    const btn = document.getElementById('themeToggle');
    const saved = localStorage.getItem('theme');
    const media = window.matchMedia('(prefers-color-scheme: dark)');
    const initial = saved || (media.matches ? 'dark' : 'light');

    // Add smooth transition class
    const addTransition = () => {
        root.style.transition = 'all 0.3s cubic-bezier(0.4, 0, 0.2, 1)';
        setTimeout(() => {
            root.style.transition = '';
        }, 300);
    };

    const setTheme = (mode, persist = true) => {
        addTransition();
        root.setAttribute('data-theme', mode);
        
        if (btn) {
            // Enhanced button animation
            btn.style.transform = 'scale(0.8) rotate(180deg)';
            
            setTimeout(() => {
                btn.innerHTML = mode === 'dark' 
                    ? '<i class="bi bi-moon-stars-fill"></i>' 
                    : '<i class="bi bi-sun-fill"></i>';
                btn.style.transform = 'scale(1) rotate(0deg)';
            }, 150);
        }
        
        if (persist) localStorage.setItem('theme', mode);

        // Add ripple effect to body
        const ripple = document.createElement('div');
        ripple.style.cssText = `
            position: fixed;
            top: 50%;
            left: 50%;
            width: 0;
            height: 0;
            border-radius: 50%;
            background: ${mode === 'dark' ? 'rgba(168, 85, 247, 0.1)' : 'rgba(124, 58, 237, 0.1)'};
            transform: translate(-50%, -50%);
            pointer-events: none;
            z-index: 9999;
            transition: all 0.6s cubic-bezier(0.4, 0, 0.2, 1);
        `;
        
        document.body.appendChild(ripple);
        
        requestAnimationFrame(() => {
            ripple.style.width = '200vmax';
            ripple.style.height = '200vmax';
        });
        
        setTimeout(() => {
            document.body.removeChild(ripple);
        }, 600);
    };

    // Initialize theme
    setTheme(initial, !!saved);

    if (btn) {
        // Enhanced button styling
        btn.style.cssText = `
            transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
            border-radius: 50%;
            width: 40px;
            height: 40px;
            display: flex;
            align-items: center;
            justify-content: center;
            background: var(--surface-2);
            border: 2px solid var(--border);
            color: var(--text);
            box-shadow: var(--shadow-1);
        `;

        btn.addEventListener('click', () => {
            const next = root.getAttribute('data-theme') === 'dark' ? 'light' : 'dark';
            setTheme(next);
        });

        // Add hover effects
        btn.addEventListener('mouseenter', () => {
            btn.style.transform = 'scale(1.1)';
            btn.style.boxShadow = 'var(--shadow-3)';
        });

        btn.addEventListener('mouseleave', () => {
            btn.style.transform = 'scale(1)';
            btn.style.boxShadow = 'var(--shadow-1)';
        });
    }

    // System preference tracking
    if (!saved) {
        media.addEventListener('change', (e) => {
            setTheme(e.matches ? 'dark' : 'light', false);
        });
    }

    // Add keyboard shortcut (Ctrl/Cmd + Shift + T)
    document.addEventListener('keydown', (e) => {
        if ((e.ctrlKey || e.metaKey) && e.shiftKey && e.key === 'T') {
            e.preventDefault();
            const next = root.getAttribute('data-theme') === 'dark' ? 'light' : 'dark';
            setTheme(next);
        }
    });
});