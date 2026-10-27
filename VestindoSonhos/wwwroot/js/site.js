// Expandir o menu lateral
var btnExp = document.querySelector('.btn-open');
var menuSide = document.querySelector('.sidebar');
var mainContainer = document.querySelector('.container');

if (btnExp && menuSide) {
    btnExp.addEventListener('click', function () {
        menuSide.classList.toggle('expandir');
        if (mainContainer) {
            mainContainer.classList.toggle('expandir');
        }
    });
}

// =========================================================================
// GERENCIAMENTO GLOBAL DO MODO ESCURO (DARK MODE EM TODO O SISTEMA)
// =========================================================================
window.applySystemTheme = function (isDark) {
    var htmlEl = document.documentElement;
    var bodyEl = document.body;

    if (isDark) {
        htmlEl.classList.add('dark-mode');
        htmlEl.setAttribute('data-bs-theme', 'dark');
        if (bodyEl) bodyEl.classList.add('dark-mode');
    } else {
        htmlEl.classList.remove('dark-mode');
        htmlEl.setAttribute('data-bs-theme', 'light');
        if (bodyEl) bodyEl.classList.remove('dark-mode');
    }

    localStorage.setItem('vs_theme', isDark ? 'dark' : 'light');
    localStorage.setItem('vs_dash_dark', isDark ? '1' : '0');

    // Atualiza botão da Sidebar
    var sidebarIcon = document.getElementById('sidebarThemeIcon');
    var sidebarLabel = document.getElementById('sidebarThemeLabel');
    if (sidebarIcon) sidebarIcon.textContent = isDark ? 'light_mode' : 'dark_mode';
    if (sidebarLabel) sidebarLabel.textContent = isDark ? 'Modo Claro' : 'Modo Escuro';

    // Atualiza botão e wrapper do Dashboard (se estiver na página do Dashboard)
    var dashWrapper = document.getElementById('dashboardWrapper');
    var dashIcon = document.getElementById('iconTheme');
    var dashLabel = document.getElementById('labelTheme');
    if (dashWrapper) {
        if (isDark) dashWrapper.classList.add('vs-dark-mode');
        else dashWrapper.classList.remove('vs-dark-mode');
    }
    if (dashIcon) dashIcon.textContent = isDark ? 'light_mode' : 'dark_mode';
    if (dashLabel) dashLabel.textContent = isDark ? 'Modo Claro' : 'Modo Escuro';

    // Atualiza cores de textos dos gráficos Chart.js ativos
    if (typeof Chart !== 'undefined') {
        Chart.defaults.color = isDark ? '#D1D5DB' : '#4B5563';
        if (Chart.instances) {
            Object.values(Chart.instances).forEach(function (chart) {
                if (chart && typeof chart.update === 'function') {
                    chart.update();
                }
            });
        }
    }
};

window.toggleSystemTheme = function () {
    var isCurrentlyDark = document.documentElement.classList.contains('dark-mode');
    window.applySystemTheme(!isCurrentlyDark);
};

document.addEventListener('DOMContentLoaded', function () {
    var savedTheme = localStorage.getItem('vs_theme');
    var legacyDark = localStorage.getItem('vs_dash_dark');
    var isDark = savedTheme === 'dark' || (!savedTheme && legacyDark === '1');
    window.applySystemTheme(isDark);
});
